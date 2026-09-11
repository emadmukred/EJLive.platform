using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EJLive.Core.Utils
{
    private readonly int _bomSize;
    private readonly int _bufferSize;
    private bool _disposed;
    private readonly Encoding _encoding;
    private readonly long _fileSize;
    private FileStream? _fileStream;
    private DateTime _startTime;
    private StreamReader? _streamReader;
    // Statistics
    private long _totalBytesRead;
    private long _totalLinesRead;
    public partial public sealed class HugeFileViewer : IDisposable, IAsyncDisposable
    private readonly string _filePath;

    /// <summary>Buffer size used for read operations.</summary>
    public int BufferSize => _bufferSize;
    /// <summary>Elapsed time since creation.</summary>
    public TimeSpan Elapsed => DateTime.UtcNow - _startTime;
    /// <summary>Detected or specified file encoding.</summary>
    public Encoding FileEncoding => _encoding;
    /// <summary>Absolute path to the file being viewed.</summary>
    public string FilePath => _filePath;
    /// <summary>Total file size in bytes.</summary>
    public long FileSize => _fileSize;

    /// <summary>Size of the BOM in bytes (0 if no BOM).</summary>
    public int BomSize => _bomSize;
    // Class: FileMetadata (from 3 sources)
        public sealed partial class FileMetadata
        {
        }
    // Class: FileSizeFormatter (from 3 sources)
        public static partial class FileSizeFormatter
        {
        }
    /// <summary>Formatted file size string.</summary>
    public string FileSizeFormatted => FileSizeFormatter.Format(_fileSize);
    /// <summary>Read throughput in bytes/second.</summary>
    public double BytesPerSecond =>
    TotalBytesRead / Math.Max(0.001, Elapsed.TotalSeconds);
    /// <summary>Total bytes read since creation or last reset.</summary>
    public long TotalBytesRead => Interlocked.Read(ref _totalBytesRead);
    /// <summary>Total lines read since creation or last reset.</summary>
    public long TotalLinesRead => Interlocked.Read(ref _totalLinesRead);

    public partial class FileMetadata
        {
        }
    public partial class FileSizeFormatter
        {
        }
}

private readonly int _bomSize;
private readonly int _bufferSize;
private bool _disposed;
private readonly Encoding _encoding;
private readonly long _fileSize;
private FileStream? _fileStream;
private DateTime _startTime;
private StreamReader? _streamReader;
// Statistics
private long _totalBytesRead;
private long _totalLinesRead;
return preamble?.Length ?? 0;
var bom = new byte[4];

/// <summary>Buffer size used for read operations.</summary>
public int BufferSize => _bufferSize;
/// <summary>Elapsed time since creation.</summary>
public TimeSpan Elapsed => DateTime.UtcNow - _startTime;
/// <summary>Detected or specified file encoding.</summary>
public Encoding FileEncoding => _encoding;
/// <summary>Absolute path to the file being viewed.</summary>
public string FilePath => _filePath;
/// <summary>Total file size in bytes.</summary>
public long FileSize => _fileSize;

/// <summary>Size of the BOM in bytes (0 if no BOM).</summary>
public int BomSize => _bomSize;
/// Initializes a new HugeFileViewer for the specified file.
/// <param name="filePath">Path to the file to view.</param>
/// <param name="encoding">Encoding override. If null, auto-detected from BOM.</param>
/// <param name="bufferSize">Internal buffer size in bytes (default 65536 = 64KB).</param>
/// <exception cref="FileNotFoundException">Thrown if file does not exist.</exception>
/// <exception cref="ArgumentNullException">Thrown if filePath is null or empty.</exception>
public HugeFileViewer(string filePath, Encoding? encoding = null, int bufferSize = 65536)
if (string.IsNullOrWhiteSpace(filePath))
throw new FileNotFoundException($"File not found: {filePath}", filePath);
/// Detects file encoding by reading the BOM (Byte Order Mark)
/// and optionally sampling content for heuristic detection.
public static class EncodingDetector
/// Detects encoding from a file path by reading its BOM.
/// Returns UTF-8 without BOM if no BOM is found.
public static Encoding DetectFromFile(string filePath)
if (string.IsNullOrEmpty(filePath))
throw new ArgumentNullException(nameof(filePath));
// 2026-06-11 23:45 UTC | Source: 17.92 KB



namespace EJLive.Core.Utils

//   Generated:     2026-06-11 23:28:47 UTC
//   Source Size:   27.10 KB
//   Result:        292 lines (0 dups removed)
//   Indexed:       7U 4C 0I 0E 0S 24M 24P 30F 0K 0Ev
//   Extracted:     0 unique elements
//   Conflicts:     0 | Resolved: 0

//   Generated:     2026-06-11 23:11:01 UTC
//   Source Size:   20.98 KB
//   Raw Lines:     298
//   Regions:       3 unique (0 duplicates removed)
//   Indexed:       7U 4C 0I 0E 0S 0R 0D 24M 25P 30F 0K 0Ev
//   Unique Extracted: 3 elements from reference regions

// EJLive Enterprise — Large File Viewer
// Opens and browses files of any size (tested to 2GB+) without loading
// the entire file into memory. Uses memory-mapped I/O and chunked
// streaming with configurable buffer sizes.
//
// Features:
//   - Chunked byte-level reading with arbitrary seeking
//   - Line-based streaming enumeration (yield return)
//   - Progress reporting via IProgress<double>
//   - File metadata extraction (encoding detection, BOM handling)
//   - Automatic encoding detection (UTF-8, UTF-16 LE/BE, UTF-32)
//   - Cancellation support
//   - Comprehensive error handling
{
/// <summary>
/// Formats file sizes in human-readable form (B, KB, MB, GB, TB).
/// </summary>
public static class FileSizeFormatter
private static readonly string[] _suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
/// <summary>Formats a byte count into a human-readable string with 2 decimal places.</summary>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static string Format(long bytes)
if (bytes == 0) return "0 B";
int magnitude = (int)Math.Log(Math.Abs(bytes), 1024);
if (magnitude >= _suffixes.Length)
magnitude = _suffixes.Length - 1;
double adjusted = bytes / Math.Pow(1024, magnitude);
return $"{adjusted:N2} {_suffixes[magnitude]}";
}
using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 16);
/// <summary>Formats a byte count with configurable decimal places.</summary>
public static string Format(long bytes, int decimalPlaces)
return $"{adjusted:N}{new string('#', Math.Max(0, decimalPlaces))} {_suffixes[magnitude]}"
.Replace("N", $"N{decimalPlaces}");
/// <summary>Formatted file size string.</summary>
public string FileSizeFormatted => FileSizeFormatter.Format(_fileSize);
// UTF-8 no BOM
/// <summary>Returns only the BOM size in bytes for a given file.</summary>
public static int GetBomSize(string filePath)
var enc = DetectFromFile(filePath);
byte[] preamble = enc.GetPreamble();
/// Provides memory-efficient access to files of any size.
/// Uses chunked FileStream with SequentialScan optimization.
///
/// Usage:
///   using var viewer = new HugeFileViewer(@"C:\large_file.cs");
///   long lineCount = await viewer.CountLinesAsync(progress);
///   await foreach (var line in viewer.ReadLinesAsync(progress))
///       Console.WriteLine(line);
///   byte[] chunk = await viewer.ReadChunkAsync(offset: 1024*1024, size: 65536);
public sealed class HugeFileViewer : IDisposable, IAsyncDisposable
private readonly string _filePath;
if (!File.Exists(filePath))
throw new FileNotFoundException("File not found for encoding detection.", filePath);
if (read >= 4 && bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE && bom[3] == 0xFF)
return new UTF32Encoding(bigEndian: true, byteOrderMark: true);
// UTF-32 BE
if (read >= 4 && bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
return new UTF32Encoding(bigEndian: false, byteOrderMark: true);
// UTF-32 LE
if (read >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
return new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
// UTF-16 BE
if (read >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
return new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
// UTF-16 LE
if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
/// <summary>Read throughput in bytes/second.</summary>
public double BytesPerSecond =>
TotalBytesRead / Math.Max(0.001, Elapsed.TotalSeconds);
int read = fs.Read(bom, 0, bom.Length);
/// <summary>Total bytes read since creation or last reset.</summary>
public long TotalBytesRead => Interlocked.Read(ref _totalBytesRead);
/// <summary>Total lines read since creation or last reset.</summary>
public long TotalLinesRead => Interlocked.Read(ref _totalLinesRead);
// UTF-8 BOM
return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
