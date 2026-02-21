using System.Net.Mime;
using MimeTypeCore;
using Serilog;
using VRCX.App.WebView;
using VRCX.App.WebView.VirtualHost;
using VRCX.Core.Services.Platform;
using VRCX.Core.WebLocalFileAccess.Services;

namespace VRCX.App.Services;

public sealed class MainWebViewService(
    IPlatformWebViewControlFactory webViewControlFactory,
    WebLocalFileAccessService webLocalFileAccessService
) : IMainWebViewService, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<MainWebViewService>();
    private PlatformWebViewControl? _webViewControl;

    internal async ValueTask<PlatformWebViewControl> GetOrCreateWebViewControlAsync()
    {
        if (_webViewControl is not null)
            return _webViewControl;

        await webViewControlFactory.InitializeAsync(OnVirtualHostRequest);
        _webViewControl = await webViewControlFactory.CreateWebViewControlAsync();

        return _webViewControl;
    }

    internal void SetWebViewControl(PlatformWebViewControl webViewControl)
    {
        _webViewControl = webViewControl;
    }

    public Task ExecuteScriptAsync(string methodName)
    {
        _webViewControl?.ExecuteScript(methodName);
        return Task.CompletedTask;
    }

    public void ShowDevTools()
    {
        _webViewControl?.OpenDevTools();
    }

    public async ValueTask<double> GetZoomLevelAsync()
    {
        if (_webViewControl is null)
            return 100;

        return await _webViewControl.GetZoomLevelAsync();
    }

    public Task SetZoomLevelAsync(double zoomLevel)
    {
        if (_webViewControl is null)
            return Task.CompletedTask;

        return _webViewControl.SetZoomLevelAsync(zoomLevel);
    }

    public async ValueTask SetUserAgentAsync(string userAgent)
    {
        if (_webViewControl is null)
            return;

        await _webViewControl.SetUserAgentAsync(userAgent);
    }

    public async ValueTask SetDarkModeAsync(bool isDarkMode)
    {
        if (_webViewControl is null)
            return;

        await _webViewControl.SetDarkModeAsync(isDarkMode);
    }

    public void Dispose()
    {
        if (_webViewControl is not null)
            _webViewControl.Close();

        webViewControlFactory.Dispose();
    }

    #region Request Handler

    private VirtualHostResponse OnVirtualHostRequest(VirtualHostRequest request)
    {
        switch (request.Uri.Host)
        {
            case AppConst.AppFileHost:
                return HandleAppFileHostRequest(request);
            case AppConst.LocalFileAccessHost:
                return HandleLocalFileAccessRequest(request);
        }

        _logger.Warning("Received request for unknown host: {RequestUri}", request.Uri);
        return new VirtualHostResponse
        {
            StatusText = "Not Found",
            StatusCode = 404,
        };
    }

    private VirtualHostResponse HandleAppFileHostRequest(VirtualHostRequest request)
    {
        if (request.Method != "GET")
        {
            _logger.Warning(
                "Received app files request with unsupported method: {Method} {RequestUri}",
                request.Method,
                request.Uri
            );

            return new VirtualHostResponse
            {
                StatusText = "Method Not Allowed",
                StatusCode = 405,
            };
        }

        var assetsFilePath = Path.Join(AppContext.BaseDirectory, "html", request.Uri.LocalPath);
        if (!File.Exists(assetsFilePath))
        {
            _logger.Warning("Requested app file not found: {RequestUri} (resolved path: {AssetsFilePath})",
                request.Uri,
                assetsFilePath
            );

            return new VirtualHostResponse
            {
                StatusText = "Not Found",
                StatusCode = 404,
            };
        }

        try
        {
            _logger.Verbose("Serving app file: {RequestUri} (resolved path: {AssetsFilePath})",
                request.Uri,
                assetsFilePath
            );

            var fileStream = File.OpenRead(assetsFilePath);
            var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(assetsFilePath));

            var response = new VirtualHostResponse
            {
                StatusText = "OK",
                StatusCode = 200,
                ContentStream = fileStream,
            };

            response.Headers.Add("Content-Type", mimeType ?? MediaTypeNames.Application.Octet);

            return response;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error serving app file: {RequestUri} (resolved path: {AssetsFilePath})",
                request.Uri,
                assetsFilePath
            );

            return new VirtualHostResponse
            {
                StatusText = "Internal Server Error",
                StatusCode = 500,
            };
        }
    }

    private VirtualHostResponse HandleLocalFileAccessRequest(VirtualHostRequest request)
    {
        if (request.Method != "GET")
        {
            _logger.Warning("Received local file access request with unsupported method: {Method} {RequestUri}",
                request.Method,
                request.Uri
            );

            return new VirtualHostResponse
            {
                StatusText = "Method Not Allowed",
                StatusCode = 405,
            };
        }

        var filePath = webLocalFileAccessService.GetDecryptedFilePath(request.Uri);
        if (filePath is null)
        {
            _logger.Warning("Received local file access request with invalid URI: {RequestUri}", request.Uri);
            return new VirtualHostResponse
            {
                StatusText = "Not Found",
                StatusCode = 404
            };
        }

        if (!File.Exists(filePath))
        {
            _logger.Warning("Requested local file not found: {RequestUri} (resolved path: {FilePath})",
                request.Uri,
                filePath
            );

            return new VirtualHostResponse
            {
                StatusText = "Not Found",
                StatusCode = 404
            };
        }

        try
        {
            _logger.Verbose("Serving local file: {RequestUri} (resolved path: {FilePath})",
                request.Uri,
                filePath
            );

            var fileStream = File.OpenRead(filePath);
            var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(filePath));

            var response = new VirtualHostResponse
            {
                StatusText = "OK",
                StatusCode = 200,
                ContentStream = fileStream,
            };

            response.Headers.Add("Content-Type", mimeType ?? MediaTypeNames.Application.Octet);

            return response;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error serving local file: {RequestUri} (resolved path: {FilePath})",
                request.Uri,
                filePath
            );

            return new VirtualHostResponse
            {
                StatusText = "Not Found",
                StatusCode = 404,
            };
        }
    }

    #endregion
}