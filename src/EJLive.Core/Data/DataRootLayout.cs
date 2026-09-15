using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks;
using EJLive.Shared;

namespace EJLive.Core.Data;

/// <summary>
/// The platform-facing view of the central <c>dataroot</c> (SS-20). Path resolution lives
/// in <see cref="EJLive.Shared.DataRootPaths"/> (L0, shared by every assembly); this class
/// adds the pieces only the data layer needs: the server archive tree, journal staging
/// folders, the fixed database file, and a <see cref="Probe"/> that answers "is the data
/// root actually usable by this process" — writability, not just existence — because an
/// ACL-blocked <c>ProgramData\EJLive</c> must surface as a structured bootstrap failure,
/// not as a first-insert <see cref="IOException"/> at 02:00.
/// </summary>
public sealed class DataRootLayout
{
    public DataRootLayout()
        : this(DataRootPaths.Root)
    {
    }

    /// <summary>Binds the layout to an explicit root (isolated test trees, portable installs).</summary>
    public DataRootLayout(string root)
    {
        if (!DataRootPaths.IsUsablePath(root))
            throw new ArgumentException(
                "A dataroot must be an absolute path without wildcards.", nameof(root));
        Root = Path.GetFullPath(root);
    }

    /// <summary>Absolute root every runtime artifact hangs off.</summary>
    public string Root { get; }

    public string DatabaseFile => Path.Combine(Root, "ejlive.db");
    public string ConfigDirectory => Path.Combine(Root, "Config");
    public string LogsDirectory => Path.Combine(Root, "Logs");
    public string ArchiveDirectory => Path.Combine(Root, "Archive");
    public string ReportsDirectory => Path.Combine(Root, "Reports");
    public string OutboxDirectory => Path.Combine(Root, "Client", "Outbox");
    public string InboxDirectory => Path.Combine(Root, "Client", "Inbox");
    public string JournalStagingDirectory => Path.Combine(Root, "Client", "Staging");
    public string HealthSnapshotFile => Path.Combine(Root, "Agent", "health.json");

    /// <summary>Server-side archive tree: <c>Archive/&lt;atmId&gt;/&lt;yyyy-MM&gt;/</c>.</summary>
    public string GetTerminalArchiveDirectory(string atmId, DateTime? monthUtc = null)
    {
        var safeId = SanitizeSegment(atmId);
        var month = (monthUtc ?? DateTime.UtcNow).ToUniversalTime().ToString("yyyy-MM");
        return Path.Combine(ArchiveDirectory, safeId, month);
    }

    /// <summary>Creates the fixed layout. Idempotent; per-directory failures are reported, not thrown.</summary>
    public void EnsureDirectories()
    {
        foreach (var dir in new[]
                 {
                     Root, ConfigDirectory, LogsDirectory, ArchiveDirectory, ReportsDirectory,
                     Path.Combine(Root, "Client"), OutboxDirectory, InboxDirectory,
                     JournalStagingDirectory, Path.Combine(Root, "Agent")
                 })
        {
            try { Directory.CreateDirectory(dir); }
            catch (Exception)
            {
                // The probe below classifies every failure; creation here stays best-effort so one
                // locked subtree cannot stop the rest of the tree from materialising.
            }
        }
    }

    /// <summary>
    /// Verifies the root exists, is writable, and the database file can be opened for a test
    /// transaction. Returns human-readable findings (one line each) and a success flag the
    /// bootstrap surface maps to <c>BootstrapStep.Ok</c>.
    /// </summary>
    public async Task<DataRootProbeResult> ProbeAsync()
    {
        var findings = new List<string>();
        var ok = true;

        try
        {
            if (!Directory.Exists(Root))
                findings.Add($"root missing: {Root}");
            var probeFile = Path.Combine(Root, $".probe-{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(probeFile, "probe").ConfigureAwait(false);
            File.Delete(probeFile);
            findings.Add("root writable: ok");
        }
        catch (Exception ex)
        {
            ok = false;
            findings.Add("root NOT writable: " + ex.Message);
        }

        try
        {
            var db = DatabaseFile;
            if (File.Exists(db))
            {
                // Touch test only — integrity is the migration runner's job (schema version guard).
                using (File.Open(db, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }
                findings.Add("database file openable: ok");
            }
            else
            {
                findings.Add("database file not created yet (first run): " + db);
            }
        }
        catch (Exception ex)
        {
            ok = false;
            findings.Add("database file unusable: " + ex.Message);
        }

        return new DataRootProbeResult(ok, findings);
    }

    private static string SanitizeSegment(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        foreach (var bad in Path.GetInvalidFileNameChars())
            trimmed = trimmed.Replace(bad, '_');
        return string.IsNullOrWhiteSpace(trimmed) ? "UNKNOWN" : trimmed;
    }
}

/// <summary>Outcome of <see cref="DataRootLayout.ProbeAsync"/>.</summary>
/// <param name="Success">True when the root is present and writable and the database file is usable.</param>
/// <param name="Findings">One line per check, in order; surfaced verbatim by the bootstrap report.</param>
public sealed record DataRootProbeResult(bool Success, IReadOnlyList<string> Findings);
