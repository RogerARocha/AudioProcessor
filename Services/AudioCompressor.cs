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
        
        var conversion = await FFmpeg.Conversions.New()
            .AddParameter($"-i \"{inputPath}\"")
            .AddParameter("-c:a aac -b:a 128k")
            .SetOutput(outputPath)
            .Start();

        return outputPath;
    }
}