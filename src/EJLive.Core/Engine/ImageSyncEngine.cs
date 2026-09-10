namespace EJLive.Core.Engine;

/// <summary>Promotes validated image assets into a configured local destination.</summary>
public sealed class ImageSyncEngine
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp", ".gif", ".jpeg", ".jpg", ".png", ".webp"
    };

    public long MaximumImageBytes { get; init; } = 50L * 1024 * 1024;

    public string SyncImage(string sourceFile, string destinationFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFolder);
        if (MaximumImageBytes <= 0)
            throw new InvalidOperationException("Maximum image size must be positive.");

        var sourcePath = Path.GetFullPath(sourceFile);
        var source = new FileInfo(sourcePath);
        if (!source.Exists)
            throw new FileNotFoundException("Image source was not found.", sourcePath);
        if (!AllowedExtensions.Contains(source.Extension))
            throw new InvalidDataException($"Image extension '{source.Extension}' is not allowed.");
        if (source.Length <= 0 || source.Length > MaximumImageBytes)
            throw new InvalidDataException($"Image size must be between 1 and {MaximumImageBytes} bytes.");

        var destinationRoot = Path.GetFullPath(destinationFolder);
        Directory.CreateDirectory(destinationRoot);
        var destination = Path.GetFullPath(Path.Combine(destinationRoot, source.Name));
        var prefix = destinationRoot.EndsWith(Path.DirectorySeparatorChar)
            ? destinationRoot
            : destinationRoot + Path.DirectorySeparatorChar;
        if (!destination.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Resolved destination escapes its configured root.");

        var temporaryPath = Path.Combine(destinationRoot, $".{source.Name}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.Copy(sourcePath, temporaryPath, overwrite: false);
            File.Move(temporaryPath, destination, overwrite: true);
            return destination;
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
