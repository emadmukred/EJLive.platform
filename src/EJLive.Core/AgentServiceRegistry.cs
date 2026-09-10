using System;
using System.Collections.Generic;
using EJLive.Core.Network;
using EJLive.Core.Health;
using EJLive.Core.Config;

namespace EJLive.Core
{
    /// <summary>
    /// Centralized service bootstrapper for the EJLive Client Agent.
    /// Registers and wires all operational services.
    /// Core services auto-start; sensitive services are policy-governed.
    /// </summary>
    public sealed class AgentServiceRegistry
    {
        public AgentConfiguration Configuration { get; }
        public NetworkSessionManager NetworkSession { get; private set; }
        public HealthReporter HealthReporter { get; }
        public Journal.EjParserRegistry ParserRegistry { get; }
        public Vendors.VendorPathRegistry VendorPathRegistry { get; }
        public Data.DatabaseMigrationRunner MigrationRunner { get; }

        private readonly List<string> _startupLog = new List<string>();
        private bool _initialized;

        public AgentServiceRegistry(AgentConfiguration config)
        {
            Configuration = config ?? throw new ArgumentNullException(nameof(config));

            // Initialize health reporter
            HealthReporter = new HealthReporter(
                System.IO.Path.Combine(config.HealthPath, "health.json"));

            // Initialize journal parser registry
            var fallback = new Journal.GenericFallbackParser();
            ParserRegistry = new Journal.EjParserRegistry(fallback);
            RegisterVendorParsers();

            // Initialize vendor path registry
            VendorPathRegistry = new Vendors.VendorPathRegistry();
            RegisterVendorPaths();

            // Initialize database migration runner
            MigrationRunner = new Data.DatabaseMigrationRunner();
            foreach (var migration in Data.DatabaseMigrationRunner.GenerateRequiredMigrations())
                MigrationRunner.Register(migration);
        }

        /// <summary>
        /// Initializes all services and returns startup log.
        /// </summary>
        public List<string> Initialize(string serverHost, int serverPort, string atmId)
        {
            if (_initialized) return _startupLog;
            _initialized = true;

            Log("EJLive Agent Service Registry initializing...");
            Log($"ATM ID: {atmId}");
            Log($"Server: {serverHost}:{serverPort}");
            Log($"Vendor: {Configuration.Vendor}");
            Log($"Journal Path: {Configuration.SourceJournalPath}");
            Log($"Health Path: {Configuration.HealthPath}");

            // Initialize network session
            NetworkSession = new NetworkSessionManager(serverHost, serverPort, atmId);
            NetworkSession.OnLog += msg => Log(msg);
            NetworkSession.OnConnectionChanged += connected =>
            {
                Log(connected ? "Network connected" : "Network disconnected");
                WriteHealthSnapshot();
            };

            // Validate configuration
            var pathErrors = Configuration.ValidatePaths();
            if (pathErrors.Count > 0)
            {
                foreach (var err in pathErrors)
                    Log($"PATH WARNING: {err}");
            }
            else
            {
                Log("All configured paths validated.");
            }

            // Write initial health snapshot
            WriteHealthSnapshot();

            Log("Agent Service Registry initialized successfully.");
            Log($"Registered parsers: {string.Join(", ", ParserRegistry.RegisteredVendors)}");
            Log($"Registered vendor paths: {string.Join(", ", VendorPathRegistry.RegisteredVendors)}");
            Log($"Pending DB migrations: {MigrationRunner.GetPending().Count}");

            return _startupLog;
        }

        /// <summary>
        /// Writes current operational state to health.json.
        /// </summary>
        public void WriteHealthSnapshot()
        {
            try
            {
                var snapshot = new Health.HealthSnapshot
                {
                    AgentState = "Running",
                    SessionId = NetworkSession?.SessionId ?? string.Empty,
                    NetworkConnected = NetworkSession?.IsConnected ?? false,
                    LastHeartbeatUtc = NetworkSession?.LastHeartbeat ?? DateTimeOffset.MinValue,
                    LastSyncUtc = DateTimeOffset.UtcNow,
                    OutboxCount = 0,
                    FailedCount = 0,
                    WatcherState = "Active",
                    ImageInboxState = "Idle",
                    HealthScore = "Healthy",
                    ErrorCount = 0,
                    LastError = string.Empty,
                    SnapshotUtc = DateTimeOffset.UtcNow
                };
                HealthReporter.WriteSnapshot(snapshot);
            }
            catch (Exception ex)
            {
                Log($"Health snapshot write failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns a summary of all configured services.
        /// </summary>
        public Dictionary<string, string> GetServiceSummary()
        {
            return new Dictionary<string, string>
            {
                ["AgentId"] = Configuration.AgentId,
                ["Vendor"] = Configuration.Vendor,
                ["ServerHost"] = Configuration.ServerHost,
                ["ServerPort"] = Configuration.ServerPort.ToString(),
                ["NetworkConnected"] = (NetworkSession?.IsConnected ?? false).ToString(),
                ["SessionId"] = NetworkSession?.SessionId ?? "(none)",
                ["Parsers"] = string.Join(", ", ParserRegistry.RegisteredVendors),
                ["VendorPaths"] = string.Join(", ", VendorPathRegistry.RegisteredVendors),
                ["PendingMigrations"] = MigrationRunner.GetPending().Count.ToString(),
                ["ConfigSource"] = Configuration.ConfigSource,
            };
        }

        public List<string> GetStartupLog() => _startupLog;

        private void RegisterVendorParsers()
        {
            ParserRegistry.Register(new Journal.NcrEjTransactionParser());
            ParserRegistry.Register(new Journal.GrgEjTransactionParser());
            ParserRegistry.Register(new Journal.WincorEjTransactionParser());
            ParserRegistry.Register(new Journal.DieboldEjTransactionParser());
            ParserRegistry.Register(new Journal.HyosungEjTransactionParser());
            ParserRegistry.Register(new Journal.CashwayEjTransactionParser());
        }

        private void RegisterVendorPaths()
        {
            VendorPathRegistry.Register(new Vendors.NcrVendorPathProvider());
            VendorPathRegistry.Register(new Vendors.GrgVendorPathProvider());
            VendorPathRegistry.Register(new Vendors.WincorVendorPathProvider());
            VendorPathRegistry.Register(new Vendors.DieboldVendorPathProvider());
            VendorPathRegistry.Register(new Vendors.HyosungVendorPathProvider());
            VendorPathRegistry.Register(new Vendors.CashwayVendorPathProvider());
            VendorPathRegistry.Register(new Vendors.GenericVendorPathProvider());
        }

        private void Log(string message)
        {
            var entry = $"[{DateTimeOffset.UtcNow:HH:mm:ss}] {message}";
            _startupLog.Add(entry);
        }
    }
}