using System.Diagnostics;
using AudioProcessor.Models;
using AudioProcessor.Repositories;
using Supabase;

namespace AudioProcessor.Services;

public class AudioService : IAudioService
{
    private readonly IAudioRepository _audioRepository; 
    private readonly IStorageService _storageService;
    private readonly IAudioCompressorService _audioCompressorService;
    private readonly IAudioTranscriptionService _audioTranscriptionService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly HashSet<string> _allowedFormats;

    public AudioService(
        IAudioRepository audioRepository, 
        IStorageService storageService, 
        IAudioCompressorService audioCompressorService,
        IAudioTranscriptionService audioTranscriptionService,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _audioRepository = audioRepository;
        _storageService = storageService;
        _audioCompressorService = audioCompressorService;
        _audioTranscriptionService = audioTranscriptionService;
        _backgroundTaskQueue= backgroundTaskQueue;
        _allowedFormats = Enum.GetValues<AudioFormat>().Select(x => x.GetExtension()).ToHashSet();
    }

    public async Task<AudioData> Upload(IFormFile file)
    {
        var totalSw = Stopwatch.StartNew();
        var uuid = Guid.NewGuid();
        var type = Path.GetExtension(file.FileName).ToLowerInvariant();
        var nameInBucket = $"{uuid}.aac";

        if (!_allowedFormats.Contains(type))
            throw new NotSupportedException($"File type {type} is not supported.");

        string? pathToProcess = null;
        byte[] fileBytes;
        
        if (type == ".aac")
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
            
            // salvando o local no disco
            pathToProcess = Path.Combine(Path.GetTempPath(), nameInBucket);
            await File.WriteAllBytesAsync(pathToProcess, fileBytes);
        }
        else
        {
            //compressor
            var tempInput = Path.Combine(Path.GetTempPath(), $"{uuid}{type}");
            using (var stream = new FileStream(tempInput, FileMode.Create))
                await file.CopyToAsync(stream);
            
            pathToProcess = await _audioCompressorService.CompressToAac(tempInput, uuid);
            fileBytes = await File.ReadAllBytesAsync(pathToProcess);
            
            if (File.Exists(tempInput)) File.Delete(tempInput);
        }

        // upload
        await _storageService.Upload(fileBytes, nameInBucket);
        var publicUrl = _storageService.GetPublicUrl(nameInBucket);

        var data = new AudioData
        {
            Id = uuid,
            FileName = file.FileName,
            FileType = type,
            FileUrl = publicUrl,
            TranscriptionPreview = "Processing...", // Placeholder para a transcription
            CreatedAt = DateTime.UtcNow
        };
        
        await _audioRepository.Save(data);
        Console.WriteLine($"[API] Uploaded data and file");
        Console.WriteLine($"[API] Time Elapsed {totalSw.ElapsedMilliseconds}ms");

        //THREAD DE BACKGROUND
        // Disparando sem 'await' para retorno rápido
        // _ = Task.Run(async () => 
        // { 
        //     var transSw = Stopwatch.StartNew(); 
        //     string? wavPath = null; 
        //     try 
        //     { 
        //         if (pathToProcess != null && File.Exists(pathToProcess)) 
        //         { 
        //             Console.WriteLine($"[THREAD] Initializing AI transcription for: {uuid}"); 
        //             await Task.Delay(100);
        //             wavPath = await _audioCompressorService.ProcessToWav(pathToProcess, uuid); 
                    
        //             var text = await _audioTranscriptionService.GetTranscription(wavPath); 
        //             Console.WriteLine($"[THREAD] Whisper concluded transcription in {transSw.Elapsed.TotalSeconds:F2}s"); 
                    
        //             data.TranscriptionPreview = text;
        //             await _audioRepository.Update(data); 
                    
        //             transSw.Stop(); 
        //             Console.WriteLine($"[THREAD SUCCESS] Total flow for task {uuid} concluded in {transSw.Elapsed.TotalSeconds:F2}s"); 
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         transSw.Stop();
        //         Console.WriteLine($"[THREAD ERRO] Error at processing {uuid} after {transSw.Elapsed.TotalSeconds:F2}s: {ex.Message}");
        //     }
        //     finally 
        //     {
        //         // Limpeza dos arquivos temporários
        //         if (wavPath != null && File.Exists(wavPath)) File.Delete(wavPath);
        //         if (pathToProcess != null && File.Exists(pathToProcess)) File.Delete(pathToProcess);
                
        //         Console.WriteLine($"[THREAD] Temp files for {uuid} removed.");
        //     }
        // });

        await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(new AudioJob(data, pathToProcess));

        totalSw.Stop();
        Console.WriteLine($"[API] Upload in {totalSw.ElapsedMilliseconds}ms.");
        
        return data;
    }
}