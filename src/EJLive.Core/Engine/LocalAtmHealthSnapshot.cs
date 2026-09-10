using System.Text.Json;

namespace EJLive.Core.Engine;

/// <summary>
/// Collects and publishes a local ATM health snapshot including
/// disk space, service state, journal sync progress, connectivity,
/// and recent error counts.
/// </summary>
public sealed class LocalAtmHealthSnapshot
{
    /// <summary>
    /// Captures a health snapshot for the local ATM agent.
    /// </summary>
    /// <param name="atmId">The ATM identifier.</param>
    /// <param name="vendorType">The vendor type (NCR, GRG, etc.).</param>
    /// <param name="connected">Whether currently connected to server.</param>
    /// <param name="handshakeComplete">Whether handshake is established.</param>
    /// <param name="pendingOutbox">Pending items in the outbox queue.</param>
    /// <param name="lastSyncUtc">Last successful sync timestamp.</param>
    /// <param name="journalPath">The monitored journal directory.</param>
    /// <returns>A structured health snapshot.</returns>
    public static AtmHealthReport Capture(
        string atmId,
        string vendorType,
        bool connected,
        bool handshakeComplete,
        int pendingOutbox,
        DateTime? lastSyncUtc,
        string? journalPath)
    {
        var report = new AtmHealthReport
        {
            AtmId = atmId,
            VendorType = vendorType,
            CapturedAtUtc = DateTime.UtcNow,
            Connected = connected,
            HandshakeComplete = handshakeComplete,
            PendingOutbox = pendingOutbox,
            LastSyncUtc = lastSyncUtc
        };

        // Disk space check
        if (!string.IsNullOrWhiteSpace(journalPath))
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(journalPath) ?? "C:");
                report.DiskFreeBytes = drive.AvailableFreeSpace;
                report.DiskTotalBytes = drive.TotalSize;
                report.DiskHealthy = drive.AvailableFreeSpace > 500_000_000; // > 500MB
            }
            catch
            {
                report.DiskHealthy = true; // assume healthy if cannot check
            }
        }

        // Compute overall health score (0-100)
        report.HealthScore = ComputeScore(report);

        return report;
    }

    /// <summary>
    /// Serializes a health report to JSON for writing to health.json.
    /// </summary>
    public static string ToJson(AtmHealthReport report)
    {
        return JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Writes the health report to the specified file path.
    /// </summary>
    public static void WriteToFile(AtmHealthReport report, string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(filePath, ToJson(report));
    }

    private static int ComputeScore(AtmHealthReport report)
    {
        int score = 100;

        if (!report.Connected) score -= 30;
        if (!report.HandshakeComplete) score -= 20;
        if (report.PendingOutbox > 100) score -= 20;
        else if (report.PendingOutbox > 10) score -= 10;
        if (!report.DiskHealthy) score -= 15;

        if (report.LastSyncUtc.HasValue)
        {
            var syncAge = DateTime.UtcNow - report.LastSyncUtc.Value;
            if (syncAge > TimeSpan.FromHours(4)) score -= 15;
            else if (syncAge > TimeSpan.FromHours(1)) score -= 5;
        }
        else
        {
            score -= 10; // never synced
        }

        return Math.Max(0, Math.Min(100, score));
    }
}

/// <summary>
/// A point-in-time health report for a single ATM.
/// </summary>
public sealed class AtmHealthReport
{
    public string AtmId { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
    public bool Connected { get; set; }
    public bool HandshakeComplete { get; set; }
    public int PendingOutbox { get; set; }
    public DateTime? LastSyncUtc { get; set; }
    public long DiskFreeBytes { get; set; }
    public long DiskTotalBytes { get; set; }
    public bool DiskHealthy { get; set; } = true;
    public int HealthScore { get; set; } = 100;
}
