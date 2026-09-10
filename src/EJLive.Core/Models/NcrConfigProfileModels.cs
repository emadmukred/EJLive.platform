using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public sealed class NcrConfigProfile
    {
        public string TerminalModel { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankId { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string LocalIpLabel { get; set; } = string.Empty;
        public bool SupportsCashIn { get; set; }
        public bool SupportsSupervisorReceipts { get; set; }
        public bool SupportsVoiceGuidance { get; set; }
        public bool SupportsCameraService { get; set; }
        public bool SupportsPopupKeyboard { get; set; }
        public bool ReportSeparateCashUnits { get; set; }
        public bool ExpandRetractJournal { get; set; }
        public bool DisableAutomaticResets { get; set; }
        public bool AllowRetractToTransport { get; set; }
        public int SuspendTimeoutSeconds { get; set; }
        public List<string> ReceiptForms { get; set; }
        public List<string> IncludedConfigFiles { get; set; }
        public List<string> TranslationKeys { get; set; }

        public NcrConfigProfile()
        {
            ReceiptForms = new List<string>();
            IncludedConfigFiles = new List<string>();
            TranslationKeys = new List<string>();
        }
    }
}
