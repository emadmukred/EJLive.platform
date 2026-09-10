using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Cashway Electronic Journal parser.
/// Supports block markers and line-level status events used in Cashway field deployments.
/// </summary>
public sealed class CashwayEjTransactionParser : IEjTransactionParser
{
    private static readonly Regex AmountRegex = new(@"AMOUNT\s*[:=]?\s*(\d+(?:[.,]\d{1,2})?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CurrencyRegex = new(@"CURRENCY\s*[:=]?\s*([A-Z]{3})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StanRegex = new(@"STAN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RrnRegex = new(@"RRN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CardRegex = new(@"CARD\s*(?:NUMBER|NO)?\s*[:=]?\s*([\d*]{10,19})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CassetteRegex = new(@"(?:CASS(?:ETTE)?|CAS)\s*(\d)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public List<EjTransaction> Parse(List<string> lines, string atmId)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);

        var transactions = new List<EjTransaction>();
        var currentBlock = new List<(string Line, int Index)>();
        var inTransaction = false;
        var counter = 0;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var upper = line.ToUpperInvariant();

            var isStart = upper.Contains("[TXN_START]", StringComparison.Ordinal)
                          || upper.Contains("CASHWAY TX START", StringComparison.Ordinal)
                          || upper.Contains("TRANSACTION START", StringComparison.Ordinal);
            var isEnd = upper.Contains("[TXN_END]", StringComparison.Ordinal)
                        || upper.Contains("CASHWAY TX END", StringComparison.Ordinal)
                        || upper.Contains("TRANSACTION END", StringComparison.Ordinal);

            if (isStart)
            {
                if (currentBlock.Count > 0 && inTransaction)
                    transactions.Add(BuildTransaction(currentBlock, atmId, counter++));

                currentBlock = new List<(string, int)> { (line, i) };
                inTransaction = true;
                continue;
            }

            if (isEnd)
            {
                currentBlock.Add((line, i));
                transactions.Add(BuildTransaction(currentBlock, atmId, counter++));
                currentBlock = new List<(string, int)>();
                inTransaction = false;
                continue;
            }

            if (inTransaction)
                currentBlock.Add((line, i));
        }

        if (currentBlock.Count > 0 && inTransaction)
            transactions.Add(BuildTransaction(currentBlock, atmId, counter));

        return transactions;
    }

    private static EjTransaction BuildTransaction(List<(string Line, int Index)> block, string atmId, int counter)
    {
        var rawLines = block.Select(x => x.Line).ToList();
        var startLine = block.First().Index;
        var endLine = block.Last().Index;
        var transactionId = $"{atmId}-CW-{counter:D5}-{startLine:D6}";

        var stan = ExtractFirst(StanRegex, rawLines) ?? string.Empty;
        var rrn = ExtractFirst(RrnRegex, rawLines) ?? string.Empty;
        var card = ExtractFirst(CardRegex, rawLines) ?? string.Empty;
        var currency = ExtractFirst(CurrencyRegex, rawLines) ?? string.Empty;
        var amount = ExtractAmount(rawLines);

        var c1 = ExtractCassette(rawLines, 1);
        var c2 = ExtractCassette(rawLines, 2);
        var c3 = ExtractCassette(rawLines, 3);
        var c4 = ExtractCassette(rawLines, 4);

        var (classification, confidence) = Classify(rawLines, stan, rrn);

        return new EjTransaction(
            TransactionId: transactionId,
            StartLine: startLine,
            EndLine: endLine,
            ATM_ID: atmId,
            CardNumber: MaskCard(card),
            AccountNumber: string.Empty,
            Amount: amount,
            Currency: currency,
            STAN: stan,
            RRN: rrn,
            Cassette1: c1,
            Cassette2: c2,
            Cassette3: c3,
            Cassette4: c4,
            MCode: string.Empty,
            RCode: string.Empty,
            RawLines: rawLines,
            Classification: classification,
            Confidence: confidence,
            Timestamp: DateTime.UtcNow);
    }

    private static (TransactionClassification Classification, double Confidence) Classify(
        List<string> lines,
        string stan,
        string rrn)
    {
        var upper = string.Join(" ", lines.Select(x => x.ToUpperInvariant()));
        var hasPresented = upper.Contains("NOTES PRESENTED", StringComparison.Ordinal) || upper.Contains("DISPENSED", StringComparison.Ordinal);
        var hasTaken = upper.Contains("NOTES TAKEN", StringComparison.Ordinal) || upper.Contains("TAKEN", StringComparison.Ordinal);

        if (upper.Contains("REVERSAL", StringComparison.Ordinal))
            return (TransactionClassification.Reversal, 0.95);
        if (upper.Contains("JAM", StringComparison.Ordinal))
            return (TransactionClassification.CashJam, 0.93);
        if (upper.Contains("CAPTURED", StringComparison.Ordinal) || upper.Contains("RETAIN", StringComparison.Ordinal))
            return (TransactionClassification.CardCaptured, 0.94);
        if (upper.Contains("RETRACT", StringComparison.Ordinal))
            return (TransactionClassification.Retract, 0.90);
        if (upper.Contains("FAULT", StringComparison.Ordinal) || upper.Contains("HARDWARE ERROR", StringComparison.Ordinal))
            return (TransactionClassification.HardwareFault, 0.90);
        if (upper.Contains("DECLINED", StringComparison.Ordinal) || upper.Contains("NOT AUTHORIZED", StringComparison.Ordinal))
            return (TransactionClassification.HostDeclined, 0.94);
        if (hasPresented && hasTaken)
            return (TransactionClassification.Success, 0.95);
        if (upper.Contains("APPROVED", StringComparison.Ordinal) && !(hasPresented && hasTaken))
            return (TransactionClassification.ApprovedNoDispense, 0.85);
        if (hasPresented && !hasTaken)
            return (TransactionClassification.PartialDispense, 0.84);
        if (string.IsNullOrWhiteSpace(stan))
            return (TransactionClassification.MissingSTAN, 0.75);
        if (string.IsNullOrWhiteSpace(rrn))
            return (TransactionClassification.MissingRRN, 0.75);

        return (TransactionClassification.Failed, 0.60);
    }

    private static string? ExtractFirst(Regex regex, List<string> lines)
    {
        foreach (var line in lines)
        {
            var match = regex.Match(line);
            if (match.Success)
                return match.Groups[1].Value.Trim();
        }

        return null;
    }

    private static decimal? ExtractAmount(List<string> lines)
    {
        foreach (var line in lines)
        {
            var match = AmountRegex.Match(line);
            if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
        }

        return null;
    }

    private static int? ExtractCassette(List<string> lines, int cassetteNumber)
    {
        foreach (var line in lines)
        {
            var match = CassetteRegex.Match(line);
            if (!match.Success)
                continue;

            if (int.TryParse(match.Groups[1].Value, out var parsedCassette) && parsedCassette == cassetteNumber &&
                int.TryParse(match.Groups[2].Value, out var count))
                return count;
        }

        return null;
    }

    private static string MaskCard(string card)
    {
        if (string.IsNullOrWhiteSpace(card) || card.Length <= 8)
            return card;

        return card[..6] + new string('*', card.Length - 10) + card[^4..];
    }
}
