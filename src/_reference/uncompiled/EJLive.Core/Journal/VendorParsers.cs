using System;
using System.Collections.Generic;
using System.Linq;

namespace EJLive.Core.Journal
{
    /// <summary>
    /// NCR Electronic Journal transaction parser.
    /// Rules: Success requires NOTES PRESENTED + NOTES TAKEN or equivalent evidence — not APPROVED alone.
    /// Preserves raw start/end lines, marks confidence explicitly.
    /// </summary>
    public sealed class NcrEjTransactionParser : IEjTransactionParser
    {
        public string Vendor => "NCR";

        public bool CanParse(EjParseContext context) =>
            context.Lines != null && context.Lines.Any(l => l != null && l.Contains("*TRANSACTION START*"));

        public EjParseResult Parse(EjParseContext context)
        {
            var result = new EjParseResult
            {
                TotalLines = context.Lines?.Length ?? 0,
                ParserUsed = "NcrEjTransactionParser"
            };

            if (context?.Lines == null || context.Lines.Length == 0)
                return result;

            EjTransaction? currentTx = null;

            for (int i = 0; i < context.Lines.Length; i++)
            {
                var line = context.Lines[i] ?? string.Empty;
                var upperLine = line.ToUpperInvariant();

                if (upperLine.Contains("*TRANSACTION START*"))
                {
                    currentTx = new EjTransaction
                    {
                        RawStartLine = i + 1,
                        AtmId = context.AtmId,
                        Vendor = Vendor,
                        Confidence = ConfidenceScore.Medium
                    };
                }
                else if (upperLine.Contains("TRANSACTION END") && currentTx != null)
                {
                    currentTx.RawEndLine = i + 1;
                    FinalizeTransaction(currentTx);
                    result.Transactions.Add(currentTx);
                    currentTx = null;
                }
                else if (currentTx != null)
                {
                    ParseNcrLine(line, upperLine, currentTx);
                }
                else
                {
                    result.Warnings.Add(new EJParseWarning { LineNumber = i + 1, Message = "Line outside transaction block", RawLine = line });
                }
            }

            // Close dangling transaction
            if (currentTx != null)
            {
                currentTx.RawEndLine = context.Lines.Length;
                FinalizeTransaction(currentTx);
                result.Transactions.Add(currentTx);
            }

            result.ParsedLines = result.Transactions.Sum(t => t.RawEndLine - t.RawStartLine + 1);
            return result;
        }

        private static void ParseNcrLine(string line, string upperLine, EjTransaction tx)
        {
            if (upperLine.Contains("CARD INSERTED"))
                tx.Evidence.Add("CARD_INSERTED");
            else if (upperLine.Contains("PIN ENTERED"))
                tx.Evidence.Add("PIN_ENTERED");
            else if (upperLine.Contains("ATR RECEIVED"))
                tx.Evidence.Add("ATR_RECEIVED");
            else if (upperLine.Contains("ARQC"))
                tx.Evidence.Add("ARQC");
            else if (upperLine.Contains("GENAC 2") || upperLine.Contains("TC"))
                tx.Evidence.Add("TC");
            else if (upperLine.Contains("NOTES PRESENTED"))
                tx.Evidence.Add("NOTES_PRESENTED");
            else if (upperLine.Contains("NOTES TAKEN"))
                tx.Evidence.Add("NOTES_TAKEN");
            else if (upperLine.Contains("NOTES STACKED"))
                tx.Evidence.Add("NOTES_STACKED");
            else if (upperLine.Contains("DIST CASH"))
                tx.Evidence.Add("DIST_CASH");

            // Extract fields
            if (upperLine.Contains("STAN:"))
            {
                var idx = line.IndexOf("STAN:", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0) tx.Stan = line.Substring(idx + 5).Trim().Split(' ')[0];
            }
            else if (upperLine.Contains("RRN:"))
            {
                var idx = line.IndexOf("RRN:", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0) tx.Rrn = line.Substring(idx + 4).Trim().Split(' ')[0];
            }
            else if (upperLine.Contains("AMOUNT:"))
            {
                var idx = line.IndexOf("AMOUNT:", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0 && decimal.TryParse(line.Substring(idx + 7).Trim().Split(' ')[0], out var amt))
                    tx.Amount = amt;
            }
            else if (upperLine.Contains("CASS 1:") || upperLine.Contains("CASS1:"))
            {
                if (int.TryParse(ExtractAfterColon(line), out var c1)) tx.Cass1 = c1;
            }
            else if (upperLine.Contains("CASS 2:") || upperLine.Contains("CASS2:"))
            {
                if (int.TryParse(ExtractAfterColon(line), out var c2)) tx.Cass2 = c2;
            }
            else if (upperLine.Contains("CASS 3:") || upperLine.Contains("CASS3:"))
            {
                if (int.TryParse(ExtractAfterColon(line), out var c3)) tx.Cass3 = c3;
            }
            else if (upperLine.Contains("CASS 4:") || upperLine.Contains("CASS4:"))
            {
                if (int.TryParse(ExtractAfterColon(line), out var c4)) tx.Cass4 = c4;
            }
            else if (upperLine.Contains("M-CODE") || upperLine.Contains("MCODE"))
            {
                tx.MCode = ExtractAfterColon(line);
            }
            else if (upperLine.Contains("R-CODE") || upperLine.Contains("RCODE"))
            {
                tx.RCode = ExtractAfterColon(line);
            }
            else if (upperLine.Contains("RESPONSE:") || upperLine.Contains("HOST RESPONSE"))
            {
                tx.HostResponse = ExtractAfterColon(line);
            }
        }

        private static string? ExtractAfterColon(string line)
        {
            var idx = line.IndexOf(':');
            if (idx < 0) return null;
            return line.Substring(idx + 1).Trim().Split(' ')[0];
        }

        private static void FinalizeTransaction(EjTransaction tx)
        {
            var hasNotesPresented = tx.Evidence.Any(e => e.Contains("NOTES_PRESENTED"));
            var hasNotesTaken = tx.Evidence.Any(e => e.Contains("NOTES_TAKEN"));
            var hasDistCash = tx.Evidence.Any(e => e.Contains("DIST_CASH"));
            var hasArqc = tx.Evidence.Any(e => e.Contains("ARQC"));
            var hasTc = tx.Evidence.Any(e => e.Contains("TC"));

            if (hasNotesPresented && hasNotesTaken)
            {
                tx.Status = EjTransactionStatus.Success;
                tx.Confidence = ConfidenceScore.Certain;
            }
            else if (hasDistCash && (hasArqc || hasTc))
            {
                tx.Status = EjTransactionStatus.Success;
                tx.Confidence = ConfidenceScore.High;
            }
            else if (hasArqc || hasTc)
            {
                tx.Status = EjTransactionStatus.ApprovedNoDispense;
                tx.Confidence = ConfidenceScore.Medium;
            }
            else if (tx.Evidence.Any(e => e.Contains("CARD_INSERTED")) && !hasArqc)
            {
                tx.Status = EjTransactionStatus.Failed;
                tx.FailureReason = "No ARQC generated — possible card read failure or timeout";
                tx.Confidence = ConfidenceScore.Medium;
            }
            else
            {
                tx.Status = EjTransactionStatus.Unknown;
                tx.Confidence = ConfidenceScore.Low;
            }
        }
    }

    /// <summary>
    /// GRG Electronic Journal / TRACE transaction parser.
    /// Supports daily EJ files and trace file pairing.
    /// </summary>
    public sealed class GrgEjTransactionParser : IEjTransactionParser
    {
        public string Vendor => "GRG";

        public bool CanParse(EjParseContext context) =>
            context.Lines != null && context.Lines.Any(l => l != null && l.IndexOf("GRG", StringComparison.OrdinalIgnoreCase) >= 0);

        public EjParseResult Parse(EjParseContext context)
        {
            var result = new EjParseResult
            {
                TotalLines = context.Lines?.Length ?? 0,
                ParserUsed = "GrgEjTransactionParser"
            };

            if (context?.Lines == null) return result;

            foreach (var line in context.Lines)
            {
                var l = line ?? string.Empty;
                if (l.IndexOf("TRANS", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    l.IndexOf("DISP", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var tx = new EjTransaction
                    {
                        AtmId = context.AtmId,
                        Vendor = Vendor,
                        Confidence = ConfidenceScore.Medium
                    };

                    if (l.IndexOf("AMOUNT", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        decimal.TryParse(ExtractGrgValue(l, "AMOUNT"), out var amt))
                    {
                        tx.Amount = amt;
                    }

                    if (l.IndexOf("STAN", StringComparison.OrdinalIgnoreCase) >= 0)
                        tx.Stan = ExtractGrgValue(l, "STAN");

                    if (l.IndexOf("RRN", StringComparison.OrdinalIgnoreCase) >= 0)
                        tx.Rrn = ExtractGrgValue(l, "RRN");

                    // Classify based on line content
                    var upper = l.ToUpperInvariant();
                    if (upper.Contains("SUCCESS") || upper.Contains("COMPLETED"))
                        tx.Status = EjTransactionStatus.Success;
                    else if (upper.Contains("DECLINED") || upper.Contains("REJECTED"))
                        tx.Status = EjTransactionStatus.HostDeclined;
                    else if (upper.Contains("JAM") || upper.Contains("FAULT"))
                        tx.Status = EjTransactionStatus.HardwareFault;
                    else if (upper.Contains("RETRACT"))
                        tx.Status = EjTransactionStatus.Retract;
                    else
                        tx.Status = EjTransactionStatus.Unknown;

                    result.Transactions.Add(tx);
                }
                else
                {
                    result.Warnings.Add(new EJParseWarning { Message = "Unrecognized GRG line", RawLine = l });
                }
            }

            result.ParsedLines = result.Transactions.Count;
            return result;
        }

        private static string? ExtractGrgValue(string line, string key)
        {
            var i = line.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            var after = line.Substring(i + key.Length).Trim();
            var space = after.IndexOf(' ');
            return space > 0 ? after.Substring(0, space) : after;
        }
    }

    /// <summary>
    /// Wincor/Nixdorf ProView EJ parser skeleton.
    /// Extracts only fields confidently parsed; unknown lines preserved as evidence.
    /// </summary>
    public sealed class WincorEjTransactionParser : IEjTransactionParser
    {
        public string Vendor => "Wincor";
        public bool CanParse(EjParseContext context) =>
            context.Vendor?.StartsWith("Wincor", StringComparison.OrdinalIgnoreCase) == true ||
            context.Lines?.Any(l => l != null && (l.Contains("ProView") || l.Contains("Wincor"))) == true;

        public EjParseResult Parse(EjParseContext context)
        {
            var result = new EjParseResult
            {
                TotalLines = context.Lines?.Length ?? 0,
                ParserUsed = "WincorEjTransactionParser"
            };

            if (context?.Lines == null) return result;

            foreach (var line in context.Lines)
            {
                var l = line ?? string.Empty;
                result.Warnings.Add(new EJParseWarning { Message = "Wincor parser skeleton — raw line preserved", RawLine = l });
                result.ParsedLines++;
            }

            return result;
        }
    }

    /// <summary>
    /// Diebold/Agilis EJ parser skeleton.
    /// </summary>
    public sealed class DieboldEjTransactionParser : IEjTransactionParser
    {
        public string Vendor => "Diebold";
        public bool CanParse(EjParseContext context) =>
            context.Vendor?.StartsWith("Diebold", StringComparison.OrdinalIgnoreCase) == true ||
            context.Lines?.Any(l => l != null && (l.Contains("Diebold") || l.Contains("Agilis"))) == true;

        public EjParseResult Parse(EjParseContext context)
        {
            var result = new EjParseResult
            {
                TotalLines = context.Lines?.Length ?? 0,
                ParserUsed = "DieboldEjTransactionParser"
            };

            if (context?.Lines == null) return result;

            foreach (var line in context.Lines)
            {
                var l = line ?? string.Empty;
                result.Warnings.Add(new EJParseWarning { Message = "Diebold parser skeleton — raw line preserved", RawLine = l });
                result.ParsedLines++;
            }

            return result;
        }
    }

    /// <summary>
    /// Hyosung EJ parser skeleton.
    /// </summary>
    public sealed class HyosungEjTransactionParser : IEjTransactionParser
    {
        public string Vendor => "Hyosung";
        public bool CanParse(EjParseContext context) =>
            context.Vendor?.StartsWith("Hyosung", StringComparison.OrdinalIgnoreCase) == true ||
            context.Lines?.Any(l => l != null && l.Contains("Hyosung")) == true;

        public EjParseResult Parse(EjParseContext context)
        {
            var result = new EjParseResult
            {
                TotalLines = context.Lines?.Length ?? 0,
                ParserUsed = "HyosungEjTransactionParser"
            };

            if (context?.Lines == null) return result;

            foreach (var line in context.Lines)
            {
                var l = line ?? string.Empty;
                result.Warnings.Add(new EJParseWarning { Message = "Hyosung parser skeleton — raw line preserved", RawLine = l });
                result.ParsedLines++;
            }

            return result;
        }
    }

    /// <summary>
    /// Cashway EJ parser skeleton.
    /// </summary>
    public sealed class CashwayEjTransactionParser : IEjTransactionParser
    {
        public string Vendor => "Cashway";
        public bool CanParse(EjParseContext context) =>
            context.Vendor?.StartsWith("Cashway", StringComparison.OrdinalIgnoreCase) == true ||
            context.Lines?.Any(l => l != null && l.Contains("Cashway")) == true;

        public EjParseResult Parse(EjParseContext context)
        {
            var result = new EjParseResult
            {
                TotalLines = context.Lines?.Length ?? 0,
                ParserUsed = "CashwayEjTransactionParser"
            };

            if (context?.Lines == null) return result;

            foreach (var line in context.Lines)
            {
                var l = line ?? string.Empty;
                result.Warnings.Add(new EJParseWarning { Message = "Cashway parser skeleton — raw line preserved", RawLine = l });
                result.ParsedLines++;
            }

            return result;
        }
    }
}