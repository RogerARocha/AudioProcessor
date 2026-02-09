namespace AudioProcessor.Services;

using Supabase;

public class StorageService: IStorageService
{
    private readonly Client _supabase;
    private const string BucketName = "Audios";

    public StorageService(Client supabase) => _supabase = supabase;

    public async Task Upload(byte[] data, string fileName) 
        => await _supabase.Storage.From(BucketName).Upload(data, fileName);

    public string GetPublicUrl(string fileName) 
        => _supabase.Storage.From(BucketName).GetPublicUrl(fileName);
    
}