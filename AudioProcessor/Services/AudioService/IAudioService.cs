using AudioProcessor.Models;

namespace AudioProcessor.Services;

public interface IAudioService
{
    Task<AudioData> Upload(IFormFile file);
}