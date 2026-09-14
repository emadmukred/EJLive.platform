using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

/// <summary>
/// Wave 5 — Smart content + value analysis pipeline (SS-27).
///
/// Re-sorts, re-organises, and prioritises uploaded journal / log /
/// configuration payloads across three concerns:
///   1. <b>Content analysis</b> — runs each line through the global XFS rule
///      library and emits a list of <see cref="SmartFinding"/> entries with
///      category, severity, recommended action, and the original snippet.
///   2. <b>Value analysis</b> — counts cassette moves, withdrawal / deposit
///      totals, error / fault density, and converts the journal into a
///      <see cref="JournalValueSummary"/> for dashboards.
///   3. <b>Re-sort / re-organise</b> — orders findings by severity, then
///      category, then ATM/trace id, and exposes helpers to bucket the
///      findings back into a category-by-category report.
///
/// All methods are pure (no file IO beyond optional <see cref="File.OpenText"/>)
/// so they can be called from the server-side REST endpoints, the
/// WinForms tab, the journal studio, or unit tests.
/// </summary>
public sealed class SmartAnalysisService
{
    private readonly XfsLogAnalysisService _xfs;

    public SmartAnalysisService(XfsLogAnalysisService? xfs = null)
    {
        _xfs = xfs ?? new XfsLogAnalysisService();
    }

    // -----------------------------------------------------------------
    // 1. Content analysis
    // -----------------------------------------------------------------

    /// <summary>
    /// Run XFS rule pipeline over an uploaded text payload (file or REST body)
    /// and return enriched findings.
    /// </summary>
    public SmartAnalysisReport AnalyzeUpload(
        string payload,
        string sourceLabel,
        string vendorHint = "",
        string traceId = "")
    {
        if (payload is null) throw new ArgumentNullException(nameof(payload));
        var lines = SplitLines(payload);

        var rawFindings = _xfs.AnalyzeOperationalFindings(lines, vendorHint);
        var findings = rawFindings
            .Select(f => new SmartFinding(
                Code: f.Code,
                Category: f.Category,
                Severity: f.Severity.ToString(),
                Vendor: f.Vendor,
                Message: f.Message,
                RecommendedAction: f.RecommendedAction,
                DetectedAtUtc: f.DetectedAtUtc,
                SourceLabel: sourceLabel))
            .ToList();

        var value = BuildValueSummary(lines, sourceLabel);

        var sorted = SortFindings(findings);

        return new SmartAnalysisReport(
            TraceId: string.IsNullOrEmpty(traceId) ? Guid.NewGuid().ToString("N")[..12] : traceId,
            SourceLabel: sourceLabel,
            VendorHint: vendorHint,
            LineCount: lines.Count,
            AnalyzedAtUtc: DateTime.UtcNow,
            Findings: sorted,
            Value: value,
            CategoryBreakdown: BuildCategoryBreakdown(sorted));
    }

    /// <summary>
    /// Read a file from disk and run <see cref="AnalyzeUpload"/>.
    /// </summary>
    public SmartAnalysisReport AnalyzeFile(
        string filePath,
        string vendorHint = "",
        string traceId = "")
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Upload target not found.", filePath);
        var text = File.ReadAllText(filePath);
        return AnalyzeUpload(text, filePath, vendorHint, traceId);
    }

    // -----------------------------------------------------------------
    // 2. Value analysis
    // -----------------------------------------------------------------

    private static JournalValueSummary BuildValueSummary(IReadOnlyList<string> lines, string source)
    {
        int withdrawals = 0, deposits = 0, faults = 0, retained = 0, paper = 0;
        decimal dispenseAmount = 0m, depositAmount = 0m;
        var cassetteMoves = new Dictionary<int, int>();
        var hourly = new Dictionary<int, int>();

        foreach (var raw in lines)
        {
            var line = raw ?? string.Empty;
            var upper = line.ToUpperInvariant();
            var ts = ExtractHour(line);
            if (ts >= 0) hourly[ts] = hourly.TryGetValue(ts, out var v) ? v + 1 : 1;

            if (upper.Contains("WITHDRAWAL") || upper.Contains("DISPENSE"))
            {
                withdrawals++;
                dispenseAmount += ExtractAmount(line);
            }
            if (upper.Contains("DEPOSIT") || upper.Contains("CASH IN"))
            {
                deposits++;
                depositAmount += ExtractAmount(line);
            }
            if (upper.Contains("ERROR") || upper.Contains("FAULT") || upper.Contains("JAM"))
                faults++;
            if (upper.Contains("CARD RETAINED") || upper.Contains("CARD CAPTURED"))
                retained++;
            if (upper.Contains("PAPER LOW") || upper.Contains("PAPER OUT"))
                paper++;

            var cassette = ExtractCassetteNoteCount(line);
            foreach (var kv in cassette)
                cassetteMoves[kv.Key] = (cassetteMoves.TryGetValue(kv.Key, out var c) ? c : 0) + kv.Value;
        }

        return new JournalValueSummary(
            Source: source,
            TotalLines: lines.Count,
            Withdrawals: withdrawals,
            Deposits: deposits,
            DispenseAmount: dispenseAmount,
            DepositAmount: depositAmount,
            Errors: faults,
            CardRetained: retained,
            PaperWarnings: paper,
            CassetteMoves: cassetteMoves,
            HourlyDensity: hourly);
    }

    // -----------------------------------------------------------------
    // 3. Re-sort / re-organise
    // -----------------------------------------------------------------

    public static List<SmartFinding> SortFindings(IReadOnlyList<SmartFinding> findings)
    {
        var severityRank = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Critical"] = 0,
            ["Warning"] = 1,
            ["Info"] = 2
        };
        return findings
            .OrderBy(f => severityRank.TryGetValue(f.Severity, out var r) ? r : 99)
            .ThenBy(f => f.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.SourceLabel, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static Dictionary<string, List<SmartFinding>> GroupByCategory(IReadOnlyList<SmartFinding> findings)
    {
        var groups = new Dictionary<string, List<SmartFinding>>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in findings)
        {
            if (!groups.TryGetValue(f.Category, out var list))
                groups[f.Category] = list = new List<SmartFinding>();
            list.Add(f);
        }
        foreach (var key in groups.Keys.ToList())
            groups[key] = SortFindings(groups[key]);
        return groups;
    }

    private static Dictionary<string, int> BuildCategoryBreakdown(IReadOnlyList<SmartFinding> sorted)
    {
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in sorted)
        {
            var key = $"{f.Severity}:{f.Category}";
            dict[key] = dict.TryGetValue(key, out var v) ? v + 1 : 1;
        }
        return dict;
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    private static IReadOnlyList<string> SplitLines(string payload)
        => payload.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

    private static readonly Regex AmountRegex = new(
        @"(?:AMT|AMOUNT|EUR|USD|SAR|GBP|VALUE)\s*[:=]?\s*([0-9]+(?:[\.,][0-9]{1,2})?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static decimal ExtractAmount(string line)
    {
        var m = AmountRegex.Match(line ?? string.Empty);
        if (!m.Success) return 0m;
        var raw = m.Groups[1].Value.Replace(',', '.');
        return decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0m;
    }

    private static readonly Regex CassetteRegex = new(
        @"(?:CAS|CASSETTE|CS)\s*([1-4])[^0-9]+([0-9]{1,4})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static IEnumerable<KeyValuePair<int, int>> ExtractCassetteNoteCount(string line)
    {
        foreach (Match m in CassetteRegex.Matches(line ?? string.Empty))
        {
            if (int.TryParse(m.Groups[1].Value, out var slot) &&
                int.TryParse(m.Groups[2].Value, out var count))
                yield return new KeyValuePair<int, int>(slot, count);
        }
    }

    private static readonly Regex HourRegex = new(
        @"\b([01]?\d|2[0-3])[:.]([0-5]\d)\b",
        RegexOptions.Compiled);

    private static int ExtractHour(string line)
    {
        var m = HourRegex.Match(line ?? string.Empty);
        if (!m.Success) return -1;
        return int.TryParse(m.Groups[1].Value, out var h) ? h : -1;
    }
}

/// <summary>A single enriched XFS finding with trace + source metadata.</summary>
public sealed record SmartFinding(
    string Code,
    string Category,
    string Severity,
    string Vendor,
    string Message,
    string RecommendedAction,
    DateTime DetectedAtUtc,
    string SourceLabel);

/// <summary>
/// Aggregated counts and totals lifted from an uploaded journal payload.
/// Pure data — no UI types, no file handles.
/// </summary>
public sealed record JournalValueSummary(
    string Source,
    int TotalLines,
    int Withdrawals,
    int Deposits,
    decimal DispenseAmount,
    decimal DepositAmount,
    int Errors,
    int CardRetained,
    int PaperWarnings,
    Dictionary<int, int> CassetteMoves,
    Dictionary<int, int> HourlyDensity);

/// <summary>
/// Full analysis result returned to the UI / REST layer — findings sorted by
/// severity then category, plus the value summary and a category breakdown
/// that the dashboard can chart.
/// </summary>
public sealed record SmartAnalysisReport(
    string TraceId,
    string SourceLabel,
    string VendorHint,
    int LineCount,
    DateTime AnalyzedAtUtc,
    IReadOnlyList<SmartFinding> Findings,
    JournalValueSummary Value,
    IReadOnlyDictionary<string, int> CategoryBreakdown)
{
    public int CriticalCount => Findings.Count(f => string.Equals(f.Severity, "Critical", StringComparison.OrdinalIgnoreCase));
    public int WarningCount => Findings.Count(f => string.Equals(f.Severity, "Warning", StringComparison.OrdinalIgnoreCase));
    public int InfoCount => Findings.Count(f => string.Equals(f.Severity, "Info", StringComparison.OrdinalIgnoreCase));
}
