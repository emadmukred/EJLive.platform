using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Hyosung Electronic Journal parser.
/// Hyosung uses pipe-delimited format: TRAN_TYPE|CARD|AMOUNT|STATUS|TIMESTAMP
/// and also supports structured block format with markers.
/// </summary>
public sealed class HyosungEjTransactionParser : IEjTransactionParser
{
    private static readonly Regex PipeDelimitedRegex = new(@"^([^|]+)\|([^|]*)\|([^|]*)\|([^|]*)\|(.*)$", RegexOptions.Compiled);
    private static readonly Regex AmountRegex = new(@"AMOUNT\s*[:=]?\s*(\d+(?:[.,]\d{1,2})?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CardRegex = new(@"CARD\s*(?:NUMBER|NO)?\s*[:=]?\s*([\d*]{10,19})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StanRegex = new(@"STAN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RrnRegex = new(@"RRN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CassetteRegex = new(@"CASS(?:ETTE)?\s*(\d)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public List<EjTransaction> Parse(List<string> lines, string atmId)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);

        // Detect format: pipe-delimited (single-line transactions) vs block format
        bool isPipeFormat = lines.Any(l => PipeDelimitedRegex.IsMatch(l.Trim()));
        return isPipeFormat
            ? ParsePipeDelimited(lines, atmId)
            : ParseBlockFormat(lines, atmId);
    }

    private static List<EjTransaction> ParsePipeDelimited(List<string> lines, string atmId)
    {
        var transactions = new List<EjTransaction>();
        int counter = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            var match = PipeDelimitedRegex.Match(lines[i].Trim());
            if (!match.Success)
                continue;

            string txType = match.Groups[1].Value.Trim();
            string card = match.Groups[2].Value.Trim();
            string amtStr = match.Groups[3].Value.Trim();
            string status = match.Groups[4].Value.Trim();
            string timestamp = match.Groups[5].Value.Trim();

            decimal? amount = decimal.TryParse(amtStr.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a : null;

            var classification = ClassifyStatus(status, txType);
            string txId = $"{atmId}-HY-{counter:D5}-{i:D6}";

            transactions.Add(new EjTransaction(
                TransactionId: txId,
                StartLine: i,
                EndLine: i,
                ATM_ID: atmId,
                CardNumber: MaskCard(card),
                AccountNumber: string.Empty,
                Amount: amount,
                Currency: string.Empty,
                STAN: string.Empty,
                RRN: string.Empty,
                Cassette1: null,
                Cassette2: null,
                Cassette3: null,
                Cassette4: null,
                MCode: string.Empty,
                RCode: string.Empty,
                RawLines: new List<string> { lines[i] },
                Classification: classification.Item1,
                Confidence: classification.Item2,
                Timestamp: DateTime.UtcNow
            ));
            counter++;
        }

        return transactions;
    }

    private static List<EjTransaction> ParseBlockFormat(List<string> lines, string atmId)
    {
        var transactions = new List<EjTransaction>();
        var currentBlock = new List<(string Line, int Index)>();
        bool inTransaction = false;
        int counter = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            string upper = lines[i].ToUpperInvariant();

            if (upper.Contains("TRANSACTION START") || upper.Contains("[TXN START]") || upper.Contains("==BEGIN=="))
            {
                if (currentBlock.Count > 0 && inTransaction)
                    transactions.Add(BuildBlockTransaction(currentBlock, atmId, counter++));

                currentBlock = new List<(string, int)> { (lines[i], i) };
                inTransaction = true;
                continue;
            }

            if (upper.Contains("TRANSACTION END") || upper.Contains("[TXN END]") || upper.Contains("==END=="))
            {
                currentBlock.Add((lines[i], i));
                transactions.Add(BuildBlockTransaction(currentBlock, atmId, counter++));
                currentBlock = new List<(string, int)>();
                inTransaction = false;
                continue;
            }

            if (inTransaction)
                currentBlock.Add((lines[i], i));
        }

        if (currentBlock.Count > 0 && inTransaction)
            transactions.Add(BuildBlockTransaction(currentBlock, atmId, counter++));

        return transactions;
    }

    private static EjTransaction BuildBlockTransaction(List<(string Line, int Index)> block, string atmId, int counter)
    {
        var rawLines = block.Select(b => b.Line).ToList();
        int startLine = block.First().Index;
        int endLine = block.Last().Index;
        string txId = $"{atmId}-HY-{counter:D5}-{startLine:D6}";

        string card = ExtractFirst(CardRegex, rawLines) ?? string.Empty;
        string stan = ExtractFirst(StanRegex, rawLines) ?? string.Empty;
        string rrn = ExtractFirst(RrnRegex, rawLines) ?? string.Empty;
        decimal? amount = ExtractAmount(rawLines);

        int? c1 = ExtractCassette(rawLines, 1);
        int? c2 = ExtractCassette(rawLines, 2);
        int? c3 = ExtractCassette(rawLines, 3);
        int? c4 = ExtractCassette(rawLines, 4);

        var (classification, confidence) = ClassifyBlock(rawLines, stan);

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

    private static (TransactionClassification, double) ClassifyStatus(string status, string txType)
    {
        var upper = status.ToUpperInvariant();
        var txUpper = txType.ToUpperInvariant();

        if (upper.Contains("APPROVED") || upper == "OK" || upper == "SUCCESS")
            return (TransactionClassification.Success, 0.92);
        if (upper.Contains("DECLINED") || upper.Contains("REJECTED"))
            return (TransactionClassification.HostDeclined, 0.94);
        if (upper.Contains("REVERSED") || upper.Contains("REVERSAL"))
            return (TransactionClassification.Reversal, 0.95);
        if (upper.Contains("JAM"))
            return (TransactionClassification.CashJam, 0.93);
        if (upper.Contains("CAPTURED"))
            return (TransactionClassification.CardCaptured, 0.94);
        if (upper.Contains("ERROR") || upper.Contains("FAULT"))
            return (TransactionClassification.HardwareFault, 0.88);

        return (TransactionClassification.Failed, 0.65);
    }

    private static (TransactionClassification, double) ClassifyBlock(List<string> lines, string stan)
    {
        var upper = string.Join(" ", lines.Select(l => l.ToUpperInvariant()));

        if (upper.Contains("REVERSAL"))
            return (TransactionClassification.Reversal, 0.95);
        if (upper.Contains("JAM"))
            return (TransactionClassification.CashJam, 0.93);
        if (upper.Contains("CARD CAPTURED"))
            return (TransactionClassification.CardCaptured, 0.94);
        if (upper.Contains("RETRACT"))
            return (TransactionClassification.Retract, 0.90);
        if (upper.Contains("DECLINED"))
            return (TransactionClassification.HostDeclined, 0.94);
        if (upper.Contains("APPROVED") && upper.Contains("DISPENSED"))
            return (TransactionClassification.Success, 0.95);
        if (upper.Contains("APPROVED"))
            return (TransactionClassification.ApprovedNoDispense, 0.85);

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
