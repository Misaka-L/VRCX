using System.ComponentModel;
using System.Runtime.CompilerServices;
using VRCX.App.WebView;

namespace VRCX.App.ViewModels;

public class OverlayDebugWindowViewModel(
    IPlatformWebViewControlFactory webViewControlFactory
    ) : INotifyPropertyChanged
{
    public PlatformWebViewControl? WebViewControl
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadAsync()
    {
        WebViewControl = await webViewControlFactory.CreateWebViewControlAsync();
        await WebViewControl.InitializeAsync();

        WebViewControl.Navigate("http://localhost:9000/vr.html");
        // // Notice: Running WebView initialization outside of UI thread will cause issues.
        // await webViewControlFactory.InitializeAsync();
        // var webview = await webViewControlFactory.CreateWebViewControlAsync();
        // // due to bad design, must mount webview to visual tree before initialization
        // WebViewControl = webview;
        //
        // await webview.InitializeAsync();
        // webview.RegisterAppJavascriptObjects(webViewJsonIpcService);
        // webview.Navigate("http://localhost:9000");
        //
        // mainWebViewService.SetWebViewControl(webview);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}