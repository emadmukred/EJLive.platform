using EJLive.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace EJLive.Core.Engine;

/// <summary>
/// Options governing screenshot capture and compression.
/// </summary>
public sealed record CaptureOptions
{
    /// <summary>JPEG quality factor (1-100). Default is 75.</summary>
    public int Quality { get; init; } = ScreenshotCaptureService.DefaultQuality;

    /// <summary>Maximum compressed size in bytes. Default is 1 MB.</summary>
    public long MaxSizeBytes { get; init; } = ScreenshotCaptureService.DefaultMaxSizeBytes;
}

/// <summary>
/// Result of a screenshot capture and delivery attempt.
/// </summary>
public sealed record ScreenshotResult
{
    /// <summary>Whether the operation succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Generated metadata for the captured screenshot.</summary>
    public ScreenshotMetadata? Metadata { get; init; }

    /// <summary>Error message if the operation failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Creates a successful result.</summary>
    public static ScreenshotResult Succeeded(ScreenshotMetadata metadata) => new() { Success = true, Metadata = metadata };

    /// <summary>Creates a failed result.</summary>
    public static ScreenshotResult Failure(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Provides raw image capture from the screen.
/// </summary>
public interface IScreenCaptureProvider
{
    /// <summary>Whether the desktop/session is available for capture.</summary>
    bool IsDesktopAvailable();

    /// <summary>Captures the screen asynchronously.</summary>
    Task<RawImageData> CaptureAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents raw captured image data.
/// </summary>
public sealed record RawImageData
{
    /// <summary>Raw pixel data.</summary>
    public required byte[] Data { get; init; }

    /// <summary>Image width in pixels.</summary>
    public required int Width { get; init; }

    /// <summary>Image height in pixels.</summary>
    public required int Height { get; init; }
}

/// <summary>
/// Encodes raw image data to a compressed format.
/// </summary>
public interface IImageEncoder
{
    /// <summary>
    /// Encodes the specified raw image data.
    /// </summary>
    /// <param name="rawData">Raw pixel data.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="quality">Compression quality (1-100).</param>
    /// <returns>Compressed image bytes.</returns>
    byte[] Encode(byte[] rawData, int width, int height, int quality);
}

/// <summary>
/// Delivers screenshot payloads to the remote server.
/// </summary>
public interface IScreenshotDeliveryClient
{
    /// <summary>Sends the screenshot metadata and payload to the server.</summary>
    Task SendAsync(ScreenshotMetadata metadata, byte[] payload, CancellationToken cancellationToken = default);
}

/// <summary>
/// Manages retention and cleanup of local screenshot artifacts.
/// </summary>
public interface IScreenshotRetentionPolicy
{
    /// <summary>Removes expired or excess local screenshot files.</summary>
    Task CleanupAsync(string atmId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Internal compressed image container.
/// </summary>
internal sealed record CompressedImage(byte[] Data, int Quality);

/// <summary>
/// Service that captures screenshots on a configurable cadence,
/// compresses them, and delivers them to the server.
/// Screenshots are never displayed locally.
/// </summary>
public class ScreenshotCaptureService
{
    private readonly IScreenCaptureProvider _captureProvider;
    private readonly IImageEncoder _imageEncoder;
    private readonly IScreenshotDeliveryClient _deliveryClient;
    private readonly IScreenshotRetentionPolicy _retentionPolicy;
    private readonly TimeProvider _timeProvider;

    /// <summary>Default capture cadence in minutes.</summary>
    public const int DefaultCadenceMinutes = 5;

    /// <summary>Default JPEG quality (1-100).</summary>
    public const int DefaultQuality = 75;

    /// <summary>Default maximum size cap in bytes (1 MB).</summary>
    public const long DefaultMaxSizeBytes = 1_048_576;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenshotCaptureService"/> class.
    /// </summary>
    public ScreenshotCaptureService(
        IScreenCaptureProvider captureProvider,
        IImageEncoder imageEncoder,
        IScreenshotDeliveryClient deliveryClient,
        IScreenshotRetentionPolicy retentionPolicy,
        TimeProvider? timeProvider = null)
    {
        _captureProvider = captureProvider ?? throw new ArgumentNullException(nameof(captureProvider));
        _imageEncoder = imageEncoder ?? throw new ArgumentNullException(nameof(imageEncoder));
        _deliveryClient = deliveryClient ?? throw new ArgumentNullException(nameof(deliveryClient));
        _retentionPolicy = retentionPolicy ?? throw new ArgumentNullException(nameof(retentionPolicy));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Captures a screenshot, compresses it, and delivers it to the server.
    /// </summary>
    public async Task<ScreenshotResult> CaptureAndDeliverAsync(
        string atmId,
        CaptureOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(atmId))
            throw new ArgumentException("ATM identifier is required.", nameof(atmId));

        options ??= new CaptureOptions();

        if (!_captureProvider.IsDesktopAvailable())
        {
            return ScreenshotResult.Failure("No desktop or session 0 environment available for capture.");
        }

        RawImageData rawImage;
        try
        {
            rawImage = await _captureProvider.CaptureAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ScreenshotResult.Failure($"Capture failed: {ex.Message}");
        }

        if (rawImage.Data is null || rawImage.Data.Length == 0)
        {
            return ScreenshotResult.Failure("Capture returned empty image data.");
        }

        var compressed = await CompressAsync(rawImage, options, cancellationToken).ConfigureAwait(false);

        string hash = ComputeSha256(compressed.Data);

        var metadata = new ScreenshotMetadata
        {
            AtmId = atmId,
            TimestampUtc = _timeProvider.GetUtcNow().UtcDateTime,
            Width = rawImage.Width,
            Height = rawImage.Height,
            Quality = compressed.Quality,
            Sha256 = hash,
            SizeBytes = compressed.Data.Length
        };

        await _deliveryClient.SendAsync(metadata, compressed.Data, cancellationToken).ConfigureAwait(false);

        await _retentionPolicy.CleanupAsync(atmId, cancellationToken).ConfigureAwait(false);

        return ScreenshotResult.Succeeded(metadata);
    }

    private async Task<CompressedImage> CompressAsync(
        RawImageData raw,
        CaptureOptions options,
        CancellationToken cancellationToken)
    {
        byte[] data = await Task.Run(
            () => _imageEncoder.Encode(raw.Data, raw.Width, raw.Height, options.Quality),
            cancellationToken).ConfigureAwait(false);

        int quality = options.Quality;

        while (data.Length > options.MaxSizeBytes && quality > 10)
        {
            cancellationToken.ThrowIfCancellationRequested();
            quality -= 5;
            data = await Task.Run(
                () => _imageEncoder.Encode(raw.Data, raw.Width, raw.Height, quality),
                cancellationToken).ConfigureAwait(false);
        }

        if (data.Length > options.MaxSizeBytes)
        {
            throw new InvalidOperationException("Unable to compress image within the configured size cap.");
        }

        return new CompressedImage(data, quality);
    }

    private static string ComputeSha256(byte[] data)
    {
        byte[] hash = SHA256.HashData(data);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
