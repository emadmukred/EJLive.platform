using System.Collections.Generic;
  using EJLive.Core.Models;

  namespace EJLive.Core.Xfs;

  /// <summary>
  /// Adapts vendor-specific XFS log formats into normalized vendor events.
  /// Supports both file-based parsing (Engine path) and line-by-line streaming (Adapters path).
  /// </summary>
  public interface IXfsVendorAdapter
  {
      /// <summary>
      /// The vendor this adapter handles (e.g., NCR, GRG, Diebold, Hyosung).
      /// </summary>
      XfsVendor Vendor { get; }

      // ── File-based API ──────────────────────────────────────────────────────
      /// <summary>
      /// Determines whether this adapter can handle the specified source file path or name.
      /// </summary>
      bool CanHandle(string sourceFile);

      /// <summary>
      /// Parses the raw log lines from a file into a list of normalized vendor events.
      /// </summary>
      /// <param name="sourceFile">The path or name of the source log file.</param>
      /// <param name="lines">The raw lines from the log file.</param>
      List<NormalizedVendorEvent> Parse(string sourceFile, List<string> lines);

      // ── Streaming / line-by-line API ────────────────────────────────────────
      /// <summary>
      /// Determines whether this adapter can handle the specified single raw journal line.
      /// Used by the streaming pipeline for per-line routing decisions.
      /// </summary>
      bool CanHandleLine(string line);

      /// <summary>
      /// Parses an enumerable of raw journal lines and returns normalized events.
      /// Suitable for streaming pipelines where the full file path may be unavailable.
      /// </summary>
      IReadOnlyList<XfsNormalizedEvent> ParseLines(IEnumerable<string> lines);
  }
  