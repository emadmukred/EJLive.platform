using System.Text.RegularExpressions;

namespace EJLive.Core.Engine;

/// <summary>
/// Parses cash distribution and cassette denomination/count data from all ATM vendor formats.
/// Provides a unified view of dispensed notes regardless of vendor.
/// </summary>
public sealed class CashDistributionParser
{
    private static readonly Regex NcrDistRegex = new(@"DIST\s*CASH\s*[:=]?\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex NcrCassetteRegex = new(@"CASS?ETTE?\s*(\d)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex GrgCassetteRegex = new(@"(?:CDM|CASS)\s*(\d)\s*(?:DENOM\s*[:=]?\s*(\d+))?\s*(?:COUNT|QTY)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex WincorLcuRegex = new(@"LCU\s*(\d)\s*[:=]?\s*(?:DENOM\s*[:=]?\s*)?(\d+)\s*(?:X|COUNT)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex GenericDenomRegex = new(@"(\d+)\s*[xX*]\s*(\d+)", RegexOptions.Compiled);

    /// <summary>
    /// Parses cash distribution from EJ lines for the specified vendor type.
    /// </summary>
    /// <param name="lines">Raw EJ lines containing cassette/distribution data.</param>
    /// <param name="vendorType">The ATM vendor type (NCR, GRG, Wincor, Diebold, Hyosung).</param>
    /// <returns>A <see cref="CashDistributionResult"/> with per-cassette breakdown.</returns>
    public CashDistributionResult Parse(IReadOnlyList<string> lines, string? vendorType)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var cassettes = new List<CassetteDispense>();
        var vendor = (vendorType ?? string.Empty).ToUpperInvariant();

        foreach (var line in lines.Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            var parsed = vendor switch
            {
                "NCR" => ParseNcrLine(line),
                "GRG" => ParseGrgLine(line),
                "WINCOR" or "WN" => ParseWincorLine(line),
                _ => ParseGenericLine(line)
            };

            if (parsed != null)
                cassettes.AddRange(parsed);
        }

        var merged = MergeCassetteReadings(cassettes);

        var totalAmount = merged.Aggregate(0L, (total, cassette) =>
            checked(total + checked((long)(cassette.Denomination ?? 0) * cassette.NotesDispensed)));
        var totalNotes = merged.Aggregate(0, (total, cassette) => checked(total + cassette.NotesDispensed));

        return new CashDistributionResult(merged, totalNotes, totalAmount);
    }

    private static List<CassetteDispense> MergeCassetteReadings(IEnumerable<CassetteDispense> readings)
    {
        var merged = new List<CassetteDispense>();
        foreach (var cassetteGroup in readings.GroupBy(reading => reading.CassetteNumber))
        {
            var knownGroups = cassetteGroup
                .Where(reading => reading.Denomination.HasValue)
                .GroupBy(reading => reading.Denomination!.Value)
                .OrderBy(group => group.Key)
                .ToArray();
            var unknownCount = cassetteGroup
                .Where(reading => !reading.Denomination.HasValue)
                .Sum(reading => reading.NotesDispensed);

            if (knownGroups.Length == 0)
            {
                merged.Add(new CassetteDispense(cassetteGroup.Key, null, unknownCount));
                continue;
            }

            for (var index = 0; index < knownGroups.Length; index++)
            {
                var group = knownGroups[index];
                var count = group.Sum(reading => reading.NotesDispensed);
                if (index == 0)
                    count = checked(count + unknownCount);
                merged.Add(new CassetteDispense(cassetteGroup.Key, group.Key, count));
            }
        }

        return merged
            .OrderBy(reading => reading.CassetteNumber)
            .ThenBy(reading => reading.Denomination)
            .ToList();
    }

    private static List<CassetteDispense>? ParseNcrLine(string line)
    {
        var results = new List<CassetteDispense>();

        // NCR cassette format: CASSETTE 1 = 5
        var matches = NcrCassetteRegex.Matches(line);
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out int cassNum) &&
                int.TryParse(match.Groups[2].Value, out int count))
            {
                results.Add(new CassetteDispense(cassNum, null, count));
            }
        }

        // NCR DIST CASH format: "DIST CASH: 5x100 3x50"
        var distMatch = NcrDistRegex.Match(line);
        if (distMatch.Success)
        {
            var denomMatches = GenericDenomRegex.Matches(distMatch.Groups[1].Value);
            int cassNum = 1;
            foreach (Match dm in denomMatches)
            {
                if (int.TryParse(dm.Groups[1].Value, out int count) &&
                    int.TryParse(dm.Groups[2].Value, out int denom))
                {
                    results.Add(new CassetteDispense(cassNum++, denom, count));
                }
            }
        }

        return results.Count > 0 ? results : null;
    }

    private static List<CassetteDispense>? ParseGrgLine(string line)
    {
        var results = new List<CassetteDispense>();
        var matches = GrgCassetteRegex.Matches(line);
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out int cassNum) &&
                int.TryParse(match.Groups[3].Value, out int count))
            {
                int? denom = int.TryParse(match.Groups[2].Value, out int d) ? d : null;
                results.Add(new CassetteDispense(cassNum, denom, count));
            }
        }
        return results.Count > 0 ? results : null;
    }

    private static List<CassetteDispense>? ParseWincorLine(string line)
    {
        var results = new List<CassetteDispense>();
        var matches = WincorLcuRegex.Matches(line);
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out int cassNum) &&
                int.TryParse(match.Groups[2].Value, out int denom) &&
                int.TryParse(match.Groups[3].Value, out int count))
            {
                results.Add(new CassetteDispense(cassNum, denom, count));
            }
        }
        return results.Count > 0 ? results : null;
    }

    private static List<CassetteDispense>? ParseGenericLine(string line)
    {
        var results = new List<CassetteDispense>();

        // Try generic cassette pattern
        var cassMatches = NcrCassetteRegex.Matches(line);
        int cassNum = 1;
        foreach (Match match in cassMatches)
        {
            if (int.TryParse(match.Groups[1].Value, out int num) &&
                int.TryParse(match.Groups[2].Value, out int count))
            {
                results.Add(new CassetteDispense(num, null, count));
                cassNum = num + 1;
            }
        }

        // Try denomination x count pattern
        if (results.Count == 0)
        {
            var denomMatches = GenericDenomRegex.Matches(line);
            foreach (Match dm in denomMatches)
            {
                if (int.TryParse(dm.Groups[1].Value, out int count) &&
                    int.TryParse(dm.Groups[2].Value, out int denom))
                {
                    results.Add(new CassetteDispense(cassNum++, denom, count));
                }
            }
        }

        return results.Count > 0 ? results : null;
    }
}

/// <summary>
/// Represents the parsed cash distribution result for a transaction.
/// </summary>
/// <param name="Cassettes">Per-cassette breakdown of dispensed notes.</param>
/// <param name="TotalNotes">Total number of notes dispensed across all cassettes.</param>
/// <param name="TotalAmount">Total monetary amount dispensed (denomination * count sum).</param>
public sealed record CashDistributionResult(
    IReadOnlyList<CassetteDispense> Cassettes,
    int TotalNotes,
    long TotalAmount);

/// <summary>
/// Represents a single cassette's dispense record.
/// </summary>
/// <param name="CassetteNumber">The physical cassette number (1-based).</param>
/// <param name="Denomination">The note denomination value, if known.</param>
/// <param name="NotesDispensed">The number of notes dispensed from this cassette.</param>
public sealed record CassetteDispense(
    int CassetteNumber,
    int? Denomination,
    int NotesDispensed);
