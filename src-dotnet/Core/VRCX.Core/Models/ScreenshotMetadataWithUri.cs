using VRCX.Core.ScreenshotManagement.Models;

namespace VRCX.Core.Models;

public sealed class ScreenshotMetadataWithUri : ScreenshotMetadata
{
    public required string Uri { get; set; }
}