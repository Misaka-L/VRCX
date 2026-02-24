using System.Net;
using Serilog;
using Serilog.Context;
using VRCX.Core.ConnectProtocol.Extensions;
using VRCX.Core.ConnectProtocol.Models.Rest;

namespace VRCX.Core.ConnectProtocol.Services;

public sealed class ConnectProtocolHttpService
{
    private readonly ILogger _logger = Log.ForContext<ConnectProtocolHttpService>();
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _loopCts = new();

    private readonly RestEndpointService _restEndpointService;

    public ConnectProtocolHttpService(RestEndpointService restEndpointService)
    {
        _restEndpointService = restEndpointService;
        _restEndpointService.MapRestEndpoint();

        _listener.Prefixes.Add("http://localhost:34583/");
    }

    public Task StartAsync()
    {
        _listener.Start();

        _ = Task.Factory.StartNew(() => HttpListenerLoopAsync(_loopCts.Token), TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    private async Task HttpListenerLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var requestContext = await _listener.GetContextAsync();
                _ = HandleHttpRequestAsync(requestContext);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in HTTP listener loop");
                break;
            }
        }
    }

    private async Task HandleHttpRequestAsync(HttpListenerContext requestContext)
    {
        var requestId = requestContext.Request.RequestTraceIdentifier;

        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("RpcClientIp", requestContext.Request.RemoteEndPoint?.Address))
        using (LogContext.PushProperty("RpcClientPort", requestContext.Request.RemoteEndPoint?.Port))
        using (LogContext.PushProperty("RpcHttpMethod", requestContext.Request.HttpMethod))
        using (LogContext.PushProperty("RpcHttpRawUrl", requestContext.Request.RawUrl))
        {
            try
            {
                _logger.Information(
                    "{RequestId} {RpcClientIp}:{RpcClientPort} {RpcHttpMethod} {RpcHttpRawUrl}",
                    requestId,
                    requestContext.Request.RemoteEndPoint?.Address,
                    requestContext.Request.RemoteEndPoint?.Port,
                    requestContext.Request.HttpMethod,
                    requestContext.Request.RawUrl
                );

                requestContext.Response.AppendHeader("X-Request-Id", requestId.ToString());

                await _restEndpointService.HandleAsync(requestContext);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Error processing HTTP request {RequestId} {RpcClientIp}:{RpcClientPort} {RpcHttpMethod} {RpcHttpRawUrl}",
                    requestId,
                    requestContext.Request.RemoteEndPoint?.Address,
                    requestContext.Request.RemoteEndPoint?.Port,
                    requestContext.Request.HttpMethod,
                    requestContext.Request.RawUrl
                );

                await requestContext.Response.WriteProblemAsync(
                    ProblemDetailsType.Undocumented,
                    (int)HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred."
                );
            }
            finally
            {
                requestContext.Response.Close();
            }
        }
    }

    public async Task StopAsync()
    {
        await _loopCts.CancelAsync();
        _listener.Stop();
    }
}