using System.Text.RegularExpressions;

namespace EJLive.Core.Services;

public enum XfsSignalSeverity
{
    Info,
    Warning,
    Critical
}

public sealed record XfsHardwareFinding(
    string Vendor,
    string Category,
    string Code,
    XfsSignalSeverity Severity,
    string Message,
    string RecommendedAction,
    DateTime DetectedAtUtc);

public sealed class XfsLogAnalysisService
{
    private static readonly XfsRule[] GlobalRules =
    [
        new XfsRule("PRINTER_JAM", "Printer", XfsSignalSeverity.Critical, "(printer|rptr|spr|receipt).*(jam|stuck)", "Verify printer path, clear jam, then run device self-test."),
        new XfsRule("PRINTER_SUPPLY_LOW", "Printer", XfsSignalSeverity.Warning, "(paper|receipt).*(low|out|empty)", "Refill receipt paper and confirm print module state."),
        new XfsRule("CASH_LOW", "CashDispenser", XfsSignalSeverity.Warning, "(cash|cassette|cdm).*(low|empty|near empty)", "Schedule cassette replenishment."),
        new XfsRule("CASH_DISPENSE_ERROR", "CashDispenser", XfsSignalSeverity.Critical, "(cash|dispens|cdm).*(error|fault|reject|purge|timeout)", "Run dispenser diagnostics and inspect reject bin."),
        new XfsRule("CARD_READER_ERROR", "CardReader", XfsSignalSeverity.Critical, "(card|reader|dip|sip).*(error|fault|jam|retain|capture)", "Inspect reader throat and clean/replace reader module."),
        new XfsRule("PINPAD_ERROR", "PinPad", XfsSignalSeverity.Critical, "(pin|epp|keypad).*(error|fault|tamper)", "Inspect pinpad security state and service seals."),
        new XfsRule("NETWORK_LINK_DOWN", "Connectivity", XfsSignalSeverity.Warning, "(network|host|line|tcp).*(down|timeout|disconnect|refused)", "Validate switch route and host availability."),
        new XfsRule("SAFE_DOOR_OPEN", "SafeDoor", XfsSignalSeverity.Critical, "(safe|vault|door).*(open|tamper|forced)", "Verify vault door state and physical security."),
        new XfsRule("DEVICE_OFFLINE", "DeviceState", XfsSignalSeverity.Warning, "(device|service provider|xfs).*(offline|not available|unavailable)", "Check SP/XFS service health and device power state.")
    ];

    private static readonly Dictionary<string, XfsRule[]> VendorRules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["NCR"] =
        [
            new XfsRule("NCR_SDC_LINK_FAILURE", "DeviceLink", XfsSignalSeverity.Critical, "(m-146|sdc).*(error|fault|timeout|lost|down)", "Check NCR SDC link and XFS provider communication channel."),
            new XfsRule("NCR_DISPENSE_HANDLER_FAULT", "CashDispenser", XfsSignalSeverity.Critical, "(\\b3a\\b|\\b3d\\b|\\b3e\\b|\\b39\\b|dispense).*(fault|error|reject|timeout)", "Review NCR handler status and run CDM diagnostics."),
            new XfsRule("NCR_CARD_CAPTURE_TIMEOUT", "CardReader", XfsSignalSeverity.Warning, "(card captured|take card timeout|capture timeout)", "Audit captured-card flow and reconcile card reader capture counters."),
            new XfsRule("NCR_PRINTER_PART_REPLACE", "Printer", XfsSignalSeverity.Warning, "(printhead|knife|ribbon).*(replace|fault|error)", "Inspect NCR printer head/knife/ribbon lifecycle and replace as needed."),
            new XfsRule("NCR_SENSOR_TAMPER", "Sensors", XfsSignalSeverity.Critical, "(silent alarm|tamper|mode switch|door sensor)", "Validate NCR security/sensor vectors and branch physical controls."),
            new XfsRule("NCR_CASSETTE_NEAR_EMPTY", "CashDispenser", XfsSignalSeverity.Warning, "(cassette).*(near empty|low|empty|out)", "Plan CIT refill and verify cassette-denomination mapping."),
            new XfsRule("NCR_OOXFS_SP_FAILURE", "ServiceProvider", XfsSignalSeverity.Critical, "(ooxfs|service provider|sp).*(fault|error|offline|unavailable)", "Restart NCR SP stack and validate OOXFS bindings."),
            new XfsRule("NCR_POWER_RESET", "Terminal", XfsSignalSeverity.Info, "(power up|power-up/reset|terminal reset)", "Correlate reset marker with prior fault and recovery timeline.")
        ],
        ["GRG"] =
        [
            new XfsRule("GRG_LINE_DOWN_OFFLINE", "Connectivity", XfsSignalSeverity.Critical, "(line down|enter offline mode|outofservice)", "Validate GRG host line and recover service state."),
            new XfsRule("GRG_DISPENSE_FAIL", "CashDispenser", XfsSignalSeverity.Critical, "(dispense fail|cash unit changed|cdm retract)", "Reconcile cassette, reject, and retract flows for GRG dispenser."),
            new XfsRule("GRG_TAKE_CASH_TIMEOUT", "CashDispenser", XfsSignalSeverity.Warning, "(take cash timeout|cash retract|customer no take)", "Track not-taken cash incidents and verify retract evidence."),
            new XfsRule("GRG_CIM_RETRACT", "CashAcceptor", XfsSignalSeverity.Warning, "(cim retract|cash deposit retract|deposit retract)", "Check GRG CIM retract reason and deposit bin state."),
            new XfsRule("GRG_CASSETTE_SNAPSHOT", "CashDispenser", XfsSignalSeverity.Info, "(cassette status|cas\\()", "Use cassette snapshot to update normalized remaining/reject counters."),
            new XfsRule("GRG_CASSETTE_LOW", "CashDispenser", XfsSignalSeverity.Warning, "(cash unit changed|cassette status|cas\\().*(low|out|empty)", "Schedule cassette replenishment and verify cash-unit seating."),
            new XfsRule("GRG_CARD_CAPTURE", "CardReader", XfsSignalSeverity.Warning, "(card captured|retain card|take card timeout)", "Validate captured-card workflow and card-reader path.")
        ],
        ["WINCOR"] =
        [
            new XfsRule("WN_CDM_FAULT", "CashDispenser", XfsSignalSeverity.Critical, "(wincor|nixdorf|protopas).*(cdm|cash unit|dispense).*(error|fault|jam|reject)", "Inspect Wincor CDM path and cassette seat/sensor state."),
            new XfsRule("WN_IDC_FAULT", "CardReader", XfsSignalSeverity.Warning, "(wincor|nixdorf|idc).*(retain|capture|jam|fault)", "Inspect Wincor IDC transport path and capture bin."),
            new XfsRule("WN_PTR_SUPPLY", "Printer", XfsSignalSeverity.Warning, "(wincor|nixdorf|ptr|receipt).*(paper|ribbon|low|out|empty)", "Refill PTR media and validate receipt printer status."),
            new XfsRule("WN_SP_OFFLINE", "ServiceProvider", XfsSignalSeverity.Critical, "(wosa/xfs|service provider|sp).*(error|offline|not available|unavailable)", "Restart WOSA/XFS stack and verify provider registration."),
            new XfsRule("WN_EPP_TAMPER", "PinPad", XfsSignalSeverity.Critical, "(wincor|nixdorf|epp|pinpad).*(tamper|fault|error)", "Apply EPP tamper SOP and verify secure PIN domain.")
        ],
        ["HYOSUNG"] =
        [
            new XfsRule("HYO_CDM_FAULT", "CashDispenser", XfsSignalSeverity.Critical, "(hyosung|nautilus|hcdm).*(dispense|cassette|reject|fault|error|jam)", "Run Hyosung CDM diagnostics and inspect reject transport."),
            new XfsRule("HYO_CARD_READER_FAULT", "CardReader", XfsSignalSeverity.Warning, "(hyosung|hcrw|card reader).*(capture|retain|jam|fault)", "Inspect Hyosung card reader throat sensors and transport."),
            new XfsRule("HYO_TAKE_CASH_TIMEOUT", "CashDispenser", XfsSignalSeverity.Warning, "(take cash timeout|cash retract|customer no take)", "Track not-taken cash and reconcile retract counters."),
            new XfsRule("HYO_PTR_SUPPLY", "Printer", XfsSignalSeverity.Warning, "(hyosung|receipt printer|ptr).*(paper|low|out|empty)", "Refill paper and verify printer mechanism status."),
            new XfsRule("HYO_EPP_ALERT", "PinPad", XfsSignalSeverity.Critical, "(hyosung|epp|pin).*(tamper|fault|error)", "Escalate secure PIN fault/tamper workflow immediately.")
        ]
    };

    public IEnumerable<string> AnalyzeFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return Array.Empty<string>();

        return AnalyzeLines(File.ReadLines(filePath));
    }

    public IEnumerable<string> AnalyzeText(string text, string sourceName = "")
    {
        _ = sourceName;
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        return AnalyzeLines(text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
    }

    public IEnumerable<string> AnalyzeLines(IEnumerable<string> lines)
    {
        foreach (var finding in AnalyzeOperationalFindings(lines))
            yield return $"{finding.Code}|{finding.Category}|{finding.Message}";
    }

    public IReadOnlyList<XfsHardwareFinding> AnalyzeOperationalFindings(string text, string vendor = "")
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<XfsHardwareFinding>();

        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        return AnalyzeOperationalFindings(lines, vendor);
    }

    public IReadOnlyList<XfsHardwareFinding> AnalyzeOperationalFindings(IEnumerable<string> lines, string vendor = "")
    {
        var normalizedVendor = string.IsNullOrWhiteSpace(vendor) ? "UNKNOWN" : vendor.Trim().ToUpperInvariant();
        var findings = new List<XfsHardwareFinding>();

        foreach (var raw in lines ?? Array.Empty<string>())
        {
            var line = (raw ?? string.Empty).Trim();
            if (line.Length == 0)
                continue;

            var effectiveVendor = normalizedVendor == "UNKNOWN" ? InferVendorFromLine(line) : normalizedVendor;
            var candidateRules = GetRulesForVendor(effectiveVendor);
            foreach (var rule in candidateRules)
            {
                if (!rule.IsMatch(line))
                    continue;

                findings.Add(new XfsHardwareFinding(
                    Vendor: effectiveVendor,
                    Category: rule.Category,
                    Code: rule.Code,
                    Severity: rule.Severity,
                    Message: line,
                    RecommendedAction: rule.RecommendedAction,
                    DetectedAtUtc: DateTime.UtcNow));
                break;
            }
        }

        return findings;
    }

    private static IEnumerable<XfsRule> GetRulesForVendor(string normalizedVendor)
    {
        if (VendorRules.TryGetValue(normalizedVendor, out var rules))
        {
            foreach (var rule in rules)
                yield return rule;
        }

        foreach (var rule in GlobalRules)
            yield return rule;
    }

    private static string InferVendorFromLine(string line)
    {
        if (line.Contains("NCR", StringComparison.OrdinalIgnoreCase)
            || line.Contains("M-146", StringComparison.OrdinalIgnoreCase)
            || line.Contains("*2*", StringComparison.OrdinalIgnoreCase))
            return "NCR";

        if (line.Contains("GRG", StringComparison.OrdinalIgnoreCase)
            || line.Contains("CAS(", StringComparison.OrdinalIgnoreCase)
            || line.Contains("ENTER OFFLINE MODE", StringComparison.OrdinalIgnoreCase))
            return "GRG";

        if (line.Contains("WINCOR", StringComparison.OrdinalIgnoreCase)
            || line.Contains("NIXDORF", StringComparison.OrdinalIgnoreCase)
            || line.Contains("PROTOPAS", StringComparison.OrdinalIgnoreCase))
            return "WINCOR";

        if (line.Contains("HYOSUNG", StringComparison.OrdinalIgnoreCase)
            || line.Contains("NAUTILUS", StringComparison.OrdinalIgnoreCase)
            || line.Contains("HCDM", StringComparison.OrdinalIgnoreCase)
            || line.Contains("HCRW", StringComparison.OrdinalIgnoreCase))
            return "HYOSUNG";

        return "UNKNOWN";
    }

    private sealed class XfsRule
    {
        private readonly Regex _regex;

        public XfsRule(
            string code,
            string category,
            XfsSignalSeverity severity,
            string pattern,
            string recommendedAction)
        {
            Code = code;
            Category = category;
            Severity = severity;
            RecommendedAction = recommendedAction;
            _regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        }

        public string Code { get; }
        public string Category { get; }
        public XfsSignalSeverity Severity { get; }
        public string RecommendedAction { get; }

        public bool IsMatch(string line) => _regex.IsMatch(line ?? string.Empty);
    }
}
