namespace AudioProcessor.DTOs;

public class AudioResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string TranscriptionPreview { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}