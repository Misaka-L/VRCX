using System.Net;
using System.Text.Json;
using VRCX.Core.ConnectProtocol.Models.Rest;

namespace VRCX.Core.ConnectProtocol.Extensions;

internal static class ResponseExtension
{
    public static async Task WriteProblemAsync(this HttpListenerResponse response,
        string type,
        int statusCode,
        string title,
        string? detail = null
    )
    {
        response.StatusCode = statusCode;
        response.AppendHeader("Content-Type", "application/problem+json");

        await JsonSerializer.SerializeAsync(response.OutputStream, new ProblemDetails
        {
            Type = type,
            Status = statusCode,
            Title = title,
            Detail = detail
        }, RestJsonContext.Default.ProblemDetails);
    }
}