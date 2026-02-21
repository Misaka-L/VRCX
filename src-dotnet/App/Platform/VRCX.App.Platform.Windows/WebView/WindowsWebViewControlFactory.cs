using DirectN;
using DirectN.Extensions.Com;
using VRCX.App.WebView;
using VRCX.App.WebView.VirtualHost;
using VRCX.Core.Shared;
using WebView2;
using WebView2.Utilities;

namespace VRCX.App.Platform.Windows.WebView;

public class WindowsWebViewControlFactory : IPlatformWebViewControlFactory
{
    private ComObject<ICoreWebView2Environment15>? _webView2Environment;
    private OnVirtualHostRequest? _virtualHostRequest;

    public async ValueTask InitializeAsync(OnVirtualHostRequest onVirtualHostRequest)
    {
        _virtualHostRequest = onVirtualHostRequest;

        var profilePath = Path.Combine(AppPathService.AppDataDirectory, "webview-profile", "webview2");

        var tcs = new TaskCompletionSource<ICoreWebView2Environment>();

        var customScheme = new CoreWebView2CustomSchemeRegistration(AppConst.AppScheme);
        customScheme.put_TreatAsSecure(BOOL.TRUE).ThrowOnError();
        customScheme.put_HasAuthorityComponent(BOOL.TRUE).ThrowOnError();

        List<string> allowOrigins =
        [
            $"{AppConst.AppScheme}://{AppConst.AppFileHost}",
            $"{AppConst.AppScheme}://{AppConst.LocalFileAccessHost}"
        ];

        if (AppDebugService.InDebugMode)
        {
            // https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/winrt/microsoft_web_webview2_core/corewebview2customschemeregistration?view=webview2-winrt-1.0.3595.46#hasauthoritycomponent
            // "Note that the port and user information are never included in the computation of origins for custom schemes."
            allowOrigins.Add(AppConst.LocalDevelopmentServerOriginWithoutPort);
        }

        customScheme.SetAllowedOrigins(allowOrigins);

        var options = new CoreWebView2EnvironmentOptions();
        options.SetCustomSchemeRegistrations([customScheme]);

        WebView2.Functions.CreateCoreWebView2EnvironmentWithOptions(
            PWSTR.Null,
            PWSTR.From(profilePath),
            options,
            new CoreWebView2CreateCoreWebView2EnvironmentCompletedHandler((
                result, env) =>
            {
                if (result.GetException() is { } ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                tcs.SetResult(env);
            }));

        _webView2Environment = new ComObject<ICoreWebView2Environment15>(await tcs.Task);
    }

    public ValueTask<PlatformWebViewControl> CreateWebViewControlAsync()
    {
        if (_webView2Environment is null || _virtualHostRequest is null)
            throw new InvalidOperationException("WebView2 environment is not initialized.");

        return ValueTask.FromResult<PlatformWebViewControl>(
            new WindowsWebViewControl(_webView2Environment, _virtualHostRequest)
        );
    }

    public void Dispose()
    {
        _webView2Environment?.Dispose();
    }
}