using EJLive.Core;
using EJLive.Core.Data;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using System;
using System.Collections.Generic;

namespace EJLive.Core.Services
{
    public partial class OperationalStateStore
    {
        public static OperationalStateStore Instance { get; } = new();
        public UnifiedRuntimeSnapshot Snapshot => new();
        public FleetSummary BuildSummary() => new();
        public void Upsert(ATMInfo atm) { }
    }

    public partial class JournalSyncTrackingService
    {
        public List<JournalSyncRecord> Records { get; } = new();
        public SyncSummary BuildSummary() => new() { Total = Records.Count };
        public void AddOrUpdate(JournalSyncRecord record) { }
    }

    public partial class JournalSyncService
    {
    }

    public partial class AlertManager
    {
        public List<AlertPayload> Alerts { get; } = new();
    }

    public partial class RoleBasedAccess
    {
    }

    public partial class VendorRootCapabilityService
    {
    }

    public partial class XfsLogAnalysisService
    {
    }

    public partial class TransactionAnalysisEngine
    {
    }

    public partial class ReportExportEngine
    {
    }

    public partial class RemoteAssistanceEngine
    {
    }

    public partial class SyncSummary
    {
    }

    public partial class AlertPayload
    {
    }

    public partial class UnifiedRuntimeSnapshot
    {
    }

    public partial class AppLogger
    {
    }

    public partial class DatabaseManager
    {
    }

}
