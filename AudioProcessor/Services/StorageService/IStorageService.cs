namespace AudioProcessor.Services;

public interface IStorageService
{
    Task Upload(byte[] data, string fileName);
    string GetPublicUrl(string fileName);

}