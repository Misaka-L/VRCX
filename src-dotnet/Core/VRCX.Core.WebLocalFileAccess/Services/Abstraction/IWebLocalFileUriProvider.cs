namespace VRCX.Core.WebLocalFileAccess.Services.Abstraction;

public interface IWebLocalFileUriProvider
{
    bool SupportEncryption { get; }
    
    public Uri GetFileUri(string fileKey);
    public string ParseFileKey(Uri uri);
}