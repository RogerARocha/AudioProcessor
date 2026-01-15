using Microsoft.AspNetCore.Mvc;
using AudioProcessor.Services;

namespace AudioProcessor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{
    private readonly IAudioService _audioService;

    public AudioController(IAudioService audioService)
    {
        _audioService = audioService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Without audio file");

        try 
        {
            var result = await _audioService.Upload(file);
            return Ok(new {
                Id = result.Id,
                FileName = result.FileName,
                FileUrl = result.FileUrl,
                CreatedAt = result.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }
}