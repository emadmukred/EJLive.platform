// MergedTraceCorrelationService (51).cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using EJLive.Core.Models;
using EJLive.Core.Xfs;

namespace EJLive.Core.Services
{
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var dt))
    return dt;
    return result;
    result.TotalEvents = result.Timeline.Count;
    else
    string year = match.Groups["y1"].Value;

    result.Timeline.Add(merged);
    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.v21_bak
    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    var result = new NcrMergedTraceCorrelationResult();
    ,

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.before_unify
    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    var result = new NcrMergedTraceCorrelationResult();
    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}", match.Groups["d1"].Value, match.Groups["m1"].Value, year, match.Groups["t1"].Value),
    new[] { "dd/MM/yyyy HH:mm:ss.fff", "dd/MM/yyyy HH:mm:ss" }
result.BySourceLayer = result.Timeline
.GroupBy(e => e.SourceLayer.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
result.ByKind = result.Timeline
.GroupBy(e => e.Kind.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
result.ByKind = result.Timeline
.GroupBy(e => e.Kind.ToString())
return result;
// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.before_unify
if (year.Length == 2)
year = "20" + year;
// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.v21_bak
if (year.Length == 2)
year = "20" + year;
// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.before_unify
if (!match.Success)
if (match.Groups["y2"].Success)
if (DateTime.TryParseExact(
string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.before_unify
if (year.Length == 2)
year = "20" + year;


// --- Methods ---
private static readonly Regex TimestampRegex = new Regex(
@"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
RegexOptions.Compiled | RegexOptions.CultureInvariant);

public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
{
    var result = new NcrMergedTraceCorrelationResult();

    foreach (var source in sources ?? Enumerable.Empty<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)>())
    {
        result.TotalRawLines += source.rawLines;
        foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
        {
            var merged = new NcrMergedTraceEvent;
            {
                Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
                SourceName = source.sourceName,
                Vendor = XfsVendor.NCR,
                SourceLayer = evt.SourceLayer,
                Kind = evt.Kind,
                Severity = evt.Severity,
                DeviceFamily = evt.DeviceFamily,
                Title = evt.Title,
                Message = evt.Message,
                RawLine = evt.RawLine,
                Data = evt.Data != null
                ? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
        result.Timeline.Add(merged);
    }
}

result.Timeline = result.Timeline
.OrderBy(e => e.Timestamp ?? DateTime.MaxValue)
.ThenBy(e => e.SourceName)
.ToList();
var match = TimestampRegex.Match(rawLine);
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
public partial class NcrMergedTraceCorrelationService
{
    RegexOptions.Compiled | RegexOptions.CultureInvariant);


    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\MergedTraceCorrelationService.cs
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())


    var merged = new NcrMergedTraceEvent;


    SourceName = source.sourceName,


    Vendor = XfsVendor.NCR,


    SourceLayer = evt.SourceLayer,


    Kind = evt.Kind,


    Severity = evt.Severity,


    DeviceFamily = evt.DeviceFamily,


    Title = evt.Title,


    Message = evt.Message,


    RawLine = evt.RawLine,


    Data = evt.Data != null


    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\MergedTraceCorrelationService.cs
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())


    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\MergedTraceCorrelationService.cs
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())


    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.before_unify
    if (!match.Success)
    if (match.Groups["y2"].Success)
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
    new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },


// Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.before_unify
foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
var merged = new NcrMergedTraceEvent;
Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
SourceName = source.sourceName,
Vendor = XfsVendor.NCR,
SourceLayer = evt.SourceLayer,
Kind = evt.Kind,
Severity = evt.Severity,
DeviceFamily = evt.DeviceFamily,
Title = evt.Title,
Message = evt.Message,
RawLine = evt.RawLine,
Data = evt.Data != null
? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
}
public partial class NcrMergedTraceCorrelationService
{
    RegexOptions.Compiled | RegexOptions.CultureInvariant);


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Services\MergedTraceCorrelationService.cs
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())


    var merged = new NcrMergedTraceEvent;


    SourceName = source.sourceName,


    Vendor = XfsVendor.NCR,


    SourceLayer = evt.SourceLayer,


    Kind = evt.Kind,


    Severity = evt.Severity,


    DeviceFamily = evt.DeviceFamily,


    Title = evt.Title,


    Message = evt.Message,


    RawLine = evt.RawLine,


    Data = evt.Data != null


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.v21_bak
    ```


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.v21_bak
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
    var merged = new NcrMergedTraceEvent;
    Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
    SourceName = source.sourceName,
    Vendor = XfsVendor.NCR,
    SourceLayer = evt.SourceLayer,
    Kind = evt.Kind,
    Severity = evt.Severity,
    DeviceFamily = evt.DeviceFamily,
    Title = evt.Title,
    Message = evt.Message,
    RawLine = evt.RawLine,
    Data = evt.Data != null
    ? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
}
public partial class NcrMergedTraceCorrelationService
{
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);


    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        var result = new NcrMergedTraceCorrelationResult();

        foreach (var source in sources ?? Enumerable.Empty<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)>())
        {
            result.TotalRawLines += source.rawLines;
            foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
            {
                var merged = new NcrMergedTraceEvent;
                {
                    Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
                    SourceName = source.sourceName,
                    Vendor = XfsVendor.NCR,
                    SourceLayer = evt.SourceLayer,
                    Kind = evt.Kind,
                    Severity = evt.Severity,
                    DeviceFamily = evt.DeviceFamily,
                    Title = evt.Title,
                    Message = evt.Message,
                    RawLine = evt.RawLine,
                    Data = evt.Data != null
                    ? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };
            result.Timeline.Add(merged);
        }
}

result.Timeline = result.Timeline
.OrderBy(e => e.Timestamp ?? DateTime.MaxValue)
.ThenBy(e => e.SourceName)
.ToList();

result.TotalEvents = result.Timeline.Count;
result.BySourceLayer = result.Timeline
.GroupBy(e => e.SourceLayer.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
result.ByKind = result.Timeline
.GroupBy(e => e.Kind.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

return result;
}


private DateTime? TryExtractTimestamp(string rawLine)
{
    if (string.IsNullOrWhiteSpace(rawLine))
    return null;

    var match = TimestampRegex.Match(rawLine);
    if (!match.Success)
    return null;

    if (match.Groups["y2"].Success)
    {
        if (DateTime.TryParseExact(
        string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
        new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var dt))
    return dt;
}
else
{
    string year = match.Groups["y1"].Value;
    if (year.Length == 2)
    year = "20" + year;
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}", match.Groups["d1"].Value, match.Groups["m1"].Value, year, match.Groups["t1"].Value),
    new[] { "dd/MM/yyyy HH:mm:ss.fff", "dd/MM/yyyy HH:mm:ss" },
CultureInfo.InvariantCulture,
DateTimeStyles.None,
out var dt))
return dt;
}

return null;
}

}
// Class: NcrMergedTraceCorrelationService (from 7 sources)
public sealed partial class NcrMergedTraceCorrelationService
{
    // --- Constants & Fields ---
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.v21_bak
    ```

    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
    var merged = new NcrMergedTraceEvent;
    Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
    SourceName = source.sourceName,
    Vendor = XfsVendor.NCR,
    SourceLayer = evt.SourceLayer,
    Kind = evt.Kind,
    Severity = evt.Severity,
    DeviceFamily = evt.DeviceFamily,
    Title = evt.Title,
    Message = evt.Message,
    RawLine = evt.RawLine,
    Data = evt.Data != null
    ? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
}
// Class: NcrMergedTraceCorrelationService (from 2 sources)
public sealed partial class NcrMergedTraceCorrelationService
{
    // --- Methods ---
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        var result = new NcrMergedTraceCorrelationResult();

        foreach (var source in sources ?? Enumerable.Empty<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)>())
        {
            result.TotalRawLines += source.rawLines;
            foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
            {
                var merged = new NcrMergedTraceEvent;
                {
                    Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
                    SourceName = source.sourceName,
                    Vendor = XfsVendor.NCR,
                    SourceLayer = evt.SourceLayer,
                    Kind = evt.Kind,
                    Severity = evt.Severity,
                    DeviceFamily = evt.DeviceFamily,
                    Title = evt.Title,
                    Message = evt.Message,
                    RawLine = evt.RawLine,
                    Data = evt.Data != null
                    ? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };
            result.Timeline.Add(merged);
        }
}

result.Timeline = result.Timeline
.OrderBy(e => e.Timestamp ?? DateTime.MaxValue)
.ThenBy(e => e.SourceName)
.ToList();

result.TotalEvents = result.Timeline.Count;
result.BySourceLayer = result.Timeline
.GroupBy(e => e.SourceLayer.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
result.ByKind = result.Timeline
.GroupBy(e => e.Kind.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

return result;
}

private DateTime? TryExtractTimestamp(string rawLine)
{
    if (string.IsNullOrWhiteSpace(rawLine))
    return null;

    var match = TimestampRegex.Match(rawLine);
    if (!match.Success)
    return null;

    if (match.Groups["y2"].Success)
    {
        if (DateTime.TryParseExact(
        string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
        new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var dt))
    return dt;
}
else
{
    string year = match.Groups["y1"].Value;
    if (year.Length == 2)
    year = "20" + year;
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}", match.Groups["d1"].Value, match.Groups["m1"].Value, year, match.Groups["t1"].Value),
    new[] { "dd/MM/yyyy HH:mm:ss.fff", "dd/MM/yyyy HH:mm:ss" },
CultureInfo.InvariantCulture,
DateTimeStyles.None,
out var dt))
return dt;
}

return null;
}

}
// Class: NcrMergedTraceCorrelationService (from 9 sources)
public sealed partial class NcrMergedTraceCorrelationService
{
    // --- Methods ---
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        var result = new NcrMergedTraceCorrelationResult();

        foreach (var source in sources ?? Enumerable.Empty<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)>())
        {
            result.TotalRawLines += source.rawLines;
            foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
            {
                var merged = new NcrMergedTraceEvent;
                {
                    Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
                    SourceName = source.sourceName,
                    Vendor = XfsVendor.NCR,
                    SourceLayer = evt.SourceLayer,
                    Kind = evt.Kind,
                    Severity = evt.Severity,
                    DeviceFamily = evt.DeviceFamily,
                    Title = evt.Title,
                    Message = evt.Message,
                    RawLine = evt.RawLine,
                    Data = evt.Data != null
                    ? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };
            result.Timeline.Add(merged);
        }
}

result.Timeline = result.Timeline
.OrderBy(e => e.Timestamp ?? DateTime.MaxValue)
.ThenBy(e => e.SourceName)
.ToList();

result.TotalEvents = result.Timeline.Count;
result.BySourceLayer = result.Timeline
.GroupBy(e => e.SourceLayer.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
result.ByKind = result.Timeline
.GroupBy(e => e.Kind.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

return result;
}

private DateTime? TryExtractTimestamp(string rawLine)
{
    if (string.IsNullOrWhiteSpace(rawLine))
    return null;

    var match = TimestampRegex.Match(rawLine);
    if (!match.Success)
    return null;

    if (match.Groups["y2"].Success)
    {
        if (DateTime.TryParseExact(
        string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
        new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var dt))
    return dt;
}
else
{
    string year = match.Groups["y1"].Value;
    if (year.Length == 2)
    year = "20" + year;
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}", match.Groups["d1"].Value, match.Groups["m1"].Value, year, match.Groups["t1"].Value),
    new[] { "dd/MM/yyyy HH:mm:ss.fff", "dd/MM/yyyy HH:mm:ss" },
CultureInfo.InvariantCulture,
DateTimeStyles.None,
out var dt))
return dt;
}

return null;
}

}
// Class: NcrMergedTraceCorrelationService (from 5 sources)
public sealed partial class NcrMergedTraceCorrelationService
{
    // --- Methods ---
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        var result = new NcrMergedTraceCorrelationResult();

        foreach (var source in sources ?? Enumerable.Empty<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)>())
        {
            result.TotalRawLines += source.rawLines;
            foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
            {
                var merged = new NcrMergedTraceEvent;
                {
                    Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
                    SourceName = source.sourceName,
                    Vendor = XfsVendor.NCR,
                    SourceLayer = evt.SourceLayer,
                    Kind = evt.Kind,
                    Severity = evt.Severity,
                    DeviceFamily = evt.DeviceFamily,
                    Title = evt.Title,
                    Message = evt.Message,
                    RawLine = evt.RawLine,
                    Data = evt.Data != null
                    ? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };
            result.Timeline.Add(merged);
        }
}

result.Timeline = result.Timeline
.OrderBy(e => e.Timestamp ?? DateTime.MaxValue)
.ThenBy(e => e.SourceName)
.ToList();

result.TotalEvents = result.Timeline.Count;
result.BySourceLayer = result.Timeline
.GroupBy(e => e.SourceLayer.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
result.ByKind = result.Timeline
.GroupBy(e => e.Kind.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

return result;
}

private DateTime? TryExtractTimestamp(string rawLine)
{
    if (string.IsNullOrWhiteSpace(rawLine))
    return null;

    var match = TimestampRegex.Match(rawLine);
    if (!match.Success)
    return null;

    if (match.Groups["y2"].Success)
    {
        if (DateTime.TryParseExact(
        string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
        new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var dt))
    return dt;
}
else
{
    string year = match.Groups["y1"].Value;
    if (year.Length == 2)
    year = "20" + year;
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}", match.Groups["d1"].Value, match.Groups["m1"].Value, year, match.Groups["t1"].Value),
    new[] { "dd/MM/yyyy HH:mm:ss.fff", "dd/MM/yyyy HH:mm:ss" },
CultureInfo.InvariantCulture,
DateTimeStyles.None,
out var dt))
return dt;
}

return null;
}

}
/// <summary>
/// NCR-only correlation layer for merged trace files that combine CARDREAD, MESSAGEIN,
/// MESSAGEOUT, OOXFS, and DEBUG streams into one chronological source.
/// </summary>
public sealed class NcrMergedTraceCorrelationService
{
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        var result = new NcrMergedTraceCorrelationResult();

        foreach (var source in sources ?? Enumerable.Empty<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)>())
        {
            result.TotalRawLines += source.rawLines;
            foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
            {
                var merged = new NcrMergedTraceEvent;
                {
                    Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
                    SourceName = source.sourceName,
                    Vendor = XfsVendor.NCR,
                    SourceLayer = evt.SourceLayer,
                    Kind = evt.Kind,
                    Severity = evt.Severity,
                    DeviceFamily = evt.DeviceFamily,
                    Title = evt.Title,
                    Message = evt.Message,
                    RawLine = evt.RawLine,
                    Data = evt.Data != null ?
                    new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase) :
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };
            result.Timeline.Add(merged);
        }
}

result.Timeline = result.Timeline
.OrderBy(e => e.Timestamp ?? DateTime.MaxValue)
.ThenBy(e => e.SourceName)
.ToList();

result.TotalEvents = result.Timeline.Count;
result.BySourceLayer = result.Timeline
.GroupBy(e => e.SourceLayer.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
result.ByKind = result.Timeline
.GroupBy(e => e.Kind.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

return result;
}

private DateTime? TryExtractTimestamp(string rawLine)
{
    if (string.IsNullOrWhiteSpace(rawLine))
    return null;

    var match = TimestampRegex.Match(rawLine);
    if (!match.Success)
    return null;

    if (match.Groups["y2"].Success)
    {
        if (DateTime.TryParseExact(
        string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
        new[] {
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss"
        },
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out
    var dt))
    return dt;
}
else
{
    string year = match.Groups["y1"].Value;
    if (year.Length == 2)
    year = "20" + year;
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}", match.Groups["d1"].Value, match.Groups["m1"].Value, year, match.Groups["t1"].Value),
    new[] {
        "dd/MM/yyyy HH:mm:ss.fff",
        "dd/MM/yyyy HH:mm:ss"
    },
CultureInfo.InvariantCulture,
DateTimeStyles.None,
out
var dt))
return dt;
}

return null;
}
}
public partial class NcrMergedTraceCorrelationService
{
    RegexOptions.Compiled | RegexOptions.CultureInvariant);
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
    var merged = new NcrMergedTraceEvent;
    SourceName = source.sourceName,
    Vendor = XfsVendor.NCR,
    SourceLayer = evt.SourceLayer,
    Kind = evt.Kind,
    Severity = evt.Severity,
    DeviceFamily = evt.DeviceFamily,
    Title = evt.Title,
    Message = evt.Message,
    RawLine = evt.RawLine,
    Data = evt.Data != null
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
    var merged = new NcrMergedTraceEvent;
    SourceName = source.sourceName,
    Vendor = XfsVendor.NCR,
    SourceLayer = evt.SourceLayer,
    Kind = evt.Kind,
    Severity = evt.Severity,
    DeviceFamily = evt.DeviceFamily,
    Title = evt.Title,
    Message = evt.Message,
    RawLine = evt.RawLine,
    Data = evt.Data != null
    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\MergedTraceCorrelationService.cs
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
    // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\MergedTraceCorrelationService.cs
    foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.before_unify
    if (!match.Success)
    if (match.Groups["y2"].Success)
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
    new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },
private static readonly Regex TimestampRegex = new Regex(
@"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
RegexOptions.Compiled | RegexOptions.CultureInvariant);
public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
{
    var result = new NcrMergedTraceCorrelationResult();
    foreach (var source in sources ?? Enumerable.Empty<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)>())
    {
        result.TotalRawLines += source.rawLines;
        foreach (var evt in source.events ?? Array.Empty<XfsNormalizedEvent>())
        {
            var merged = new NcrMergedTraceEvent;
            {
                Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
                SourceName = source.sourceName,
                Vendor = XfsVendor.NCR,
                SourceLayer = evt.SourceLayer,
                Kind = evt.Kind,
                Severity = evt.Severity,
                DeviceFamily = evt.DeviceFamily,
                Title = evt.Title,
                Message = evt.Message,
                RawLine = evt.RawLine,
                Data = evt.Data != null
                ? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
        result.Timeline.Add(merged);
    }
}
result.Timeline = result.Timeline
.OrderBy(e => e.Timestamp ?? DateTime.MaxValue)
.ThenBy(e => e.SourceName)
.ToList();
result.TotalEvents = result.Timeline.Count;
result.BySourceLayer = result.Timeline
.GroupBy(e => e.SourceLayer.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
result.ByKind = result.Timeline
.GroupBy(e => e.Kind.ToString())
.ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
return result;
}
private DateTime? TryExtractTimestamp(string rawLine)
{
    if (string.IsNullOrWhiteSpace(rawLine))
    return null;
    var match = TimestampRegex.Match(rawLine);
    if (!match.Success)
    return null;
    if (match.Groups["y2"].Success)
    {
        if (DateTime.TryParseExact(
        string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
        new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var dt))
    return dt;
}
else
{
    string year = match.Groups["y1"].Value;
    if (year.Length == 2)
    year = "20" + year;
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}", match.Groups["d1"].Value, match.Groups["m1"].Value, year, match.Groups["t1"].Value),
    new[] { "dd/MM/yyyy HH:mm:ss.fff", "dd/MM/yyyy HH:mm:ss" },
CultureInfo.InvariantCulture,
DateTimeStyles.None,
out var dt))
return dt;
}
return null;
}
Timestamp = evt.Timestamp ?? TryExtractTimestamp(evt.RawLine),
? new Dictionary<string, string>(evt.Data, StringComparer.OrdinalIgnoreCase)
: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
}
public partial public sealed class NcrMergedTraceCorrelationService
{
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        private DateTime? TryExtractTimestamp(string rawLine)
        {
            public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
            {
            }

        public partial public public sealed class MergedTraceCorrelationService
        {
            public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
            {
            }

    }
public partial public public class NcrMergedTraceCorrelationService
{
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        private DateTime? TryExtractTimestamp(string rawLine)
        {
            public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
            {
            }

        public partial public public sealed class MergedTraceCorrelationService
        {
            public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
            {
            }

    }
public partial public class NcrMergedTraceCorrelationService
{
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        private DateTime? TryExtractTimestamp(string rawLine)
        {
        }

    public partial public sealed class MergedTraceCorrelationService
    {
        public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
        {
        }

}
public partial public sealed class NcrMergedTraceCorrelationService
{
    private static readonly Regex TimestampRegex = new Regex(
    @"(?<d1>\d{2})[\-/](?<m1>\d{2})[\-/](?<y1>\d{2,4})\s+(?<t1>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)|(?<y2>\d{4})-(?<m2>\d{2})-(?<d2>\d{2})\s+(?<t2>\d{2}:\d{2}:\d{2}(?:\.\d{3})?)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);
    public NcrMergedTraceCorrelationResult Correlate(IEnumerable<(string sourceName, IReadOnlyList<XfsNormalizedEvent> events, int rawLines)> sources)
    {
        private DateTime? TryExtractTimestamp(string rawLine)
        {
        }

    public partial public sealed class MergedTraceCorrelationService
    {
        public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
        {
        }

}
result.Timeline = result.Timeline
.OrderBy(e => e.Timestamp ?? DateTime.MaxValue)
.ThenBy(e => e.SourceName)
.ToList();
}

private DateTime? TryExtractTimestamp(string rawLine)
{
    if (string.IsNullOrWhiteSpace(rawLine))
    return null;

    var match = TimestampRegex.Match(rawLine);
    if (!match.Success)
    return null;

    if (match.Groups["y2"].Success)
    {
        if (DateTime.TryParseExact(
        string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", match.Groups["y2"].Value, match.Groups["m2"].Value, match.Groups["d2"].Value, match.Groups["t2"].Value),
        new[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" },
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var dt))
    return dt;
}
else
{
    string year = match.Groups["y1"].Value;
    if (year.Length == 2)
    year = "20" + year;
    if (DateTime.TryParseExact(
    string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2} {3}", match.Groups["d1"].Value, match.Groups["m1"].Value, year, match.Groups["t1"].Value),
    new[] { "dd/MM/yyyy HH:mm:ss.fff", "dd/MM/yyyy HH:mm:ss" },
CultureInfo.InvariantCulture,
DateTimeStyles.None,
out var dt))
return dt;
}

return null;
}
// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.v21_bak
private DateTime? TryExtractTimestamp(string rawLine)
if (string.IsNullOrWhiteSpace(rawLine))
return null;
// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\MergedTraceCorrelationService.cs.before_unify
private DateTime? TryExtractTimestamp(string rawLine)
if (string.IsNullOrWhiteSpace(rawLine))
return null;
}


{
    public class MergedTraceCorrelationService { }
}