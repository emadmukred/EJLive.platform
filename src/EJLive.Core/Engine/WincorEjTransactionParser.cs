using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Wincor Nixdorf / ProView Electronic Journal parser.
/// Handles both structured XML-like entries and ProView trace format with daily files.
/// </summary>
public sealed class WincorEjTransactionParser : IEjTransactionParser
{
    private static readonly Regex XmlTagRegex = new(@"<(\w+)>(.*?)</\1>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AmountRegex = new(@"(?:AMOUNT|BETRAG|DISPENSE)\s*[:=]?\s*(\d+(?:[.,]\d{1,2})?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CardRegex = new(@"(?:CARD|KARTE)\s*(?:NUMBER|NR)?\s*[:=]?\s*([\d*]{10,19})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StanRegex = new(@"STAN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RrnRegex = new(@"RRN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CassetteRegex = new(@"(?:CASS|LCU)\s*(\d)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

            bool isStart = upper.Contains("<TRANSACTION>") ||
                           upper.Contains("***TRANSACTION START***") ||
                           upper.Contains("== TRANSACTION BEGIN ==") ||
                           upper.Contains("TRANSACTION STARTED");
            bool isEnd = upper.Contains("</TRANSACTION>") ||
                         upper.Contains("***TRANSACTION END***") ||
                         upper.Contains("== TRANSACTION END ==") ||
                         upper.Contains("TRANSACTION COMPLETED");

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
            transactions.Add(BuildTransaction(currentBlock, atmId, counter++));

        return transactions;
    }

    private static EjTransaction BuildTransaction(List<(string Line, int Index)> block, string atmId, int counter)
    {
        var rawLines = block.Select(b => b.Line).ToList();
        int startLine = block.First().Index;
        int endLine = block.Last().Index;
        string txId = $"{atmId}-WN-{counter:D5}-{startLine:D6}";

        // Try XML-style extraction first, then fall back to regex
        var xmlFields = ExtractXmlFields(rawLines);

        string card = xmlFields.GetValueOrDefault("CardNumber") ?? xmlFields.GetValueOrDefault("Card")
            ?? ExtractFirst(CardRegex, rawLines) ?? string.Empty;
        string stan = xmlFields.GetValueOrDefault("STAN") ?? ExtractFirst(StanRegex, rawLines) ?? string.Empty;
        string rrn = xmlFields.GetValueOrDefault("RRN") ?? ExtractFirst(RrnRegex, rawLines) ?? string.Empty;
        string status = xmlFields.GetValueOrDefault("Status") ?? string.Empty;
        string currency = xmlFields.GetValueOrDefault("Currency") ?? string.Empty;

        decimal? amount = null;
        if (xmlFields.TryGetValue("Amount", out var amtStr) &&
            decimal.TryParse(amtStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var a))
            amount = a;
        amount ??= ExtractAmount(rawLines);

        int? c1 = ExtractCassette(rawLines, 1);
        int? c2 = ExtractCassette(rawLines, 2);
        int? c3 = ExtractCassette(rawLines, 3);
        int? c4 = ExtractCassette(rawLines, 4);

        var (classification, confidence) = Classify(rawLines, status, stan, rrn, amount);

        return new EjTransaction(
            TransactionId: txId,
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
            Timestamp: DateTime.UtcNow
        );
    }

    private static Dictionary<string, string> ExtractXmlFields(List<string> lines)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            foreach (Match match in XmlTagRegex.Matches(line))
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(value))
                    fields[key] = value;
            }
        }
        return fields;
    }

    private static (TransactionClassification, double) Classify(
        List<string> lines, string status, string stan, string rrn, decimal? amount)
    {
        var upper = string.Join(" ", lines.Select(l => l.ToUpperInvariant()));

        if (upper.Contains("REVERSAL") || upper.Contains("STORNO"))
            return (TransactionClassification.Reversal, 0.95);
        if (upper.Contains("JAM") || upper.Contains("JAMMED"))
            return (TransactionClassification.CashJam, 0.93);
        if (upper.Contains("CARD CAPTURED") || upper.Contains("CARD RETAINED") || upper.Contains("KARTE EINGEZOGEN"))
            return (TransactionClassification.CardCaptured, 0.94);
        if (upper.Contains("RETRACT"))
            return (TransactionClassification.Retract, 0.90);
        if (upper.Contains("HARDWARE") && (upper.Contains("FAULT") || upper.Contains("ERROR")))
            return (TransactionClassification.HardwareFault, 0.90);

        if (status.Contains("DECLINED", StringComparison.OrdinalIgnoreCase) ||
            upper.Contains("DECLINED") || upper.Contains("ABGELEHNT"))
            return (TransactionClassification.HostDeclined, 0.94);

        if (status.Contains("APPROVED", StringComparison.OrdinalIgnoreCase) ||
            upper.Contains("APPROVED") || upper.Contains("GENEHMIGT"))
        {
            if (upper.Contains("DISPENSED") || upper.Contains("AUSGEGEBEN") ||
                upper.Contains("NOTES TAKEN") || upper.Contains("TOOK NOTES") ||
                upper.Contains("CDM DISPENSE"))
                return (TransactionClassification.Success, 0.95);
            return (TransactionClassification.ApprovedNoDispense, 0.85);
        }

        if (string.IsNullOrEmpty(stan))
            return (TransactionClassification.MissingSTAN, 0.75);

        return (TransactionClassification.Failed, 0.60);
    }

    private static string? ExtractFirst(Regex regex, List<string> lines)
    {
        foreach (var line in lines)
        {
            var match = regex.Match(line);
            if (match.Success) return match.Groups[1].Value.Trim();
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
                NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
                return r;
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
