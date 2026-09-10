using System.Security.Cryptography;
using System.Text;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Thread-safe in-memory inquiry index for parsed journal transactions.
/// Sensitive account identifiers are indexed by digest and are never copied into index keys.
/// </summary>
public sealed class TransactionInquiryEngine
{
    private readonly Dictionary<string, List<EjTransaction>> _transactionsByAtm = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<EjTransaction>> _stanIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<EjTransaction>> _rrnIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<EjTransaction>> _cardIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<EjTransaction>> _accountIndex = new(StringComparer.Ordinal);
    private readonly HashSet<string> _fingerprints = new(StringComparer.Ordinal);
    private readonly object _syncRoot = new();

    /// <summary>Indexes a transaction once. Replaying the same transaction is idempotent.</summary>
    public bool Index(EjTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        var atmId = NormalizeRequired(transaction.ATM_ID, nameof(transaction.ATM_ID));
        var fingerprint = BuildFingerprint(transaction, atmId);

        lock (_syncRoot)
        {
            if (!_fingerprints.Add(fingerprint))
                return false;

            AddToIndex(_transactionsByAtm, atmId, transaction);
            AddToIndex(_stanIndex, NormalizeOptional(transaction.STAN), transaction);
            AddToIndex(_rrnIndex, NormalizeOptional(transaction.RRN), transaction);

            var cardKey = NormalizeCard(transaction.CardNumber);
            AddToIndex(_cardIndex, cardKey, transaction);

            var accountKey = DigestIdentifier(transaction.AccountNumber);
            AddToIndex(_accountIndex, accountKey, transaction);
            return true;
        }
    }

    public IReadOnlyList<EjTransaction> QueryByStan(string stan) => QueryIndex(_stanIndex, NormalizeRequired(stan, nameof(stan)));

    public IReadOnlyList<EjTransaction> QueryByRrn(string rrn) => QueryIndex(_rrnIndex, NormalizeRequired(rrn, nameof(rrn)));

    public IReadOnlyList<EjTransaction> QueryByCard(string cardNumber)
    {
        var key = NormalizeCard(NormalizeRequired(cardNumber, nameof(cardNumber)));
        return QueryIndex(_cardIndex, key);
    }

    public IReadOnlyList<EjTransaction> QueryByAccount(string accountNumber)
    {
        var key = DigestIdentifier(NormalizeRequired(accountNumber, nameof(accountNumber)));
        return QueryIndex(_accountIndex, key);
    }

    public IReadOnlyList<EjTransaction> QueryByAtmAndDate(string atmId, DateTime fromUtc, DateTime toUtc)
    {
        var key = NormalizeRequired(atmId, nameof(atmId));
        fromUtc = NormalizeUtc(fromUtc, nameof(fromUtc));
        toUtc = NormalizeUtc(toUtc, nameof(toUtc));
        if (toUtc < fromUtc)
            throw new ArgumentException("End timestamp must not precede start timestamp.", nameof(toUtc));

        lock (_syncRoot)
        {
            if (!_transactionsByAtm.TryGetValue(key, out var transactions))
                return Array.Empty<EjTransaction>();

            return transactions
                .Where(transaction => NormalizeUtc(transaction.Timestamp, nameof(transaction.Timestamp)) >= fromUtc &&
                                      NormalizeUtc(transaction.Timestamp, nameof(transaction.Timestamp)) <= toUtc)
                .OrderBy(transaction => transaction.Timestamp)
                .ToArray();
        }
    }

    public IReadOnlyList<EjTransaction> QueryByClassification(TransactionClassification classification)
    {
        lock (_syncRoot)
        {
            return _transactionsByAtm.Values
                .SelectMany(transactions => transactions)
                .Where(transaction => transaction.Classification == classification)
                .OrderByDescending(transaction => transaction.Timestamp)
                .ToArray();
        }
    }

    public IReadOnlyList<EjTransaction> QueryByTransactionId(string atmId, string transactionIdFragment)
    {
        var key = NormalizeRequired(atmId, nameof(atmId));
        var fragment = NormalizeRequired(transactionIdFragment, nameof(transactionIdFragment));

        lock (_syncRoot)
        {
            if (!_transactionsByAtm.TryGetValue(key, out var transactions))
                return Array.Empty<EjTransaction>();

            return transactions
                .Where(transaction => transaction.TransactionId.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                .OrderBy(transaction => transaction.Timestamp)
                .ToArray();
        }
    }

    public int TotalIndexedCount
    {
        get
        {
            lock (_syncRoot)
                return _fingerprints.Count;
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _transactionsByAtm.Clear();
            _stanIndex.Clear();
            _rrnIndex.Clear();
            _cardIndex.Clear();
            _accountIndex.Clear();
            _fingerprints.Clear();
        }
    }

    private IReadOnlyList<EjTransaction> QueryIndex(
        IReadOnlyDictionary<string, List<EjTransaction>> index,
        string key)
    {
        if (string.IsNullOrEmpty(key))
            return Array.Empty<EjTransaction>();

        lock (_syncRoot)
        {
            return index.TryGetValue(key, out var transactions)
                ? transactions.OrderByDescending(transaction => transaction.Timestamp).ToArray()
                : Array.Empty<EjTransaction>();
        }
    }

    private static void AddToIndex(
        IDictionary<string, List<EjTransaction>> index,
        string key,
        EjTransaction transaction)
    {
        if (string.IsNullOrEmpty(key))
            return;
        if (!index.TryGetValue(key, out var transactions))
        {
            transactions = new List<EjTransaction>();
            index[key] = transactions;
        }
        transactions.Add(transaction);
    }

    private static string BuildFingerprint(EjTransaction transaction, string atmId)
    {
        var value = string.Join('\u001f',
            atmId,
            transaction.TransactionId,
            transaction.STAN,
            transaction.RRN,
            transaction.Timestamp.ToUniversalTime().Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture),
            transaction.StartLine.ToString(System.Globalization.CultureInfo.InvariantCulture),
            transaction.EndLine.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static string NormalizeCard(string? value)
    {
        var normalized = NormalizeOptional(value)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);
        if (normalized.Length < 4)
            return string.Empty;

        if (normalized.Length <= 10)
            return normalized;

        return normalized[..6] + "******" + normalized[^4..];
    }

    private static string DigestIdentifier(string? value)
    {
        var normalized = NormalizeOptional(value);
        return normalized.Length == 0
            ? string.Empty
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private static string NormalizeRequired(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required.", parameterName);
        return value.Trim();
    }

    private static string NormalizeOptional(string? value) => value?.Trim() ?? string.Empty;

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
            throw new ArgumentException("Timestamp is required.", parameterName);
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
