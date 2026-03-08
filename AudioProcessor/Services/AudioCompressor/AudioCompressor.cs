using System.Diagnostics;
using Xabe.FFmpeg;

namespace AudioProcessor.Services;


public class AudioCompressorService : IAudioCompressorService
{
    private readonly string _tempPath = Path.GetTempPath();

    public async Task<string> CompressToAac(string inputPath, Guid taskId)
    {
        var outputPath = Path.Combine(_tempPath, $"{taskId}.aac");
        
        var stopwatch = Stopwatch.StartNew();//tempo 

        try
        {
            var conversion = await FFmpeg.Conversions.New()
                .AddParameter($"-i \"{inputPath}\"")
                .AddParameter("-c:a aac -b:a 128k")
                .SetOutput(outputPath)
                .Start();
            
            stopwatch.Stop();
            Console.WriteLine($"[COMPRESSION]Conversion Complete to {taskId}");
            Console.WriteLine($"[COMPRESSION]Time Elapsed: {stopwatch.ElapsedMilliseconds} ms");
            return outputPath;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"[COMPRESSION]Error on Compression for {taskId} after {stopwatch.ElapsedMilliseconds}ms : {ex.Message}");
            throw;
        }
    }
    public async Task<string> ProcessToWav(string inputPath, Guid taskId)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"{taskId}_transc.wav");
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"[COMPRESSION] Starting WAV conversion for Whisper: {taskId}");

        try
        {
            // Whisper requirements: 16kHz, Mono, PCM 16-bit
            await FFmpeg.Conversions.New()
                .AddParameter($"-i \"{inputPath}\"")
                .AddParameter("-ar 16000")
                .AddParameter("-ac 1")
                .AddParameter("-c:a pcm_s16le")
                .SetOutput(outputPath)
                .SetOverwriteOutput(true)//error
                .Start();

            stopwatch.Stop();
            Console.WriteLine($"[COMPRESSION] WAV Conversion Complete: {taskId}");
            Console.WriteLine($"[COMPRESSION] Time Elapsed: {stopwatch.ElapsedMilliseconds}ms");

            return outputPath;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"[COMPRESSION] Error on WAV Conversion for {taskId} after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            throw;
        }
    }

    public async Task<string> FilterToSlowed(string inputPath, Guid taskId)
    {
        var outputPath = Path.Combine(_tempPath, $"{taskId}_slowed.aac");
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"[FILTER] Starting Slowed conversion: {taskId}");

        try
        {
            await FFmpeg.Conversions.New()
                .AddParameter($"-i \"{inputPath}\"")
                .AddParameter("-filter:a \"atempo=0.8\"") // O filtro de velocidade
                .AddParameter("-c:a aac -b:a 128k")       // Formato de saída para o bucket
                .SetOutput(outputPath)
                .SetOverwriteOutput(true)
                .Start();

            stopwatch.Stop();
            Console.WriteLine($"[FILTER] Slowed conversion complete: {taskId}");
            Console.WriteLine($"[FILTER] Time Elapsed: {stopwatch.ElapsedMilliseconds}ms");

            return outputPath;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"[FILTER] Error on Slowed filter for {taskId}: {ex.Message}");
            throw;
        }
    }
}