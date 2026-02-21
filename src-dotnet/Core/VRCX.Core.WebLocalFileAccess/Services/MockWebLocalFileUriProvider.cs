using VRCX.Core.WebLocalFileAccess.Services.Abstraction;

namespace VRCX.Core.WebLocalFileAccess.Services;

internal sealed class MockWebLocalFileUriProvider : IWebLocalFileUriProvider
{
    public bool SupportEncryption => false;

    public Uri GetFileUri(string fileKey)
    {
        var builder = new UriBuilder
        {
            Scheme = "file",
            Path = fileKey,
            Host = ""
        };

        return builder.Uri;
    }

    public string ParseFileKey(Uri uri)
    {
        if (uri.Scheme != "file")
            throw new FormatException("Invalid URI scheme. Expected 'file'.");

        return uri.LocalPath;
    }
}