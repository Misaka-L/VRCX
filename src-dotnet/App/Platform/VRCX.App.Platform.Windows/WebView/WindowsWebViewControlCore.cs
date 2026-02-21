using System.Diagnostics;
using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using DirectN;
using DirectN.Extensions.Com;
using DirectN.Extensions.Utilities;
using Serilog;
using VRCX.App.WebView;
using VRCX.App.WebView.VirtualHost;
using WebView2;
using WebView2.Utilities;

namespace VRCX.App.Platform.Windows.WebView;

internal sealed class WindowsWebViewControlCore(
    ComObject<ICoreWebView2Environment15> webView2Environment,
    OnVirtualHostRequest onVirtualHostRequest
) : NativeControlHost
{
    private readonly ILogger _logger = Log.ForContext<WindowsWebViewControlCore>();

    public EventHandler<PlatformWebViewMessageEventArgs>? OnMessageReceived { get; set; }
    public EventHandler<EventArgs>? NavigationCompleted { get; set; }

    private readonly TaskCompletionSource<IntPtr> _handlerTcs = new();

    private IComObject<ICoreWebView2Controller4>? _controller;
    private IComObject<ICoreWebView2_28>? _coreWebView2;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var childHandler = base.CreateNativeControlCore(parent);

        _handlerTcs.SetResult(childHandler.Handle);
        return new PlatformHandle(childHandler.Handle, "HWND");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (IsVisible)
        {
            _controller?.Object.put_IsVisible(BOOL.TRUE);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _controller?.Object.put_IsVisible(BOOL.FALSE).ThrowOnError();
    }

    internal async Task InitializeAsync()
    {
        var handle = await _handlerTcs.Task;

        var tcs = new TaskCompletionSource<ICoreWebView2Controller>();

        webView2Environment.Object.CreateCoreWebView2Controller(new HWND(handle),
            new CoreWebView2CreateCoreWebView2ControllerCompletedHandler((
                result, controller) =>
            {
                if (result.GetException() is { } ex)
                {
                    _logger.Error(ex, "Failed to create WebView2 controller.");
                    tcs.SetException(ex);
                    return;
                }

                tcs.SetResult(controller);
            })).ThrowOnError();

        var webView2Controller = new ComObject<ICoreWebView2Controller>(await tcs.Task);

        webView2Controller.As<ICoreWebView2Controller4>(throwOnError: true)!.Object.get_CoreWebView2(
            out var coreWebView2Com).ThrowOnError();

        var coreWebView2 = new ComObject<ICoreWebView2_28>(coreWebView2Com);
        coreWebView2.Object.AddWebResourceRequestedFilter(PWSTR.From($"{AppConst.AppScheme}://*"),
            COREWEBVIEW2_WEB_RESOURCE_CONTEXT.COREWEBVIEW2_WEB_RESOURCE_CONTEXT_ALL).ThrowOnError();

        var token = new EventRegistrationToken();
        coreWebView2.Object.add_WebResourceRequested(new CoreWebView2WebResourceRequestedEventHandler((_, args) =>
        {
            Uri? uri = null;
            try
            {
                args.get_Request(out var request).ThrowOnError();

                request.get_Uri(out var uriPtr).ThrowOnError();
                if (uriPtr.ToStringAndDispose() is not { } uriString)
                {
                    Debug.Fail("Failed to get URI from request.");
                    throw new Exception("ICoreWebView2WebResourceRequest.get_Uri returned null string");
                }

                uri = new Uri(uriString);

                request.get_Method(out var methodPtr).ThrowOnError();
                if (methodPtr.ToStringAndDispose() is not { } method)
                {
                    Debug.Fail("Failed to get method from request.");
                    throw new Exception("ICoreWebView2WebResourceRequest.get_Method returned null string");
                }

                request.get_Headers(out var headers).ThrowOnError();
                headers.GetIterator(out var headersIterator).ThrowOnError();

                var hasNextHeader = BOOL.Null;

                var headersDictionary = new Dictionary<string, string>();
                headersIterator.MoveNext(ref hasNextHeader).ThrowOnError();
                while (hasNextHeader == BOOL.TRUE)
                {
                    if (headersIterator.GetCurrentHeader(out var namePtr, out var valuePtr).GetException() is { } ex)
                    {
                        Debug.Fail("Failed to get header from request.", ex.ToString());
                        _logger.Error(ex, "Failed to get header from request.");
                        break;
                    }

                    var name = namePtr.ToStringAndDispose() ?? throw new Exception(
                        "ICoreWebView2HttpRequestHeadersIterator.GetCurrentHeader returned null string for header name");
                    var value = valuePtr.ToStringAndDispose() ?? throw new Exception(
                        "ICoreWebView2HttpRequestHeadersIterator.GetCurrentHeader returned null string for header value");

                    headersDictionary.Add(name, value);
                    headersIterator.MoveNext(ref hasNextHeader).ThrowOnError();
                }

                var virtualRequest = new VirtualHostRequest
                {
                    Uri = uri,
                    Method = method,
                    Headers = headersDictionary.AsReadOnly()
                };

                var virtualResponse = onVirtualHostRequest(virtualRequest);

                webView2Environment.Object.CreateWebResourceResponse(
                    new ManagedIStream(new BurnAfterReadStream(virtualResponse.ContentStream)),
                    virtualResponse.StatusCode
                    , PWSTR.From(virtualResponse.StatusText),
                    PWSTR.From(
                        virtualResponse.Headers
                            .Select(header => $"{header.Key}: {header.Value}")
                            .Aggregate((a, b) => $"{a}\r\n{b}")
                    ),
                    out var response
                ).ThrowOnError();

                args.put_Response(response);
            }
            catch (Exception ex)
            {
                if (uri is null)
                {
                    _logger.Error(ex, "Error handling WebResourceRequested event.");
                }
                else
                {
                    _logger.Error(ex, "Error handling WebResourceRequested event for URI: {Uri}", uri);
                }
            }
        }), ref token);

        coreWebView2.Object.add_WebMessageReceived(new CoreWebView2WebMessageReceivedEventHandler((_, args) =>
        {
            var ex = args.TryGetWebMessageAsString(out var pwstr).GetException();
            if (ex is not null)
            {
                Debug.Fail("Failed to get message string from WebMessageReceived event.", ex.ToString());
                _logger.Error(ex, "Failed to get message string from WebMessageReceived event.");
                return;
            }

            if (pwstr.ToStringAndDispose() is not { } message)
            {
                Debug.Fail("Failed to get message string from WebMessageReceived event.");
                _logger.Error("Failed to get message string from WebMessageReceived event.");
                return;
            }

            OnMessageReceived?.Invoke(this, new PlatformWebViewMessageEventArgs(message));
        }), ref token);

        _controller = webView2Controller.As<ICoreWebView2Controller4>(throwOnError: true);
        _coreWebView2 = coreWebView2;
    }

    internal void Navigate(string url)
    {
        _coreWebView2?.Object.Navigate(PWSTR.From(url)).ThrowOnError();
    }

    internal void ExecuteScript(string script)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_controller is null)
                return;

            try
            {
                var tcs = new TaskCompletionSource();

                _coreWebView2?.Object.ExecuteScript(PWSTR.From(script),
                    new CoreWebView2ExecuteScriptCompletedHandler((result, _) =>
                    {
                        if (result.GetException() is { } ex)
                        {
                            tcs.SetException(ex);
                            return;
                        }

                        tcs.SetResult();
                    })).ThrowOnError();

                await tcs.Task;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute script: {Script}", script);
            }
        });
    }

    internal void OpenDevTools()
    {
        _coreWebView2?.Object.OpenDevToolsWindow().ThrowOnError();
    }

    internal double GetZoomLevel()
    {
        var zoomFactor = 100d;
        _controller?.Object.get_ZoomFactor(ref zoomFactor);
        return zoomFactor;
    }

    internal void SetZoomLevel(double zoomLevel)
    {
        _controller?.Object.put_ZoomFactor(zoomLevel).ThrowOnError();
    }

    public void SetDarkMode(bool isDarkMode)
    {
        if (_coreWebView2 is null)
            return;

        _coreWebView2.Object.get_Profile(out var profile).ThrowOnError();
        var preferredColorScheme = isDarkMode
            ? COREWEBVIEW2_PREFERRED_COLOR_SCHEME.COREWEBVIEW2_PREFERRED_COLOR_SCHEME_DARK
            : COREWEBVIEW2_PREFERRED_COLOR_SCHEME.COREWEBVIEW2_PREFERRED_COLOR_SCHEME_LIGHT;

        profile.get_PreferredColorScheme(ref preferredColorScheme);
    }

    public void SetUserAgent(string userAgent)
    {
        if (_coreWebView2 is null)
            return;

        _coreWebView2.Object.get_Settings(out var settings).ThrowOnError();

        using var com = new ComObject<ICoreWebView2Settings9>(settings);
        com.Object.put_UserAgent(PWSTR.From(userAgent));
    }

    internal void PostMessage(string message)
    {
        _coreWebView2?.Object.PostWebMessageAsString(PWSTR.From(message)).ThrowOnError();
    }

    internal void OnBoundsChanged(Rectangle rectangle)
    {
        _controller?.Object.put_Bounds(new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom))
            .ThrowOnError();
    }

    internal void Close()
    {
        _controller?.Object.Close().ThrowOnError();
        _coreWebView2?.Dispose();
        _controller?.Dispose();
    }
}