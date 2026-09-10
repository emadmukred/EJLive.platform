using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class UnifiedJournalStorageService
{
    private readonly UnifiedJournalEvidenceAnalyzer _analyzer;

    public UnifiedJournalStorageService(UnifiedJournalEvidenceAnalyzer? analyzer = null)
    {
        _analyzer = analyzer ?? new UnifiedJournalEvidenceAnalyzer();
    }

    public JournalStorageResult StoreJournalData(
        string storagePath,
        string atmId,
        string atmType,
        string fileName,
        byte[] data,
        string checksum = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(data);

        var monthFolder = Path.Combine(storagePath, SafePath(atmId), DateTime.UtcNow.ToString("yyyy-MM"));
        Directory.CreateDirectory(monthFolder);

        var fullPath = Path.Combine(monthFolder, SafePath(fileName));
        File.WriteAllBytes(fullPath, data);

        var text = DecodeJournalText(data);
        var evidence = _analyzer.Analyze(atmId, atmType, text);

        return new JournalStorageResult(
            atmId,
            fileName,
            fullPath,
            data.LongLength,
            string.IsNullOrWhiteSpace(checksum) ? EJLive.Shared.SecurityHelper.MD5Hash(data) : checksum,
            DateTime.UtcNow,
            evidence);
    }

    public string ArchiveMonth(string storagePath, string archivePath, string atmId, string yearMonth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        ArgumentException.ThrowIfNullOrWhiteSpace(yearMonth);

        var source = Path.Combine(storagePath, SafePath(atmId), yearMonth);
        if (!Directory.Exists(source))
            return string.Empty;

        Directory.CreateDirectory(archivePath);
        var zipPath = Path.Combine(archivePath, $"{SafePath(atmId)}_{yearMonth.Replace("-", string.Empty, StringComparison.Ordinal)}.zip");
        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(source, zipPath);
        return zipPath;
    }

    public string ExportCsvReport(IEnumerable<JournalStorageResult> records, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("ATM_ID,FileName,FileSize,ReceivedAtUtc,Checksum,Approved,Declined,CapturedCards,CashErrors,TotalCash");
        foreach (var record in records)
        {
            writer.WriteLine(string.Join(",",
                Csv(record.ATM_ID),
                Csv(record.FileName),
                record.FileSize,
                Csv(record.ReceivedAtUtc.ToString("O")),
                Csv(record.Checksum),
                record.Evidence.ApprovedTransactions,
                record.Evidence.DeclinedTransactions,
                record.Evidence.CapturedCards,
                record.Evidence.CashErrorEvents,
                record.Evidence.TotalCashDispensed));
        }

        return filePath;
    }

    public string ExportHtmlReport(IEnumerable<JournalStorageResult> records, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        var rows = records.ToArray();
        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>EJLive Journal Report</title></head><body>");
        sb.AppendLine("<h1>EJLive Journal Report</h1>");
        sb.AppendLine($"<p>Files: {rows.Length} | Approved: {rows.Sum(r => r.Evidence.ApprovedTransactions)} | Errors: {rows.Sum(r => r.Evidence.CashErrorEvents)}</p>");
        sb.AppendLine("<table><thead><tr><th>ATM</th><th>File</th><th>Size</th><th>Approved</th><th>Cash Errors</th></tr></thead><tbody>");
        foreach (var record in rows)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{Html(record.ATM_ID)}</td>");
            sb.Append($"<td>{Html(record.FileName)}</td>");
            sb.Append($"<td>{record.FileSize}</td>");
            sb.Append($"<td>{record.Evidence.ApprovedTransactions}</td>");
            sb.Append($"<td>{record.Evidence.CashErrorEvents}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table></body></html>");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }

    private static string DecodeJournalText(byte[] data)
    {
        try
        {
            return Encoding.UTF8.GetString(data);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Default.GetString(data);
        }
    }

    private static string SafePath(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        return new string(chars);
    }

    private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string Html(string value)
    {
        return (value ?? string.Empty)
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal);
    }
}

public sealed class UnifiedJournalRoutingService
{
    private readonly UnifiedJournalStorageService _journalStorage;
    private readonly ConcurrentDictionary<string, JournalDeliveryReceipt> _receipts = new(StringComparer.OrdinalIgnoreCase);

    public UnifiedJournalRoutingService(UnifiedJournalStorageService? journalStorage = null)
    {
        _journalStorage = journalStorage ?? new UnifiedJournalStorageService();
    }

    public IReadOnlyList<JournalDeliveryReceipt> Receipts =>
        _receipts.Values
            .OrderByDescending(item => item.ReceivedAtUtc)
            .ToArray();

    public JournalDeliveryReceipt RegisterPending(
        string transferId,
        string atmId,
        string atmType,
        string fileName,
        long expectedBytes,
        string checksum = "",
        string routeHint = "")
    {
        var effectiveTransferId = NormalizeTransferId(transferId, atmId, fileName);
        var category = ResolveCategory(fileName, routeHint);
        var now = DateTime.UtcNow;

        var receipt = _receipts.AddOrUpdate(
            effectiveTransferId,
            _ => new JournalDeliveryReceipt(
                effectiveTransferId,
                SafePath(atmId),
                SafePath(atmType),
                SafePath(fileName),
                category,
                string.Empty,
                Math.Max(0, expectedBytes),
                checksum ?? string.Empty,
                now,
                false,
                "Pending transfer started."),
            (_, existing) => existing with
            {
                ATM_ID = SafePath(atmId),
                ATM_Type = SafePath(atmType),
                FileName = SafePath(fileName),
                Category = category,
                FileSize = Math.Max(existing.FileSize, expectedBytes),
                Checksum = string.IsNullOrWhiteSpace(checksum) ? existing.Checksum : checksum,
                ReceivedAtUtc = now,
                Confirmed = false,
                Detail = "Pending transfer updated."
            });

        return receipt;
    }

    public JournalDeliveryReceipt RegisterFailed(string transferId, string detail)
    {
        var effectiveTransferId = NormalizeTransferId(transferId, "UNKNOWN", "unknown.bin");
        var now = DateTime.UtcNow;

        return _receipts.AddOrUpdate(
            effectiveTransferId,
            _ => new JournalDeliveryReceipt(
                effectiveTransferId,
                "UNKNOWN",
                "UNKNOWN",
                "unknown.bin",
                "unknown",
                string.Empty,
                0,
                string.Empty,
                now,
                false,
                string.IsNullOrWhiteSpace(detail) ? "Transfer failed." : detail),
            (_, existing) => existing with
            {
                ReceivedAtUtc = now,
                Confirmed = false,
                Detail = string.IsNullOrWhiteSpace(detail) ? "Transfer failed." : detail
            });
    }

    public JournalDeliveryReceipt StoreInbound(
        string storageRoot,
        string atmId,
        string atmType,
        string fileName,
        byte[] data,
        string checksum = "",
        string transferId = "",
        string routeHint = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(data);

        var effectiveTransferId = NormalizeTransferId(transferId, atmId, fileName);
        var safeAtmId = SafePath(atmId);
        var safeAtmType = SafePath(string.IsNullOrWhiteSpace(atmType) ? "UNKNOWN" : atmType);
        var safeFileName = SafePath(fileName);
        var category = ResolveCategory(fileName, routeHint);
        var now = DateTime.UtcNow;
        var checksumValue = string.IsNullOrWhiteSpace(checksum)
            ? EJLive.Shared.SecurityHelper.MD5Hash(data)
            : checksum;
        var bytes = Math.Max(0, data.LongLength);
        string storagePath;

        if (string.Equals(category, "journals", StringComparison.OrdinalIgnoreCase))
        {
            var result = _journalStorage.StoreJournalData(
                Path.Combine(storageRoot, safeAtmType),
                safeAtmId,
                safeAtmType,
                safeFileName,
                data,
                checksumValue);
            storagePath = result.StoragePath;
        }
        else
        {
            var folder = Path.Combine(
                storageRoot,
                safeAtmType,
                safeAtmId,
                category,
                now.ToString("yyyy-MM"),
                now.ToString("dd"));
            Directory.CreateDirectory(folder);
            storagePath = Path.Combine(folder, safeFileName);
            File.WriteAllBytes(storagePath, data);
        }

        var detail = $"Stored in {category} partition.";
        var receipt = new JournalDeliveryReceipt(
            effectiveTransferId,
            safeAtmId,
            safeAtmType,
            safeFileName,
            category,
            storagePath,
            bytes,
            checksumValue,
            now,
            true,
            detail);

        _receipts[effectiveTransferId] = receipt;
        return receipt;
    }

    public IReadOnlyList<JournalDeliveryReceipt> FindPending(TimeSpan olderThan)
    {
        var threshold = DateTime.UtcNow - olderThan;
        return Receipts
            .Where(item => !item.Confirmed && item.ReceivedAtUtc <= threshold)
            .ToArray();
    }

    public IReadOnlyList<JournalDeliveryReceipt> FindRecentFailures(TimeSpan window)
    {
        var threshold = DateTime.UtcNow - window;
        return Receipts
            .Where(item => !item.Confirmed &&
                           item.ReceivedAtUtc >= threshold &&
                           item.Detail.Contains("fail", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static string NormalizeTransferId(string transferId, string atmId, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(transferId))
            return transferId.Trim();
        return $"{SafePath(atmId)}:{SafePath(fileName)}:{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static string ResolveCategory(string fileName, string routeHint)
    {
        var hint = (routeHint ?? string.Empty).Trim().ToLowerInvariant();
        if (hint.Contains("screenshot", StringComparison.Ordinal) || hint.Contains("screen", StringComparison.Ordinal))
            return "screenshots";
        if (hint.Contains("image", StringComparison.Ordinal) || hint.Contains("photo", StringComparison.Ordinal))
            return "images";
        if (hint.Contains("backup", StringComparison.Ordinal) || hint.Contains("archive", StringComparison.Ordinal))
            return "backups";
        if (hint.Contains("journal", StringComparison.Ordinal) || hint.Contains("log", StringComparison.Ordinal))
            return "journals";

        var name = (fileName ?? string.Empty).Trim();
        if (name.StartsWith("SCR_", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
        {
            return "screenshots";
        }

        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return "backups";

        if (name.Contains("EJDATA", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".log", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".jrn", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".ej", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            return "journals";
        }

        return "files";
    }

    private static string SafePath(string value)
    {
        var source = string.IsNullOrWhiteSpace(value) ? "UNKNOWN" : value.Trim();
        var invalid = Path.GetInvalidFileNameChars();
        var chars = source.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        return new string(chars);
    }
}

public sealed class UnifiedRemoteCommandOrchestrator
{
    private readonly UnifiedRemoteCommandPolicy _policy;
    private readonly ConcurrentDictionary<string, RemoteCommandDispatch> _history = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<ScheduledRemoteCommand> _schedule = new();

    public UnifiedRemoteCommandOrchestrator(UnifiedRemoteCommandPolicy? policy = null)
    {
        _policy = policy ?? new UnifiedRemoteCommandPolicy();
    }

    public IReadOnlyList<RemoteCommandDispatch> History =>
        _history.Values.OrderByDescending(item => item.Command.CreatedAtUtc).ToArray();

    public IReadOnlyList<ScheduledRemoteCommand> Scheduled => _schedule.ToArray();

    public RemoteCommandDispatch Queue(
        string atmId,
        string commandType,
        string payload = "",
        string role = "Admin",
        bool operatorConfirmed = true,
        bool maintenanceWindow = true)
    {
        var command = new RemoteCommand
        {
            ATM_ID = atmId,
            CommandType = commandType,
            Payload = payload,
            RequiresConfirmation = AppConstants.CommandsRequireConfirmation.Contains(commandType, StringComparer.OrdinalIgnoreCase)
        };

        var decision = _policy.Evaluate(command, role, operatorConfirmed, maintenanceWindow);
        command.Status = decision.Allowed ? RemoteCommandStatus.Sent : RemoteCommandStatus.Failed;
        command.SentAtUtc = decision.Allowed ? DateTime.UtcNow : null;
        command.Result = decision.Allowed ? "Queued for transport." : decision.Reason;

        var dispatch = new RemoteCommandDispatch(command, decision, DateTime.UtcNow);
        _history[command.CommandId] = dispatch;
        return dispatch;
    }

    public ScheduledRemoteCommand Schedule(
        string atmId,
        string commandType,
        DateTime executeAtUtc,
        string payload = "",
        string role = "Admin")
    {
        var scheduled = new ScheduledRemoteCommand(
            Guid.NewGuid().ToString("N"),
            atmId,
            commandType,
            payload,
            role,
            executeAtUtc,
            DateTime.UtcNow);
        _schedule.Enqueue(scheduled);
        return scheduled;
    }

    public bool Complete(string commandId, bool success, string result)
    {
        if (!_history.TryGetValue(commandId, out var dispatch))
            return false;

        dispatch.Command.CompletedAtUtc = DateTime.UtcNow;
        dispatch.Command.Status = success ? RemoteCommandStatus.Completed : RemoteCommandStatus.Failed;
        dispatch.Command.Result = result;
        return true;
    }
}

public sealed class UnifiedClientServiceSupervisor
{
    private readonly ConcurrentDictionary<string, ClientServiceState> _states = new(StringComparer.OrdinalIgnoreCase);

    public UnifiedClientServiceSupervisor()
    {
        foreach (var service in DefaultServices)
            _states[service] = new ClientServiceState(service, ClientServiceStatus.Stopped, "Ready", DateTime.UtcNow);
    }

    public IReadOnlyList<ClientServiceState> Services =>
        _states.Values.OrderBy(service => service.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    public ClientServiceState Start(string serviceName, string detail = "Running")
    {
        return Update(serviceName, ClientServiceStatus.Running, detail);
    }

    public ClientServiceState Stop(string serviceName, string detail = "Stopped")
    {
        return Update(serviceName, ClientServiceStatus.Stopped, detail);
    }

    public ClientServiceState MarkFaulted(string serviceName, string detail)
    {
        return Update(serviceName, ClientServiceStatus.Faulted, detail);
    }

    public ClientServiceSupervisorReport BuildReport()
    {
        var services = Services;
        return new ClientServiceSupervisorReport(
            services.Count,
            services.Count(service => service.Status == ClientServiceStatus.Running),
            services.Count(service => service.Status == ClientServiceStatus.Faulted),
            services);
    }

    private ClientServiceState Update(string serviceName, ClientServiceStatus status, string detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        var state = new ClientServiceState(serviceName.Trim(), status, detail, DateTime.UtcNow);
        _states[serviceName] = state;
        return state;
    }

    private static readonly string[] DefaultServices =
    [
        "Agent Controller",
        "File Watcher",
        "Socket Data",
        "Socket Files",
        "Screenshot",
        "Remote Session Access",
        "Windows Startup",
        "Supabase Sync",
        "Network Monitor",
        "Time Sync",
        "Log Backup",
        "Journal Sync"
    ];
}

public enum ClientServiceStatus
{
    Stopped,
    Running,
    Faulted
}

public sealed record JournalStorageResult(
    string ATM_ID,
    string FileName,
    string StoragePath,
    long FileSize,
    string Checksum,
    DateTime ReceivedAtUtc,
    JournalEvidenceReport Evidence);

public sealed record JournalDeliveryReceipt(
    string TransferId,
    string ATM_ID,
    string ATM_Type,
    string FileName,
    string Category,
    string StoragePath,
    long FileSize,
    string Checksum,
    DateTime ReceivedAtUtc,
    bool Confirmed,
    string Detail);

public sealed record RemoteCommandDispatch(
    RemoteCommand Command,
    RemoteCommandPolicyDecision Policy,
    DateTime QueuedAtUtc);

public sealed record ScheduledRemoteCommand(
    string ScheduleId,
    string ATM_ID,
    string CommandType,
    string Payload,
    string Role,
    DateTime ExecuteAtUtc,
    DateTime CreatedAtUtc);

public sealed record ClientServiceState(
    string Name,
    ClientServiceStatus Status,
    string Detail,
    DateTime UpdatedAtUtc);

public sealed record ClientServiceSupervisorReport(
    int Total,
    int Running,
    int Faulted,
    IReadOnlyList<ClientServiceState> Services);
