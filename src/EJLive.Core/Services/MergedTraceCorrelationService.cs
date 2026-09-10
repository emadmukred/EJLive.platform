using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;
using EJLive.Core.Xfs;

namespace EJLive.Core.Services;

public sealed class MergedTraceCorrelationService
{
    public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
    {
        return (hostMessages ?? Array.Empty<string>())
            .Concat(xfsEvents ?? Array.Empty<string>())
            .OrderBy(line => line, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

/// <summary>
/// Builds a chronological NCR trace from normalized events emitted by separate
/// journal, host-transport, XFS, driver, and diagnostic sources.
/// </summary>
public sealed class NcrMergedTraceCorrelationService
{
    private static readonly Regex TimestampPattern = new(
        @"(?<d1>\d{2})[-/](?<m1>\d{2})[-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public NcrMergedTraceCorrelationResult Correlate(
        IEnumerable<(string SourceName, IReadOnlyList<XfsNormalizedEvent> Events, int RawLines)> sources)
    {
        var result = new NcrMergedTraceCorrelationResult();

        foreach (var source in sources ?? [])
        {
            result.TotalRawLines += Math.Max(0, source.RawLines);
            foreach (var item in source.Events ?? [])
            {
                var data = new Dictionary<string, string>(item.Attributes, StringComparer.OrdinalIgnoreCase);
                foreach (var pair in item.Data)
                    data[pair.Key] = pair.Value;

                result.Timeline.Add(new NcrMergedTraceEvent
                {
                    Timestamp = item.Timestamp ?? TryExtractTimestamp(item.RawLine) ?? NormalizeOccurredAt(item.OccurredAtUtc),
                    SourceName = source.SourceName?.Trim() ?? string.Empty,
                    SourceLayer = item.SourceLayer,
                    Kind = item.Kind,
                    Severity = item.Severity,
                    DeviceFamily = item.DeviceFamily,
                    Title = item.Title,
                    Message = item.Message,
                    RawLine = item.RawLine,
                    Data = data
                });
            }
        }

        result.Timeline = result.Timeline
            .OrderBy(item => item.Timestamp ?? DateTime.MaxValue)
            .ThenBy(item => item.SourceName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        result.TotalEvents = result.Timeline.Count;
        result.BySourceLayer = CountBy(result.Timeline, item => item.SourceLayer.ToString());
        result.ByKind = CountBy(result.Timeline, item => item.Kind.ToString());
        return result;
    }

    private static Dictionary<string, int> CountBy(
        IEnumerable<NcrMergedTraceEvent> events,
        Func<NcrMergedTraceEvent, string> selector)
    {
        return events
            .GroupBy(selector, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
    }

    private static DateTime? NormalizeOccurredAt(DateTime value) =>
        value == default ? null : value;

    private static DateTime? TryExtractTimestamp(string rawLine)
    {
        if (string.IsNullOrWhiteSpace(rawLine))
            return null;

        var match = TimestampPattern.Match(rawLine);
        if (!match.Success)
            return null;

        if (match.Groups["y2"].Success)
        {
            var text = string.Format(
                CultureInfo.InvariantCulture,
                "{0}-{1}-{2} {3}",
                match.Groups["y2"].Value,
                match.Groups["m2"].Value,
                match.Groups["d2"].Value,
                match.Groups["t2"].Value);
            return ParseTimestamp(text, "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss");
        }

        var year = match.Groups["y1"].Value;
        if (year.Length == 2)
            year = "20" + year;

        var europeanText = string.Format(
            CultureInfo.InvariantCulture,
            "{0}/{1}/{2} {3}",
            match.Groups["d1"].Value,
            match.Groups["m1"].Value,
            year,
            match.Groups["t1"].Value);
        return ParseTimestamp(europeanText, "dd/MM/yyyy HH:mm:ss.fff", "dd/MM/yyyy HH:mm:ss");
    }

    private static DateTime? ParseTimestamp(string value, params string[] formats)
    {
        return DateTime.TryParseExact(
            value,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out var parsed)
                ? parsed
                : null;
    }
}
