using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using EJLive.Core.Models;

namespace EJLive.Core.Services
{
    public sealed class NcrConfigurationCapabilityParser : NcrConfigCapabilityParser
    {
        private static readonly Regex IncludeRegex = new Regex(@"<\?xml-include\s+(?<path>[^\?]+)\?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegistryRegex = new Regex("\"(?<key>[^\"]+)\"=(?<value>.+)$", RegexOptions.Compiled);
        private static readonly Regex AssemblyRegex = new Regex(@"NCR\.APTRA\.(?<name>[A-Za-z0-9\.]+)", RegexOptions.Compiled);

        public NcrConfigurationProfile Parse(IEnumerable<(string path, string content)> files)
        {
            var profile = new NcrConfigurationProfile();
            var list = (files ?? Enumerable.Empty<(string path, string content)>()).ToList();

            foreach (var item in list)
            {
                string path = item.path ?? string.Empty;
                string content = item.content ?? string.Empty;

                if (path.EndsWith("TerminalConfiguration.dat", StringComparison.OrdinalIgnoreCase))
                {
                    profile.TerminalModel = ExtractTerminalModel(content) ?? profile.TerminalModel;
                    profile.Manufacturer = content.Contains("NCR.APTRA", StringComparison.OrdinalIgnoreCase) ? "NCR" : profile.Manufacturer;
                    profile.RootNamespace = "NCR.APTRA";
                }
                else if (path.EndsWith(".accfg", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Match m in IncludeRegex.Matches(content))
                        profile.IncludedConfigFiles.Add(m.Groups["path"].Value.Trim());

                    foreach (var subsystem in ExtractSubsystems(content))
                        if (!profile.Subsystems.Contains(subsystem))
                            profile.Subsystems.Add(subsystem);

                    foreach (var form in ExtractReceiptForms(content))
                        if (!profile.ReceiptForms.Contains(form))
                            profile.ReceiptForms.Add(form);
                }
                else if (path.EndsWith(".reg", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var kv in ExtractRegistryFlags(content))
                        profile.RegistryFlags[kv.Key] = kv.Value;
                }
                else if (path.EndsWith("SupervisorKeyboard.xml", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var kv in ExtractSupervisorKeys(content))
                        profile.SupervisorKeyboardMap[kv.Key] = kv.Value;
                }
                else if (path.IndexOf("Translation", StringComparison.OrdinalIgnoreCase) >= 0 && path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    string? domain = ExtractTranslationDomain(content);
                    if (!string.IsNullOrWhiteSpace(domain) && !profile.TranslationDomains.Contains(domain))
                        profile.TranslationDomains.Add(domain);
                }
            }

            return profile;
        }

        private string? ExtractTerminalModel(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            var match = Regex.Match(content, @"\b(66\d{2})\b");
            return match.Success ? match.Groups[1].Value : null;
        }

        private IEnumerable<string> ExtractSubsystems(string content)
        {
            var subsystems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in AssemblyRegex.Matches(content ?? string.Empty))
            {
                string full = m.Groups["name"].Value;
                string? token = full.Split('.').FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(token))
                    subsystems.Add(token);
            }
            return subsystems;
        }

        private IEnumerable<string> ExtractReceiptForms(string content)
        {
            var forms = new List<string>();
            if (string.IsNullOrWhiteSpace(content)) return forms;
            try
            {
                var doc = XDocument.Parse(content);
                foreach (var prop in doc.Descendants().Where(e => e.Name.LocalName == "Property"))
                {
                    var name = prop.Attribute("Name")?.Value;
                    var value = prop.Attribute("Value")?.Value;
                    if (string.Equals(name, "FormName", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value))
                        forms.Add(value);
                }
            }
            catch { }
            return forms;
        }

        private Dictionary<string, string> ExtractRegistryFlags(string content)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(content)) return result;
            foreach (var line in content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("\"")) continue;
                var match = RegistryRegex.Match(trimmed);
                if (match.Success)
                    result[match.Groups["key"].Value] = match.Groups["value"].Value.Trim();
            }
            return result;
        }

        private Dictionary<string, string> ExtractSupervisorKeys(string content)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(content)) return result;
            try
            {
                var doc = XDocument.Parse(content);
                foreach (var element in doc.Descendants().Where(e => e.Name.LocalName.StartsWith("Key", StringComparison.OrdinalIgnoreCase)))
                {
                    result[element.Name.LocalName] = element.Value;
                }
            }
            catch { }
            return result;
        }

        private string? ExtractTranslationDomain(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            try
            {
                var doc = XDocument.Parse(content);
                var ctx = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Context");
                if (ctx != null) return ctx.Value;
                var cl = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Cl");
                return cl?.Attribute("NameSpace")?.Value;
            }
            catch
            {
                if (content.IndexOf("TcpipSupportTask", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "NCR.APTRA.TcpipSupportTask";
                return null;
            }
        }
    }
}
