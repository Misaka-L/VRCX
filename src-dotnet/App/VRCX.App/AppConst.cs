namespace VRCX.App;

public static class AppConst
{
    public const string AppScheme = "vrcx-app";
    public const string AppFileHost = "app";
    public const string LocalFileAccessHost = "local-files";

    public const string LocalDevelopmentServerUri = "http://localhost:9000";

    // https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/winrt/microsoft_web_webview2_core/corewebview2customschemeregistration?view=webview2-winrt-1.0.3595.46#hasauthoritycomponent
    // "Note that the port and user information are never included in the computation of origins for custom schemes."
    public const string LocalDevelopmentServerOriginWithoutPort = "http://localhost";
}