using AudioProcessor.Models;
using Supabase.Postgrest;
using Client = Supabase.Client;

namespace AudioProcessor.Repositories;

public interface IAudioRepository
{
    Task Save(AudioData data);
    Task Update(AudioData data);
    Task<IEnumerable<AudioData>> GetAll();
}
public class AudioRepository :  IAudioRepository
{
    private readonly Client _supabase;
    public AudioRepository(Client supabase) => _supabase = supabase;

    //CREATE
    public async Task Save(AudioData data)
    {
        await _supabase.From<AudioData>().Insert(data);
    }
    
    //UPDATE
    public async Task Update(AudioData data)
    {
        await _supabase
            .From<AudioData>()
            .Where(x => x.Id == data.Id)
            .Update(data);
    }
    
    //READ
    public async Task<IEnumerable<AudioData>> GetAll()
    {
        var response = await _supabase.From<AudioData>()
            .Order(x => x.CreatedAt, Constants.Ordering.Descending).Get();
        
        return response.Models;
    }
}