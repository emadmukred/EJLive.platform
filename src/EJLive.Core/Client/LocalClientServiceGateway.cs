using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EJLive.Core.Client
{
    /// <summary>
        /// In-process fallback gateway for when IPC is not available.
        /// Wraps agent status and provides stub implementations for all operations.
        /// </summary>
        public class LocalClientServiceGateway : IClientServiceGateway
        {
            private ClientConnectionConfig _config = new();
            private readonly List<ClientLogEntry> _logs = new();
    
            public LocalClientServiceGateway()
            {
                _config.ServerHost = "127.0.0.1";
                _config.ServerPort = 5656;
            }
    
            public Task<ClientRuntimeSnapshot> GetRuntimeSnapshotAsync() => Task.FromResult(new ClientRuntimeSnapshot
            {
                State = "Running",
                NetworkConnected = false,
                AtmId = "ATM-001",
                Vendor = "NCR",
                HealthScore = 85.0
            });
    
            public Task<bool> TestConnectionAsync() => Task.FromResult(false);
            public Task<bool> PingServerAsync() => Task.FromResult(false);
            public Task<List<string>> ValidatePathsAsync() => Task.FromResult(new List<string>());
    
            public ClientConnectionConfig GetConnectionConfig() => _config;
            public void SaveConnectionConfig(ClientConnectionConfig config) => _config = config ?? new ClientConnectionConfig();
    
            public Task<ClientSyncSnapshot> GetSyncSnapshotAsync() => Task.FromResult(new ClientSyncSnapshot());
            public Task ForceSyncAsync() => Task.CompletedTask;
            public Task ClearFailedAsync() => Task.CompletedTask;
    
            public Task<ClientCommandSnapshot> GetCommandSnapshotAsync() => Task.FromResult(new ClientCommandSnapshot());
            public Task RequestScreenshotAsync() => Task.CompletedTask;
            public Task RequestRestartAsync() => Task.CompletedTask;
            public Task RequestTimeSyncAsync() => Task.CompletedTask;
            public Task RequestRemoteAssistanceAsync() => Task.CompletedTask;
    
            public Task<List<ClientServiceStatus>> GetServiceStatusesAsync()
            {
                return Task.FromResult(new List<ClientServiceStatus>
                {
                    new() { Name = "Agent Controller", Status = "Running", Details = "Headless agent operational" },
                    new() { Name = "File Watcher", Status = "Running", Details = "Watching journal paths" },
                    new() { Name = "Heartbeat", Status = "Stopped", Details = "No server connection" }
                });
            }
    
            public Task StartServiceAsync(string name) => Task.CompletedTask;
            public Task StopServiceAsync(string name) => Task.CompletedTask;
    
            public Task<List<ClientDiagResult>> RunDiagnosticsAsync() => Task.FromResult(new List<ClientDiagResult>
            {
                new() { Check = "Service Existence", Passed = true, Detail = "Service registered" },
                new() { Check = "Path Permissions", Passed = true, Detail = "Journal paths accessible" },
                new() { Check = "Server Reachability", Passed = false, Detail = "localhost:5656 unreachable" }
            });
    
            public Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync() => Task.FromResult(new ClientImageSyncSnapshot());
    
            public Task<List<ClientVendorPathEntry>> GetVendorPathsAsync()
            {
                return Task.FromResult(new List<ClientVendorPathEntry>
                {
                    new() { Vendor = "NCR", JournalSource = @"C:\Program Files\NCR APATRA\Advance NDC\Data\", BackupPath = @"C:\NCR_BackupLog\" },
                    new() { Vendor = "GRG", JournalSource = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\", BackupPath = @"D:\GRG_BackupLog\" },
                    new() { Vendor = "WINCOR", JournalSource = @"C:\journal\", BackupPath = @"C:\WN_BackupLog\" },
                    new() { Vendor = "DIEBOLD", JournalSource = @"C:\Diebold\EJ\", BackupPath = @"C:\Diebold_BackupLog\" },
                    new() { Vendor = "HYOSUNG", JournalSource = @"C:\Hyosung\EJ\", BackupPath = @"C:\Hyosung_BackupLog\" },
                    new() { Vendor = "CASHWAY", JournalSource = @"C:\Cashway\EJ\", BackupPath = @"C:\Cashway_BackupLog\" }
                });
            }
    
            public Task<List<ClientLogEntry>> GetLogsAsync(string? component, string? severity, int maxEntries)
            {
                return Task.FromResult(_logs.TakeLast(maxEntries).ToList());
            }
    
            public void AppendLog(string component, string severity, string message)
            {
                _logs.Add(new ClientLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Component = component,
                    Severity = severity,
                    Message = message
                });
            }
    
            public Task<Dictionary<string, string>> GetAgentConfigAsync()
            {
                return Task.FromResult(new Dictionary<string, string>
                {
                    ["AgentId"] = "ATM-001",
                    ["Vendor"] = "NCR",
                    ["ServerHost"] = _config.ServerHost,
                    ["ServerPort"] = _config.ServerPort.ToString(),
                    ["HeartbeatIntervalSec"] = "30",
                    ["ReconnectIntervalSec"] = "15"
                });
            }
    
            public Task SaveAgentConfigAsync(Dictionary<string, string> config)
            {
                foreach (var kvp in config)
                {
                    if (kvp.Key.Equals("ServerHost", StringComparison.OrdinalIgnoreCase))
                        _config.ServerHost = kvp.Value;
                    else if (kvp.Key.Equals("ServerPort", StringComparison.OrdinalIgnoreCase) && int.TryParse(kvp.Value, out var port))
                        _config.ServerPort = port;
                }
                return Task.CompletedTask;
            }
    
            public string? GetJournalContent() => null;
            public Task InstallServiceAsync() => Task.CompletedTask;
            public Task RemoveServiceAsync() => Task.CompletedTask;
            public Task ActivateServiceAsync() => Task.CompletedTask;
            public Task<bool> CheckPrerequisitesAsync() => Task.FromResult(true);
            public Task RollbackAsync() => Task.CompletedTask;
            public string? ReadHealthFile() => null;
        }
    public partial class LocalClientServiceGateway : IClientServiceGateway
        {
            private ClientConnectionConfig _config = new();
    
    
            private readonly List<ClientLogEntry> _logs = new();
    
    
            public LocalClientServiceGateway()
            {
                _config.ServerHost = "127.0.0.1";
                _config.ServerPort = 5656;
            }
    
    
            public Task<ClientRuntimeSnapshot> GetRuntimeSnapshotAsync() => Task.FromResult(new ClientRuntimeSnapshot
            {
                State = "Running",
                NetworkConnected = false,
                AtmId = "ATM-001",
                Vendor = "NCR",
                HealthScore = 85.0
            });
    
    
            public Task<bool> TestConnectionAsync() => Task.FromResult(false);
    
    
            public Task<bool> PingServerAsync() => Task.FromResult(false);
    
    
            public Task<List<string>> ValidatePathsAsync() => Task.FromResult(new List<string>());
    
    
            public ClientConnectionConfig GetConnectionConfig() => _config;
    
    
            public void SaveConnectionConfig(ClientConnectionConfig config) => _config = config ?? new ClientConnectionConfig();
    
    
            public Task<ClientSyncSnapshot> GetSyncSnapshotAsync() => Task.FromResult(new ClientSyncSnapshot());
    
    
            public Task ForceSyncAsync() => Task.CompletedTask;
    
    
            public Task ClearFailedAsync() => Task.CompletedTask;
    
    
            public Task<ClientCommandSnapshot> GetCommandSnapshotAsync() => Task.FromResult(new ClientCommandSnapshot());
    
    
            public Task RequestScreenshotAsync() => Task.CompletedTask;
    
    
            public Task RequestRestartAsync() => Task.CompletedTask;
    
    
            public Task RequestTimeSyncAsync() => Task.CompletedTask;
    
    
            public Task RequestRemoteAssistanceAsync() => Task.CompletedTask;
    
    
            public Task<List<ClientServiceStatus>> GetServiceStatusesAsync()
            {
                return Task.FromResult(new List<ClientServiceStatus>
                {
                    new() { Name = "Agent Controller", Status = "Running", Details = "Headless agent operational" },
                    new() { Name = "File Watcher", Status = "Running", Details = "Watching journal paths" },
                    new() { Name = "Heartbeat", Status = "Stopped", Details = "No server connection" }
                });
            }
    
    
            public Task StartServiceAsync(string name) => Task.CompletedTask;
    
    
            public Task StopServiceAsync(string name) => Task.CompletedTask;
    
    
            public Task<List<ClientDiagResult>> RunDiagnosticsAsync() => Task.FromResult(new List<ClientDiagResult>
            {
                new() { Check = "Service Existence", Passed = true, Detail = "Service registered" },
                new() { Check = "Path Permissions", Passed = true, Detail = "Journal paths accessible" },
                new() { Check = "Server Reachability", Passed = false, Detail = "localhost:5656 unreachable" }
            });
    
    
            public Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync() => Task.FromResult(new ClientImageSyncSnapshot());
    
    
            public Task<List<ClientVendorPathEntry>> GetVendorPathsAsync()
            {
                return Task.FromResult(new List<ClientVendorPathEntry>
                {
                    new() { Vendor = "NCR", JournalSource = @"C:\Program Files\NCR APATRA\Advance NDC\Data\", BackupPath = @"C:\NCR_BackupLog\" },
                    new() { Vendor = "GRG", JournalSource = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\", BackupPath = @"D:\GRG_BackupLog\" },
                    new() { Vendor = "WINCOR", JournalSource = @"C:\journal\", BackupPath = @"C:\WN_BackupLog\" },
                    new() { Vendor = "DIEBOLD", JournalSource = @"C:\Diebold\EJ\", BackupPath = @"C:\Diebold_BackupLog\" },
                    new() { Vendor = "HYOSUNG", JournalSource = @"C:\Hyosung\EJ\", BackupPath = @"C:\Hyosung_BackupLog\" },
                    new() { Vendor = "CASHWAY", JournalSource = @"C:\Cashway\EJ\", BackupPath = @"C:\Cashway_BackupLog\" }
                });
            }
    
    
            public Task<List<ClientLogEntry>> GetLogsAsync(string? component, string? severity, int maxEntries)
            {
                return Task.FromResult(_logs.TakeLast(maxEntries).ToList());
            }
    
    
            public void AppendLog(string component, string severity, string message)
            {
                _logs.Add(new ClientLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Component = component,
                    Severity = severity,
                    Message = message
                });
            }
    
    
            public Task<Dictionary<string, string>> GetAgentConfigAsync()
            {
                return Task.FromResult(new Dictionary<string, string>
                {
                    ["AgentId"] = "ATM-001",
                    ["Vendor"] = "NCR",
                    ["ServerHost"] = _config.ServerHost,
                    ["ServerPort"] = _config.ServerPort.ToString(),
                    ["HeartbeatIntervalSec"] = "30",
                    ["ReconnectIntervalSec"] = "15"
                });
            }
    
    
            public Task SaveAgentConfigAsync(Dictionary<string, string> config)
            {
                foreach (var kvp in config)
                {
                    if (kvp.Key.Equals("ServerHost", StringComparison.OrdinalIgnoreCase))
                        _config.ServerHost = kvp.Value;
                    else if (kvp.Key.Equals("ServerPort", StringComparison.OrdinalIgnoreCase) && int.TryParse(kvp.Value, out var port))
                        _config.ServerPort = port;
                }
                return Task.CompletedTask;
            }
    
    
            public string? GetJournalContent() => null;
    
    
            public Task InstallServiceAsync() => Task.CompletedTask;
    
    
            public Task RemoveServiceAsync() => Task.CompletedTask;
    
    
            public Task ActivateServiceAsync() => Task.CompletedTask;
    
    
            public Task<bool> CheckPrerequisitesAsync() => Task.FromResult(true);
    
    
            public Task RollbackAsync() => Task.CompletedTask;
    
    
            public string? ReadHealthFile() => null;
    
    
        }
    // Class: LocalClientServiceGateway (from 1 sources)
        public partial class LocalClientServiceGateway : IClientServiceGateway
        {
            // --- Constants & Fields ---
                    private ClientConnectionConfig _config = new();
    
                    private readonly List<ClientLogEntry> _logs = new();
    
    
            // --- Constructors ---
                    public LocalClientServiceGateway()
                    {
                        _config.ServerHost = "127.0.0.1";
                        _config.ServerPort = 5656;
                    }
    
    
            // --- Methods ---
                    public Task<ClientRuntimeSnapshot> GetRuntimeSnapshotAsync() => Task.FromResult(new ClientRuntimeSnapshot
                    {
                        State = "Running",
                        NetworkConnected = false,
                        AtmId = "ATM-001",
                        Vendor = "NCR",
                        HealthScore = 85.0
                    });
    
                    public Task<bool> TestConnectionAsync() => Task.FromResult(false);
    
                    public Task<bool> PingServerAsync() => Task.FromResult(false);
    
                    public Task<List<string>> ValidatePathsAsync() => Task.FromResult(new List<string>());
    
                    public ClientConnectionConfig GetConnectionConfig() => _config;
    
                    public void SaveConnectionConfig(ClientConnectionConfig config) => _config = config ?? new ClientConnectionConfig();
    
                    public Task<ClientSyncSnapshot> GetSyncSnapshotAsync() => Task.FromResult(new ClientSyncSnapshot());
    
                    public Task ForceSyncAsync() => Task.CompletedTask;
    
                    public Task ClearFailedAsync() => Task.CompletedTask;
    
                    public Task<ClientCommandSnapshot> GetCommandSnapshotAsync() => Task.FromResult(new ClientCommandSnapshot());
    
                    public Task RequestScreenshotAsync() => Task.CompletedTask;
    
                    public Task RequestRestartAsync() => Task.CompletedTask;
    
                    public Task RequestTimeSyncAsync() => Task.CompletedTask;
    
                    public Task RequestRemoteAssistanceAsync() => Task.CompletedTask;
    
                    public Task<List<ClientServiceStatus>> GetServiceStatusesAsync()
                    {
                        return Task.FromResult(new List<ClientServiceStatus>
                        {
                            new() { Name = "Agent Controller", Status = "Running", Details = "Headless agent operational" },
                            new() { Name = "File Watcher", Status = "Running", Details = "Watching journal paths" },
                            new() { Name = "Heartbeat", Status = "Stopped", Details = "No server connection" }
                        });
                    }
    
                    public Task StartServiceAsync(string name) => Task.CompletedTask;
    
                    public Task StopServiceAsync(string name) => Task.CompletedTask;
    
                    public Task<List<ClientDiagResult>> RunDiagnosticsAsync() => Task.FromResult(new List<ClientDiagResult>
                    {
                        new() { Check = "Service Existence", Passed = true, Detail = "Service registered" },
                        new() { Check = "Path Permissions", Passed = true, Detail = "Journal paths accessible" },
                        new() { Check = "Server Reachability", Passed = false, Detail = "localhost:5656 unreachable" }
                    });
    
                    public Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync() => Task.FromResult(new ClientImageSyncSnapshot());
    
                    public Task<List<ClientVendorPathEntry>> GetVendorPathsAsync()
                    {
                        return Task.FromResult(new List<ClientVendorPathEntry>
                        {
                            new() { Vendor = "NCR", JournalSource = @"C:\Program Files\NCR APATRA\Advance NDC\Data\", BackupPath = @"C:\NCR_BackupLog\" },
                            new() { Vendor = "GRG", JournalSource = @"D:\Program Files\DTATMW\Bin\ATMAPP\Log\", BackupPath = @"D:\GRG_BackupLog\" },
                            new() { Vendor = "WINCOR", JournalSource = @"C:\journal\", BackupPath = @"C:\WN_BackupLog\" },
                            new() { Vendor = "DIEBOLD", JournalSource = @"C:\Diebold\EJ\", BackupPath = @"C:\Diebold_BackupLog\" },
                            new() { Vendor = "HYOSUNG", JournalSource = @"C:\Hyosung\EJ\", BackupPath = @"C:\Hyosung_BackupLog\" },
                            new() { Vendor = "CASHWAY", JournalSource = @"C:\Cashway\EJ\", BackupPath = @"C:\Cashway_BackupLog\" }
                        });
                    }
    
                    public Task<List<ClientLogEntry>> GetLogsAsync(string? component, string? severity, int maxEntries)
                    {
                        return Task.FromResult(_logs.TakeLast(maxEntries).ToList());
                    }
    
                    public void AppendLog(string component, string severity, string message)
                    {
                        _logs.Add(new ClientLogEntry
                        {
                            Timestamp = DateTime.UtcNow,
                            Component = component,
                            Severity = severity,
                            Message = message
                        });
                    }
    
                    public Task<Dictionary<string, string>> GetAgentConfigAsync()
                    {
                        return Task.FromResult(new Dictionary<string, string>
                        {
                            ["AgentId"] = "ATM-001",
                            ["Vendor"] = "NCR",
                            ["ServerHost"] = _config.ServerHost,
                            ["ServerPort"] = _config.ServerPort.ToString(),
                            ["HeartbeatIntervalSec"] = "30",
                            ["ReconnectIntervalSec"] = "15"
                        });
                    }
    
                    public Task SaveAgentConfigAsync(Dictionary<string, string> config)
                    {
                        foreach (var kvp in config)
                        {
                            if (kvp.Key.Equals("ServerHost", StringComparison.OrdinalIgnoreCase))
                                _config.ServerHost = kvp.Value;
                            else if (kvp.Key.Equals("ServerPort", StringComparison.OrdinalIgnoreCase) && int.TryParse(kvp.Value, out var port))
                                _config.ServerPort = port;
                        }
                        return Task.CompletedTask;
                    }
    
                    public string? GetJournalContent() => null;
    
                    public Task InstallServiceAsync() => Task.CompletedTask;
    
                    public Task RemoveServiceAsync() => Task.CompletedTask;
    
                    public Task ActivateServiceAsync() => Task.CompletedTask;
    
                    public Task<bool> CheckPrerequisitesAsync() => Task.FromResult(true);
    
                    public Task RollbackAsync() => Task.CompletedTask;
    
                    public string? ReadHealthFile() => null;
    
    
        }
}
