using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Adapts vendor-specific XFS log formats into normalized vendor events.
/// </summary>
public interface ICorrelationEventAdapter
{
    /// <summary>
    /// Determines whether this adapter can handle the specified source file.
    /// </summary>
    /// <param name="sourceFile">The path or name of the log file.</param>
    /// <returns><c>true</c> if this adapter supports the file format; otherwise <c>false</c>.</returns>
    bool CanHandle(string sourceFile);

    /// <summary>
    /// Parses the raw log lines into a list of normalized vendor events.
    /// </summary>
    /// <param name="sourceFile">The path or name of the source log file.</param>
    /// <param name="lines">The raw lines from the log file.</param>
    /// <returns>A list of normalized vendor events.</returns>
    List<NormalizedVendorEvent> Parse(string sourceFile, List<string> lines);
}
