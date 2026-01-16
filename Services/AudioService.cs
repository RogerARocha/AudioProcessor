using AudioProcessor.Models;
using AudioProcessor.Repositories;
using Supabase;

namespace AudioProcessor.Services;

public interface IAudioService
{
    Task<AudioData> Upload(IFormFile file);
}

public class AudioService : IAudioService
{
    private readonly IAudioRepository _audioRepository; 
    private readonly Client _supabaseClient;

    private readonly HashSet<string> _allowedFormats;
    
    private readonly IAudioCompressorService _audioCompressorService;
    
    public AudioService(IAudioRepository audioRepository, 
        Client supabaseClient, 
        IAudioCompressorService audioCompressorService)
    {
        _audioRepository = audioRepository;
        _supabaseClient = supabaseClient;
        
        _allowedFormats = Enum.GetValues<AudioFormat>().Select(x => x.GetExtension()).ToHashSet();
        
        _audioCompressorService = audioCompressorService;
    }

    public async Task<AudioData> Upload(IFormFile file)
    {
        var bucket = "Audios";
        var uuid = Guid.NewGuid();
        var type = Path.GetExtension(file.FileName);
        var nameInBucket = $"{uuid}.aac";

        if (!_allowedFormats.Contains(type))
        {
            throw new NotSupportedException($"File type {type} is not supported.");
        }
        
        //  .aac, pula o processamento
        byte[] fileBytes; 
        if (type == ".aac")
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }
        else
        {
            var tempInput = Path.Combine(Path.GetTempPath(), $"{uuid}{type}");
            try 
            {
                using (var stream = new FileStream(tempInput, FileMode.Create))
                    await file.CopyToAsync(stream);
                
                var compressedPath = await _audioCompressorService.CompressToAac(tempInput, uuid);
                
                fileBytes = await File.ReadAllBytesAsync(compressedPath);
                
                if (File.Exists(compressedPath)) File.Delete(compressedPath);
            }
            finally 
            {
                // limpeza
                if (File.Exists(tempInput)) File.Delete(tempInput);
            }
        }

        // upload
        await _supabaseClient.Storage.From(bucket).Upload(fileBytes, nameInBucket);
        var publicUrl = _supabaseClient.Storage.From(bucket).GetPublicUrl(nameInBucket);

        var data = new AudioData
        {
            Id = uuid,
            FileName = file.FileName,
            FileType = type,
            FileUrl = publicUrl,
            CreatedAt = DateTime.UtcNow
        };
        
        await _audioRepository.Save(data);
        return data;
    }
}