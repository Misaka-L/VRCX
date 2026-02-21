using System.Collections.Specialized;
using Serilog;
using VRCX.App.WebView;
using VRCX.App.WebView.VirtualHost;
using VRCX.Core.Shared;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue.Common.Shared;

namespace VRCX.App.Platform.CefDesktop.WebView;

public sealed class CefWebViewFactory : IPlatformWebViewControlFactory
{
    public ValueTask InitializeAsync(OnVirtualHostRequest onVirtualHostRequest)
    {
        var profilePath = Path.Combine(AppPathService.AppDataDirectory, "webview-profile", "cefglue");

        CefRuntimeLoader.Initialize(new CefSettings
            {
                RootCachePath = profilePath,
                CachePath = profilePath
            }, customSchemes:
            [
                new CustomScheme
                {
                    SchemeName = AppConst.AppScheme,
                    SchemeHandlerFactory = new VirtualSchemeHandlerFactory(onVirtualHostRequest)
                }
            ]);

        return ValueTask.CompletedTask;
    }

    public ValueTask<PlatformWebViewControl> CreateWebViewControlAsync()
    {
        PlatformWebViewControl control = new CefWebViewControl();
        return ValueTask.FromResult(control);
    }

    public void Dispose()
    {
    }
}

public class VirtualSchemeHandlerFactory(OnVirtualHostRequest onVirtualHostRequest) : CefSchemeHandlerFactory
{
    private readonly ILogger _logger = Log.ForContext<VirtualSchemeHandlerFactory>();

    protected override CefResourceHandler Create(
        CefBrowser browser,
        CefFrame frame,
        string schemeName,
        CefRequest request
    )
    {
        try
        {
            var uri = new Uri(request.Url);

            var headersMap = request.GetHeaderMap();
            var headersDictionary = headersMap.AllKeys.ToDictionary(key => key!, key => headersMap[key]!);

            var virtualRequest = new VirtualHostRequest
            {
                Uri = uri,
                Method = request.Method,
                Headers = headersDictionary.AsReadOnly()
            };

            var virtualResponse = onVirtualHostRequest(virtualRequest);
            var responseHeadersMap = new NameValueCollection();

            foreach (var header in virtualResponse.Headers)
            {
                responseHeadersMap.Add(header.Key, header.Value);
            }

            var handler = new DefaultResourceHandler
            {
                Response = virtualResponse.ContentStream,
                Headers = responseHeadersMap,
                StatusText = virtualResponse.StatusText,
                Status = virtualResponse.StatusCode,
                MimeType = virtualResponse.Headers.GetValueOrDefault("Content-Type", "application/octet-stream")
            };

            return handler;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading resource: {Url}", request.Url);

            return new DefaultResourceHandler
            {
                Status = 500,
                StatusText = "Internal Server Error",
            };
        }
    }
}