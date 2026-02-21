namespace VRCX.App.WebView.VirtualHost;

public class VirtualHostRequest
{
    public required Uri Uri { get; init; }
    public required string Method { get; init; }
    public required IReadOnlyDictionary<string, string> Headers { get; init; }
}