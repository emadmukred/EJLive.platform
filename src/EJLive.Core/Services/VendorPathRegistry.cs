using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed record AtmVendorPathProfile(
    string VendorKey,
    string DisplayName,
    IReadOnlyList<string> JournalSourcePaths,
    IReadOnlyList<string> BackupPaths,
    IReadOnlyList<string> TraceLogPaths,
    string ImageInboxPath,
    IReadOnlyList<string> ImageDestinationPaths,
    string ScreenshotCachePath,
    IReadOnlyList<string> SupportedExtensions,
    string RolloverBehavior,
    string ValidationRule,
    bool RequiresWritePermission,
    bool RequiresRestartAfterPromotion,
    string RestartNotes);

public interface IAtmVendorPathProvider
{
    string VendorKey { get; }

    AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides);
}

public sealed class VendorPathRegistry
{
    private readonly Dictionary<string, IAtmVendorPathProvider> _providers;
    private readonly IAtmVendorPathProvider _genericProvider;

    public VendorPathRegistry()
    {
        var providers = new IAtmVendorPathProvider[]
        {
            new NcrVendorPathProvider(),
            new GrgVendorPathProvider(),
            new WincorVendorPathProvider(),
            new DieboldVendorPathProvider(),
            new HyosungVendorPathProvider(),
            new CashwayVendorPathProvider(),
            new GenericVendorPathProvider()
        };

        _providers = providers.ToDictionary(provider => provider.VendorKey, StringComparer.OrdinalIgnoreCase);
        _genericProvider = _providers["GENERIC"];
    }

    public AtmVendorPathProfile Resolve(string? vendor, AppConfig config)
    {
        var normalized = AppConstants.NormalizeATMType(vendor);
        if (_providers.TryGetValue(normalized, out var provider))
            return provider.BuildProfile(config, useConfigOverrides: true);

        if (!string.IsNullOrWhiteSpace(vendor) && _providers.TryGetValue(vendor.Trim(), out provider))
            return provider.BuildProfile(config, useConfigOverrides: true);

        return _genericProvider.BuildProfile(config, useConfigOverrides: true) with
        {
            VendorKey = string.IsNullOrWhiteSpace(vendor) ? "GENERIC" : vendor.Trim().ToUpperInvariant()
        };
    }

    public IReadOnlyList<AtmVendorPathProfile> GetAllProfiles(AppConfig config)
    {
        return _providers.Values
            .Select(provider => provider.BuildProfile(config, useConfigOverrides: false))
            .OrderBy(profile => profile.VendorKey, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

public sealed class NcrVendorPathProvider : VendorPathProviderBase
{
    public override string VendorKey => AppConstants.ATM_TYPE_NCR;

    public override AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides)
    {
        var source = useConfigOverrides ? SelectSourcePath(config, AppConstants.NCR_JournalPath) : AppConstants.NCR_JournalPath;
        var backup = useConfigOverrides ? SelectBackupPath(config, AppConstants.NCR_BackupPath) : AppConstants.NCR_BackupPath;
        return BuildCommonProfile(
            config,
            vendorKey: VendorKey,
            displayName: "NCR APTRA",
            sourcePaths: new[] { source },
            backupPaths: new[] { backup },
            tracePaths: new[] { Path.Combine(source, "TRACE"), Path.Combine(source, "LOGS") },
            extensions: new[] { ".log", ".lOb", ".txt", ".ej" },
            rolloverBehavior: "Fixed files tracked by offset (overwrite-aware).",
            validationRule: "Require read permission on source and write permission on backup/inbox/destination.",
            requiresRestartAfterPromotion: false,
            restartNotes: "No restart usually required for file promotions."
        );
    }
}

public sealed class GrgVendorPathProvider : VendorPathProviderBase
{
    public override string VendorKey => AppConstants.ATM_TYPE_GRG;

    public override AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides)
    {
        var source = useConfigOverrides ? SelectSourcePath(config, AppConstants.GRG_JournalPath) : AppConstants.GRG_JournalPath;
        var backup = useConfigOverrides ? SelectBackupPath(config, AppConstants.GRG_BackupPath) : AppConstants.GRG_BackupPath;
        return BuildCommonProfile(
            config,
            vendorKey: VendorKey,
            displayName: "GRG Banking",
            sourcePaths: new[] { source },
            backupPaths: new[] { backup },
            tracePaths: new[] { Path.Combine(source, "TRACE"), Path.Combine(source, "XFS") },
            extensions: new[] { ".dat", ".log", ".txt" },
            rolloverBehavior: "Daily file families with identity/date/checksum tracking.",
            validationRule: "Require consistent daily file naming and writable backup staging.",
            requiresRestartAfterPromotion: false,
            restartNotes: "Restart normally not required."
        );
    }
}

public sealed class WincorVendorPathProvider : VendorPathProviderBase
{
    public override string VendorKey => AppConstants.ATM_TYPE_WN;

    public override AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides)
    {
        var source = useConfigOverrides ? SelectSourcePath(config, AppConstants.WN_JournalPath) : AppConstants.WN_JournalPath;
        var backup = useConfigOverrides ? SelectBackupPath(config, AppConstants.WN_BackupPath) : AppConstants.WN_BackupPath;
        return BuildCommonProfile(
            config,
            vendorKey: VendorKey,
            displayName: "Wincor/Nixdorf",
            sourcePaths: new[] { source },
            backupPaths: new[] { backup },
            tracePaths: new[] { Path.Combine(source, "TRACE"), Path.Combine(source, "XFS") },
            extensions: new[] { ".ej", ".log", ".txt", ".jrn" },
            rolloverBehavior: "Daily journal and trace files tracked by identity/checksum.",
            validationRule: "Require path existence checks and destination write rights before promote.",
            requiresRestartAfterPromotion: true,
            restartNotes: "Some deployments require application restart after content replacement."
        );
    }
}

public sealed class DieboldVendorPathProvider : VendorPathProviderBase
{
    public override string VendorKey => AppConstants.ATM_TYPE_DN;

    public override AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides)
    {
        var source = useConfigOverrides ? SelectSourcePath(config, AppConstants.DN_JournalPath) : AppConstants.DN_JournalPath;
        var backup = useConfigOverrides ? SelectBackupPath(config, AppConstants.DN_BackupPath) : AppConstants.DN_BackupPath;
        return BuildCommonProfile(
            config,
            vendorKey: VendorKey,
            displayName: "Diebold",
            sourcePaths: new[] { source },
            backupPaths: new[] { backup },
            tracePaths: new[] { Path.Combine(source, "TRACE"), Path.Combine(source, "MDS") },
            extensions: new[] { ".jrn", ".log", ".txt" },
            rolloverBehavior: "Rolling daily files with fallback to journal snapshots.",
            validationRule: "Require writable backup and destination allowlist validation.",
            requiresRestartAfterPromotion: true,
            restartNotes: "Restart may be required for certain UI/image assets."
        );
    }
}

public sealed class HyosungVendorPathProvider : VendorPathProviderBase
{
    public override string VendorKey => AppConstants.ATM_TYPE_HY;

    public override AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides)
    {
        var source = useConfigOverrides ? SelectSourcePath(config, AppConstants.HY_JournalPath) : AppConstants.HY_JournalPath;
        var backup = useConfigOverrides ? SelectBackupPath(config, AppConstants.HY_BackupPath) : AppConstants.HY_BackupPath;
        return BuildCommonProfile(
            config,
            vendorKey: VendorKey,
            displayName: "Hyosung",
            sourcePaths: new[] { source },
            backupPaths: new[] { backup },
            tracePaths: new[] { Path.Combine(source, "TRACE"), Path.Combine(source, "LOG") },
            extensions: new[] { ".dat", ".log", ".txt" },
            rolloverBehavior: "Daily journal files with checksum-based deduplication.",
            validationRule: "Require readable source, writable staging, and checksum verification before promotion.",
            requiresRestartAfterPromotion: true,
            restartNotes: "Restart may be required depending on content target."
        );
    }
}

public sealed class CashwayVendorPathProvider : VendorPathProviderBase
{
    public override string VendorKey => AppConstants.ATM_TYPE_CW;

    public override AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides)
    {
        var source = useConfigOverrides ? SelectSourcePath(config, AppConstants.CW_JournalPath) : AppConstants.CW_JournalPath;
        var backup = useConfigOverrides ? SelectBackupPath(config, AppConstants.CW_BackupPath) : AppConstants.CW_BackupPath;
        return BuildCommonProfile(
            config,
            vendorKey: VendorKey,
            displayName: "Cashway",
            sourcePaths: new[] { source },
            backupPaths: new[] { backup },
            tracePaths: new[] { Path.Combine(source, "TRACE"), Path.Combine(source, "XFS"), Path.Combine(source, "LOG") },
            extensions: new[] { ".ej", ".log", ".txt", ".dat" },
            rolloverBehavior: "Daily journal batches with optional rolling diagnostics traces.",
            validationRule: "Require source readability, destination allowlist validation, and writable backup before promote.",
            requiresRestartAfterPromotion: true,
            restartNotes: "Restart may be required for terminal UI/content refresh depending on deployed package."
        );
    }
}

public sealed class GenericVendorPathProvider : VendorPathProviderBase
{
    public override string VendorKey => "GENERIC";

    public override AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides)
    {
        var source = useConfigOverrides ? SelectSourcePath(config, @"C:\Journal\") : @"C:\Journal\";
        var backup = useConfigOverrides ? SelectBackupPath(config, @"C:\EJLive_BackupLog\") : @"C:\EJLive_BackupLog\";
        return BuildCommonProfile(
            config,
            vendorKey: VendorKey,
            displayName: "Generic ATM",
            sourcePaths: new[] { source },
            backupPaths: new[] { backup },
            tracePaths: new[] { Path.Combine(source, "TRACE") },
            extensions: new[] { ".log", ".txt", ".ej", ".dat", ".jrn" },
            rolloverBehavior: "Mixed rollover; detect by file identity and timestamp.",
            validationRule: "Require explicit local validation before activation.",
            requiresRestartAfterPromotion: false,
            restartNotes: "Restart requirement depends on target application."
        );
    }
}

public abstract class VendorPathProviderBase : IAtmVendorPathProvider
{
    public abstract string VendorKey { get; }

    public abstract AtmVendorPathProfile BuildProfile(AppConfig config, bool useConfigOverrides);

    protected static string SelectSourcePath(AppConfig config, string fallback)
    {
        return string.IsNullOrWhiteSpace(config.SourcePath) ? fallback : config.SourcePath.Trim();
    }

    protected static string SelectBackupPath(AppConfig config, string fallback)
    {
        return string.IsNullOrWhiteSpace(config.BackupPath) ? fallback : config.BackupPath.Trim();
    }

    protected static string SelectInboxPath(AppConfig config)
    {
        return string.IsNullOrWhiteSpace(config.ImageInboxPath)
            ? AppConstants.DefaultClientInboxPath
            : config.ImageInboxPath.Trim();
    }

    protected static string SelectDestinationRoot(AppConfig config)
    {
        return string.IsNullOrWhiteSpace(config.BackupPath)
            ? AppConstants.DefaultClientOutboxPath
            : config.BackupPath.Trim();
    }

    protected AtmVendorPathProfile BuildCommonProfile(
        AppConfig config,
        string vendorKey,
        string displayName,
        IReadOnlyList<string> sourcePaths,
        IReadOnlyList<string> backupPaths,
        IReadOnlyList<string> tracePaths,
        IReadOnlyList<string> extensions,
        string rolloverBehavior,
        string validationRule,
        bool requiresRestartAfterPromotion,
        string restartNotes)
    {
        var inbox = SelectInboxPath(config);
        var destinationRoot = SelectDestinationRoot(config);
        var destination = Path.Combine(destinationRoot, "Images", vendorKey);
        var screenshot = Path.Combine(destinationRoot, "Screenshots");

        return new AtmVendorPathProfile(
            VendorKey: vendorKey,
            DisplayName: displayName,
            JournalSourcePaths: sourcePaths.Select(Path.GetFullPath).ToArray(),
            BackupPaths: backupPaths.Select(Path.GetFullPath).ToArray(),
            TraceLogPaths: tracePaths.Select(Path.GetFullPath).ToArray(),
            ImageInboxPath: Path.GetFullPath(inbox),
            ImageDestinationPaths: new[] { Path.GetFullPath(destination) },
            ScreenshotCachePath: Path.GetFullPath(screenshot),
            SupportedExtensions: extensions,
            RolloverBehavior: rolloverBehavior,
            ValidationRule: validationRule,
            RequiresWritePermission: true,
            RequiresRestartAfterPromotion: requiresRestartAfterPromotion,
            RestartNotes: restartNotes);
    }
}
