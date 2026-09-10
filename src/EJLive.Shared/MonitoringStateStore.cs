using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace EJLive.Shared.Monitoring
{
    public class MonitoringStateStore
    {
        private readonly object _sync = new object();
        private readonly string _storagePath;
        private readonly string _stateDirectory;
        private readonly string _stateFilePath;

        public MonitoringStateStore(string storagePath)
        {
            _storagePath = string.IsNullOrWhiteSpace(storagePath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")
                : storagePath;
            _stateDirectory = Path.Combine(_storagePath, "monitoring");
            _stateFilePath = Path.Combine(_stateDirectory, "monitoring-state.xml");
        }

        public string StoragePath { get { return _storagePath; } }
        public string StateFilePath { get { return _stateFilePath; } }

        public void EnsureInitialized()
        {
            lock (_sync)
            {
                EnsureDirectory();
                if (!File.Exists(_stateFilePath))
                    SaveInternal(new MonitoringSystemState());
            }
        }

        public MonitoringSystemState Load()
        {
            lock (_sync)
            {
                EnsureDirectory();
                if (!File.Exists(_stateFilePath))
                {
                    var initial = new MonitoringSystemState();
                    SaveInternal(initial);
                    return initial;
                }

                var serializer = new XmlSerializer(typeof(MonitoringSystemState));
                using (var stream = File.OpenRead(_stateFilePath))
                {
                    var state = serializer.Deserialize(stream) as MonitoringSystemState;
                    if (state == null) return new MonitoringSystemState();
                    if (state.Terminals == null) state.Terminals = new System.Collections.Generic.List<MonitoringTerminalState>();
                    if (state.Alerts == null) state.Alerts = new System.Collections.Generic.List<MonitoringAlertEntry>();
                    return state;
                }
            }
        }

        public void RecordTerminalConnected(string terminalId, string endpoint)
        {
            Update(state =>
            {
                var terminal = GetOrCreateTerminal(state, terminalId);
                terminal.RemoteEndpoint = endpoint ?? string.Empty;
                terminal.Status = "Connected";
                terminal.Health = "Online";
                terminal.LastHeartbeatUtc = DateTime.UtcNow;
            });
        }

        public void RecordHeartbeat(string terminalId)
        {
            Update(state =>
            {
                var terminal = GetOrCreateTerminal(state, terminalId);
                terminal.Status = "Connected";
                terminal.Health = "Online";
                terminal.LastHeartbeatUtc = DateTime.UtcNow;
            });
        }

        public void RecordEjSync(string terminalId, int payloadBytes)
        {
            Update(state =>
            {
                var terminal = GetOrCreateTerminal(state, terminalId);
                terminal.Status = "Connected";
                terminal.Health = "Online";
                terminal.LastHeartbeatUtc = DateTime.UtcNow;
                terminal.LastEjSyncUtc = DateTime.UtcNow;
                terminal.LastTransaction = "EJ sync received (" + payloadBytes + " bytes)";
            });
        }

        public void RecordTerminalDisconnected(string terminalId)
        {
            Update(state =>
            {
                var terminal = GetOrCreateTerminal(state, terminalId);
                terminal.Status = "Disconnected";
                terminal.Health = "Offline";
            });
        }

        public void UpdateTerminalMetadata(string terminalId, string branchName, string vendor, string network, string region)
        {
            Update(state =>
            {
                var terminal = GetOrCreateTerminal(state, terminalId);
                if (!string.IsNullOrWhiteSpace(branchName)) terminal.BranchName = branchName.Trim();
                if (!string.IsNullOrWhiteSpace(vendor)) terminal.Vendor = vendor.Trim();
                if (!string.IsNullOrWhiteSpace(network)) terminal.Network = network.Trim();
                if (!string.IsNullOrWhiteSpace(region)) terminal.Region = region.Trim();
            });
        }

        public void UpdateCashState(string terminalId, MonitoringCashState cashState)
        {
            Update(state =>
            {
                var terminal = GetOrCreateTerminal(state, terminalId);
                terminal.Cash = cashState ?? new MonitoringCashState();
                terminal.Cash.UpdatedAtUtc = DateTime.UtcNow;
            });
        }

        public void RecordAlert(string terminalId, string severity, string message)
        {
            Update(state =>
            {
                var normalizedTerminalId = NormalizeTerminalId(terminalId);
                var terminal = GetOrCreateTerminal(state, normalizedTerminalId);
                terminal.ActiveAlerts++;
                if (!string.Equals(severity, "Info", StringComparison.OrdinalIgnoreCase))
                    terminal.Health = string.Equals(severity, "Critical", StringComparison.OrdinalIgnoreCase) ? "Critical" : "Warning";

                state.Alerts.Insert(0, new MonitoringAlertEntry
                {
                    RaisedAtUtc = DateTime.UtcNow,
                    TerminalId = normalizedTerminalId,
                    Severity = string.IsNullOrWhiteSpace(severity) ? "Info" : severity,
                    Message = message ?? string.Empty
                });

                if (state.Alerts.Count > 200)
                    state.Alerts = state.Alerts.Take(200).ToList();
            });
        }

        public void RecordSystemEvent(string severity, string message)
        {
            RecordAlert("SYSTEM", severity, message);
        }

        public static string ResolveStoragePathFromConfig(string baseDirectory)
        {
            string monitorConfig = Path.Combine(baseDirectory, "ejlive_monitor.ini");
            string serverConfig = Path.Combine(baseDirectory, "ejlive_server.ini");

            string fromMonitorConfig = ReadStoragePathFromIni(monitorConfig);
            if (!string.IsNullOrWhiteSpace(fromMonitorConfig)) return fromMonitorConfig;

            string fromServerConfig = ReadStoragePathFromIni(serverConfig);
            if (!string.IsNullOrWhiteSpace(fromServerConfig)) return fromServerConfig;

            return Path.Combine(baseDirectory, "Data");
        }

        private void Update(Action<MonitoringSystemState> updateAction)
        {
            lock (_sync)
            {
                EnsureDirectory();
                MonitoringSystemState state;
                if (!File.Exists(_stateFilePath))
                {
                    state = new MonitoringSystemState();
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(MonitoringSystemState));
                    using (var stream = File.OpenRead(_stateFilePath))
                    {
                        state = serializer.Deserialize(stream) as MonitoringSystemState ?? new MonitoringSystemState();
                    }
                    if (state.Terminals == null) state.Terminals = new System.Collections.Generic.List<MonitoringTerminalState>();
                    if (state.Alerts == null) state.Alerts = new System.Collections.Generic.List<MonitoringAlertEntry>();
                }

                updateAction(state);
                state.LastUpdatedUtc = DateTime.UtcNow;
                SaveInternal(state);
            }
        }

        private void SaveInternal(MonitoringSystemState state)
        {
            var serializer = new XmlSerializer(typeof(MonitoringSystemState));
            using (var stream = File.Create(_stateFilePath))
            {
                serializer.Serialize(stream, state);
            }
        }

        private void EnsureDirectory()
        {
            if (!Directory.Exists(_stateDirectory)) Directory.CreateDirectory(_stateDirectory);
        }

        private static MonitoringTerminalState GetOrCreateTerminal(MonitoringSystemState state, string terminalId)
        {
            string normalized = NormalizeTerminalId(terminalId);
            var terminal = state.Terminals.FirstOrDefault(t => string.Equals(t.TerminalId, normalized, StringComparison.OrdinalIgnoreCase));
            if (terminal == null)
            {
                terminal = new MonitoringTerminalState { TerminalId = normalized };
                state.Terminals.Add(terminal);
            }
            return terminal;
        }

        private static string NormalizeTerminalId(string terminalId)
        {
            return string.IsNullOrWhiteSpace(terminalId) ? "Unknown" : terminalId.Trim();
        }

        private static string ReadStoragePathFromIni(string path)
        {
            if (!File.Exists(path)) return null;
            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();
                if (line.StartsWith("StoragePath=", StringComparison.OrdinalIgnoreCase))
                    return line.Substring("StoragePath=".Length).Trim();
            }
            return null;
        }
    }
}
