namespace VRCX.App.WebView.VirtualHost;

public class VirtualHostResponse
{
    public int StatusCode { get; set; }
    public required string StatusText { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];

    public Stream ContentStream { get; set; } = Stream.Null;
}