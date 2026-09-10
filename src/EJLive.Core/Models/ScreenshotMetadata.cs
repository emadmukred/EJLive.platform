namespace EJLive.Core.Models;

/// <summary>
/// Metadata describing a captured screenshot.
/// </summary>
public sealed record ScreenshotMetadata
{
    /// <summary>Identifier of the ATM where the screenshot was captured.</summary>
    public required string AtmId { get; init; }

    /// <summary>UTC timestamp when the screenshot was captured.</summary>
    public required DateTime TimestampUtc { get; init; }

    /// <summary>Width of the captured image in pixels.</summary>
    public required int Width { get; init; }

    /// <summary>Height of the captured image in pixels.</summary>
    public required int Height { get; init; }

    /// <summary>JPEG quality factor used (1-100).</summary>
    public required int Quality { get; init; }

    /// <summary>SHA-256 hash of the compressed image bytes.</summary>
    public required string Sha256 { get; init; }

    /// <summary>Size of the compressed image in bytes.</summary>
    public required long SizeBytes { get; init; }
}
