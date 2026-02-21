using VRCX.App.WebView.VirtualHost;

namespace VRCX.App.WebView;

public interface IPlatformWebViewControlFactory : IDisposable
{
    ValueTask InitializeAsync(OnVirtualHostRequest onVirtualHostRequest);
    ValueTask<PlatformWebViewControl> CreateWebViewControlAsync();
}

public delegate VirtualHostResponse OnVirtualHostRequest(VirtualHostRequest request);