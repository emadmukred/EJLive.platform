using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Services
{
    public class NcrConfigCapabilityParser
    {
        private static readonly Regex IncludeRegex = new Regex(@"<\?xml-include\s+(?<path>[^?]+)\?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex FormNameRegex = new Regex(@"FormName""\s+Value=""(?<name>[^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PropertyRegex = new Regex(@"Property\s+Name=""(?<name>[^""]+)""\s+Value=""(?<value>[^""]*)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex TranslationContextRegex = new Regex(@"<Context\s+Name=""(?<name>[^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Dictionary<string, string> Parse(string text)
        {
            return (text ?? string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(new[] { '=' }, 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);
        }

        public NcrConfigCapabilityProfile Parse(IDictionary<string, string> files)
        {
            var profile = new NcrConfigCapabilityProfile { SourceName = "NCR Config Bundle" };
            if (files == null || files.Count == 0)
                return profile;

            foreach (var kv in files)
            {
                var path = kv.Key ?? string.Empty;
                var content = kv.Value ?? string.Empty;

                if (path.EndsWith("AdvanceNDC.accfg", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Match m in IncludeRegex.Matches(content))
                        profile.IncludedConfigFiles.Add(CleanIncludePath(m.Groups["path"].Value));
                }

                if (path.IndexOf("SupervisorReceiptConfig", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    profile.HasSupervisorReceiptConfig = true;
                    foreach (Match m in FormNameRegex.Matches(content))
                        AddUnique(profile.ReceiptForms, m.Groups["name"].Value);
                }

                if (path.IndexOf("AdvanceNDCCore.accfg", StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf("AdvanceNDCCustom.accfg", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ParseAccfg(content, profile);
                }

                if (path.EndsWith("TerminalConfiguration.dat", StringComparison.OrdinalIgnoreCase))
                {
                    ParseTerminalConfiguration(content, profile);
                }

                if (path.EndsWith("SupervisorKeyboard.xml", StringComparison.OrdinalIgnoreCase))
                {
                    profile.HasPopupKeyboard = content.IndexOf("SMSKeyboardConfiguration", StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (path.EndsWith(".reg", StringComparison.OrdinalIgnoreCase))
                {
                    ParseRegistry(content, profile);
                }

                if (path.IndexOf("Translation", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    foreach (Match m in TranslationContextRegex.Matches(content))
                        AddUnique(profile.TranslationDomains, m.Groups["name"].Value);
                }
            }

            return profile;
        }

        private void ParseAccfg(string content, NcrConfigCapabilityProfile profile)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            if (content.IndexOf("CashInCore.accfg", StringComparison.OrdinalIgnoreCase) >= 0 || content.IndexOf("CashInCustom.accfg", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.HasCashInConfiguration = true;
            if (content.IndexOf("NDCVoiceGuidance", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.HasVoiceGuidance = true;
            if (content.IndexOf("PopupKeyboardEnabled\" Value=\"true", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.HasPopupKeyboard = true;
            if (content.IndexOf("EnhancedAudioService", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.HasEnhancedAudio = true;
            if (content.IndexOf("NDCCameraService", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.HasCameraService = true;
            if (content.IndexOf("ChequeAcceptService", StringComparison.OrdinalIgnoreCase) >= 0 || content.IndexOf("IChequeAcceptService", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.HasChequeAcceptService = true;
            if (content.IndexOf("PersistentDataService", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.HasPersistentDataService = true;

            foreach (Match m in PropertyRegex.Matches(content))
            {
                string name = m.Groups["name"].Value;
                string value = m.Groups["value"].Value;
                if (string.Equals(name, "FormName", StringComparison.OrdinalIgnoreCase))
                    AddUnique(profile.ReceiptForms, value);
            }
        }

        private void ParseTerminalConfiguration(string content, NcrConfigCapabilityProfile profile)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            if (content.IndexOf("bankName", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.BankName = "Configured";
            if (content.IndexOf("branchId", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.BranchId = "Configured";
            if (content.IndexOf("networkId", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.NetworkId = "Configured";
            if (content.IndexOf("terminalId", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.TerminalId = "Configured";

            var modelMatch = Regex.Match(content, @"\b(6623|6624|6625|56\d\d)\b");
            if (modelMatch.Success)
                profile.TerminalModel = modelMatch.Groups[1].Value;

            if (content.IndexOf("manufacturer", StringComparison.OrdinalIgnoreCase) >= 0)
                profile.Manufacturer = "NCR";
        }

        private void ParseRegistry(string content, NcrConfigCapabilityProfile profile)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            if (content.IndexOf("ReportSeparateCashUnits", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                profile.ReportSeparateCashUnits = content.IndexOf("ReportSeparateCashUnits\"=dword:00000001", StringComparison.OrdinalIgnoreCase) >= 0;
                AddUnique(profile.RegistryFlags, "ReportSeparateCashUnits");
            }
            if (content.IndexOf("ExpandCIMRetractNoteList", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                profile.ExpandCimRetractNoteList = content.IndexOf("ExpandCIMRetractNoteList\"=dword:00000001", StringComparison.OrdinalIgnoreCase) >= 0;
                AddUnique(profile.RegistryFlags, "ExpandCIMRetractNoteList");
            }
            if (content.IndexOf("AllowRetractToTransport", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                profile.AllowRetractToTransport = content.IndexOf("AllowRetractToTransport\"=dword:00000001", StringComparison.OrdinalIgnoreCase) >= 0;
                AddUnique(profile.RegistryFlags, "AllowRetractToTransport");
            }
            if (content.IndexOf("Disable Automatic Resets", StringComparison.OrdinalIgnoreCase) >= 0 || content.IndexOf("DisableAutomaticResets", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                profile.DisableAutomaticResets = content.IndexOf("00000001", StringComparison.OrdinalIgnoreCase) >= 0 && content.IndexOf("Disable", StringComparison.OrdinalIgnoreCase) >= 0;
                AddUnique(profile.RegistryFlags, "DisableAutomaticResets");
            }
            if (content.IndexOf("Tamper Sense Suspend", StringComparison.OrdinalIgnoreCase) >= 0 || content.IndexOf("TamperSenseSuspend", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                profile.TamperSenseSuspend = content.IndexOf("00000001", StringComparison.OrdinalIgnoreCase) >= 0;
                AddUnique(profile.RegistryFlags, "TamperSenseSuspend");
            }

            var suspendMatch = Regex.Match(content, @"SuspendTimeout""=dword:(?<hex>[0-9a-fA-F]{8})");
            if (suspendMatch.Success)
                profile.SuspendTimeoutSeconds = Convert.ToInt32(suspendMatch.Groups["hex"].Value, 16);

            var resetMatch = Regex.Match(content, @"ResetActionOnInitialise""=dword:(?<hex>[0-9a-fA-F]{8})");
            if (resetMatch.Success)
                profile.ResetActionOnInitialise = Convert.ToInt32(resetMatch.Groups["hex"].Value, 16);
        }

        private string CleanIncludePath(string raw)
        {
            return raw.Replace("\t", " ").Trim();
        }

        private void AddUnique(List<string> target, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            if (!target.Contains(value, StringComparer.OrdinalIgnoreCase))
                target.Add(value);
        }
    }
}
