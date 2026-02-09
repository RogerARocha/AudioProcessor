namespace AudioProcessor.Services;

public interface IAudioCompressorService
{
    Task<string> CompressToAac(string inputPath, Guid taskId);
    Task<string> ProcessToWav(string inputPath, Guid taskId);
}