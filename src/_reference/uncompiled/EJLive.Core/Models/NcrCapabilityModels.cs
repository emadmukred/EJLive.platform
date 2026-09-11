using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public sealed class NcrConfigCapabilityProfile
    {
        public string TerminalModel { get; set; }
        public string Manufacturer { get; set; }
        public bool HasAdvanceNdcConfig { get; set; }
        public bool HasCoreConfig { get; set; }
        public bool HasCustomConfig { get; set; }
        public bool HasSupervisorKeyboardConfig { get; set; }
        public bool HasSupervisorReceiptConfig { get; set; }
        public bool HasRegistryOperationalFlags { get; set; }
        public bool HasTcpipTranslationTables { get; set; }
        public List<string> Subsystems { get; set; } = new List<string>();
        public Dictionary<string, string> OperationalFlags { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Paths { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<string> ExceptionReceiptForms { get; set; } = new List<string>();
        public List<string> TranslationContexts { get; set; } = new List<string>();
    }
}
