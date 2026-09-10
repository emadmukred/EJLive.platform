using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Generates operational, financial, and audit reports from ATM snapshots
    /// and transaction evidence. Supports export to structured formats.
    /// Aligned with section 10 (Reports Center) and section 10.1 capabilities.
    /// </summary>
    public sealed class ReportingExportEngine
    {
        private readonly string _reportsPath;
        private readonly List<string> _exportHistory = new();

        public event EventHandler<string>? OnLog;
        public event EventHandler<string>? OnReportExported;

        public ReportingExportEngine()
        {
            _reportsPath = AppConstants.DefaultReportsPath;
            EnsureDirectory();
        }

        public ReportingExportEngine(string reportsPath)
        {
            _reportsPath = reportsPath ?? AppConstants.DefaultReportsPath;
            EnsureDirectory();
        }

        public IReadOnlyList<string> GetExportHistory() => _exportHistory.AsReadOnly();

        private void EnsureDirectory() { if (!Directory.Exists(_reportsPath)) Directory.CreateDirectory(_reportsPath); }
        private void Log(string msg) => OnLog?.Invoke(this, $"[ReportExport] {msg}");

        /// <summary>Writes generated CSV content beneath the configured reports directory.</summary>
        public string SaveCsv(string fileName, string csvContent)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentNullException.ThrowIfNull(csvContent);

            EnsureDirectory();
            var safeName = Path.GetFileName(fileName.Trim());
            if (!safeName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                safeName += ".csv";

            var destination = Path.Combine(_reportsPath, safeName);
            File.WriteAllText(destination, csvContent, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            _exportHistory.Add(destination);
            Log($"Saved report '{safeName}'.");
            OnReportExported?.Invoke(this, destination);
            return destination;
        }

        // ===== Fleet Status Reports =====

        /// <summary>Generate a fleet-wide status summary.</summary>
        public FleetStatusReport GenerateFleetStatusReport(IEnumerable<ATMRealTimeStatusSnapshot> snapshots)
        {
            var list = snapshots.ToList();
            var report = new FleetStatusReport
            {
                GeneratedUtc = DateTime.UtcNow,
                TotalATMs = list.Count,
                OnlineATMs = list.Count(s => s.IsOnline),
                OfflineATMs = list.Count(s => !s.IsOnline),
                DegradedATMs = list.Count(s => s.ConnectionState == "Degraded"),
                WarningATMs = list.Count(s => s.Priority is "Medium" or "Low"),
                CriticalATMs = list.Count(s => s.Priority is "Critical" or "High"),
                OutOfServiceATMs = list.Count(s => s.OperationalMode == "OutOfService"),
                SyncSuccessRate = list.Count > 0
                    ? Math.Round((double)list.Count(s => s.OutboxState == "Idle" || s.OutboxState == "Active") / list.Count * 100, 1)
                    : 0,
                OpenAlerts = list.Sum(s => s.ActiveAlarms.Count),
                FailedTransfers = list.Count(s => s.OutboxState == "Error"),
                LastIngestionUtc = list.Max(s => (DateTime?)s.LastFileSyncUtc),
                AverageConfidence = list.Count > 0 ? Math.Round(list.Average(s => s.ConfidenceScore), 2) : 0
            };

            // Vendor distribution
            report.VendorDistribution = list.GroupBy(s => string.IsNullOrEmpty(s.Vendor) ? "Unknown" : s.Vendor)
                .ToDictionary(g => g.Key, g => g.Count());

            // Region distribution
            report.RegionDistribution = list.GroupBy(s => string.IsNullOrEmpty(s.Region) ? "Unknown" : s.Region)
                .ToDictionary(g => g.Key, g => g.Count());

            // ATM details
            report.AtmDetails = list.Select(s => new AtmStatusEntry
            {
                ATM_ID = s.ATM_ID,
                ATM_Name = s.ATM_Name,
                Vendor = s.Vendor,
                Branch = s.Branch,
                Region = s.Region,
                ConnectionState = s.ConnectionState,
                OperationalMode = s.OperationalMode,
                HealthScore = Math.Round(s.ConfidenceScore * 100, 0),
                Priority = s.Priority,
                LastHeartbeat = s.LastHeartbeatUtc,
                ActiveAlarms = s.ActiveAlarms,
                RecommendedAction = s.RecommendedAction
            }).ToList();

            return report;
        }

        // ===== ATM Detail Report =====

        /// <summary>Generate a detailed report for a single ATM.</summary>
        public AtmDetailReport GenerateAtmDetailReport(ATMRealTimeStatusSnapshot snapshot)
        {
            return new AtmDetailReport
            {
                GeneratedUtc = DateTime.UtcNow,
                Snapshot = snapshot,
                DeviceSummary = GenerateDeviceSummary(snapshot),
                CashSummary = GenerateCashSummary(snapshot),
                RiskSummary = GenerateRiskSummary(snapshot),
                RemoteOpsSummary = GenerateRemoteOpsSummary(snapshot),
                ImpactSummary = GenerateImpactSummary(snapshot)
            };
        }

        private DeviceSummary GenerateDeviceSummary(ATMRealTimeStatusSnapshot s) => new()
        {
            CashDispenser = s.CashDispenserStatus,
            CardReader = s.CardReaderStatus,
            PinPad = s.PinPadStatus,
            ReceiptPrinter = s.ReceiptPrinterStatus,
            JournalPrinter = s.JournalPrinterStatus,
            SIU = s.SiuStatus,
            Camera = s.CameraStatus,
            Depository = s.DepositoryStatus,
            HDD = s.HddHealth,
            FaultCount = new[] { s.CashDispenserStatus, s.CardReaderStatus, s.PinPadStatus, s.ReceiptPrinterStatus,
                s.JournalPrinterStatus, s.SiuStatus, s.CameraStatus, s.DepositoryStatus, s.HddHealth }
                .Count(d => d.Contains("Fault") || d.Contains("Error") || d.Contains("Critical"))
        };

        private CashSummary GenerateCashSummary(ATMRealTimeStatusSnapshot s) => new()
        {
            TotalRemaining = s.TotalCashRemaining,
            CassetteCount = s.CassetteRemaining.Count,
            CassetteDetails = s.CassetteRemaining.ToDictionary(k => k.Key, v => v.Value),
            RejectBin = s.RejectBinCount,
            RetractBin = s.RetractBinCount,
            CashLow = s.CashLow,
            CashOut = s.CashOut,
            Currency = s.Currency,
            Denominations = s.Denominations.ToDictionary(k => k.Key, v => v.Value)
        };

        private RiskSummary GenerateRiskSummary(ATMRealTimeStatusSnapshot s) => new()
        {
            ApprovedNoDispense = s.ApprovedNoDispense,
            PartialDispense = s.PartialDispense,
            CashJam = s.CashJam,
            RetractDetected = s.RetractDetected,
            ReversalDetected = s.ReversalDetected,
            DuplicateSequence = s.DuplicateSequence,
            MissingSequence = s.MissingSequence,
            SuspectTransaction = s.SuspectTransaction,
            DeclinedTransaction = s.DeclinedTransaction,
            TotalRiskFlags = new[] { s.ApprovedNoDispense, s.PartialDispense, s.CashJam, s.RetractDetected,
                s.ReversalDetected, s.DuplicateSequence, s.MissingSequence, s.SuspectTransaction }
                .Count(r => r)
        };

        private RemoteOpsSummary GenerateRemoteOpsSummary(ATMRealTimeStatusSnapshot s) => new()
        {
            LastRemoteDesktop = s.LastRemoteDesktopSessionUtc,
            LastRemoteAssistance = s.LastRemoteAssistanceSessionUtc,
            LastControlledCommand = s.LastControlledCommandId,
            ApprovalAuditState = s.RemoteApprovalAuditState
        };

        private ImpactSummary GenerateImpactSummary(ATMRealTimeStatusSnapshot s) => new()
        {
            Customer = s.CustomerImpact,
            Cash = s.CashImpact,
            Device = s.DeviceImpact,
            Switch = s.SwitchImpact,
            Audit = s.AuditImpact
        };

        // ===== Sync Failures Report =====

        /// <summary>Generate a sync failures report.</summary>
        public SyncFailuresReport GenerateSyncFailuresReport(IEnumerable<ATMRealTimeStatusSnapshot> snapshots)
        {
            var syncIssues = snapshots
                .Where(s => s.OutboxState == "Error" || s.JournalStale || s.SyncState == "Failed")
                .Select(s => new SyncFailureEntry
                {
                    ATM_ID = s.ATM_ID,
                    ATM_Name = s.ATM_Name,
                    OutboxState = s.OutboxState,
                    SyncState = s.SyncState,
                    JournalStale = s.JournalStale,
                    LastFileSync = s.LastFileSyncUtc,
                    LastEJournalSync = s.LastEJournalSyncUtc,
                    JournalDeltaLines = s.JournalDeltaLines,
                    RecommendedAction = s.RecommendedAction
                }).ToList();

            return new SyncFailuresReport
            {
                GeneratedUtc = DateTime.UtcNow,
                TotalSyncFailures = syncIssues.Count,
                FailureEntries = syncIssues
            };
        }

        // ===== Journal Parser Summary =====

        /// <summary>Generate a journal parser summary report.</summary>
        public JournalParserSummary GenerateJournalParserSummary(
            IEnumerable<ATMRealTimeStatusSnapshot> snapshots,
            Dictionary<string, JournalParserStats>? vendorStats = null)
        {
            var entries = snapshots.Select(s => new JournalParserEntry
            {
                ATM_ID = s.ATM_ID,
                ATM_Name = s.ATM_Name,
                Vendor = s.Vendor,
                LastJournalFile = s.LastJournalFile,
                LastJournalLine = s.LastJournalLine,
                JournalHealth = s.JournalHealth,
                EJournalBackupState = s.EJournalBackupState,
                JournalStale = s.JournalStale,
                JournalDeltaLines = s.JournalDeltaLines,
                LastJournalSync = s.LastJournalSyncUtc
            }).ToList();

            return new JournalParserSummary
            {
                GeneratedUtc = DateTime.UtcNow,
                TotalParsedATMs = entries.Count(e => !string.IsNullOrEmpty(e.LastJournalFile)),
                StaleATMs = entries.Count(e => e.JournalStale),
                Entries = entries,
                VendorStats = vendorStats ?? new Dictionary<string, JournalParserStats>()
            };
        }

        // ===== Cash Dispense Risk Report =====

        /// <summary>Generate a cash/dispense risk report.</summary>
        public CashDispenseRiskReport GenerateCashDispenseRiskReport(IEnumerable<ATMRealTimeStatusSnapshot> snapshots)
        {
            var riskEntries = snapshots
                .Where(s => s.ApprovedNoDispense || s.PartialDispense || s.CashJam ||
                            s.ReversalDetected || s.CashLow || s.CashOut)
                .Select(s => new CashRiskEntry
                {
                    ATM_ID = s.ATM_ID,
                    ATM_Name = s.ATM_Name,
                    Branch = s.Branch,
                    ApprovedNoDispense = s.ApprovedNoDispense,
                    PartialDispense = s.PartialDispense,
                    CashJam = s.CashJam,
                    Reversal = s.ReversalDetected,
                    CashLow = s.CashLow,
                    CashOut = s.CashOut,
                    TotalRemaining = s.TotalCashRemaining,
                    RejectBin = s.RejectBinCount,
                    RetractBin = s.RetractBinCount,
                    RecommendedAction = s.RecommendedAction
                }).ToList();

            return new CashDispenseRiskReport
            {
                GeneratedUtc = DateTime.UtcNow,
                TotalRiskATMs = riskEntries.Count,
                ApprovedNoDispenseCount = riskEntries.Count(r => r.ApprovedNoDispense),
                PartialDispenseCount = riskEntries.Count(r => r.PartialDispense),
                CashJamCount = riskEntries.Count(r => r.CashJam),
                CashOutCount = riskEntries.Count(r => r.CashOut),
                RiskEntries = riskEntries
            };
        }

        // ===== Vendor Status Summary =====

        /// <summary>Generate a vendor-specific status summary.</summary>
        public VendorStatusSummary GenerateVendorStatusSummary(IEnumerable<ATMRealTimeStatusSnapshot> snapshots)
        {
            var vendorGroups = snapshots
                .GroupBy(s => string.IsNullOrEmpty(s.Vendor) ? "Unknown" : s.Vendor)
                .Select(g => new VendorGroupSummary
                {
                    Vendor = g.Key,
                    TotalATMs = g.Count(),
                    OnlineATMs = g.Count(s => s.IsOnline),
                    OfflineATMs = g.Count(s => !s.IsOnline),
                    OutOfService = g.Count(s => s.OperationalMode == "OutOfService"),
                    AverageConfidence = Math.Round(g.Average(s => s.ConfidenceScore), 2),
                    DeviceFaultCount = g.Sum(s =>
                        new[] { s.CashDispenserStatus, s.CardReaderStatus, s.PinPadStatus }
                        .Count(d => d.Contains("Fault") || d.Contains("Error"))),
                    RiskFlagCount = g.Sum(s => new[] { s.ApprovedNoDispense, s.PartialDispense, s.CashJam }
                        .Count(r => r))
                }).ToList();

            return new VendorStatusSummary
            {
                GeneratedUtc = DateTime.UtcNow,
                VendorGroups = vendorGroups
            };
        }

        // ===== Device Health Ranking =====

        /// <summary>Generate a device health ranking across the fleet.</summary>
        public DeviceHealthRanking GenerateDeviceHealthRanking(IEnumerable<ATMRealTimeStatusSnapshot> snapshots)
        {
            var list = snapshots.ToList();
            var deviceFaults = new Dictionary<string, int>();

            foreach (var s in list)
            {
                CountDeviceFault(deviceFaults, "CashDispenser", s.CashDispenserStatus);
                CountDeviceFault(deviceFaults, "CardReader", s.CardReaderStatus);
                CountDeviceFault(deviceFaults, "PinPad", s.PinPadStatus);
                CountDeviceFault(deviceFaults, "ReceiptPrinter", s.ReceiptPrinterStatus);
                CountDeviceFault(deviceFaults, "JournalPrinter", s.JournalPrinterStatus);
                CountDeviceFault(deviceFaults, "SIU", s.SiuStatus);
                CountDeviceFault(deviceFaults, "Camera", s.CameraStatus);
                CountDeviceFault(deviceFaults, "Depository", s.DepositoryStatus);
                CountDeviceFault(deviceFaults, "HDD", s.HddHealth);
            }

            return new DeviceHealthRanking
            {
                GeneratedUtc = DateTime.UtcNow,
                TotalATMs = list.Count,
                DeviceFaultCounts = deviceFaults.OrderByDescending(kv => kv.Value)
                    .ToDictionary(k => k.Key, v => v.Value),
                AtmHealthScores = list.Select(s => new AtmHealthEntry
                {
                    ATM_ID = s.ATM_ID,
                    ATM_Name = s.ATM_Name,
                    HealthScore = Math.Round(s.ConfidenceScore * 100, 0),
                    DeviceFaults = new[] { s.CashDispenserStatus, s.CardReaderStatus, s.PinPadStatus,
                        s.ReceiptPrinterStatus, s.JournalPrinterStatus, s.SiuStatus }
                        .Count(d => d.Contains("Fault") || d.Contains("Error"))
                }).OrderBy(a => a.HealthScore).ToList()
            };
        }

        // ===== Export to CSV =====

        /// <summary>Export a fleet status report to CSV format.</summary>
        public string ExportFleetStatusToCsv(IEnumerable<ATMRealTimeStatusSnapshot> snapshots)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ATM_ID,ATM_Name,Vendor,Branch,Region,ConnectionState,OperationalMode,IsOnline,Priority,HealthScore,LastHeartbeat,ActiveAlarms,RecommendedAction");

            foreach (var s in snapshots)
            {
                var alarms = s.ActiveAlarms.Count > 0
                    ? $"\"{string.Join("; ", s.ActiveAlarms)}\""
                    : "";
                var action = !string.IsNullOrEmpty(s.RecommendedAction)
                    ? $"\"{s.RecommendedAction}\""
                    : "";

                sb.AppendLine($"{s.ATM_ID},{s.ATM_Name},{s.Vendor},{s.Branch},{s.Region}," +
                    $"{s.ConnectionState},{s.OperationalMode},{s.IsOnline},{s.Priority}," +
                    $"{Math.Round(s.ConfidenceScore * 100, 0)}," +
                    $"{s.LastHeartbeatUtc:yyyy-MM-dd HH:mm:ss}," +
                    $"{alarms},{action}");
            }

            return sb.ToString();
        }

        /// <summary>Export cash risk entries to CSV.</summary>
        public string ExportCashRiskToCsv(CashDispenseRiskReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ATM_ID,ATM_Name,Branch,ApprovedNoDispense,PartialDispense,CashJam,Reversal,CashLow,CashOut,TotalRemaining,RejectBin,RetractBin,Action");

            foreach (var r in report.RiskEntries)
            {
                sb.AppendLine($"{r.ATM_ID},{r.ATM_Name},{r.Branch}," +
                    $"{r.ApprovedNoDispense},{r.PartialDispense},{r.CashJam},{r.Reversal}," +
                    $"{r.CashLow},{r.CashOut},{r.TotalRemaining},{r.RejectBin},{r.RetractBin}," +
                    $"\"{r.RecommendedAction}\"");
            }

            return sb.ToString();
        }

        /// <summary>Export sync failures to CSV.</summary>
        public string ExportSyncFailuresToCsv(SyncFailuresReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ATM_ID,ATM_Name,OutboxState,SyncState,JournalStale,LastFileSync,LastEJournalSync,DeltaLines,Action");

            foreach (var f in report.FailureEntries)
            {
                sb.AppendLine($"{f.ATM_ID},{f.ATM_Name},{f.OutboxState},{f.SyncState}," +
                    $"{f.JournalStale},{f.LastFileSync:yyyy-MM-dd HH:mm:ss}," +
                    $"{f.LastEJournalSync:yyyy-MM-dd HH:mm:ss},{f.JournalDeltaLines}," +
                    $"\"{f.RecommendedAction}\"");
            }

            return sb.ToString();
        }

        // ===== Helpers =====

        private static void CountDeviceFault(Dictionary<string, int> faults, string device, string status)
        {
            if (status.Contains("Fault") || status.Contains("Error") || status.Contains("Critical"))
            {
                faults.TryGetValue(device, out var count);
                faults[device] = count + 1;
            }
        }
    }

    // ===== Report Models =====

    /// <summary>Fleet-wide operational status report.</summary>
    public sealed class FleetStatusReport
    {
        public DateTime GeneratedUtc { get; set; }
        public int TotalATMs { get; set; }
        public int OnlineATMs { get; set; }
        public int OfflineATMs { get; set; }
        public int DegradedATMs { get; set; }
        public int WarningATMs { get; set; }
        public int CriticalATMs { get; set; }
        public int OutOfServiceATMs { get; set; }
        public double SyncSuccessRate { get; set; }
        public int OpenAlerts { get; set; }
        public int FailedTransfers { get; set; }
        public DateTime? LastIngestionUtc { get; set; }
        public double AverageConfidence { get; set; }
        public Dictionary<string, int> VendorDistribution { get; set; } = new();
        public Dictionary<string, int> RegionDistribution { get; set; } = new();
        public List<AtmStatusEntry> AtmDetails { get; set; } = new();
    }

    public sealed class AtmStatusEntry
    {
        public string ATM_ID { get; set; } = string.Empty;
        public string ATM_Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string ConnectionState { get; set; } = string.Empty;
        public string OperationalMode { get; set; } = string.Empty;
        public double HealthScore { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateTime? LastHeartbeat { get; set; }
        public List<string> ActiveAlarms { get; set; } = new();
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>Detailed report for a single ATM.</summary>
    public sealed class AtmDetailReport
    {
        public DateTime GeneratedUtc { get; set; }
        public ATMRealTimeStatusSnapshot Snapshot { get; set; } = new();
        public DeviceSummary DeviceSummary { get; set; } = new();
        public CashSummary CashSummary { get; set; } = new();
        public RiskSummary RiskSummary { get; set; } = new();
        public RemoteOpsSummary RemoteOpsSummary { get; set; } = new();
        public ImpactSummary ImpactSummary { get; set; } = new();
    }

    public sealed class DeviceSummary
    {
        public string CashDispenser { get; set; } = "Unknown";
        public string CardReader { get; set; } = "Unknown";
        public string PinPad { get; set; } = "Unknown";
        public string ReceiptPrinter { get; set; } = "Unknown";
        public string JournalPrinter { get; set; } = "Unknown";
        public string SIU { get; set; } = "Unknown";
        public string Camera { get; set; } = "Unknown";
        public string Depository { get; set; } = "Unknown";
        public string HDD { get; set; } = "Unknown";
        public int FaultCount { get; set; }
    }

    public sealed class CashSummary
    {
        public long TotalRemaining { get; set; }
        public int CassetteCount { get; set; }
        public Dictionary<string, long> CassetteDetails { get; set; } = new();
        public long RejectBin { get; set; }
        public long RetractBin { get; set; }
        public bool CashLow { get; set; }
        public bool CashOut { get; set; }
        public string Currency { get; set; } = string.Empty;
        public Dictionary<string, int> Denominations { get; set; } = new();
    }

    public sealed class RiskSummary
    {
        public bool ApprovedNoDispense { get; set; }
        public bool PartialDispense { get; set; }
        public bool CashJam { get; set; }
        public bool RetractDetected { get; set; }
        public bool ReversalDetected { get; set; }
        public bool DuplicateSequence { get; set; }
        public bool MissingSequence { get; set; }
        public bool SuspectTransaction { get; set; }
        public bool DeclinedTransaction { get; set; }
        public int TotalRiskFlags { get; set; }
    }

    public sealed class RemoteOpsSummary
    {
        public DateTime? LastRemoteDesktop { get; set; }
        public DateTime? LastRemoteAssistance { get; set; }
        public string LastControlledCommand { get; set; } = string.Empty;
        public bool ApprovalAuditState { get; set; }
    }

    public sealed class ImpactSummary
    {
        public string Customer { get; set; } = "None";
        public string Cash { get; set; } = "None";
        public string Device { get; set; } = "None";
        public string Switch { get; set; } = "None";
        public string Audit { get; set; } = "None";
    }

    /// <summary>Sync failures report.</summary>
    public sealed class SyncFailuresReport
    {
        public DateTime GeneratedUtc { get; set; }
        public int TotalSyncFailures { get; set; }
        public List<SyncFailureEntry> FailureEntries { get; set; } = new();
    }

    public sealed class SyncFailureEntry
    {
        public string ATM_ID { get; set; } = string.Empty;
        public string ATM_Name { get; set; } = string.Empty;
        public string OutboxState { get; set; } = string.Empty;
        public string SyncState { get; set; } = string.Empty;
        public bool JournalStale { get; set; }
        public DateTime? LastFileSync { get; set; }
        public DateTime? LastEJournalSync { get; set; }
        public int JournalDeltaLines { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>Journal parser summary report.</summary>
    public sealed class JournalParserSummary
    {
        public DateTime GeneratedUtc { get; set; }
        public int TotalParsedATMs { get; set; }
        public int StaleATMs { get; set; }
        public List<JournalParserEntry> Entries { get; set; } = new();
        public Dictionary<string, JournalParserStats> VendorStats { get; set; } = new();
    }

    public sealed class JournalParserEntry
    {
        public string ATM_ID { get; set; } = string.Empty;
        public string ATM_Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string LastJournalFile { get; set; } = string.Empty;
        public long LastJournalLine { get; set; }
        public string JournalHealth { get; set; } = string.Empty;
        public string EJournalBackupState { get; set; } = string.Empty;
        public bool JournalStale { get; set; }
        public int JournalDeltaLines { get; set; }
        public DateTime? LastJournalSync { get; set; }
    }

    public sealed class JournalParserStats
    {
        public int TotalFiles { get; set; }
        public int TransactionsFound { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SuspiciousCount { get; set; }
    }

    /// <summary>Cash dispense risk report.</summary>
    public sealed class CashDispenseRiskReport
    {
        public DateTime GeneratedUtc { get; set; }
        public int TotalRiskATMs { get; set; }
        public int ApprovedNoDispenseCount { get; set; }
        public int PartialDispenseCount { get; set; }
        public int CashJamCount { get; set; }
        public int CashOutCount { get; set; }
        public List<CashRiskEntry> RiskEntries { get; set; } = new();
    }

    public sealed class CashRiskEntry
    {
        public string ATM_ID { get; set; } = string.Empty;
        public string ATM_Name { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public bool ApprovedNoDispense { get; set; }
        public bool PartialDispense { get; set; }
        public bool CashJam { get; set; }
        public bool Reversal { get; set; }
        public bool CashLow { get; set; }
        public bool CashOut { get; set; }
        public long TotalRemaining { get; set; }
        public long RejectBin { get; set; }
        public long RetractBin { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>Vendor status summary report.</summary>
    public sealed class VendorStatusSummary
    {
        public DateTime GeneratedUtc { get; set; }
        public List<VendorGroupSummary> VendorGroups { get; set; } = new();
    }

    public sealed class VendorGroupSummary
    {
        public string Vendor { get; set; } = string.Empty;
        public int TotalATMs { get; set; }
        public int OnlineATMs { get; set; }
        public int OfflineATMs { get; set; }
        public int OutOfService { get; set; }
        public double AverageConfidence { get; set; }
        public int DeviceFaultCount { get; set; }
        public int RiskFlagCount { get; set; }
    }

    /// <summary>Device health ranking report.</summary>
    public sealed class DeviceHealthRanking
    {
        public DateTime GeneratedUtc { get; set; }
        public int TotalATMs { get; set; }
        public Dictionary<string, int> DeviceFaultCounts { get; set; } = new();
        public List<AtmHealthEntry> AtmHealthScores { get; set; } = new();
    }

    public sealed class AtmHealthEntry
    {
        public string ATM_ID { get; set; } = string.Empty;
        public string ATM_Name { get; set; } = string.Empty;
        public double HealthScore { get; set; }
        public int DeviceFaults { get; set; }
    }
}
