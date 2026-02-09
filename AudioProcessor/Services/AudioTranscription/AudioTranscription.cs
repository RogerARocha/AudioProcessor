using System.Diagnostics;
using Whisper.net;
using Whisper.net.Ggml;

namespace AudioProcessor.Services;


public class AudioTranscriptionService : IAudioTranscriptionService
{
    private readonly string _modelPath = "ggml-base-bin.bin";

    public async Task<string> GetTranscription(string audioPath)
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"[TRANSCRIPTION] Initializing local inference...");

        try
        {
            if (!File.Exists(_modelPath))
            {
                Console.WriteLine("[TRANSCRIPTION] Downloading model (first exec)...");
                using var httpClient = new HttpClient(); 
                var downloader = new WhisperGgmlDownloader(httpClient); 
                using var modelStream = await downloader.GetGgmlModelAsync(GgmlType.Base);
                using var fileStream = File.OpenWrite(_modelPath);
                await modelStream.CopyToAsync(fileStream);
            }
            
            using var factory = WhisperFactory.FromPath(_modelPath);

            await using var fileStreamForWhisper = File.OpenRead(audioPath);
            
            await using var processor = factory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            var fullText = "";

            // O processor.ProcessAsync mantém um estado interno que exige que o processor
            // esteja vivo até o fim da iteração do loop.
            await foreach (var result in processor.ProcessAsync(fileStreamForWhisper))
            {
                fullText += result.Text;
                if (fullText.Length >= 60) break; 
            }

            stopwatch.Stop();
            var resultText = fullText.Trim();
            
            Console.WriteLine($"[TRANSCRIPTION] Success in {stopwatch.Elapsed.TotalSeconds:F2}s.");

            return resultText.Length <= 50 ? resultText : resultText.Substring(0, 50) + "...";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            // Log detalhado para identificar se o erro é no arquivo ou no modelo
            Console.WriteLine($"[TRANSCRIPTION ERRO] Critical failure after {stopwatch.Elapsed.TotalSeconds:F2}s: {ex.Message}");
            return "Transcription unavailable";
        }
    }
}