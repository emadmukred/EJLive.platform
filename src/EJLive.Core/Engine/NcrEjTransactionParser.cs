using System.Globalization;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// NCR-specific Electronic Journal transaction parser using a finite state machine.
/// </summary>
public sealed class NcrEjTransactionParser : IEjTransactionParser
{
    private enum State
    {
        Idle,
        TransactionStart,
        CardInserted,
        PinEntered,
        AtrReceived,
        GenAc1,
        GenAc2,
        NotesStacked,
        NotesPresented,
        NotesTaken,
        TransactionEnd
    }

    private static readonly Regex AmountRegex = new(@"AMOUNT\s*[:=]?\s*(\d+(?:[.,]\d{1,2})?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CurrencyRegex = new(@"CURRENCY\s*[:=]?\s*([A-Z]{3})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StanRegex = new(@"STAN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RrnRegex = new(@"RRN\s*[:=]?\s*(\d{1,12})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AccountRegex = new(@"ACCOUNT\s*[:=]?\s*(\d{4,20})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MaskedCardRegex = new(@"CARD\s*(?:NUMBER)?\s*[:=]?\s*(\d{4,6}[*]+\d{1,4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CassetteRegex = new(@"CASS\??ETTE\s*(\d)\s*[:=]?\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MCodeRegex = new(@"M[-_]?CODE\s*[:=]?\s*(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RCodeRegex = new(@"R[-_]?CODE\s*[:=]?\s*(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public List<EjTransaction> Parse(List<string> lines, string atmId)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);

        var transactions = new List<EjTransaction>();
        var currentBlock = new List<(string Line, int Index)>();
        State state = State.Idle;
        int transactionCounter = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            string upper = line.ToUpperInvariant();

            bool isStart = upper.Contains("*TRANSACTION START*", StringComparison.Ordinal);
            bool isEnd = upper.Contains("TRANSACTION END", StringComparison.Ordinal);

            if (isStart)
            {
                // If we were already collecting, finalize the previous block first
                if (currentBlock.Count > 0 && state != State.Idle)
                {
                    transactions.Add(BuildTransaction(currentBlock, atmId, transactionCounter++));
                }

                currentBlock = new List<(string, int)> { (line, i) };
                state = State.TransactionStart;
                continue;
            }

            if (isEnd)
            {
                currentBlock.Add((line, i));
                state = State.TransactionEnd;
                transactions.Add(BuildTransaction(currentBlock, atmId, transactionCounter++));
                currentBlock = new List<(string, int)>();
                state = State.Idle;
                continue;
            }

            if (state != State.Idle)
            {
                currentBlock.Add((line, i));
                state = TransitionState(state, upper);
            }
        }

        // Handle dangling block
        if (currentBlock.Count > 0 && state != State.Idle)
        {
            transactions.Add(BuildTransaction(currentBlock, atmId, transactionCounter++));
        }

        return transactions;
    }

    private static State TransitionState(State current, string upper)
    {
        if (upper.Contains("CARD INSERTED"))
            return State.CardInserted;
        if (upper.Contains("PIN ENTERED"))
            return State.PinEntered;
        if (upper.Contains("ATR RECEIVED"))
            return State.AtrReceived;
        if (upper.Contains("GENAC 1 : ARQC") || upper.Contains("GENAC1"))
            return State.GenAc1;
        if (upper.Contains("GENAC 2 : TC") || upper.Contains("GENAC2"))
            return State.GenAc2;
        if (upper.Contains("NOTES STACKED"))
            return State.NotesStacked;
        if (upper.Contains("NOTES PRESENTED"))
            return State.NotesPresented;
        if (upper.Contains("NOTES TAKEN"))
            return State.NotesTaken;

        return current;
    }

    private static EjTransaction BuildTransaction(List<(string Line, int Index)> block, string atmId, int counter)
    {
        var rawLines = block.Select(b => b.Line).ToList();
        int startLine = block.First().Index;
        int endLine = block.Last().Index;
        string txId = $"{atmId}-{counter:D5}-{startLine:D6}";

        string stan = ExtractFirstMatch(StanRegex, rawLines) ?? string.Empty;
        string rrn = ExtractFirstMatch(RrnRegex, rawLines) ?? string.Empty;
        string account = MaskValue(ExtractFirstMatch(AccountRegex, rawLines));
        string card = ExtractFirstMatch(MaskedCardRegex, rawLines) ?? string.Empty;
        string currency = ExtractFirstMatch(CurrencyRegex, rawLines) ?? string.Empty;
        string mCode = ExtractFirstMatch(MCodeRegex, rawLines) ?? string.Empty;
        string rCode = ExtractFirstMatch(RCodeRegex, rawLines) ?? string.Empty;
        decimal? amount = ExtractAmount(rawLines);

        int? c1 = ExtractCassette(rawLines, 1);
        int? c2 = ExtractCassette(rawLines, 2);
        int? c3 = ExtractCassette(rawLines, 3);
        int? c4 = ExtractCassette(rawLines, 4);

        (TransactionClassification classification, double confidence) = Classify(block, stan, rrn, amount, currency, mCode, rCode);

        return new EjTransaction(
            TransactionId: txId,
            StartLine: startLine,
            EndLine: endLine,
            ATM_ID: atmId,
            CardNumber: card,
            AccountNumber: account,
            Amount: amount,
            Currency: currency,
            STAN: stan,
            RRN: rrn,
            Cassette1: c1,
            Cassette2: c2,
            Cassette3: c3,
            Cassette4: c4,
            MCode: mCode,
            RCode: rCode,
            RawLines: rawLines,
            Classification: classification,
            Confidence: confidence,
            Timestamp: DateTime.UtcNow
        );
    }

    private static (TransactionClassification, double) Classify(
        List<(string Line, int Index)> block,
        string stan,
        string rrn,
        decimal? amount,
        string currency,
        string mCode,
        string rCode)
    {
        var lines = block.Select(b => b.Line).ToList();
        var upperLines = lines.Select(l => l.ToUpperInvariant()).ToList();
        string combined = string.Join(" ", upperLines);

        bool hasNotesPresented = upperLines.Any(l => l.Contains("NOTES PRESENTED"));
        bool hasNotesTaken = upperLines.Any(l => l.Contains("NOTES TAKEN"));
        bool hasNotesStacked = upperLines.Any(l => l.Contains("NOTES STACKED"));
        bool hasReversal = upperLines.Any(l => l.Contains("REVERSAL"));
        bool hasRejected = upperLines.Any(l => l.Contains("REJECTED"));
        bool hasDistCash = upperLines.Any(l => l.Contains("DIST CASH"));
        bool hasApproved = upperLines.Any(l =>
            l.Contains("APPROVED") ||
            l.Contains("ARQC") ||
            l.Contains("GENAC1") ||
            l.Contains("GENAC 1"));
        bool hasDeclined = upperLines.Any(l => l.Contains("DECLINED") || l.Contains("NOT AUTHORISED") || l.Contains("NOT AUTHORIZED"));
        bool hasCashJam = upperLines.Any(l => l.Contains("JAM") || l.Contains("JAMMED"));
        bool hasRetract = upperLines.Any(l => l.Contains("RETRACT") || l.Contains("RETRACTED"));
        bool hasCardCaptured = upperLines.Any(l => l.Contains("CARD CAPTURED") || l.Contains("CARD RETAINED"));
        bool hasHardwareFault = upperLines.Any(l => l.Contains("HARDWARE FAULT") || l.Contains("DEVICE FAULT") || l.Contains("FAULT"));

        // Reversal is definitive
        if (hasReversal)
            return (TransactionClassification.Reversal, 0.98);

        // Cash jam
        if (hasCashJam)
            return (TransactionClassification.CashJam, 0.95);

        // Card captured
        if (hasCardCaptured)
            return (TransactionClassification.CardCaptured, 0.95);

        // Retract
        if (hasRetract)
            return (TransactionClassification.Retract, 0.92);

        // Hardware fault
        if (hasHardwareFault)
            return (TransactionClassification.HardwareFault, 0.90);

        // Host declined
        if (hasDeclined)
            return (TransactionClassification.HostDeclined, 0.95);

        // Success requires NOTES PRESENTED + NOTES TAKEN (or documented alternative)
        if (hasNotesPresented && hasNotesTaken)
        {
            // Verify STAN/RRN presence for strong success confidence
            bool hasIdentifiers = !string.IsNullOrEmpty(stan) && !string.IsNullOrEmpty(rrn);
            double conf = hasIdentifiers ? 0.98 : 0.90;
            return (TransactionClassification.Success, conf);
        }

        // A rejected request with missing identifiers is primarily a data-integrity gap.
        if (hasRejected && string.IsNullOrEmpty(stan))
            return (TransactionClassification.MissingSTAN, 0.80);
        if (hasRejected && string.IsNullOrEmpty(rrn))
            return (TransactionClassification.MissingRRN, 0.80);
        if (hasRejected)
            return (TransactionClassification.Failed, 0.90);

        // Approved but no dispense
        if (hasApproved && !hasNotesPresented && !hasNotesTaken)
        {
            return (TransactionClassification.ApprovedNoDispense, 0.88);
        }

        // Partial dispense: notes stacked/presented but not taken
        if ((hasNotesStacked || hasNotesPresented) && !hasNotesTaken)
        {
            return (TransactionClassification.PartialDispense, 0.85);
        }

        // Missing identifiers
        if (string.IsNullOrEmpty(stan))
            return (TransactionClassification.MissingSTAN, 0.80);
        if (string.IsNullOrEmpty(rrn))
            return (TransactionClassification.MissingRRN, 0.80);

        // Default to suspicious if approved is present but no clear outcome
        if (hasApproved)
            return (TransactionClassification.Suspicious, 0.70);

        return (TransactionClassification.Failed, 0.60);
    }

    private static string? ExtractFirstMatch(Regex regex, List<string> lines)
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
            if (match.Success)
            {
                string value = match.Groups[1].Value.Replace(',', '.');
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    return result;
            }
        }
        return null;
    }

    private static int? ExtractCassette(List<string> lines, int cassetteNumber)
    {
        foreach (var line in lines)
        {
            var match = CassetteRegex.Match(line);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int num) && num == cassetteNumber)
            {
                if (int.TryParse(match.Groups[2].Value, out int count))
                    return count;
            }
        }
        return null;
    }

    private static string MaskValue(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= 4)
            return value ?? string.Empty;

        return value[..2] + new string('*', value.Length - 4) + value[^2..];
    }
}
