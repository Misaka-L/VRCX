using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace VRCX.Core.ConnectProtocol.Services;

public sealed class ConnectEventSourceService
{
    private readonly ConcurrentDictionary<Guid, Channel<EventSourcePayload>> _clientChannels = new();

    public async Task SendEventAsync(string eventType, string data)
    {
        var payload = new EventSourcePayload(eventType, data);

        foreach (var channelKv in _clientChannels)
        {
            await channelKv.Value.Writer.WriteAsync(payload);
        }
    }

    public async IAsyncEnumerable<EventSourcePayload> GetEventPayloadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<EventSourcePayload>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        _clientChannels[id] = channel;

        try
        {
            var reader = channel.Reader;

            while (!cancellationToken.IsCancellationRequested && await reader.WaitToReadAsync(cancellationToken))
            {
                await foreach (var payload in reader.ReadAllAsync(cancellationToken))
                {
                    yield return payload;
                }
            }
        }
        finally
        {
            _clientChannels.TryRemove(id, out _);
        }
    }
}

public record EventSourcePayload(string EventType, string Data);