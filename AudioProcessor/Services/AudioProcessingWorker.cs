using AudioProcessor.Repositories;
using AudioProcessor.Models;

namespace AudioProcessor.Services;

public class AudioProcessingWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;

    public AudioProcessingWorker(IBackgroundTaskQueue taskQueue, IServiceProvider serviceProvider)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var parallelOptions = new ParallelOptions 
        { 
            MaxDegreeOfParallelism = 4, 
            CancellationToken = stoppingToken 
        };

        await Parallel.ForEachAsync(
            Enumerable.Range(0, parallelOptions.MaxDegreeOfParallelism), 
            parallelOptions, 
            async (_, ct) => 
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try 
                {
                    var job = await _taskQueue.DequeueAsync(stoppingToken);
                    
                    using var scope = _serviceProvider.CreateScope();
                    var compressor = scope.ServiceProvider.GetRequiredService<IAudioCompressorService>();
                    var transcriber = scope.ServiceProvider.GetRequiredService<IAudioTranscriptionService>();
                    var repository = scope.ServiceProvider.GetRequiredService<IAudioRepository>();
                    var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();

                    string? wavPath = null;
                    string? slowedPath = null;

                    try 
                    {
                        Console.WriteLine($"[WORKER] Processing parallel task: {job.Data.Id}");
                        
                        wavPath = await compressor.ProcessToWav(job.PathToProcess, job.Data.Id);
                        var text = await transcriber.GetTranscription(wavPath);
                        
                        Console.WriteLine($"[WORKER] Applying Slowed filter for: {job.Data.Id}");
                        slowedPath = await compressor.FilterToSlowed(job.PathToProcess, job.Data.Id);
                        
                        byte[] slowedBytes = await File.ReadAllBytesAsync(slowedPath);
                        string nameInBucket = $"{job.Data.Id}_slowed.aac";
                        await storage.Upload(slowedBytes, nameInBucket);
                        
                        // 4. Atualiza os dados do objeto
                        job.Data.Transcription = text;
                        // job.Data.SlowedFileUrl = storage.GetPublicUrl(nameInBucket);
                        
                        // 5. Salva no Banco de Dados
                        await repository.Update(job.Data);
                        
                        Console.WriteLine($"[WORKER] Task {job.Data.Id} completed with Slowed version.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WORKER INNER ERROR] Task {job.Data.Id} failed: {ex.Message}");
                    }
                    finally 
                    {
                        // Limpeza rigorosa de temporários
                        if (wavPath != null && File.Exists(wavPath)) File.Delete(wavPath);
                        if (slowedPath != null && File.Exists(slowedPath)) File.Delete(slowedPath);
                        if (File.Exists(job.PathToProcess)) File.Delete(job.PathToProcess);
                    }
                }
                catch (OperationCanceledException) { /* App parando */ }
                catch (Exception ex) { Console.WriteLine($"[WORKER ERROR] {ex.Message}"); }
            }
        });
    }
}