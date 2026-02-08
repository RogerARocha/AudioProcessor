namespace AudioProcessor.Models;

public enum AudioFormat
{
    Mp3,
    Mpeg,
    Wav,
    Ogg,
    Aac,
    M4a,
    Flac
}

public static class AudioFormatExtensions
{
    public static string GetExtension(this AudioFormat format) =>
        format switch
        {
            AudioFormat.Mp3   => ".mp3",
            AudioFormat.Mpeg  => ".mpeg",
            AudioFormat.Wav   => ".wav",
            AudioFormat.Ogg   => ".ogg",
            AudioFormat.Aac   => ".aac",
            AudioFormat.M4a   => ".m4a",
            AudioFormat.Flac  => ".flac",
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Formato não mapeado.")
        };
}