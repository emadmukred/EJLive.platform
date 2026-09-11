namespace EJLive.Core.Engine;

/// <summary>
/// Registry for ATM vendor-specific strategies including journal paths,
/// file patterns, parser types, XFS adapter types, and operational profiles.
/// Centralizes all vendor-specific knowledge for the platform.
/// </summary>
public sealed class AtmVendorStrategyRegistry
{
    private readonly Dictionary<string, VendorStrategy> _strategies = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the singleton default registry with all known vendor strategies.
    /// </summary>
    public static AtmVendorStrategyRegistry Default { get; } = CreateDefault();

    /// <summary>
    /// Registers a vendor strategy.
    /// </summary>
    public void Register(VendorStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategies[strategy.VendorType] = strategy;
    }

    /// <summary>
    /// Gets the strategy for a vendor type.
    /// </summary>
    public VendorStrategy? GetStrategy(string vendorType) =>
        _strategies.TryGetValue(vendorType ?? string.Empty, out var s) ? s : null;

    /// <summary>
    /// Gets all registered vendor types.
    /// </summary>
    public IReadOnlyCollection<string> Vendors => _strategies.Keys.ToArray();

    /// <summary>
    /// Returns the default journal path for the given vendor.
    /// </summary>
    public string GetDefaultJournalPath(string vendorType) =>
        GetStrategy(vendorType)?.DefaultJournalPath ?? string.Empty;

    /// <summary>
    /// Returns the file tracking mode (offset-based or daily-file-based).
    /// </summary>
    public FileTrackingMode GetTrackingMode(string vendorType) =>
        GetStrategy(vendorType)?.TrackingMode ?? FileTrackingMode.OffsetBased;

    private static AtmVendorStrategyRegistry CreateDefault()
    {
        var registry = new AtmVendorStrategyRegistry();

        registry.Register(new VendorStrategy
        {
            VendorType = "NCR",
            DisplayName = "NCR APTRA",
            DefaultJournalPath = @"C:\Program Files\NCR APATRA\Advance NDC\Data\",
            JournalFilePattern = "EJDATA.LOG",
            TraceFilePattern = "*.log",
            TrackingMode = FileTrackingMode.OffsetBased,
            SupportsXfs = true,
            XfsRuntimeName = "OOXFS"
        });

        registry.Register(new VendorStrategy
        {
            VendorType = "GRG",
            DisplayName = "GRG Banking",
            DefaultJournalPath = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\",
            JournalFilePattern = "EJ_*.dat",
            TraceFilePattern = "TRACE*",
            TrackingMode = FileTrackingMode.DailyFile,
            SupportsXfs = true,
            XfsRuntimeName = "GRG_XFS"
        });

        registry.Register(new VendorStrategy
        {
            VendorType = "Wincor",
            DisplayName = "Wincor Nixdorf / Diebold Nixdorf (ProView)",
            DefaultJournalPath = @"C:\journal\",
            JournalFilePattern = "*.ej",
            TraceFilePattern = "*.log",
            TrackingMode = FileTrackingMode.DailyFile,
            SupportsXfs = true,
            XfsRuntimeName = "WOSA/XFS"
        });

        registry.Register(new VendorStrategy
        {
            VendorType = "Diebold",
            DisplayName = "Diebold / Agilis",
            DefaultJournalPath = @"C:\Diebold\EJ\",
            JournalFilePattern = "*.jrn",
            TraceFilePattern = "*.log",
            TrackingMode = FileTrackingMode.DailyFile,
            SupportsXfs = true,
            XfsRuntimeName = "MDS_XFS"
        });

        registry.Register(new VendorStrategy
        {
            VendorType = "Hyosung",
            DisplayName = "Hyosung / Nautilus",
            DefaultJournalPath = @"C:\Hyosung\EJ\",
            JournalFilePattern = "EJ_*.dat",
            TraceFilePattern = "*.log",
            TrackingMode = FileTrackingMode.DailyFile,
            SupportsXfs = true,
            XfsRuntimeName = "HYOSUNG_XFS"
        });

        return registry;
    }
}

/// <summary>
/// Defines the operational strategy for a specific ATM vendor.
/// </summary>
public sealed class VendorStrategy
{
    /// <summary>The canonical vendor type identifier.</summary>
    public string VendorType { get; set; } = string.Empty;

    /// <summary>Human-readable display name for the vendor.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Default journal path on the ATM filesystem.</summary>
    public string DefaultJournalPath { get; set; } = string.Empty;

    /// <summary>Glob pattern for journal files.</summary>
    public string JournalFilePattern { get; set; } = string.Empty;

    /// <summary>Glob pattern for trace/diagnostic files.</summary>
    public string TraceFilePattern { get; set; } = string.Empty;

    /// <summary>Whether journal files are tracked by offset or by daily file identity.</summary>
    public FileTrackingMode TrackingMode { get; set; } = FileTrackingMode.OffsetBased;

    /// <summary>Whether this vendor supports XFS/CEN standard.</summary>
    public bool SupportsXfs { get; set; }

    /// <summary>Name of the XFS runtime used by this vendor.</summary>
    public string XfsRuntimeName { get; set; } = string.Empty;
}

/// <summary>
/// Defines how journal file progress is tracked for synchronization.
/// </summary>
public enum FileTrackingMode
{
    /// <summary>NCR-style: single append-only file tracked by byte offset.</summary>
    OffsetBased,

    /// <summary>GRG/Wincor-style: daily files tracked by file identity, date, and checksum.</summary>
    DailyFile
}
