using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VRCX.Core.ConnectProtocol.Services;

namespace VRCX.Core.ConnectProtocol.Endpoints;

public static class EventSourceEndpoint
{
    private static readonly ILogger Logger = Log.ForContext(typeof(EventSourceEndpoint));

    public static void MapEventSourceEndpoint(this RestEndpointService restEndpointService)
    {
        restEndpointService.Map("GET", "/connect-v0/event-source", HandleEventSourceEndpointAsync);
    }

    private static async Task HandleEventSourceEndpointAsync(HttpListenerContext context, IServiceProvider services)
    {
        var eventSourceService = services.GetRequiredService<ConnectEventSourceService>();

        context.Response.AppendHeader("Content-Type", "text/event-stream");

        var id = 1;
        var cts = new CancellationTokenSource();
        try
        {
            await WriteEventAsync(context.Response, "hello", "{}", id, cts.Token);
            id++;

            await foreach (var payload in eventSourceService.GetEventPayloadAsync(cts.Token))
            {
                await WriteEventAsync(context.Response, payload.EventType, payload.Data, id, cts.Token);
                id++;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Error in EventSourceEndpoint loop");
        }
        finally
        {
            await cts.CancelAsync();
            context.Response.Close();
        }
    }

    private static async Task WriteEventAsync(
        HttpListenerResponse response,
        string eventType,
        string data,
        int id,
        CancellationToken cancellationToken
    )
    {
        var eventPayload = $"event: {eventType}\ndata: {data}\nid: {id}\n\n";
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(eventPayload);

        await response.OutputStream.WriteAsync(payloadBytes, 0, payloadBytes.Length, cancellationToken);
        await response.OutputStream.FlushAsync(cancellationToken);
    }
}