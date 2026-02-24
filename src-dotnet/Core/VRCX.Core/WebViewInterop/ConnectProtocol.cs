using VRCX.Core.ConnectProtocol.Services;

namespace VRCX.Core.WebViewInterop;

public sealed class ConnectProtocol(ConnectEventSourceService eventSourceService)
{
    public async Task SendEventAsync(string eventType, string data)
    {
        await eventSourceService.SendEventAsync(eventType, data);
    }
}