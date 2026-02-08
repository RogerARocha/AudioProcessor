using AudioProcessor.DTOs;
using AudioProcessor.Models;
using AudioProcessor.Repositories;
using Microsoft.AspNetCore.Mvc;
using AudioProcessor.Services;

namespace AudioProcessor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{
    private readonly IAudioService _audioService;
    private readonly IAudioRepository _audioRepository;

    public AudioController(IAudioService audioService,  IAudioRepository audioRepository)
    {
        _audioService = audioService;
        _audioRepository = audioRepository;
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
                result.Id,
                result.FileName,
                result.FileUrl,
                result.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }
    
    [HttpGet("audios")]
    public async Task<IActionResult> Audios()
    {
        try
        {
            var audios = await _audioRepository.GetAll();
            var response = audios.Select(a => new AudioResponseDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                FileUrl = a.FileUrl,
                TranscriptionPreview =  a.TranscriptionPreview,
                CreatedAt = a.CreatedAt
            });
            
            return Ok(response);
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Server error at Get Audios: {ex.Message}");
        }
    }
}