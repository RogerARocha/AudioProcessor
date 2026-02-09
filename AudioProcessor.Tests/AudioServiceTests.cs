using Moq;
using AudioProcessor.Services;
using AudioProcessor.Repositories;
using AudioProcessor.Models;
using Microsoft.AspNetCore.Http;

namespace AudioProcessor.Tests;

public class AudioServiceTests
{
    private readonly Mock<IAudioRepository> _repositoryMock;
    private readonly Mock<IAudioCompressorService> _compressorMock;
    private readonly Mock<IAudioTranscriptionService> _transcriptionMock;
    private readonly AudioService _sut; // System Under Test

    public AudioServiceTests()
    {
        _repositoryMock = new Mock<IAudioRepository>();
        _compressorMock = new Mock<IAudioCompressorService>();
        _transcriptionMock = new Mock<IAudioTranscriptionService>();
        
        // Mockamos as dependências para isolar a lógica do Service
        _sut = new AudioService(
            _repositoryMock.Object, 
            null!, // O Client do Supabase não é chamado nos métodos testados aqui
            _compressorMock.Object, 
            _transcriptionMock.Object
        );
    }

    [Fact]
    public async Task Upload_ShouldThrowException_WhenFormatIsNotSupported()
    {
        // 1. Arrange: Criamos um arquivo com extensão não permitida (.txt)
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("documento.txt"); 

        // 2. Act & 3. Assert: Verificamos se ele lança a exceção de suporte
        await Assert.ThrowsAsync<NotSupportedException>(() => _sut.Upload(fileMock.Object));
    }

    [Fact]
    public async Task Upload_ShouldGenerateValidUuid_AndCallSaveRepository()
    {
        // Arrange
        var repoMock = new Mock<IAudioRepository>();
        var storageMock = new Mock<IStorageService>(); // Mock simples!
        var compressorMock = new Mock<IAudioCompressorService>();
        var transMock = new Mock<IAudioTranscriptionService>();

        var sut = new AudioService(repoMock.Object, storageMock.Object, compressorMock.Object, transMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("audio.aac");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[10]));

        // Act
        var result = await sut.Upload(fileMock.Object);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        repoMock.Verify(r => r.Save(It.IsAny<AudioData>()), Times.Once);
        storageMock.Verify(s => s.Upload(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Once);
    }
}