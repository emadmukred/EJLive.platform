using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// GRG-specific Electronic Journal and TRACE transaction parser.
/// GRG journals use "---BEGIN TRANSACTION---" / "---END TRANSACTION---" markers
/// and a TRACE format with timestamped lines.
/// </summary>
public sealed class GrgEjTransactionParser : IEjTransactionParser
{
    private static readonly Regex TimestampRegex = new(@"(\d{4}[-/]\d{2}[-/]\d{2}\s+\d{2}:\d{2}:\d{2})", RegexOptions.Compiled);
    private static readonly Regex AmountRegex = new(@"AMOUNT\s*[:=]?\s*(\d+(?:[.,]\d{1,2})?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CardRegex = new(@"CARD\s*(?:NUMBER|NO)?\s*[:=]?\s*([\d*]{10,19})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StanRegex = new(@"STAN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RrnRegex = new(@"RRN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CassetteRegex = new(@"CASS(?:ETTE)?\s*(\d)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DeviceStatusRegex = new(@"DEVICE\s*STATUS\s*[:=]?\s*(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public List<EjTransaction> Parse(List<string> lines, string atmId)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);

        var transactions = new List<EjTransaction>();
        var currentBlock = new List<(string Line, int Index)>();
        bool inTransaction = false;
        int counter = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            string upper = line.ToUpperInvariant();

            if (upper.Contains("---BEGIN TRANSACTION---") ||
                upper.Contains("BEGIN TRANSACTION") ||
                upper.Contains("TRANSACTION REQUEST"))
            {
                if (currentBlock.Count > 0 && inTransaction)
                    transactions.Add(BuildTransaction(currentBlock, atmId, counter++));

                currentBlock = new List<(string, int)> { (line, i) };
                inTransaction = true;
                continue;
            }

            if (upper.Contains("---END TRANSACTION---") ||
                upper.Contains("END TRANSACTION") ||
                upper.Contains("TRANSACTION REPLY NEXT"))
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
            transactions.Add(BuildTransaction(currentBlock, atmId, counter++));

        return transactions;
    }

    private static EjTransaction BuildTransaction(List<(string Line, int Index)> block, string atmId, int counter)
    {
        var rawLines = block.Select(b => b.Line).ToList();
        int startLine = block.First().Index;
        int endLine = block.Last().Index;
        string txId = $"{atmId}-GRG-{counter:D5}-{startLine:D6}";

        string card = ExtractFirst(CardRegex, rawLines) ?? string.Empty;
        string stan = ExtractFirst(StanRegex, rawLines) ?? string.Empty;
        string rrn = ExtractFirst(RrnRegex, rawLines) ?? string.Empty;
        decimal? amount = ExtractAmount(rawLines);
        string deviceStatus = ExtractFirst(DeviceStatusRegex, rawLines) ?? string.Empty;

        int? c1 = ExtractCassette(rawLines, 1);
        int? c2 = ExtractCassette(rawLines, 2);
        int? c3 = ExtractCassette(rawLines, 3);
        int? c4 = ExtractCassette(rawLines, 4);

        var (classification, confidence) = Classify(rawLines, stan, rrn, amount, deviceStatus);

        return new EjTransaction(
            TransactionId: txId,
            StartLine: startLine,
            EndLine: endLine,
            ATM_ID: atmId,
            CardNumber: MaskCard(card),
            AccountNumber: string.Empty,
            Amount: amount,
            Currency: string.Empty,
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
            Timestamp: DateTime.UtcNow
        );
    }

    private static (TransactionClassification, double) Classify(
        List<string> lines, string stan, string rrn, decimal? amount, string deviceStatus)
    {
        var upper = string.Join(" ", lines.Select(l => l.ToUpperInvariant()));

        if (upper.Contains("REVERSAL"))
            return (TransactionClassification.Reversal, 0.95);
        if (upper.Contains("JAM") || upper.Contains("JAMMED"))
            return (TransactionClassification.CashJam, 0.93);
        if (upper.Contains("CARD CAPTURED") || upper.Contains("CARD RETAINED"))
            return (TransactionClassification.CardCaptured, 0.94);
        if (upper.Contains("RETRACT"))
            return (TransactionClassification.Retract, 0.90);
        if (upper.Contains("HARDWARE") && upper.Contains("FAULT"))
            return (TransactionClassification.HardwareFault, 0.90);
        if (upper.Contains("DECLINED") || upper.Contains("NOT AUTHORIZED") || upper.Contains("NOT AUTHORISED"))
            return (TransactionClassification.HostDeclined, 0.94);
        if ((upper.Contains("APPROVED") && upper.Contains("DISPENSED")) ||
            (upper.Contains("DISPENSE SUCCESS") && upper.Contains("NOTES TAKEN")))
            return (TransactionClassification.Success, 0.95);
        if (upper.Contains("APPROVED") && !upper.Contains("DISPENSED"))
            return (TransactionClassification.ApprovedNoDispense, 0.85);
        if (upper.Contains("STATUS") && upper.Contains("OK") && amount.HasValue)
            return (TransactionClassification.Success, 0.88);

        if (string.IsNullOrEmpty(stan))
            return (TransactionClassification.MissingSTAN, 0.75);

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
            if (match.Success && decimal.TryParse(
                match.Groups[1].Value.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;
        }
        return null;
    }

    private static int? ExtractCassette(List<string> lines, int num)
    {
        foreach (var line in lines)
        {
            var match = CassetteRegex.Match(line);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int n) && n == num)
                if (int.TryParse(match.Groups[2].Value, out int count))
                    return count;
        }
        return null;
    }

    private static string MaskCard(string card)
    {
        if (string.IsNullOrEmpty(card) || card.Length <= 8)
            return card;
        return card[..6] + new string('*', card.Length - 10) + card[^4..];
    }
}
