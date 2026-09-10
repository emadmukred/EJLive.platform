using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class UnifiedJournalEvidenceAnalyzer
{
    private static readonly Regex AmountPattern = new(
        @"(?i)\b(?:AMOUNT|CASH|DISPENSED|SAR)\s*[:=]?\s*(?<amount>[0-9]{2,7}(?:\.[0-9]{1,2})?)\b",
        RegexOptions.Compiled);

    public JournalEvidenceReport Analyze(string atmId, string atmType, string journalText)
    {
        var normalizedVendor = DetectVendor(atmType, journalText);
        var signals = new List<JournalSignal>();
        var lines = SplitLines(journalText);

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            foreach (var signal in DetectSignals(atmId, normalizedVendor, line, index + 1))
                signals.Add(signal);
        }

        var findings = BuildFindings(lines.Length, normalizedVendor, signals);
        var approved = signals.Count(signal => signal.Kind == JournalSignalKind.Approved);
        var declined = signals.Count(signal => signal.Kind == JournalSignalKind.Declined);
        var captured = signals.Count(signal => signal.Kind == JournalSignalKind.CardCapture);
        var cashErrors = signals.Count(signal => signal.Kind == JournalSignalKind.CashError);
        var totalCash = signals.Sum(signal => signal.Amount ?? 0m);

        return new JournalEvidenceReport(
            atmId,
            normalizedVendor,
            lines.Length,
            approved,
            declined,
            captured,
            cashErrors,
            totalCash,
            signals,
            findings);
    }

    public static string DetectVendor(string? atmType, string? text)
    {
        var candidate = AppConstants.NormalizeATMType(atmType);
        var evidence = (text ?? string.Empty).ToUpperInvariant();

        if (evidence.Contains("APTRA") || evidence.Contains("NCR") || evidence.Contains("EJDATA"))
            return AppConstants.ATM_TYPE_NCR;
        if (evidence.Contains("GRG") || evidence.Contains("YDC") || evidence.Contains("DTATMW"))
            return AppConstants.ATM_TYPE_GRG;
        if (evidence.Contains("WINCOR") || evidence.Contains("PROCASH") || evidence.Contains("DIEBOLD NIXDORF"))
            return AppConstants.ATM_TYPE_WN;
        if (evidence.Contains("DIEBOLD") || evidence.Contains("MDS"))
            return AppConstants.ATM_TYPE_DN;
        if (evidence.Contains("HYOSUNG") || evidence.Contains("NAUTILUS"))
            return AppConstants.ATM_TYPE_HY;

        return candidate;
    }

    private static string[] SplitLines(string text)
    {
        return (text ?? string.Empty)
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();
    }

    private static IEnumerable<JournalSignal> DetectSignals(string atmId, string vendor, string line, int lineNumber)
    {
        if (ContainsAny(line, "APPROVED", "SUCCESS", "NOTES DISPENSED", "CASH DISPENSED", "DISPENSE SUCCESS"))
            yield return BuildSignal(atmId, vendor, line, lineNumber, JournalSignalKind.Approved);

        if (ContainsAny(line, "DECLIN", "DENIED", "REJECTED", "UNABLE", "FAILED TRANSACTION"))
            yield return BuildSignal(atmId, vendor, line, lineNumber, JournalSignalKind.Declined);

        if (ContainsAny(line, "CARD CAPTURE", "CARD CAPTURED", "RETAIN", "RETAINED"))
            yield return BuildSignal(atmId, vendor, line, lineNumber, JournalSignalKind.CardCapture);

        if (ContainsAny(line, "CASH ERROR", "DISPENSE ERROR", "M-02", "M-03", "M-05", "M-10", "M-11", "M-18", "ERROR E3"))
            yield return BuildSignal(atmId, vendor, line, lineNumber, JournalSignalKind.CashError);

        if (ContainsAny(line, "POWER UP", "STARTUP", "RESTART", "RESET"))
            yield return BuildSignal(atmId, vendor, line, lineNumber, JournalSignalKind.PowerReset);

        if (ContainsAny(line, "SUPERVISOR", "OPERATOR"))
            yield return BuildSignal(atmId, vendor, line, lineNumber, JournalSignalKind.Supervisor);

        if (ContainsAny(line, "XFS", "WFS", "SPERROR", "DEVICE ERROR", "FAULT"))
            yield return BuildSignal(atmId, vendor, line, lineNumber, JournalSignalKind.XfsEvent);

        if (ContainsAny(line, "HOST", "NDC", "MESSAGE IN", "MESSAGE OUT"))
            yield return BuildSignal(atmId, vendor, line, lineNumber, JournalSignalKind.HostMessage);
    }

    private static JournalSignal BuildSignal(string atmId, string vendor, string line, int lineNumber, JournalSignalKind kind)
    {
        return new JournalSignal(atmId, vendor, kind, lineNumber, line, ExtractAmount(line));
    }

    private static decimal? ExtractAmount(string line)
    {
        var match = AmountPattern.Match(line ?? string.Empty);
        return match.Success && decimal.TryParse(match.Groups["amount"].Value, out var amount)
            ? amount
            : null;
    }

    private static IReadOnlyList<OperationalFinding> BuildFindings(int lineCount, string vendor, IReadOnlyList<JournalSignal> signals)
    {
        var findings = new List<OperationalFinding>();
        if (lineCount == 0)
            findings.Add(new OperationalFinding(OperationalFindingSeverity.Warning, "Journal", "No journal lines were available for analysis."));

        if (signals.Any(signal => signal.Kind == JournalSignalKind.CashError))
            findings.Add(new OperationalFinding(OperationalFindingSeverity.Critical, vendor, "Cash or dispenser error evidence was detected."));

        if (signals.Any(signal => signal.Kind == JournalSignalKind.CardCapture))
            findings.Add(new OperationalFinding(OperationalFindingSeverity.Warning, vendor, "Card capture or retention evidence was detected."));

        if (signals.Any(signal => signal.Kind == JournalSignalKind.XfsEvent))
            findings.Add(new OperationalFinding(OperationalFindingSeverity.Warning, vendor, "XFS or device-layer events were detected."));

        if (signals.Count > 0 && findings.Count == 0)
            findings.Add(new OperationalFinding(OperationalFindingSeverity.Info, vendor, "Journal evidence parsed successfully."));

        return findings;
    }

    private static bool ContainsAny(string text, params string[] patterns)
    {
        return patterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class UnifiedRemoteCommandPolicy
{
    public RemoteCommandPolicyDecision Evaluate(RemoteCommand command, string role, bool operatorConfirmed, bool maintenanceWindow)
    {
        var type = command.CommandType ?? string.Empty;
        var risk = GetRisk(type);
        var requiresConfirmation = command.RequiresConfirmation ||
                                   risk is RemoteCommandRisk.High or RemoteCommandRisk.Critical ||
                                   AppConstants.CommandsRequireConfirmation.Contains(type, StringComparer.OrdinalIgnoreCase);
        var requiresMaintenance = risk == RemoteCommandRisk.Critical;
        var hasRole = CanRoleExecute(role, risk);

        if (risk == RemoteCommandRisk.Unknown)
            return new RemoteCommandPolicyDecision(false, risk, "Command type is not allowlisted.", true, true);

        if (!hasRole)
            return new RemoteCommandPolicyDecision(false, risk, "Role is not allowed to execute this command.", requiresConfirmation, requiresMaintenance);

        if (requiresConfirmation && !operatorConfirmed)
            return new RemoteCommandPolicyDecision(false, risk, "Operator confirmation is required.", true, requiresMaintenance);

        if (requiresMaintenance && !maintenanceWindow)
            return new RemoteCommandPolicyDecision(false, risk, "Maintenance window is required for destructive commands.", requiresConfirmation, true);

        return new RemoteCommandPolicyDecision(true, risk, "Command accepted by unified policy.", requiresConfirmation, requiresMaintenance);
    }

    private static RemoteCommandRisk GetRisk(string commandType)
    {
        if (string.Equals(commandType, AppConstants.CMD_SHUTDOWN, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_CHANGE_PASSWORD, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_RESTART, StringComparison.OrdinalIgnoreCase))
            return RemoteCommandRisk.Critical;

        if (string.Equals(commandType, AppConstants.CMD_REMOTE_SESSION_START, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_REMOTE_CONFIG, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_WINDOWS_REMOTE_START, StringComparison.OrdinalIgnoreCase))
            return RemoteCommandRisk.High;

        if (string.Equals(commandType, AppConstants.CMD_SCREENSHOT, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_FORCE_SYNC, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_SEND_FILE, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_GET_FILE, StringComparison.OrdinalIgnoreCase))
            return RemoteCommandRisk.Medium;

        if (string.Equals(commandType, AppConstants.CMD_PING, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_GET_STATS, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_SYNC_TIME, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(commandType, AppConstants.CMD_WINDOWS_REMOTE_CHECK, StringComparison.OrdinalIgnoreCase))
            return RemoteCommandRisk.Low;

        return RemoteCommandRisk.Unknown;
    }

    private static bool CanRoleExecute(string role, RemoteCommandRisk risk)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(role, "Support", StringComparison.OrdinalIgnoreCase))
            return risk is RemoteCommandRisk.Low or RemoteCommandRisk.Medium;

        if (string.Equals(role, "Auditor", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Observer", StringComparison.OrdinalIgnoreCase))
            return risk == RemoteCommandRisk.Low;

        return false;
    }
}

public sealed class UnifiedFleetReadinessService
{
    public FleetReadinessAssessment Assess(IEnumerable<ATMInfo> atms, IEnumerable<JournalSyncRecord> syncRecords, DateTime? nowUtc = null)
    {
        var now = nowUtc ?? DateTime.UtcNow;
        var atmList = (atms ?? Array.Empty<ATMInfo>()).ToArray();
        var syncList = (syncRecords ?? Array.Empty<JournalSyncRecord>()).ToArray();
        var findings = new List<OperationalFinding>();

        foreach (var atm in atmList)
        {
            atm.RecalculateHealthScore();
            var atmId = atm.ATM_ID ?? atm.ATMId ?? "UNKNOWN";
            var heartbeatAge = atm.LastHeartbeatUtc == DateTime.MinValue
                ? TimeSpan.MaxValue
                : now - atm.LastHeartbeatUtc;
            var dataAge = atm.LastDataReceivedUtc == DateTime.MinValue
                ? TimeSpan.MaxValue
                : now - atm.LastDataReceivedUtc;

            if (atm.ConnectionStatus == ConnectionStatus.Disconnected || heartbeatAge.TotalMinutes >= AppConstants.AlertDisconnectCriticalMin)
                findings.Add(new OperationalFinding(OperationalFindingSeverity.Critical, atmId, "ATM is disconnected or heartbeat is stale."));
            else if (heartbeatAge.TotalMinutes >= AppConstants.AlertDisconnectWarningMin)
                findings.Add(new OperationalFinding(OperationalFindingSeverity.Warning, atmId, "ATM heartbeat is approaching the warning threshold."));

            if (dataAge.TotalMinutes >= AppConstants.AlertNoDataCriticalMin)
                findings.Add(new OperationalFinding(OperationalFindingSeverity.Critical, atmId, "No journal data has been received for the critical threshold."));
            else if (dataAge.TotalMinutes >= AppConstants.AlertNoDataWarningMin)
                findings.Add(new OperationalFinding(OperationalFindingSeverity.Warning, atmId, "No journal data has been received for the warning threshold."));

            if (syncList.Any(record => string.Equals(record.ATM_ID, atmId, StringComparison.OrdinalIgnoreCase) &&
                                       record.State == JournalSyncState.Failed))
                findings.Add(new OperationalFinding(OperationalFindingSeverity.Warning, atmId, "Failed journal sync records require retry."));
        }

        var fleet = new FleetSummary
        {
            Total = atmList.Length,
            Connected = atmList.Count(atm => atm.ConnectionStatus is ConnectionStatus.Connected or ConnectionStatus.Syncing or ConnectionStatus.WaitingReply),
            Syncing = atmList.Count(atm => atm.ConnectionStatus == ConnectionStatus.Syncing || atm.SyncState is SyncStatus.Syncing or SyncStatus.InProgress or SyncStatus.Resyncing),
            Offline = atmList.Count(atm => atm.ConnectionStatus == ConnectionStatus.Disconnected || atm.Status is ATMStatus.Offline or ATMStatus.CriticalFault),
            AverageHealth = atmList.Length == 0 ? 0 : (int)Math.Round(atmList.Average(atm => atm.HealthScore))
        };

        return new FleetReadinessAssessment(
            fleet,
            syncList.Count(record => record.State == JournalSyncState.Failed),
            findings.Count(finding => finding.Severity >= OperationalFindingSeverity.Warning),
            GetOverallSeverity(findings),
            findings);
    }

    private static OperationalFindingSeverity GetOverallSeverity(IReadOnlyList<OperationalFinding> findings)
    {
        if (findings.Any(finding => finding.Severity == OperationalFindingSeverity.Critical))
            return OperationalFindingSeverity.Critical;
        if (findings.Any(finding => finding.Severity == OperationalFindingSeverity.Warning))
            return OperationalFindingSeverity.Warning;
        return OperationalFindingSeverity.Info;
    }
}

public sealed class UnifiedOperationalFusionService
{
    private readonly UnifiedJournalEvidenceAnalyzer _journalAnalyzer = new();
    private readonly UnifiedRemoteCommandPolicy _commandPolicy = new();
    private readonly UnifiedFleetReadinessService _fleetReadiness = new();

    public UnifiedOperationalFusionSnapshot Build(
        IEnumerable<ATMInfo> atms,
        IEnumerable<JournalSyncRecord> syncRecords,
        string journalText,
        RemoteCommand? command = null,
        string role = "Admin",
        bool operatorConfirmed = true,
        bool maintenanceWindow = true)
    {
        var atmList = (atms ?? Array.Empty<ATMInfo>()).ToArray();
        var primaryAtm = atmList.FirstOrDefault();
        var journal = _journalAnalyzer.Analyze(
            primaryAtm?.ATM_ID ?? "UNKNOWN",
            primaryAtm?.ATM_Type ?? AppConstants.ATM_TYPE_NCR,
            journalText ?? string.Empty);
        var fleet = _fleetReadiness.Assess(atmList, syncRecords ?? Array.Empty<JournalSyncRecord>());
        var commandDecision = command is null
            ? null
            : _commandPolicy.Evaluate(command, role, operatorConfirmed, maintenanceWindow);

        return new UnifiedOperationalFusionSnapshot(journal, fleet, commandDecision);
    }
}

public enum JournalSignalKind
{
    Approved,
    Declined,
    CardCapture,
    CashError,
    PowerReset,
    Supervisor,
    XfsEvent,
    HostMessage
}

public enum OperationalFindingSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2
}

public enum RemoteCommandRisk
{
    Unknown = -1,
    Low,
    Medium,
    High,
    Critical
}

public sealed record JournalSignal(
    string AtmId,
    string Vendor,
    JournalSignalKind Kind,
    int LineNumber,
    string RawLine,
    decimal? Amount);

public sealed record OperationalFinding(
    OperationalFindingSeverity Severity,
    string Source,
    string Message);

public sealed record JournalEvidenceReport(
    string AtmId,
    string Vendor,
    int LineCount,
    int ApprovedTransactions,
    int DeclinedTransactions,
    int CapturedCards,
    int CashErrorEvents,
    decimal TotalCashDispensed,
    IReadOnlyList<JournalSignal> Signals,
    IReadOnlyList<OperationalFinding> Findings);

public sealed record FleetReadinessAssessment(
    FleetSummary Summary,
    int FailedSyncRecords,
    int AttentionItems,
    OperationalFindingSeverity OverallSeverity,
    IReadOnlyList<OperationalFinding> Findings);

public sealed record RemoteCommandPolicyDecision(
    bool Allowed,
    RemoteCommandRisk Risk,
    string Reason,
    bool RequiresConfirmation,
    bool RequiresMaintenanceWindow);

public sealed record UnifiedOperationalFusionSnapshot(
    JournalEvidenceReport JournalEvidence,
    FleetReadinessAssessment FleetReadiness,
    RemoteCommandPolicyDecision? CommandPolicy);
