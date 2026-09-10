using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Core.Services
{
    public class VendorRootCapabilityService
    {
        private static readonly Regex FilterRuleRegex = new Regex(
            @"^(?<provider>\d+)\:(?<event>\d+)\=(?<pattern>.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex WfmNameRegex = new Regex(
            @"XFSMEDIA\s+""(?<name>[^""]+)""",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex WfmUnitRegex = new Regex(
            @"UNIT\s+(?<unit>[A-Z]+)\s*\,\s*\d+\s*\,\s*\d+",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex WfmSizeRegex = new Regex(
            @"SIZE\s+(?<w>\d+)\s*\,\s*(?<h>\d+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public IReadOnlyList<string> GetCapabilities(string vendor) => AppConstants.NormalizeATMType(vendor) switch
        {
            AppConstants.ATM_TYPE_NCR => new[] { "EJDATA", "EJRCPY", "OOXFS", "HOST_MESSAGES" },
            AppConstants.ATM_TYPE_GRG => new[] { "DAILY_EJ", "TRACE", "JOURNAL_ADAPTER" },
            AppConstants.ATM_TYPE_DN => new[] { "MDS", "JOURNAL", "TRACE" },
            _ => new[] { "JOURNAL", "TRACE" }
        };

        public VendorRootProfile BuildKnownProfileByVendorName(string vendorName)
        {
            string normalized = (vendorName ?? string.Empty).Trim().ToUpperInvariant();
            var profile = new VendorRootProfile { VendorName = string.IsNullOrWhiteSpace(vendorName) ? "Unknown" : vendorName };

            if (normalized.Contains("NCR"))
            {
                profile.PlatformLineage = VendorPlatformLineage.Hybrid;
                profile.HasFilterIni = true;
                profile.HasDispenserConfigData = true;
                profile.HasKeyboardMapData = true;
                profile.HasKbapeConfig = true;
                profile.FilterHeaderHint = "APTRA/XFS + ProAgent event filter mappings + NCR-specific dispenser and keypad config";
                AddArtifact(profile, "PROAGENT/DATA/filter.ini", "filter.ini", "Shared ProAgent filter rules and event-number mappings");
                AddArtifact(profile, "XFS/data/cdmdata/exp.dat", "cdm-exp-data", "Currency exponent mapping for cash dispenser handling");
                AddArtifact(profile, "XFS/data/ttu/dckeymap.dat", "keyboard-map", "Supervisor/operator keyboard remapping data");
                AddArtifact(profile, "XFS/kbape.cnf", "kbape-config", "BAPE/EOP keypad capability and mapping configuration");
                AddArtifact(profile, "AdvanceNDC.accfg / AdvanceNDCCore.accfg / AdvanceNDCCustom.accfg", "composed-config", "Composed APTRA runtime graph for receipt, cash-in, voice, camera, screen, and persistent services");
                AddArtifact(profile, "SupervisorReceiptConfig.accfg", "receipt-config", "Supervisor and exception receipt form configuration");
                AddArtifact(profile, "SupervisorKeyboard.xml", "supervisor-keyboard", "Supervisor keyboard layout and multi-tap input timing");
                AddArtifact(profile, "TerminalConfiguration.dat", "terminal-metadata", "Serialized terminal metadata including model, owner, network, and branch context");
                AddArtifact(profile, "*.reg operational parameters", "registry-flags", "Operational behavior flags such as retract handling, resets, reconciliation, and reporting");
                AddArtifact(profile, "TranslationTables/*TcpipSupportTask*", "translation-table", "Network/TCP-IP support task vocabulary and operator-facing labels");
                return profile;
            }

            if (normalized.Contains("GRG"))
            {
                profile.PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA;
                profile.HasFilterIni = true;
                profile.HasXfsMediaTemplates = true;
                profile.FilterHeaderHint = "ProTopas/PrvPro event mapping with GRG business-journal and media-template context";
                AddArtifact(profile, "PROAGENT/DATA/filter.ini", "filter.ini", "Shared ProAgent filter mappings for event classification");
                AddArtifact(profile, "XFS/Media/RPTR/*.wfm", "receipt-media-template", "Receipt printer form factor descriptors");
                AddArtifact(profile, "XFS/Media/SPTR/*.wfm", "document-media-template", "Statement/document printer form factor descriptors");
                AddArtifact(profile, "GRG EJ logs", "business-journal", "High-level transaction and cassette-flow journals");
                return profile;
            }

            if (normalized.Contains("WN") || normalized.Contains("WINCOR"))
            {
                profile.PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA;
                profile.HasFilterIni = true;
                profile.FilterHeaderHint = "Wincor/ProTopas NDC-DDC filter baseline with vendor-specific message taxonomy";
                AddArtifact(profile, "PROAGENT/DATA/filter.ini", "filter.ini", "WN-specific filter/event mapping catalog");
                AddArtifact(profile, "WINCOR journal/debug logs", "journal-expected", "Expected source family for WN flow and middleware interpretation");
                return profile;
            }

            if (normalized.Contains("NAUTILUS"))
            {
                profile.PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA;
                profile.HasFilterIni = true;
                profile.FilterHeaderHint = "Nautilus root currently exposes event-mapping/filter configuration only";
                AddArtifact(profile, "PROAGENT/DATA/filter.ini", "filter.ini", "Nautilus event/filter mapping base");
                return profile;
            }

            if (normalized.Contains("DIEBOLD") || normalized.Contains("AGILIS") || normalized.Contains("DN"))
            {
                profile.PlatformLineage = VendorPlatformLineage.Unknown;
                profile.HasFilterIni = true;
                profile.FilterHeaderHint = "Diebold requires dedicated MDS/912 status decoding in addition to any root filter mappings";
                AddArtifact(profile, "PROAGENT/DATA/filter.ini", "filter.ini", "Available root event mapping catalog if present in deployment");
                AddArtifact(profile, "Agilis 3 91x status manuals", "mds-reference", "MDS/912 device status ecosystem for CR01/D901/DP01/DR01/PR*/SP01/EP01/SY01");
                AddArtifact(profile, "Diebold MDS status format", "mds-format", "Solicited/unsolicited code + device number + handler byte + detail bytes");
                return profile;
            }

            if (normalized.Contains("DELARUE") || normalized.Contains("DE LA RUE"))
            {
                profile.PlatformLineage = VendorPlatformLineage.ProTopas_ProView_WOSA;
                profile.HasFilterIni = true;
                profile.FilterHeaderHint = "DeLaRue-specific NDC-DDC filter mappings with older Windows NT era event definitions";
                AddArtifact(profile, "PROAGENT/DATA/filter.ini", "filter.ini", "DeLaRue event/filter mapping catalog");
                return profile;
            }

            if (normalized.Contains("CASHWAY") || normalized.Contains("CASH WAY"))
            {
                profile.PlatformLineage = VendorPlatformLineage.Unknown;
                profile.HasFilterIni = false;
                profile.FilterHeaderHint = "CashWay support expected from logs first, then root/config files when available";
                AddArtifact(profile, "CashWay ATM Log samples", "journal-expected", "CashWay requires dedicated vendor parsing once more logs/config arrive");
                return profile;
            }

            profile.PlatformLineage = VendorPlatformLineage.Unknown;
            profile.FilterHeaderHint = "No known root capability profile matched";
            return profile;
        }

        public VendorRootProfile BuildProfile(string vendorName, IEnumerable<(string path, string content)> files)
        {
            var entries = (files ?? Enumerable.Empty<(string path, string content)>()).ToList();
            var profile = new VendorRootProfile
            {
                VendorName = vendorName ?? "Unknown"
            };

            foreach (var entry in entries)
            {
                string path = entry.path ?? string.Empty;
                string content = entry.content ?? string.Empty;

                if (path.EndsWith("filter.ini", StringComparison.OrdinalIgnoreCase))
                {
                    profile.HasFilterIni = true;
                    profile.FilterHeaderHint = ExtractFilterHeaderHint(content) ?? string.Empty;
                    AddArtifact(profile, path, "filter.ini", string.IsNullOrWhiteSpace(profile.FilterHeaderHint) ? "Vendor filter catalog" : profile.FilterHeaderHint);
                }
                else if (path.EndsWith(".wfm", StringComparison.OrdinalIgnoreCase))
                {
                    profile.HasXfsMediaTemplates = true;
                    AddArtifact(profile, path, "xfs-media-template", "Printer/media layout descriptor");
                }
                else if (path.EndsWith("exp.dat", StringComparison.OrdinalIgnoreCase))
                {
                    profile.HasDispenserConfigData = true;
                    AddArtifact(profile, path, "cdm-exp-data", "Currency exponent configuration");
                }
                else if (path.EndsWith("dckeymap.dat", StringComparison.OrdinalIgnoreCase))
                {
                    profile.HasKeyboardMapData = true;
                    AddArtifact(profile, path, "keyboard-map", "Operator panel key mapping");
                }
                else if (path.EndsWith("kbape.cnf", StringComparison.OrdinalIgnoreCase))
                {
                    profile.HasKbapeConfig = true;
                    AddArtifact(profile, path, "kbape-config", "Keyboard / BAPE capability profile");
                }
            }

            profile.PlatformLineage = InferLineage(profile, entries.Select(e => e.content));
            return profile;
        }

        public IReadOnlyList<FilterRuleDefinition> ExtractFilterRules(string sourceFile, string content)
        {
            var rules = new List<FilterRuleDefinition>();
            if (string.IsNullOrWhiteSpace(content))
                return rules;

            foreach (var rawLine in content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                var match = FilterRuleRegex.Match(line);
                if (!match.Success)
                    continue;

                rules.Add(new FilterRuleDefinition
                {
                    ProviderId = match.Groups["provider"].Value,
                    EventId = match.Groups["event"].Value,
                    Pattern = match.Groups["pattern"].Value,
                    SourceFile = sourceFile
                });
            }

            return rules;
        }

        public XfsMediaTemplateDefinition? ExtractMediaTemplate(string sourceFile, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            var name = WfmNameRegex.Match(content);
            var unit = WfmUnitRegex.Match(content);
            var size = WfmSizeRegex.Match(content);
            if (!name.Success && !unit.Success && !size.Success)
                return null;

            return new XfsMediaTemplateDefinition
            {
                LogicalName = name.Success ? name.Groups["name"].Value : string.Empty,
                UnitType = unit.Success ? unit.Groups["unit"].Value : string.Empty,
                Width = size.Success ? SafeInt(size.Groups["w"].Value) : 0,
                Height = size.Success ? SafeInt(size.Groups["h"].Value) : 0,
                SourceFile = sourceFile
            };
        }

        private VendorPlatformLineage InferLineage(VendorRootProfile profile, IEnumerable<string> contents)
        {
            string merged = string.Join("\n", contents ?? Enumerable.Empty<string>());
            bool hasAptra = merged.IndexOf("APTRA", StringComparison.OrdinalIgnoreCase) >= 0 || merged.IndexOf("kbape", StringComparison.OrdinalIgnoreCase) >= 0;
            bool hasProTopas = merged.IndexOf("ProTopas", StringComparison.OrdinalIgnoreCase) >= 0 || merged.IndexOf("ProView", StringComparison.OrdinalIgnoreCase) >= 0 || merged.IndexOf("WOSA", StringComparison.OrdinalIgnoreCase) >= 0;

            if (hasAptra && hasProTopas)
                return VendorPlatformLineage.Hybrid;
            if (hasAptra || profile.HasKbapeConfig || profile.HasDispenserConfigData)
                return VendorPlatformLineage.NCR_APTRA_XFS;
            if (hasProTopas || profile.HasFilterIni)
                return VendorPlatformLineage.ProTopas_ProView_WOSA;
            return VendorPlatformLineage.Unknown;
        }

        private string? ExtractFilterHeaderHint(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            foreach (var rawLine in content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var line = rawLine.Trim().TrimStart(';').Trim();
                if (line.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
                    return line;
            }

            return null;
        }

        private int SafeInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }

        private static void AddArtifact(VendorRootProfile profile, string path, string artifactType, string summary)
        {
            if (profile.Artifacts.Any(a => string.Equals(a.RelativePath, path, StringComparison.OrdinalIgnoreCase) && string.Equals(a.ArtifactType, artifactType, StringComparison.OrdinalIgnoreCase)))
                return;

            profile.Artifacts.Add(new RootConfigArtifact
            {
                RelativePath = path,
                ArtifactType = artifactType,
                Summary = summary
            });
        }
    }
}
