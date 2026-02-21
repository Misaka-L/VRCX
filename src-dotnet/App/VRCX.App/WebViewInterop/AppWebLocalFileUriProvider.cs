using VRCX.Core.WebLocalFileAccess.Services.Abstraction;

namespace VRCX.App.WebViewInterop;

public sealed class AppWebLocalFileUriProvider : IWebLocalFileUriProvider
{
    public bool SupportEncryption => true;

    public Uri GetFileUri(string fileKey)
    {
        var builder = new UriBuilder
        {
            Scheme = AppConst.AppScheme,
            Host = AppConst.LocalFileAccessHost,
            Path = fileKey
        };

        return builder.Uri;
    }

    public string ParseFileKey(Uri uri)
    {
        if (uri.Scheme != AppConst.AppScheme)
            throw new FormatException($"Invalid URI scheme, expected '{AppConst.AppScheme}'");

        if (uri.Host != AppConst.LocalFileAccessHost)
            throw new FormatException($"Invalid URI host, expected '{AppConst.LocalFileAccessHost}'");

        return uri.AbsolutePath.TrimStart('/');
    }
}