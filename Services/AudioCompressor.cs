using System.Diagnostics;
using Xabe.FFmpeg;

namespace AudioProcessor.Services;

public interface IAudioCompressorService
{
    Task<string> CompressToAac(string inputPath, Guid taskId);
}

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
            Console.WriteLine($"Conversion Complete to {taskId}");
            Console.WriteLine($"Time Elapsed: {stopwatch.ElapsedMilliseconds} ms");
            return outputPath;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"Error on Compression for {taskId} after {stopwatch.ElapsedMilliseconds}ms : {ex.Message}");
            throw;
        }
        
    }
}