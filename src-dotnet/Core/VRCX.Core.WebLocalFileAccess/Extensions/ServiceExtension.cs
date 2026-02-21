using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.WebLocalFileAccess.Services;

namespace VRCX.Core.WebLocalFileAccess.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddWebLocalFileAccess(this IServiceCollection services)
    {
        services.AddSingleton<WebLocalFileAccessService>();

        return services;
    }
}