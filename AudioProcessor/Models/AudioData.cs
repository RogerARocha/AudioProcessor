using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;

namespace AudioProcessor.Models;
using Supabase.Postgrest.Models;

[Table("audios_data")]
public class AudioData : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("file_name")]
    public string FileName { get; set; } = string.Empty;
    
    [Column("file_type")]
    public string FileType { get; set; } = string.Empty;
    
    [Column("file_url")]
    public string FileUrl { get; set; } =  string.Empty;
    
    [Column("transcription_preview")]
    public string Transcription { get; set; } = string.Empty;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } =  DateTime.Now;
}
