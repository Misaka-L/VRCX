using System.Net;
using Serilog;
using VRCX.Core.ConnectProtocol.Extensions;
using VRCX.Core.ConnectProtocol.Models.Rest;

namespace VRCX.Core.ConnectProtocol.Services;

public sealed class RestEndpointService(IServiceProvider serviceProvider)
{
    private readonly ILogger _logger = Log.ForContext<RestEndpointService>();

    private readonly Dictionary<EndpointInfo, Func<HttpListenerContext, IServiceProvider, Task>> _handlers = [];

    public void Map(string method, string path, Func<HttpListenerContext, IServiceProvider, Task> handler)
    {
        var key = new EndpointInfo(path, method.ToUpperInvariant());
        _handlers[key] = handler;
    }

    public async Task HandleAsync(HttpListenerContext context)
    {
        if (context.Request.Url is null)
        {
            _logger.Error("Invalid request URL provided {RawUrl}", context.Request.RawUrl);
            await context.Response.WriteProblemAsync(
                ProblemDetailsType.Undocumented,
                (int)HttpStatusCode.BadRequest,
                "Bad Request",
                "The request URL is invalid.");

            return;
        }

        var requestPath = context.Request.Url.AbsolutePath;
        var requestMethod = context.Request.HttpMethod.ToUpperInvariant();

        var key = new EndpointInfo(requestPath, requestMethod);
        if (_handlers.TryGetValue(key, out var handler))
        {
            try
            {
                await handler(context, serviceProvider);
                return;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error handling request {Method} {Path}", requestMethod, requestPath);
                await context.Response.WriteProblemAsync(
                    ProblemDetailsType.Undocumented,
                    (int)HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred."
                );

                return;
            }
        }

        if (_handlers.Any(pair => pair.Key.Path == requestPath))
        {
            await context.Response.WriteProblemAsync(
                ProblemDetailsType.Undocumented,
                (int)HttpStatusCode.MethodNotAllowed,
                "Method Not Allowed",
                "The method is not allowed for the requested Endpoint."
            );

            return;
        }

        await context.Response.WriteProblemAsync(
            ProblemDetailsType.Undocumented,
            (int)HttpStatusCode.NotFound,
            "Not Found",
            "The requested Endpoint was not found on the server."
        );
    }

    private record EndpointInfo(string Path, string Method);
}