using System.Threading.Channels;
using AudioProcessor.Models;

namespace AudioProcessor.Services;

// A Interface para podermos injetar no AudioService
public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(AudioJob job);
    ValueTask<AudioJob> DequeueAsync(CancellationToken cancellationToken);
}

public record AudioJob(AudioData Data, string PathToProcess);

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<AudioJob> _queue;

    public BackgroundTaskQueue()
    {
        // lmite de 100 para nao estourar ram
        var options = new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };
        _queue = Channel.CreateBounded<AudioJob>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(AudioJob job) 
        => await _queue.Writer.WriteAsync(job);

    public async ValueTask<AudioJob> DequeueAsync(CancellationToken ct) 
        => await _queue.Reader.ReadAsync(ct);
}