using EJLive.Business;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Application;

public sealed class EJLiveApplicationHost : IDisposable
{
    private readonly UnifiedBusinessRuntime _runtime;

    public EJLiveApplicationHost(UnifiedBusinessRuntime runtime)
    {
        _runtime = runtime;
    }

    public UnifiedBusinessRuntime Runtime => _runtime;

    public static EJLiveApplicationHost Create(string? databasePath = null)
    {
        return new EJLiveApplicationHost(UnifiedBusinessRuntime.CreateInitialized(databasePath));
    }

    public ApplicationReadinessReport ValidateReadiness()
    {
        var snapshot = _runtime.BuildSnapshot();
        const string readinessJournal = "NCR EJDATA APPROVED AMOUNT 500\nM-18 CASH ERROR\nHOST MESSAGE OUT";
        var fusion = _runtime.BuildOperationalFusion(readinessJournal);
        var journalAnalysis = _runtime.JournalStorage.Analyze("ATM-APP-HOST", readinessJournal);
        var fleetReadiness = _runtime.FleetReadiness.Evaluate();
        var clientStates = _runtime.ClientServiceSupervisor.GetAllStates();
        var pendingCommands = _runtime.RemoteCommands.GetPendingCommands();
        var distinctCapabilities = snapshot.Capabilities
            .Select(capability => $"{capability.Layer}:{capability.Name}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var checks = new List<ApplicationReadinessCheck>
        {
            new("Database initialized", _runtime.Database.IsInitialized, "SQLite schema and indexes are ready."),
            new("Functional map populated", snapshot.Capabilities.Count >= 8 && distinctCapabilities == snapshot.Capabilities.Count, $"{snapshot.Capabilities.Count} unique runtime capabilities mapped."),
            new("Operational fusion active", fusion.JournalLinesAnalyzed == 3 && fusion.CommandAllowed, $"Journal lines analyzed: {fusion.JournalLinesAnalyzed}; command policy: {fusion.CommandReason}."),
            new("Journal analysis active", journalAnalysis.TotalLines == 3 && journalAnalysis.Errors.Count == 0, journalAnalysis.Summary),
            new("Fleet readiness active", fleetReadiness.GeneratedAtUtc != default, fleetReadiness.Summary),
            new("Client supervision state valid", clientStates.All(state => !string.IsNullOrWhiteSpace(state.AgentId) && state.ServerPort is > 0 and <= 65535), $"Registered client agents: {clientStates.Count}."),
            new("Remote command state available", pendingCommands.All(command => !string.IsNullOrWhiteSpace(command.CommandId)), $"Pending commands: {pendingCommands.Count}."),
            new("Vendor catalog available", _runtime.VendorCapabilities.GetCapabilities(AppConstants.ATM_TYPE_NCR).Count > 0, "NCR capabilities resolved."),
            new("Access policy available", _runtime.Access.Can("Admin", "remote"), "Admin role can execute remote permission.")
        };

        return new ApplicationReadinessReport(checks);
    }

    public IReadOnlyList<DataFlowStep> DescribeDataFlow()
    {
        return new[]
        {
            new DataFlowStep(1, "Presentation", "The companion UI reads status snapshots and submits explicit operator intent through the application boundary."),
            new DataFlowStep(2, "Application", "The application host validates the request and selects the business workflow."),
            new DataFlowStep(3, "Business", "Business services enforce policy, coordinate journal work, update ATM state, and publish outcome snapshots."),
            new DataFlowStep(4, "Core", "Protocol, security, parsers, and engines validate and normalize operational payloads."),
            new DataFlowStep(5, "Data", "DatabaseManager persists audit and sync records in SQLite."),
            new DataFlowStep(6, "Presentation", "Dashboards render read-only status, alerts, journal summaries, and reports.")
        };
    }

    public ATMInfo SeedDemoAtm(string atmId = "ATM-DEMO")
    {
        return _runtime.RegisterAtm(atmId, "Demo Terminal", AppConstants.ATM_TYPE_NCR, "127.0.0.1");
    }

    public void Dispose()
    {
        _runtime.Dispose();
    }
}

public sealed record DataFlowStep(int Order, string Layer, string Description);

public sealed record ApplicationReadinessCheck(string Name, bool Passed, string Detail);

public sealed class ApplicationReadinessReport
{
    public ApplicationReadinessReport(IReadOnlyList<ApplicationReadinessCheck> checks)
    {
        Checks = checks;
    }

    public IReadOnlyList<ApplicationReadinessCheck> Checks { get; }
    public bool Passed => Checks.All(check => check.Passed);
}
