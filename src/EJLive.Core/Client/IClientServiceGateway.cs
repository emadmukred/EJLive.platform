using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EJLive.Core.Client
{
    // Interface: IClientServiceGateway (from 1 sources)
        public partial interface IClientServiceGateway
        {
            // --- Methods ---
                    Task<ClientRuntimeSnapshot> GetRuntimeSnapshotAsync();
    
                    Task<bool> TestConnectionAsync();
    
                    Task<bool> PingServerAsync();
    
                    Task<List<string>> ValidatePathsAsync();
    
                    ClientConnectionConfig GetConnectionConfig();
    
                    void SaveConnectionConfig(ClientConnectionConfig config);
    
                    Task<ClientSyncSnapshot> GetSyncSnapshotAsync();
    
                    Task ForceSyncAsync();
    
                    Task ClearFailedAsync();
    
                    Task<ClientCommandSnapshot> GetCommandSnapshotAsync();
    
                    Task RequestScreenshotAsync();
    
                    Task RequestRestartAsync();
    
                    Task RequestTimeSyncAsync();
    
                    Task RequestRemoteAssistanceAsync();
    
                    Task<List<ClientServiceStatus>> GetServiceStatusesAsync();
    
                    Task StartServiceAsync(string serviceName);
    
                    Task StopServiceAsync(string serviceName);
    
                    Task<List<ClientDiagResult>> RunDiagnosticsAsync();
    
                    Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync();
    
                    Task<List<ClientVendorPathEntry>> GetVendorPathsAsync();
    
                    Task<List<ClientLogEntry>> GetLogsAsync(string? component, string? severity, int maxEntries);
    
                    Task<Dictionary<string, string>> GetAgentConfigAsync();
    
                    Task SaveAgentConfigAsync(Dictionary<string, string> config);
    
                    string? GetJournalContent();
    
                    Task InstallServiceAsync();
    
                    Task RemoveServiceAsync();
    
                    Task ActivateServiceAsync();
    
                    Task<bool> CheckPrerequisitesAsync();
    
                    Task RollbackAsync();
    
                    string? ReadHealthFile();
    
    
        }
    public partial interface IClientServiceGateway
        {
            Task<ClientRuntimeSnapshot> GetRuntimeSnapshotAsync();
    
    
            Task<bool> TestConnectionAsync();
    
    
            Task<bool> PingServerAsync();
    
    
            Task<List<string>> ValidatePathsAsync();
    
    
            ClientConnectionConfig GetConnectionConfig();
    
    
            void SaveConnectionConfig(ClientConnectionConfig config);
    
    
            Task<ClientSyncSnapshot> GetSyncSnapshotAsync();
    
    
            Task ForceSyncAsync();
    
    
            Task ClearFailedAsync();
    
    
            Task<ClientCommandSnapshot> GetCommandSnapshotAsync();
    
    
            Task RequestScreenshotAsync();
    
    
            Task RequestRestartAsync();
    
    
            Task RequestTimeSyncAsync();
    
    
            Task RequestRemoteAssistanceAsync();
    
    
            Task<List<ClientServiceStatus>> GetServiceStatusesAsync();
    
    
            Task StartServiceAsync(string serviceName);
    
    
            Task StopServiceAsync(string serviceName);
    
    
            Task<List<ClientDiagResult>> RunDiagnosticsAsync();
    
    
            Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync();
    
    
            Task<List<ClientVendorPathEntry>> GetVendorPathsAsync();
    
    
            Task<List<ClientLogEntry>> GetLogsAsync(string? component, string? severity, int maxEntries);
    
    
            Task<Dictionary<string, string>> GetAgentConfigAsync();
    
    
            Task SaveAgentConfigAsync(Dictionary<string, string> config);
    
    
            string? GetJournalContent();
    
    
            Task InstallServiceAsync();
    
    
            Task RemoveServiceAsync();
    
    
            Task ActivateServiceAsync();
    
    
            Task<bool> CheckPrerequisitesAsync();
    
    
            Task RollbackAsync();
    
    
            string? ReadHealthFile();
    
    
        }
    public interface IClientServiceGateway
        {
            Task<ClientRuntimeSnapshot> GetRuntimeSnapshotAsync();
            Task<bool> TestConnectionAsync();
            Task<bool> PingServerAsync();
            Task<List<string>> ValidatePathsAsync();
            ClientConnectionConfig GetConnectionConfig();
            void SaveConnectionConfig(ClientConnectionConfig config);
            Task<ClientSyncSnapshot> GetSyncSnapshotAsync();
            Task ForceSyncAsync();
            Task ClearFailedAsync();
            Task<ClientCommandSnapshot> GetCommandSnapshotAsync();
            Task RequestScreenshotAsync();
            Task RequestRestartAsync();
            Task RequestTimeSyncAsync();
            Task RequestRemoteAssistanceAsync();
            Task<List<ClientServiceStatus>> GetServiceStatusesAsync();
            Task StartServiceAsync(string serviceName);
            Task StopServiceAsync(string serviceName);
            Task<List<ClientDiagResult>> RunDiagnosticsAsync();
            Task<ClientImageSyncSnapshot> GetImageSyncSnapshotAsync();
            Task<List<ClientVendorPathEntry>> GetVendorPathsAsync();
            Task<List<ClientLogEntry>> GetLogsAsync(string? component, string? severity, int maxEntries);
            Task<Dictionary<string, string>> GetAgentConfigAsync();
            Task SaveAgentConfigAsync(Dictionary<string, string> config);
            string? GetJournalContent();
            Task InstallServiceAsync();
            Task RemoveServiceAsync();
            Task ActivateServiceAsync();
            Task<bool> CheckPrerequisitesAsync();
            Task RollbackAsync();
            string? ReadHealthFile();
        }
}
