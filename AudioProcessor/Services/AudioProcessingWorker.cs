using AudioProcessor.Repositories;
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

        //4 "trabalhadores" internos que ficam lendo a fila
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

                    string? wavPath = null;
                    try 
                    {
                        Console.WriteLine($"[WORKER] Processing parallel task: {job.Data.Id}");
                        wavPath = await compressor.ProcessToWav(job.PathToProcess, job.Data.Id);
                        var text = await transcriber.GetTranscription(wavPath);
                        
                        job.Data.TranscriptionPreview = text;
                        await repository.Update(job.Data);
                        Console.WriteLine($"[WORKER] Task {job.Data.Id} completed.");
                    }
                    finally 
                    {
                        if (File.Exists(wavPath)) File.Delete(wavPath);
                        if (File.Exists(job.PathToProcess)) File.Delete(job.PathToProcess);
                    }
                }
                catch (OperationCanceledException) { /* App parando */ }
                catch (Exception ex) { Console.WriteLine($"[WORKER ERROR] {ex.Message}"); }
            }
        });
    }
}