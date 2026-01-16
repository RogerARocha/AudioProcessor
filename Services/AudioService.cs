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
    
    public AudioService(IAudioRepository audioRepository, Client supabaseClient)
    {
        _audioRepository = audioRepository;
        _supabaseClient = supabaseClient;
        
        _allowedFormats = Enum.GetValues<AudioFormat>().Select(x => x.GetExtension()).ToHashSet();
    }

    public async Task<AudioData> Upload(IFormFile file)
    {
        var bucket = "Audios";
        var uuid = Guid.NewGuid();
        var type = Path.GetExtension(file.FileName);
        var nameInBucket = $"{uuid}{type}";

        if (!_allowedFormats.Contains(type))
        {
            throw new NotSupportedException($"File type {type} is not supported.");
        }
        
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();
        
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