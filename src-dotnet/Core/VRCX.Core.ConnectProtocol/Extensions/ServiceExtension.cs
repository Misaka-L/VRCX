using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.ConnectProtocol.Services;

namespace VRCX.Core.ConnectProtocol.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddConnectProtocol(this IServiceCollection services)
    {
        services.AddSingleton<ConnectProtocolHttpService>();
        services.AddSingleton<RestEndpointService>();
        services.AddSingleton<ConnectEventSourceService>();

        return services;
    }
}