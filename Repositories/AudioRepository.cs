using AudioProcessor.Models;
using Supabase;

namespace AudioProcessor.Repositories;

public interface IAudioRepository
{
    Task Save(AudioData data);
    Task Update(AudioData data);
}
public class AudioRepository :  IAudioRepository
{
    private readonly Client _supabase;
    public AudioRepository(Client supabase) => _supabase = supabase;

    public async Task Save(AudioData data)
    {
        await _supabase.From<AudioData>().Insert(data);
    }
    
    public async Task Update(AudioData data)
    {
        await _supabase
            .From<AudioData>()
            .Where(x => x.Id == data.Id)
            .Update(data);
    }

}