using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public sealed class NcrConfigCapabilityProfile
    {
        public string TerminalModel { get; set; }
        public string Manufacturer { get; set; }
        public bool HasAdvanceNdcCompositeConfig { get; set; }
        public bool HasCoreConfig { get; set; }
        public bool HasCustomConfig { get; set; }
        public bool HasSupervisorKeyboardConfig { get; set; }
        public bool HasSupervisorReceiptConfig { get; set; }
        public bool HasRegistryOperationalParameters { get; set; }
        public bool HasTcpipSupportTranslation { get; set; }
        public bool HasValidationRuleSet { get; set; }
        public List<string> IncludedConfigFiles { get; set; } = new List<string>();
        public List<string> EnabledSubsystems { get; set; } = new List<string>();
        public List<string> OperationalFlags { get; set; } = new List<string>();
        public List<string> ReceiptForms { get; set; } = new List<string>();
        public List<string> NetworkLabels { get; set; } = new List<string>();
    }
}
