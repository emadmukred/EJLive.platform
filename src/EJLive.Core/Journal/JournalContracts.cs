using System;
using System.Collections.Generic;

namespace EJLive.Core.Journal
{
    /// <summary>
    /// Vendor-specific electronic journal transaction parser.
    /// Each ATM vendor must have its own parser strategy.
    /// Generic fallback is last resort only.
    /// </summary>
    public interface IEjTransactionParser
    {
        /// <summary>Supported ATM vendor identifier (NCR, GRG, Wincor, Diebold, Hyosung, Cashway).</summary>
        string Vendor { get; }

        /// <summary>Returns true if this parser can handle the given file context.</summary>
        bool CanParse(EjParseContext context);

        /// <summary>Parse raw journal lines into structured transactions.</summary>
        EjParseResult Parse(EjParseContext context);
    }

    /// <summary>
    /// Context for a journal parse run.
    /// </summary>
    public sealed class EjParseContext
    {
        public string AtmId { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
        public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;
        public string[] Lines { get; set; } = Array.Empty<string>();
        public DateTimeOffset? FileTimestamp { get; set; }
        public string? TimeZone { get; set; }
    }

    /// <summary>
    /// Result of a journal parse run.
    /// </summary>
    public sealed class EjParseResult
    {
        public List<EjTransaction> Transactions { get; set; } = new List<EjTransaction>();
        public List<EJParseWarning> Warnings { get; set; } = new List<EJParseWarning>();
        public int TotalLines { get; set; }
        public int ParsedLines { get; set; }
        public string? ParserUsed { get; set; }
        public DateTimeOffset ParsedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// A single parsed ATM transaction from the electronic journal.
    /// </summary>
    public sealed class EjTransaction
    {
        public int TransactionNumber { get; set; }
        public DateTimeOffset? Date { get; set; }
        public string? Time { get; set; }
        public string AtmId { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string? CardMasked { get; set; }
        public string? AccountNumber { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? Stan { get; set; }
        public string? Rrn { get; set; }
        public int? Cass1 { get; set; }
        public int? Cass2 { get; set; }
        public int? Cass3 { get; set; }
        public int? Cass4 { get; set; }
        public string? MCode { get; set; }
        public string? RCode { get; set; }
        public string? HostResponse { get; set; }
        public EjTransactionStatus Status { get; set; } = EjTransactionStatus.Unknown;
        public string? FailureReason { get; set; }
        public int RawStartLine { get; set; }
        public int RawEndLine { get; set; }
        public List<string> Evidence { get; set; } = new List<string>();
        public ConfidenceScore Confidence { get; set; } = ConfidenceScore.Uncertain;
        public string? RawTransactionBlock { get; set; }
    }

    /// <summary>
    /// Transaction outcome classification. Success requires dispense evidence, not APPROVED alone.
    /// </summary>
    public enum EjTransactionStatus
    {
        Unknown,
        Success,
        Failed,
        Suspicious,
        Reversal,
        PartialDispense,
        ApprovedNoDispense,
        CashJam,
        Retract,
        CardCaptured,
        HostDeclined,
        HardwareFault,
        MissingSequence,
        DuplicateSequence
    }

    /// <summary>
    /// Parser confidence level for each extracted field.
    /// </summary>
    public enum ConfidenceScore
    {
        Certain,
        High,
        Medium,
        Uncertain,
        Low
    }

    /// <summary>
    /// Non-fatal warning from parser (e.g., unrecognized line, missing optional field).
    /// </summary>
    public sealed class EJParseWarning
    {
        public int LineNumber { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RawLine { get; set; }
    }

    /// <summary>
    /// Registry that selects the correct parser for a given ATM vendor context.
    /// </summary>
    public sealed class EjParserRegistry
    {
        private readonly Dictionary<string, IEjTransactionParser> _parsers = new Dictionary<string, IEjTransactionParser>(StringComparer.OrdinalIgnoreCase);
        private readonly IEjTransactionParser _fallback;

        public EjParserRegistry(GenericFallbackParser fallback)
        {
            _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
        }

        public void Register(IEjTransactionParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            _parsers[parser.Vendor] = parser;
        }

        public IEjTransactionParser Resolve(string vendor)
        {
            if (_parsers.TryGetValue(vendor ?? string.Empty, out var parser))
                return parser;
            return _fallback;
        }

        public IReadOnlyCollection<string> RegisteredVendors => _parsers.Keys;
    }

    /// <summary>
    /// Last-resort parser used when no vendor-specific parser matches.
    /// Preserves all raw lines and marks confidence as Uncertain.
    /// </summary>
    public sealed class GenericFallbackParser : IEjTransactionParser
    {
        public string Vendor => "Generic";

        public bool CanParse(EjParseContext context) => true;

        public EjParseResult Parse(EjParseContext context)
        {
            var result = new EjParseResult
            {
                TotalLines = context.Lines.Length,
                ParsedLines = 0,
                ParserUsed = "GenericFallback"
            };

            if (context.Lines.Length == 0)
                return result;

            // Preserve all lines as a single unknown transaction
            var tx = new EjTransaction
            {
                TransactionNumber = 0,
                AtmId = context.AtmId,
                Vendor = context.Vendor,
                Status = EjTransactionStatus.Unknown,
                Confidence = ConfidenceScore.Uncertain,
                RawStartLine = 0,
                RawEndLine = context.Lines.Length - 1,
                Evidence = new List<string>(context.Lines)
            };

            result.Transactions.Add(tx);
            result.ParsedLines = context.Lines.Length;
            return result;
        }
    }
}