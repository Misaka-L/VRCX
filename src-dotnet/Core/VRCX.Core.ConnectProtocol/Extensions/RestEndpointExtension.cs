using VRCX.Core.ConnectProtocol.Endpoints;
using VRCX.Core.ConnectProtocol.Services;

namespace VRCX.Core.ConnectProtocol.Extensions;

internal static class RestEndpointExtension
{
    public static RestEndpointService MapRestEndpoint(this RestEndpointService restEndpointService)
    {
        restEndpointService.MapEventSourceEndpoint();

        return restEndpointService;
    }
}