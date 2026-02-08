namespace AudioProcessor.Services;

public interface IAudioTranscriptionService
{
    Task<string> GetTranscription(string audioPath);
}   