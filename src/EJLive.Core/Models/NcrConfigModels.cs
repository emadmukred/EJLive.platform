using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public sealed class NcrConfigCapabilityProfile
    {
        public string SourceName { get; set; } = string.Empty;
        public string TerminalModel { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string NetworkId { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public bool HasCashInConfiguration { get; set; }
        public bool HasSupervisorReceiptConfig { get; set; }
        public bool HasVoiceGuidance { get; set; }
        public bool HasPopupKeyboard { get; set; }
        public bool HasEnhancedAudio { get; set; }
        public bool HasCameraService { get; set; }
        public bool HasChequeAcceptService { get; set; }
        public bool HasPersistentDataService { get; set; }
        public bool ReportSeparateCashUnits { get; set; }
        public bool ExpandCimRetractNoteList { get; set; }
        public bool AllowRetractToTransport { get; set; }
        public bool DisableAutomaticResets { get; set; }
        public bool TamperSenseSuspend { get; set; }
        public int SuspendTimeoutSeconds { get; set; }
        public int ResetActionOnInitialise { get; set; }
        public List<string> IncludedConfigFiles { get; set; }
        public List<string> ReceiptForms { get; set; }
        public List<string> TranslationDomains { get; set; }
        public List<string> RegistryFlags { get; set; }

        public NcrConfigCapabilityProfile()
        {
            IncludedConfigFiles = new List<string>();
            ReceiptForms = new List<string>();
            TranslationDomains = new List<string>();
            RegistryFlags = new List<string>();
        }
    }
}
