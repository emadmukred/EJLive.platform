using System;
using System.Collections.Generic;

namespace EJLive.Core.Xfs.Models
{
    public enum XfsEventSourceLayer
    {
        Unknown,
        BusinessJournal,
        XfsStatus,
        DriverError,
        HardwareDiagnostic
    }

    public enum XfsSeverity
    {
        Info,
        Warning,
        Critical
    }

    public enum XfsDeviceFamily
    {
        Unknown,
        Terminal,
        CardReader,
        CashDispenser,
        CashAcceptor,
        Depository,
        ReceiptPrinter,
        JournalPrinter,
        StatementPrinter,
        Encryptor,
        Sensors,
        Contactless,
        OperatorPanel,
        Camera,
        Alarm,
        Misc
    }

    public sealed class XfsCassetteSnapshot
    {
        public string CassetteId { get; set; }
        public int RemainingCount { get; set; }
        public int RejectCount { get; set; }
        public int Denomination { get; set; }
        public string Currency { get; set; }
        public string CassetteState { get; set; }
        public string CassetteType { get; set; }
        public string NoteType { get; set; }
    }

    public sealed class XfsNormalizedEvent
    {
        public string Vendor { get; set; }
        public XfsEventSourceLayer SourceLayer { get; set; }
        public XfsDeviceFamily DeviceFamily { get; set; }
        public string EventCode { get; set; }
        public string EventName { get; set; }
        public XfsSeverity Severity { get; set; }
        public string OperationalImpact { get; set; }
        public string RecommendedAction { get; set; }
        public string TransactionSerialNumber { get; set; }
        public string RawLine { get; set; }
        public DateTime? Timestamp { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public List<XfsCassetteSnapshot> Cassettes { get; set; } = new List<XfsCassetteSnapshot>();
    }
}
