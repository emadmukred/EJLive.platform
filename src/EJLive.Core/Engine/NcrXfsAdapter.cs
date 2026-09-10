using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// NCR-specific XFS vendor adapter that normalizes NCR XFS trace logs
/// into vendor-neutral <see cref="NormalizedVendorEvent"/> records.
/// </summary>
public sealed class NcrXfsAdapter : ICorrelationEventAdapter
{
    private static readonly Regex TimestampRegex = new(
        @"^(\d{2}:\d{2}:\d{2})\s+",
        RegexOptions.Compiled);

    private static readonly Regex EventCodeRegex = new(
        @"\b([A-Z]{2,10}[-_]?\d{3,6})\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StanRegex = new(
        @"STAN\s*[:=]?\s*(\d{1,12})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RrnRegex = new(
        @"RRN\s*[:=]?\s*(\d{1,12})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TransferIdRegex = new(
        @"TRANSFER\s*ID\s*[:=]?\s*([A-Z0-9\-]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TxnNumRegex = new(
        @"TRANSACTION\s*(?:NUMBER|NUM|NO)?\s*[:=]?\s*(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DeviceClassRegex = new(
        @"\b(CashDispenser|CardReader|ReceiptPrinter|JournalPrinter|PinPad|Status|Sensor|Depository|CheckReader)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CassetteRegex = new(
        @"CASS?ETTE\s*(\d)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SeverityRegex = new(
        @"\b(INFO|WARN|WARNING|ERROR|ERR|FATAL|DEBUG|TRACE)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SessionIdRegex = new(
        @"SESSION\s*ID\s*[:=]?\s*(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AtmIdRegex = new(
        @"ATM[_\-]?ID\s*[:=]?\s*([A-Z0-9\-]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public bool CanHandle(string sourceFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        string name = Path.GetFileName(sourceFile).ToUpperInvariant();
        return name.Contains("NCR", StringComparison.Ordinal) ||
               name.Contains("XFS", StringComparison.Ordinal) ||
               name.EndsWith(".NTR", StringComparison.Ordinal) ||
               name.EndsWith(".TRC", StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public List<NormalizedVendorEvent> Parse(string sourceFile, List<string> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFile);
        ArgumentNullException.ThrowIfNull(lines);

        var events = new List<NormalizedVendorEvent>();
        string inferredAtmId = InferAtmId(sourceFile, lines);
        DateTime? fileBaseDate = InferBaseDate(sourceFile);

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string upper = line.ToUpperInvariant();
            DateTime? ts = ExtractTimestamp(line, fileBaseDate);
            string severity = ExtractSeverity(upper) ?? "INFO";
            string deviceClass = ExtractDeviceClass(line) ?? "Unknown";
            string code = ExtractEventCode(line) ?? "NCR-UNKNOWN";
            string message = line.Trim();

            // Extract correlation candidates
            string stan = ExtractFirstMatch(StanRegex, line) ?? string.Empty;
            string rrn = ExtractFirstMatch(RrnRegex, line) ?? string.Empty;
            string transferId = ExtractFirstMatch(TransferIdRegex, line) ?? string.Empty;
            string txnNumber = ExtractFirstMatch(TxnNumRegex, line) ?? string.Empty;
            string sessionId = ExtractFirstMatch(SessionIdRegex, line) ?? string.Empty;
            string cassette = ExtractFirstMatch(CassetteRegex, line) ?? string.Empty;

            double confidence = 0.85;
            if (string.IsNullOrEmpty(stan) && string.IsNullOrEmpty(rrn) && string.IsNullOrEmpty(transferId) && string.IsNullOrEmpty(txnNumber))
                confidence = 0.70;

            string eventId = $"{inferredAtmId}-{i:D6}";

            var evt = new NormalizedVendorEvent(
                EventId: eventId,
                ATM_ID: inferredAtmId,
                Vendor: "NCR",
                DeviceClass: deviceClass,
                Severity: severity,
                Code: code,
                Message: message,
                Timestamp: ts ?? DateTime.UtcNow,
                RawLine: line,
                SourceFile: sourceFile,
                ConfidenceScore: confidence,
                ImpactedTransactionId: string.Empty,
                CorrelationReason: string.Empty,
                FalsePositiveRisk: 0.50,
                OperatorExplanation: $"NCR {deviceClass} event ({code}) at {ts?.ToString("HH:mm:ss") ?? "unknown"}."
            );

            events.Add(evt);
        }

        return events;
    }

    private static string InferAtmId(string sourceFile, List<string> lines)
    {
        // Try to find ATM_ID in first 20 lines
        foreach (var line in lines.Take(20))
        {
            var match = AtmIdRegex.Match(line);
            if (match.Success)
                return match.Groups[1].Value.Trim();
        }

        // Fallback to filename
        string fileName = Path.GetFileNameWithoutExtension(sourceFile);
        if (fileName.Length > 3)
            return fileName;

        return "UNKNOWN-ATM";
    }

    private static DateTime? InferBaseDate(string sourceFile)
    {
        string fileName = Path.GetFileNameWithoutExtension(sourceFile);
        // Attempt to parse date from filename patterns like ATM_20240522 or ATM_2024-05-22
        var match = Regex.Match(fileName, @"(\d{4})(\d{2})(\d{2})");
        if (match.Success)
        {
            if (DateTime.TryParseExact(match.Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt;
        }
        return null;
    }

    private static DateTime? ExtractTimestamp(string line, DateTime? baseDate)
    {
        var match = TimestampRegex.Match(line);
        if (match.Success)
        {
            string timePart = match.Groups[1].Value;
            if (TimeSpan.TryParse(timePart, out TimeSpan ts))
            {
                DateTime baseDt = baseDate?.Date ?? DateTime.UtcNow.Date;
                return baseDt.Add(ts);
            }
        }
        return null;
    }

    private static string? ExtractSeverity(string upper)
    {
        var match = SeverityRegex.Match(upper);
        if (match.Success)
        {
            string sev = match.Groups[1].Value.ToUpperInvariant();
            return sev switch
            {
                "WARN" => "WARNING",
                "ERR" => "ERROR",
                _ => sev
            };
        }
        return null;
    }

    private static string? ExtractDeviceClass(string upper)
    {
        var match = DeviceClassRegex.Match(upper);
        if (match.Success)
            return match.Groups[1].Value;
        return null;
    }

    private static string? ExtractEventCode(string line)
    {
        var match = EventCodeRegex.Match(line);
        if (match.Success)
            return match.Groups[1].Value;
        return null;
    }

    private static string? ExtractFirstMatch(Regex regex, string line)
    {
        var match = regex.Match(line);
        if (match.Success)
            return match.Groups[1].Value.Trim();
        return null;
    }
}
