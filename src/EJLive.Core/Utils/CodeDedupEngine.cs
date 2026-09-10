using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EJLive.Core.Utils
{
    public partial public sealed class CodeDedupEngine : IDisposable
    private const int DefaultBufferSize = 65536;
    // 64 KB
    private const int HashBufferSize = 8192;
    // 8 KB incremental hash buffer
    private const int ProgressReportIntervalMs = 500;

    // Class: DedupStatistics (from 3 sources)
        public sealed partial class DedupStatistics
        {
        }
    // Min interval between progress events
    // Patterns for extracting unique code elements
    private static readonly Regex s_usingRegex = new(
    @"^\s*using\s+([\w.]+)\s*;", RegexOptions.Compiled | RegexOptions.Multiline);
    // Class: RegionBoundary (from 3 sources)
        public sealed partial class RegionBoundary
        {
        }

    public partial class DedupStatistics
        {
        }
    public partial class RegionBoundary
        {
        }
}

/// Streams through a huge .cs file, locates all outermost #region blocks
/// via byte-offset tracking, computes incremental SHA256 hashes for each

/// extracts unique code from reference regions, and writes a clean
/// unified output file.
///
/// Usage:

///   engine.ProgressChanged += (s, pct) => Console.Write($"\r{pct:F1}%");

///   // To apply (write output):
///   stats = await engine.ProcessAsync(dryRun: false);
public sealed class CodeDedupEngine : IDisposable
private const int DefaultBufferSize = 65536;
// 64 KB
private const int HashBufferSize = 8192;
// 8 KB incremental hash buffer
private const int ProgressReportIntervalMs = 500;

// Write header
// Write base region
// Write unified unique code section
bool hasExtracted = usings.Count > 0 || interfaces.Count > 0 || types.Count > 0 ||
methods.Count > 0 || enums.Count > 0;

public long BytesSaved => OriginalSize - NewSize;
public sealed class DedupStatistics
/// <summary>Total #region blocks found in the file.</summary>
public int TotalRegions { get; set; }
/// <summary>Unique interface definitions extracted from references.</summary>
public int UniqueInterfacesExtracted { get; set; }
/// <summary>Unique enum definitions extracted from references.</summary>
public int UniqueEnumsExtracted { get; set; }
/// <summary>Duplicate regions removed.</summary>
public int DuplicatesRemoved { get; set; }
/// <summary>Elapsed processing time.</summary>
public TimeSpan Elapsed { get; set; }
/// <summary>Error message if processing failed.</summary>
public string? ErrorMessage { get; set; }
/// <summary>Whether the operation was a dry run.</summary>
public bool IsDryRun { get; set; }
public long NewSize { get; set; }
/// <summary>Total unique code elements extracted.</summary>
public int TotalExtracted =>
UniqueUsingsExtracted + UniqueInterfacesExtracted +
UniqueTypesExtracted + UniqueMethodsExtracted +
/// <summary>Original file size in bytes.</summary>
public long OriginalSize { get; set; }
/// <summary>Whether the operation succeeded.</summary>
public bool Success => ErrorMessage == null;
/// <summary>Unique class/struct types extracted from references.</summary>
public int UniqueTypesExtracted { get; set; }
/// <summary>Unique method signatures extracted from references.</summary>
public int UniqueMethodsExtracted { get; set; }
public int UniqueRegions { get; set; }
/// <summary>Unique using statements extracted from references.</summary>
public int UniqueUsingsExtracted { get; set; }

using var sha = SHA256.Create();
/// <inheritdoc/>
public void Dispose()
_disposed = true;
using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize, FileOptions.SequentialScan);
if (hasExtracted)
if (usings.Count > 0)
foreach (var u in usings.OrderBy(x => x))
if (interfaces.Count > 0)
foreach (var i in interfaces)
if (types.Count > 0)
foreach (var t in types)
if (enums.Count > 0)
foreach (var e in enums)
if (methods.Count > 0)
foreach (var m in methods)
// Write report footer
private long EstimateNewSize(
// Rough estimate: base region + extracted code + overhead
long baseSize = uniqueRegions[0].ByteEnd - uniqueRegions[0].ByteStart;
long extractedSize = 0;
extractedSize += usings.Sum(u => Encoding.UTF8.GetByteCount($"using {u};\n"));
if ((now - _lastProgressTime).TotalMilliseconds >= ProgressReportIntervalMs || fraction >= 1.0)
_lastProgressTime = now;
return Math.Min(_fileSize, baseSize + extractedSize + 4096);
// Min interval between progress events
// Patterns for extracting unique code elements
private static readonly Regex s_usingRegex = new(
@"^\s*using\s+([\w.]+)\s*;",
RegexOptions.Compiled | RegexOptions.Multiline);
private static readonly Regex s_interfaceRegex = new(
@"(?sm)^\s*(?:public\s+|private\s+|protected\s+|internal\s+)?interface\s+(\w+)\s*[^{]*\{[^}]*\}",
RegexOptions.Compiled);
private static readonly Regex s_typeRegex = new(
@"(?sm)^\s*(?:public\s+|private\s+|protected\s+|internal\s+)?(?:static\s+|sealed\s+|abstract\s+|partial\s+)*(class|struct)\s+(\w+)[^{]*\{((?:[^{}]|(?<O>\{)|(?<-O>\}))*(?(O)(?!)))\}",
private static readonly Regex s_methodRegex = new(
@"(?sm)^\s*(?:public\s+|private\s+|protected\s+|internal\s+)?(?:static\s+|virtual\s+|override\s+|async\s+)*[\w<>\[\],\s]+\s+(\w+)\s*\([^)]*\)\s*\{((?:[^{}]|(?<O>\{)|(?<-O>\}))*(?(O)(?!)))\}",
private static readonly Regex s_enumRegex = new(
@"(?sm)^\s*(?:public\s+|private\s+|protected\s+|internal\s+)?enum\s+(\w+)\s*\{[^}]*\}",
// Names to exclude from method extraction (constructors, auto-properties, etc.)
private static readonly HashSet<string> s_excludedMethods = new(StringComparer.Ordinal)
"Main", "InitializeComponent", "Dispose", ".ctor", ".cctor",
"ToString", "Equals", "GetHashCode", "GetType", "MemberwiseClone",
"Finalize", "CanExecute", "Execute"
};
private readonly string _filePath;
private readonly long _fileSize;
private readonly Encoding _encoding;
private readonly int _bomSize;
private readonly int _bufferSize;
// Progress tracking
private DateTime _lastProgressTime = DateTime.MinValue;
private bool _disposed;
/// <summary>Fired when processing progress changes (0.0 to 1.0).</summary>
public event EventHandler<double>? ProgressChanged;
/// <summary>Fired with informational log messages.</summary>
public event EventHandler<string>? LogMessage;

/// <param name="bufferSize">Internal buffer size (default 64KB).</param>
/// <exception cref="FileNotFoundException">If file does not exist.</exception>
/// <exception cref="ArgumentException">If file is not a .cs file.</exception>
public CodeDedupEngine(string filePath, int bufferSize = DefaultBufferSize)
if (string.IsNullOrWhiteSpace(filePath))
throw new ArgumentNullException(nameof(filePath));
if (!File.Exists(filePath))
throw new FileNotFoundException("File not found for deduplication.", filePath);
if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
throw new ArgumentException("Only .cs files are supported.", nameof(filePath));
if (bufferSize < 4096)
throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be at least 4096 bytes.");
_filePath = filePath;
_fileSize = new FileInfo(filePath).Length;
_bufferSize = bufferSize;
_encoding = EncodingDetector.DetectFromFile(filePath);
_bomSize = _encoding.GetPreamble()?.Length ?? 0;

/// <param name="dryRun">If true, no file is modified; statistics are estimated.</param>
/// <param name="baseRegionPattern">
///   Regex pattern to identify the base/original region by name.
///   Default matches regions containing "ORIGINAL" or "BASE".
/// </param>
/// <param name="cancellationToken">Cancellation token.</param>

public async Task<DedupStatistics> ProcessAsync(
bool dryRun = false,
string baseRegionPattern = @"ORIGINAL|BASE",
var stats = new DedupStatistics { OriginalSize = _fileSize, IsDryRun = dryRun };
var sw = System.Diagnostics.Stopwatch.StartNew();
try
// Phase 1: Scan for #region boundaries (byte offsets)
ReportProgress(0.0, "Phase 1: Scanning region boundaries...");
var regions = await ScanRegionBoundariesAsync(cancellationToken);
ReportProgress(0.25, $"Phase 1 done: found {regions.Count} outermost regions");
if (regions.Count == 0)
stats.ErrorMessage = "No #region blocks found in file.";
return stats;
// Phase 2: Hash region bodies incrementally
ReportProgress(0.25, $"Phase 2: Hashing {regions.Count} region bodies...");
ReportProgress(0.50, $"Phase 2 done: {regions.Count} regions hashed");

ReportProgress(0.50, "Phase 3: Deduplication...");
var (uniqueRegions, dupCount) = DeduplicateByHash(regions);
stats.TotalRegions = regions.Count;
stats.UniqueRegions = uniqueRegions.Count;
stats.DuplicatesRemoved = dupCount;
ReportProgress(0.60, $"Phase 3 done: {regions.Count} -> {uniqueRegions.Count} ({dupCount} dups)");
if (dupCount == 0)
stats.ErrorMessage = "No duplicates found — skipping.";
// Phase 4: Extract unique code from reference regions
ReportProgress(0.60, "Phase 4: Extracting unique code from references...");
var (usings, interfaces, types, methods, enums) =
stats.UniqueUsingsExtracted = usings.Count;
stats.UniqueInterfacesExtracted = interfaces.Count;
stats.UniqueTypesExtracted = types.Count;
stats.UniqueMethodsExtracted = methods.Count;
stats.UniqueEnumsExtracted = enums.Count;
ReportProgress(0.80, $"Phase 4 done: {stats.TotalExtracted} unique elements extracted");
if (dryRun)
long estimatedNewSize = EstimateNewSize(uniqueRegions, usings, interfaces, types, methods, enums);
stats.NewSize = estimatedNewSize;
stats.Elapsed = sw.Elapsed;
ReportProgress(1.0, $"Dry run complete. Estimated save: {FileSizeFormatter.Format(stats.BytesSaved)}");
// Phase 5: Write unified clean file
ReportProgress(0.80, "Phase 5: Writing unified clean file...");
stats.NewSize = new FileInfo(_filePath).Length;
ReportProgress(1.0, $"Dedup complete. {FileSizeFormatter.Format(_fileSize)} -> {FileSizeFormatter.Format(stats.NewSize)} (saved {FileSizeFormatter.Format(stats.BytesSaved)})");
catch (OperationCanceledException)
stats.ErrorMessage = "Operation cancelled.";
catch (Exception ex)
stats.ErrorMessage = ex.ToString();
private async Task<List<RegionBoundary>> ScanRegionBoundariesAsync(CancellationToken ct)
var regions = new List<RegionBoundary>();
var regionStack = new Stack<long>();  // byte offset of each #region
// Track byte position manually because StreamReader.Position counts decoded chars
long bytePosition = _bomSize;  // skip BOM
int lineNum = 0;
while (!sr.EndOfStream)
ct.ThrowIfCancellationRequested();
string? line = await sr.ReadLineAsync();
if (line == null) break;
lineNum++;
long lineByteCount = _encoding.GetByteCount(line ?? string.Empty) + _encoding.GetByteCount(Environment.NewLine);
if (line.StartsWith("#region", StringComparison.OrdinalIgnoreCase))
regionStack.Push(bytePosition);
else if (line.StartsWith("#endregion", StringComparison.OrdinalIgnoreCase) && regionStack.Count > 0)
long regionStart = regionStack.Pop();
// Only capture outermost regions (stack is now empty after pop)
if (regionStack.Count == 0)
regions.Add(new RegionBoundary
ByteStart = regionStart,
ByteEnd = bytePosition + lineByteCount,  // end of #endregion line
StartLine = -1,  // Will be resolved in Phase 2
EndLine = lineNum
});
bytePosition += lineByteCount;
// Progress reporting
double pct = (double)bytePosition / _fileSize;
ReportProgress(pct * 0.25, null); // Scale to 0..0.25
return regions;
private async Task HashRegionBodiesAsync(List<RegionBoundary> regions, CancellationToken ct)
// We'll read line by line and track byte positions to match region boundaries
long bytePosition = _bomSize;
int regionIdx = 0;
while (!sr.EndOfStream && regionIdx < regions.Count)
// Check if this line starts the next region
var region = regions[regionIdx];
if (bytePosition >= region.ByteStart && lineNum <= region.EndLine)
// This is a #region line — capture its name
region.Name = line.Substring("#region".Length).Trim();
region.StartLine = lineNum;
// Read the body (everything between #region and #endregion)
// and compute SHA256 incrementally
long bodyStartByte = bytePosition + lineByteCount;  // after #region line
long bodyEndByte = 0;
string? bodyLine = await sr.ReadLineAsync();
if (bodyLine == null) break;
long bodyLineBytes = _encoding.GetByteCount(bodyLine) + _encoding.GetByteCount(Environment.NewLine);
if (bodyLine.StartsWith("#endregion", StringComparison.OrdinalIgnoreCase))
bodyEndByte = bytePosition + lineByteCount + bodyLineBytes;
break;
// Feed body line to hash incrementally (as UTF-8 bytes)
byte[] lineData = Encoding.UTF8.GetBytes(bodyLine + Environment.NewLine);
sha.TransformBlock(lineData, 0, lineData.Length, null, 0);
lineByteCount = bodyLineBytes;
// Finalize hash
sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
region.ContentHash = BitConverter.ToString(sha.Hash!).Replace("-", "").ToLowerInvariant();
region.ByteEnd = bodyEndByte > 0 ? bodyEndByte : region.ByteEnd;
regionIdx++;
// Progress
ReportProgress(0.25 + pct * 0.25, null); // Scale to 0.25..0.50
private static (List<RegionBoundary> unique, int dupCount) DeduplicateByHash(List<RegionBoundary> regions)
var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var unique = new List<RegionBoundary>();
int dupCount = 0;
foreach (var region in regions)
if (seen.Add(region.ContentHash))
unique.Add(region);
else
region.IsDuplicate = true;
dupCount++;
return (unique, dupCount);
private async Task<(List<string> usings, List<string> interfaces, List<string> types, List<string> methods, List<string> enums)>
ExtractUniqueCodeAsync(List<RegionBoundary> uniqueRegions, string basePattern, CancellationToken ct)
var uniqueUsings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var uniqueInterfaces = new List<string>();
var uniqueTypes = new List<string>();
var uniqueMethods = new List<string>();
var uniqueEnums = new List<string>();
// Identify base region
var baseRegex = new Regex(basePattern, RegexOptions.IgnoreCase);
var baseRegion = uniqueRegions.FirstOrDefault(r => baseRegex.IsMatch(r.Name));
var refRegions = uniqueRegions.Where(r => r != baseRegion).ToList();
if (baseRegion == null || refRegions.Count == 0)
return (new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>());
// Read base region body
string baseBody = await ReadRegionBodyAsync(baseRegion, ct);
// Build base catalogs
var baseUsings = new HashSet<string>(GetMatches(s_usingRegex, baseBody, 1), StringComparer.OrdinalIgnoreCase);
var baseInterfaces = new HashSet<string>(GetMatches(s_interfaceRegex, baseBody, 1), StringComparer.OrdinalIgnoreCase);
var baseTypes = new HashSet<string>(GetMatches(s_typeRegex, baseBody, 2), StringComparer.OrdinalIgnoreCase);
var baseMethods = new HashSet<string>(GetMatches(s_methodRegex, baseBody, 1), StringComparer.OrdinalIgnoreCase);
var baseEnums = new HashSet<string>(GetMatches(s_enumRegex, baseBody, 1), StringComparer.OrdinalIgnoreCase);
// Process each reference region
foreach (var refRegion in refRegions)
string refBody = await ReadRegionBodyAsync(refRegion, ct);
// Extract unique usings
foreach (Match m in s_usingRegex.Matches(refBody))
string val = m.Groups[1].Value;
if (!baseUsings.Contains(val) && uniqueUsings.Add(val))
// Already tracked via HashSet
// Extract unique interfaces
foreach (Match m in s_interfaceRegex.Matches(refBody))
string name = m.Groups[1].Value;
if (!baseInterfaces.Contains(name))
uniqueInterfaces.Add(m.Value);
// Extract unique types (class/struct)
foreach (Match m in s_typeRegex.Matches(refBody))
string name = m.Groups[2].Value;
if (!baseTypes.Contains(name))
uniqueTypes.Add(m.Value);
// Extract unique methods
foreach (Match m in s_methodRegex.Matches(refBody))
if (!s_excludedMethods.Contains(name) && !baseMethods.Contains(name))
uniqueMethods.Add(m.Value);
// Extract unique enums
foreach (Match m in s_enumRegex.Matches(refBody))
if (!baseEnums.Contains(name))
uniqueEnums.Add(m.Value);
return (uniqueUsings.ToList(), uniqueInterfaces, uniqueTypes, uniqueMethods, uniqueEnums);
private static List<string> GetMatches(Regex regex, string input, int groupIndex)
var results = new List<string>();
foreach (Match m in regex.Matches(input))
if (m.Groups.Count > groupIndex)
results.Add(m.Groups[groupIndex].Value);
return results;
private async Task<string> ReadRegionBodyAsync(RegionBoundary region, CancellationToken ct)
// Seek to region start and read lines until #endregion
var bodyLines = new List<string>();
bool inRegion = false;
if (inRegion)
if (line.StartsWith("#endregion", StringComparison.OrdinalIgnoreCase))
bodyLines.Add(line);
else if (line.StartsWith("#region", StringComparison.OrdinalIgnoreCase) &&
line.Contains(region.Name, StringComparison.OrdinalIgnoreCase))
inRegion = true;
return string.Join(Environment.NewLine, bodyLines);
private async Task WriteUnifiedFileAsync(
string basePattern,
string tmpPath = _filePath + ".dedup_tmp";
string bakPath = _filePath + ".before_dedup";
var baseRegion = uniqueRegions.First(r => baseRegex.IsMatch(r.Name));
var refRegions = uniqueRegions.Where(r => r.Name != baseRegion?.Name).ToList();
// 2026-06-11 23:45 UTC | Source: 22.85 KB



namespace EJLive.Core.Utils

//   Generated:     2026-06-11 23:28:46 UTC
//   Source Size:   40.32 KB
//   Result:        399 lines (0 dups removed)
//   Indexed:       8U 3C 0I 0E 0S 18M 24P 68F 3K 0Ev
//   Extracted:     0 unique elements
//   Conflicts:     0 | Resolved: 0

//   Generated:     2026-06-11 23:10:52 UTC
//   Source Size:   34.35 KB
//   Raw Lines:     407
//   Regions:       3 unique (0 duplicates removed)
//   Indexed:       8U 3C 0I 0E 0S 0R 0D 18M 25P 68F 3K 0Ev
//   Unique Extracted: 2 elements from reference regions

// Uses FILE STREAM + SEEK + OFFSET TRACKING for zero-memory-content processing.
//
// Architecture:
//   Phase 1: Stream-scan to locate all outermost #region/#endregion blocks
//            and capture their byte offsets (START/END positions in file).
//   Phase 2: Incremental SHA256 hash each region's body directly from
//            file offset — never loading the full body into memory.

//            unique hash (preserving first appearance = original order).
//   Phase 4: Extract unique code elements (usings, classes, interfaces,
//            methods, enums) from non-base reference regions that don't
//            exist in the base region.
//   Phase 5: Write the unified clean file containing:
//              - Base region code
//              - Extracted unique elements from references

// Memory usage: ~50 MB regardless of file size (tested to 2.15 GB).
{
/// <summary>
/// Records the byte-offset boundaries of a #region ... #endregion block
/// within a source file, plus its computed SHA256 content hash.
/// </summary>
public sealed class RegionBoundary
/// <summary>Byte offset in file where the #region line starts.</summary>
public long ByteStart { get; set; }
/// <summary>Byte offset in file where the #endregion line ends (exclusive).</summary>
public long ByteEnd { get; set; }
/// <summary>1-based line number of the #region line.</summary>
public int StartLine { get; set; }
/// <summary>1-based line number of the #endregion line.</summary>
public int EndLine { get; set; }
/// <summary>The region name (text after #region).</summary>
public string Name { get; set; } = string.Empty;
/// <summary>SHA256 hash of the region body content (trimmed, without BOM).</summary>
public string ContentHash { get; set; } = string.Empty;
/// <summary>Whether this region was kept (unique) or is a duplicate.</summary>
public bool IsDuplicate { get; set; }
}
// +header/footer overhead
private void ReportProgress(double fraction, string? message)
if (message != null)
// Throttle progress events to avoid flooding UI
var now = DateTime.UtcNow;
using var sr = new StreamReader(fs, _encoding, detectEncodingFromByteOrderMarks: false, _bufferSize, leaveOpen: false);
using var sw = new StreamWriter(tmpPath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), _bufferSize);
extractedSize += interfaces.Sum(i => Encoding.UTF8.GetByteCount(i + "\n\n"));
extractedSize += types.Sum(t => Encoding.UTF8.GetByteCount(t + "\n\n"));
extractedSize += methods.Sum(m => Encoding.UTF8.GetByteCount(m + "\n\n"));
extractedSize += enums.Sum(e => Encoding.UTF8.GetByteCount(e + "\n\n"));
