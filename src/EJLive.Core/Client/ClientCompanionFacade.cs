using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EJLive.Core.Client
{
    public partial class ClientCompanionFacade
        {
            private readonly IClientServiceGateway _gateway;
    
    
            private ClientRuntimeSnapshot? _cachedSnapshot;
    
    
            public ClientRuntimeSnapshot? CachedSnapshot => _cachedSnapshot;
    
    
            public ClientCompanionFacade(IClientServiceGateway gateway)
            {
                _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            }
    
    
            public async Task<ClientRuntimeSnapshot> GetSnapshotAsync()
            {
                _cachedSnapshot = await _gateway.GetRuntimeSnapshotAsync();
                return _cachedSnapshot;
            }
    
    
            public async Task<bool> TestConnectionAsync() => await _gateway.TestConnectionAsync();
    
    
            public async Task<bool> PingServerAsync() => await _gateway.PingServerAsync();
    
    
            public async Task<List<string>> ValidatePathsAsync() => await _gateway.ValidatePathsAsync();
    
    
            public ClientConnectionConfig GetConnectionConfig() => _gateway.GetConnectionConfig();
    
    
            public void SaveConnectionConfig(ClientConnectionConfig config) => _gateway.SaveConnectionConfig(config);
    
    
            public Task<ClientSyncSnapshot> GetSyncSnapshotAsync() => _gateway.GetSyncSnapshotAsync();
    
    
            public Task ForceSyncAsync() => _gateway.ForceSyncAsync();
    
    
            public Task ClearFailedAsync() => _gateway.ClearFailedAsync();
    
    
            public Task<ClientCommandSnapshot> GetCommandSnapshotAsync() => _gateway.GetCommandSnapshotAsync();
    
    
            public Task RequestScreenshotAsync() => _gateway.RequestScreenshotAsync();
    
    
            public Task RequestRestartAsync() => _gateway.RequestRestartAsync();
    
    
            public Task RequestTimeSyncAsync() => _gateway.RequestTimeSyncAsync();
    
    
            public Task RequestRemoteAssistanceAsync() => _gateway.RequestRemoteAssistanceAsync();
    
    
            public Task<List<ClientServiceStatus>> GetServiceStatusesAsync() => _gateway.GetServiceStatusesAsync();
    
    
            public Task StartServiceAsync(string name) => _gateway.StartServiceAsync(name);
    
    
            public Task StopServiceAsync(string name) => _gateway.StopServiceAsync(name);
    
    
            public Task<List<ClientDiagResult>> RunDiagnosticsAsync() => _gateway.RunDiagnosticsAsync();
    
    
            public Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync() => _gateway.GetImageSyncSnapshotAsync();
    
    
            public Task<List<ClientVendorPathEntry>> GetVendorPathsAsync() => _gateway.GetVendorPathsAsync();
    
    
            public Task<List<ClientLogEntry>> GetLogsAsync(string? component = null, string? severity = null, int max = 500)
                => _gateway.GetLogsAsync(component, severity, max);
    
    
            public Task<Dictionary<string, string>> GetAgentConfigAsync() => _gateway.GetAgentConfigAsync();
    
    
            public Task SaveAgentConfigAsync(Dictionary<string, string> config) => _gateway.SaveAgentConfigAsync(config);
    
    
            public string? GetJournalContent() => _gateway.GetJournalContent();
    
    
            public Task InstallServiceAsync() => _gateway.InstallServiceAsync();
    
    
            public Task RemoveServiceAsync() => _gateway.RemoveServiceAsync();
    
    
            public Task ActivateServiceAsync() => _gateway.ActivateServiceAsync();
    
    
            public Task<bool> CheckPrerequisitesAsync() => _gateway.CheckPrerequisitesAsync();
    
    
            public Task RollbackAsync() => _gateway.RollbackAsync();
    
    
            public string? ReadHealthFile() => _gateway.ReadHealthFile();
    
    
        }

    /// <summary>
        /// Facade between Client Companion UI and the client service runtime.
        /// Routes all UI requests through a safe gateway with snapshot caching.
        /// </summary>
        public class ClientCompanionFacade
        {
            private readonly IClientServiceGateway _gateway;
            private ClientRuntimeSnapshot? _cachedSnapshot;
    
            public ClientCompanionFacade(IClientServiceGateway gateway)
            {
                _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            }
    
            public async Task<ClientRuntimeSnapshot> GetSnapshotAsync()
            {
                _cachedSnapshot = await _gateway.GetRuntimeSnapshotAsync();
                return _cachedSnapshot;
            }
    
            public ClientRuntimeSnapshot? CachedSnapshot => _cachedSnapshot;
    
            public async Task<bool> TestConnectionAsync() => await _gateway.TestConnectionAsync();
            public async Task<bool> PingServerAsync() => await _gateway.PingServerAsync();
            public async Task<List<string>> ValidatePathsAsync() => await _gateway.ValidatePathsAsync();
            public ClientConnectionConfig GetConnectionConfig() => _gateway.GetConnectionConfig();
            public void SaveConnectionConfig(ClientConnectionConfig config) => _gateway.SaveConnectionConfig(config);
            public Task<ClientSyncSnapshot> GetSyncSnapshotAsync() => _gateway.GetSyncSnapshotAsync();
            public Task ForceSyncAsync() => _gateway.ForceSyncAsync();
            public Task ClearFailedAsync() => _gateway.ClearFailedAsync();
            public Task<ClientCommandSnapshot> GetCommandSnapshotAsync() => _gateway.GetCommandSnapshotAsync();
            public Task RequestScreenshotAsync() => _gateway.RequestScreenshotAsync();
            public Task RequestRestartAsync() => _gateway.RequestRestartAsync();
            public Task RequestTimeSyncAsync() => _gateway.RequestTimeSyncAsync();
            public Task RequestRemoteAssistanceAsync() => _gateway.RequestRemoteAssistanceAsync();
            public Task<List<ClientServiceStatus>> GetServiceStatusesAsync() => _gateway.GetServiceStatusesAsync();
            public Task StartServiceAsync(string name) => _gateway.StartServiceAsync(name);
            public Task StopServiceAsync(string name) => _gateway.StopServiceAsync(name);
            public Task<List<ClientDiagResult>> RunDiagnosticsAsync() => _gateway.RunDiagnosticsAsync();
            public Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync() => _gateway.GetImageSyncSnapshotAsync();
            public Task<List<ClientVendorPathEntry>> GetVendorPathsAsync() => _gateway.GetVendorPathsAsync();
            public Task<List<ClientLogEntry>> GetLogsAsync(string? component = null, string? severity = null, int max = 500)
                => _gateway.GetLogsAsync(component, severity, max);
            public Task<Dictionary<string, string>> GetAgentConfigAsync() => _gateway.GetAgentConfigAsync();
            public Task SaveAgentConfigAsync(Dictionary<string, string> config) => _gateway.SaveAgentConfigAsync(config);
            public string? GetJournalContent() => _gateway.GetJournalContent();
            public Task InstallServiceAsync() => _gateway.InstallServiceAsync();
            public Task RemoveServiceAsync() => _gateway.RemoveServiceAsync();
            public Task ActivateServiceAsync() => _gateway.ActivateServiceAsync();
            public Task<bool> CheckPrerequisitesAsync() => _gateway.CheckPrerequisitesAsync();
            public Task RollbackAsync() => _gateway.RollbackAsync();
            public string? ReadHealthFile() => _gateway.ReadHealthFile();
        }
    // Class: ClientCompanionFacade (from 1 sources)
        public partial class ClientCompanionFacade
        {
            // --- Constants & Fields ---
                    private readonly IClientServiceGateway _gateway;
    
                    private ClientRuntimeSnapshot? _cachedSnapshot;
    
    
            // --- Properties ---
                    public ClientRuntimeSnapshot? CachedSnapshot => _cachedSnapshot;
    
    
            // --- Constructors ---
                    public ClientCompanionFacade(IClientServiceGateway gateway)
                    {
                        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
                    }
    
    
            // --- Methods ---
                    public async Task<ClientRuntimeSnapshot> GetSnapshotAsync()
                    {
                        _cachedSnapshot = await _gateway.GetRuntimeSnapshotAsync();
                        return _cachedSnapshot;
                    }
    
                    public async Task<bool> TestConnectionAsync() => await _gateway.TestConnectionAsync();
    
                    public async Task<bool> PingServerAsync() => await _gateway.PingServerAsync();
    
                    public async Task<List<string>> ValidatePathsAsync() => await _gateway.ValidatePathsAsync();
    
                    public ClientConnectionConfig GetConnectionConfig() => _gateway.GetConnectionConfig();
    
                    public void SaveConnectionConfig(ClientConnectionConfig config) => _gateway.SaveConnectionConfig(config);
    
                    public Task<ClientSyncSnapshot> GetSyncSnapshotAsync() => _gateway.GetSyncSnapshotAsync();
    
                    public Task ForceSyncAsync() => _gateway.ForceSyncAsync();
    
                    public Task ClearFailedAsync() => _gateway.ClearFailedAsync();
    
                    public Task<ClientCommandSnapshot> GetCommandSnapshotAsync() => _gateway.GetCommandSnapshotAsync();
    
                    public Task RequestScreenshotAsync() => _gateway.RequestScreenshotAsync();
    
                    public Task RequestRestartAsync() => _gateway.RequestRestartAsync();
    
                    public Task RequestTimeSyncAsync() => _gateway.RequestTimeSyncAsync();
    
                    public Task RequestRemoteAssistanceAsync() => _gateway.RequestRemoteAssistanceAsync();
    
                    public Task<List<ClientServiceStatus>> GetServiceStatusesAsync() => _gateway.GetServiceStatusesAsync();
    
                    public Task StartServiceAsync(string name) => _gateway.StartServiceAsync(name);
    
                    public Task StopServiceAsync(string name) => _gateway.StopServiceAsync(name);
    
                    public Task<List<ClientDiagResult>> RunDiagnosticsAsync() => _gateway.RunDiagnosticsAsync();
    
                    public Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync() => _gateway.GetImageSyncSnapshotAsync();
    
                    public Task<List<ClientVendorPathEntry>> GetVendorPathsAsync() => _gateway.GetVendorPathsAsync();
    
                    public Task<List<ClientLogEntry>> GetLogsAsync(string? component = null, string? severity = null, int max = 500)
                        => _gateway.GetLogsAsync(component, severity, max);
    
                    public Task<Dictionary<string, string>> GetAgentConfigAsync() => _gateway.GetAgentConfigAsync();
    
                    public Task SaveAgentConfigAsync(Dictionary<string, string> config) => _gateway.SaveAgentConfigAsync(config);
    
                    public string? GetJournalContent() => _gateway.GetJournalContent();
    
                    public Task InstallServiceAsync() => _gateway.InstallServiceAsync();
    
                    public Task RemoveServiceAsync() => _gateway.RemoveServiceAsync();
    
                    public Task ActivateServiceAsync() => _gateway.ActivateServiceAsync();
    
                    public Task<bool> CheckPrerequisitesAsync() => _gateway.CheckPrerequisitesAsync();
    
                    public Task RollbackAsync() => _gateway.RollbackAsync();
    
                    public string? ReadHealthFile() => _gateway.ReadHealthFile();
    
    
        }
}
