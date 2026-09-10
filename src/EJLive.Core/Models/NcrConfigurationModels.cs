using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public sealed class NcrConfigurationProfile
    {
        public string TerminalModel { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string RootNamespace { get; set; } = string.Empty;
        public List<string> IncludedConfigFiles { get; set; } = new List<string>();
        public List<string> Subsystems { get; set; } = new List<string>();
        public Dictionary<string, string> RegistryFlags { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> SupervisorKeyboardMap { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<string> ReceiptForms { get; set; } = new List<string>();
        public List<string> TranslationDomains { get; set; } = new List<string>();
    }
}
