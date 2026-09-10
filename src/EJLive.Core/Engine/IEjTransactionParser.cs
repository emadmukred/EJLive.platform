using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Parses raw Electronic Journal lines into structured <see cref="EjTransaction"/> records.
/// </summary>
public interface IEjTransactionParser
{
    /// <summary>
    /// Parses the provided raw EJ lines into a list of transactions.
    /// </summary>
    /// <param name="lines">The raw lines from an electronic journal file.</param>
    /// <param name="atmId">The ATM identifier associated with this journal.</param>
    /// <returns>A list of parsed transactions with forensic metadata.</returns>
    List<EjTransaction> Parse(List<string> lines, string atmId);
}

/// <summary>
/// Lossless fallback used when no vendor-specific parser is registered.
/// It preserves the complete input as one low-confidence transaction so an
/// unknown vendor never causes journal data to disappear.
/// </summary>
public sealed class PreservingFallbackEjTransactionParser : IEjTransactionParser
{
    public List<EjTransaction> Parse(List<string> lines, string atmId)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);

        if (lines.Count == 0)
            return new List<EjTransaction>();

        return new List<EjTransaction>
        {
            new(
                TransactionId: $"{atmId}-UNCLASSIFIED-00000",
                StartLine: 0,
                EndLine: lines.Count - 1,
                ATM_ID: atmId,
                CardNumber: string.Empty,
                AccountNumber: string.Empty,
                Amount: null,
                Currency: string.Empty,
                STAN: string.Empty,
                RRN: string.Empty,
                Cassette1: null,
                Cassette2: null,
                Cassette3: null,
                Cassette4: null,
                MCode: string.Empty,
                RCode: string.Empty,
                RawLines: new List<string>(lines),
                Classification: TransactionClassification.Suspicious,
                Confidence: 0,
                Timestamp: DateTime.UtcNow)
        };
    }
}
