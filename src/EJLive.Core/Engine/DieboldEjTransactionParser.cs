using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Diebold Nixdorf / Agilis Electronic Journal parser.
/// Supports fixed-width record format and Agilis extended format with field delimiters.
/// </summary>
public sealed class DieboldEjTransactionParser : IEjTransactionParser
{
    private static readonly Regex AmountRegex = new(@"AMOUNT\s*[:=]?\s*(\d+(?:[.,]\d{1,2})?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CardRegex = new(@"CARD\s*(?:NUMBER|NO)?\s*[:=]?\s*([\d*]{10,19})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StanRegex = new(@"STAN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RrnRegex = new(@"RRN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CassetteRegex = new(@"CASS(?:ETTE)?\s*(\d)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TxStartRegex = new(@"^={3,}|^\*{3,}|^-{5,}\s*TRANSACTION", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

            bool isStart = upper.Contains("TRANSACTION START") ||
                           upper.Contains("[TXN BEGIN]") ||
                           TxStartRegex.IsMatch(line);
            bool isEnd = upper.Contains("TRANSACTION END") ||
                         upper.Contains("[TXN END]") ||
                         upper.Contains("TRANSACTION COMPLETE");

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
        string txId = $"{atmId}-DN-{counter:D5}-{startLine:D6}";

        string card = ExtractFirst(CardRegex, rawLines) ?? string.Empty;
        string stan = ExtractFirst(StanRegex, rawLines) ?? string.Empty;
        string rrn = ExtractFirst(RrnRegex, rawLines) ?? string.Empty;
        decimal? amount = ExtractAmount(rawLines);

        // Try fixed-width extraction for Agilis format
        if (string.IsNullOrEmpty(card) || string.IsNullOrEmpty(stan))
            TryFixedWidthExtraction(rawLines, ref card, ref stan, ref rrn, ref amount);

        int? c1 = ExtractCassette(rawLines, 1);
        int? c2 = ExtractCassette(rawLines, 2);
        int? c3 = ExtractCassette(rawLines, 3);
        int? c4 = ExtractCassette(rawLines, 4);

        var (classification, confidence) = Classify(rawLines, stan, rrn, amount);

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

    /// <summary>
    /// Attempts to parse fields from Diebold/Agilis fixed-width record format.
    /// Agilis uses known column offsets for specific fields.
    /// </summary>
    private static void TryFixedWidthExtraction(
        List<string> lines, ref string card, ref string stan, ref string rrn, ref decimal? amount)
    {
        foreach (var line in lines)
        {
            if (line.Length < 40)
                continue;

            // Agilis extended format: positions vary, but common layout:
            // Pos 0-15: timestamp, 16-35: card (masked), 36-47: amount, 48-59: STAN
            if (string.IsNullOrEmpty(card) && line.Length >= 36)
            {
                var cardField = line.Substring(16, Math.Min(19, line.Length - 16)).Trim();
                if (Regex.IsMatch(cardField, @"^\d[\d*]{8,18}$"))
                    card = cardField;
            }

            if (!amount.HasValue && line.Length >= 48)
            {
                var amtField = line.Substring(36, Math.Min(12, line.Length - 36)).Trim();
                if (decimal.TryParse(amtField, NumberStyles.Any, CultureInfo.InvariantCulture, out var a) && a > 0)
                    amount = a;
            }

            if (string.IsNullOrEmpty(stan) && line.Length >= 60)
            {
                var stanField = line.Substring(48, Math.Min(12, line.Length - 48)).Trim();
                if (Regex.IsMatch(stanField, @"^\d{4,12}$"))
                    stan = stanField;
            }
        }
    }

    private static (TransactionClassification, double) Classify(
        List<string> lines, string stan, string rrn, decimal? amount)
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
        if (upper.Contains("HARDWARE FAULT") || upper.Contains("DEVICE FAULT"))
            return (TransactionClassification.HardwareFault, 0.90);
        if (upper.Contains("DECLINED") || upper.Contains("NOT AUTHORIZED"))
            return (TransactionClassification.HostDeclined, 0.94);
        if ((upper.Contains("APPROVED") || upper.Contains("COMPLETED")) &&
            (upper.Contains("DISPENSED") || upper.Contains("NOTES TAKEN")))
            return (TransactionClassification.Success, 0.95);
        if (upper.Contains("APPROVED") && !upper.Contains("DISPENSED"))
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
