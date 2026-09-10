using System;
using System.Collections.Generic;
using System.Linq;

namespace EJLive.Core.Client
{
    public partial class ClientBackgroundService
        {
            public string Name { get; set; } = string.Empty;
    
    
            public string Status { get; set; } = "Stopped";
    
    
            public ServiceCategory Category { get; set; }
    
    
            public bool IsAllowedByPolicy { get; set; } = true;
    
    
            public string? Description { get; set; }
    
    
            public string? LastError { get; set; }
    
    
            public string? RecommendedAction { get; set; }
    
    
            public DateTime? LastTransitionUtc { get; set; }
    
    
            public string StatusDisplay => Status == "Running" ? "● Running" : Status == "Disabled by Policy" ? "✕ Disabled" : "○ Stopped";
    
    
        }
    public sealed class ClientBackgroundService
        {
            public string Name { get; set; } = string.Empty;
            public string Status { get; set; } = "Stopped";
            public ServiceCategory Category { get; set; }
            public bool IsAllowedByPolicy { get; set; } = true;
            public string? Description { get; set; }
            public string? LastError { get; set; }
            public string? RecommendedAction { get; set; }
            public DateTime? LastTransitionUtc { get; set; }
            public string StatusDisplay => Status == "Running" ? "● Running" : Status == "Disabled by Policy" ? "✕ Disabled" : "○ Stopped";
        }

    // Class: ClientBackgroundService (from 3 sources)
        public sealed partial class ClientBackgroundService
        {
            // --- Properties ---
                    public string Name { get; set; } = string.Empty;
    
                    public string Status { get; set; } = "Stopped";
    
                    public ServiceCategory Category { get; set; }
    
                    public bool IsAllowedByPolicy { get; set; } = true;
    
                    public string? Description { get; set; }
    
                    public string? LastError { get; set; }
    
                    public string? RecommendedAction { get; set; }
    
                    public DateTime? LastTransitionUtc { get; set; }
    
                    public string StatusDisplay => Status == "Running" ? "● Running" : Status == "Disabled by Policy" ? "✕ Disabled" : "○ Stopped";
    
    
        }
    // Class: ClientServiceRegistry (from 3 sources)
        public sealed partial class ClientServiceRegistry
        {
            // --- Constants & Fields ---
                    private readonly List<ClientBackgroundService> _services = new();
    
    
            // --- Constructors ---
                    public ClientServiceRegistry()
                    {
                        // Core services (auto-started by default)
                        _services.Add(new ClientBackgroundService { Name = "Agent Controller", Category = ServiceCategory.Core, Status = "Stopped", Description = "Central coordinator for all client components" });
                        _services.Add(new ClientBackgroundService { Name = "Handshake Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Secure session establishment with server" });
                        _services.Add(new ClientBackgroundService { Name = "Heartbeat Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Periodic pulse signals to server" });
                        _services.Add(new ClientBackgroundService { Name = "Reconnect Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Automatic reconnection with exponential backoff" });
                        _services.Add(new ClientBackgroundService { Name = "File Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Monitors EJ/LOG/TRACE directories for changes" });
                        _services.Add(new ClientBackgroundService { Name = "Journal Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Watches journal files and tracks offsets" });
                        _services.Add(new ClientBackgroundService { Name = "Image Inbox Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Monitors image/content inbox for new packages" });
                        _services.Add(new ClientBackgroundService { Name = "Outbox Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Manages pending file send queue with retry" });
                        _services.Add(new ClientBackgroundService { Name = "Sync Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Chunked file transfer to server with SHA256" });
                        _services.Add(new ClientBackgroundService { Name = "Health Reporter", Category = ServiceCategory.Core, Status = "Stopped", Description = "Writes health.json snapshot for UI consumption" });
                        _services.Add(new ClientBackgroundService { Name = "Local Audit Logger", Category = ServiceCategory.Core, Status = "Stopped", Description = "Records all local operations to audit trail" });
    
                        // Policy-governed services (require approval/allowlist)
                        _services.Add(new ClientBackgroundService { Name = "Socket Data", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Real-time data socket communication" });
                        _services.Add(new ClientBackgroundService { Name = "Socket Files", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "File transfer socket channel" });
                        _services.Add(new ClientBackgroundService { Name = "Screenshot Capture", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Screen capture on request" });
                        _services.Add(new ClientBackgroundService { Name = "Remote Assistance (RDP)", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Remote desktop/assistance readiness" });
                        _services.Add(new ClientBackgroundService { Name = "Password Change", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Remote password change via governed command" });
                        _services.Add(new ClientBackgroundService { Name = "Firewall / Registry", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Policy-controlled system configuration" });
                        _services.Add(new ClientBackgroundService { Name = "Image / File Promotion", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Promotes images/files to ATM destination" });
                        _services.Add(new ClientBackgroundService { Name = "Command Receiver", Category = ServiceCategory.Core, Status = "Stopped", Description = "Receives and queues commands from server" });
                    }
    
    
            // --- Methods ---
                    public IReadOnlyList<ClientBackgroundService> GetAll() => _services.AsReadOnly();
    
                    public IEnumerable<ClientBackgroundService> GetCoreServices() => _services.Where(s => s.Category == ServiceCategory.Core);
    
                    public IEnumerable<ClientBackgroundService> GetPolicyServices() => _services.Where(s => s.Category == ServiceCategory.PolicyGoverned);
    
                    public ClientBackgroundService? Find(string name) =>
                        _services.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    
                    public void SetStatus(string name, string status, string? lastError = null)
                    {
                        var svc = Find(name);
                        if (svc != null)
                        {
                            svc.Status = status;
                            svc.LastError = lastError;
                            svc.LastTransitionUtc = DateTime.UtcNow;
                        }
                    }
    
    
        }
    public partial class ClientServiceRegistry
        {
            private readonly List<ClientBackgroundService> _services = new();
    
    
            public ClientServiceRegistry()
            {
                // Core services (auto-started by default)
                _services.Add(new ClientBackgroundService { Name = "Agent Controller", Category = ServiceCategory.Core, Status = "Stopped", Description = "Central coordinator for all client components" });
                _services.Add(new ClientBackgroundService { Name = "Handshake Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Secure session establishment with server" });
                _services.Add(new ClientBackgroundService { Name = "Heartbeat Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Periodic pulse signals to server" });
                _services.Add(new ClientBackgroundService { Name = "Reconnect Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Automatic reconnection with exponential backoff" });
                _services.Add(new ClientBackgroundService { Name = "File Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Monitors EJ/LOG/TRACE directories for changes" });
                _services.Add(new ClientBackgroundService { Name = "Journal Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Watches journal files and tracks offsets" });
                _services.Add(new ClientBackgroundService { Name = "Image Inbox Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Monitors image/content inbox for new packages" });
                _services.Add(new ClientBackgroundService { Name = "Outbox Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Manages pending file send queue with retry" });
                _services.Add(new ClientBackgroundService { Name = "Sync Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Chunked file transfer to server with SHA256" });
                _services.Add(new ClientBackgroundService { Name = "Health Reporter", Category = ServiceCategory.Core, Status = "Stopped", Description = "Writes health.json snapshot for UI consumption" });
                _services.Add(new ClientBackgroundService { Name = "Local Audit Logger", Category = ServiceCategory.Core, Status = "Stopped", Description = "Records all local operations to audit trail" });
    
                // Policy-governed services (require approval/allowlist)
                _services.Add(new ClientBackgroundService { Name = "Socket Data", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Real-time data socket communication" });
                _services.Add(new ClientBackgroundService { Name = "Socket Files", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "File transfer socket channel" });
                _services.Add(new ClientBackgroundService { Name = "Screenshot Capture", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Screen capture on request" });
                _services.Add(new ClientBackgroundService { Name = "Remote Assistance (RDP)", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Remote desktop/assistance readiness" });
                _services.Add(new ClientBackgroundService { Name = "Password Change", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Remote password change via governed command" });
                _services.Add(new ClientBackgroundService { Name = "Firewall / Registry", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Policy-controlled system configuration" });
                _services.Add(new ClientBackgroundService { Name = "Image / File Promotion", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Promotes images/files to ATM destination" });
                _services.Add(new ClientBackgroundService { Name = "Command Receiver", Category = ServiceCategory.Core, Status = "Stopped", Description = "Receives and queues commands from server" });
            }
    
    
            public IReadOnlyList<ClientBackgroundService> GetAll() => _services.AsReadOnly();
    
    
            public IEnumerable<ClientBackgroundService> GetCoreServices() => _services.Where(s => s.Category == ServiceCategory.Core);
    
    
            public IEnumerable<ClientBackgroundService> GetPolicyServices() => _services.Where(s => s.Category == ServiceCategory.PolicyGoverned);
    
    
            public ClientBackgroundService? Find(string name) =>
                _services.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    
    
            public void SetStatus(string name, string status, string? lastError = null)
            {
                var svc = Find(name);
                if (svc != null)
                {
                    svc.Status = status;
                    svc.LastError = lastError;
                    svc.LastTransitionUtc = DateTime.UtcNow;
                }
            }
    
    
        }
    /// <summary>
        /// Registry of all 12 client background services with live status.
        /// Used by ClientAutoBootstrap on service start and ClientMainForm Services tab.
        /// </summary>
        public sealed class ClientServiceRegistry
        {
            private readonly List<ClientBackgroundService> _services = new();
    
            public ClientServiceRegistry()
            {
                // Core services (auto-started by default)
                _services.Add(new ClientBackgroundService { Name = "Agent Controller", Category = ServiceCategory.Core, Status = "Stopped", Description = "Central coordinator for all client components" });
                _services.Add(new ClientBackgroundService { Name = "Handshake Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Secure session establishment with server" });
                _services.Add(new ClientBackgroundService { Name = "Heartbeat Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Periodic pulse signals to server" });
                _services.Add(new ClientBackgroundService { Name = "Reconnect Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Automatic reconnection with exponential backoff" });
                _services.Add(new ClientBackgroundService { Name = "File Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Monitors EJ/LOG/TRACE directories for changes" });
                _services.Add(new ClientBackgroundService { Name = "Journal Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Watches journal files and tracks offsets" });
                _services.Add(new ClientBackgroundService { Name = "Image Inbox Watcher", Category = ServiceCategory.Core, Status = "Stopped", Description = "Monitors image/content inbox for new packages" });
                _services.Add(new ClientBackgroundService { Name = "Outbox Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Manages pending file send queue with retry" });
                _services.Add(new ClientBackgroundService { Name = "Sync Service", Category = ServiceCategory.Core, Status = "Stopped", Description = "Chunked file transfer to server with SHA256" });
                _services.Add(new ClientBackgroundService { Name = "Health Reporter", Category = ServiceCategory.Core, Status = "Stopped", Description = "Writes health.json snapshot for UI consumption" });
                _services.Add(new ClientBackgroundService { Name = "Local Audit Logger", Category = ServiceCategory.Core, Status = "Stopped", Description = "Records all local operations to audit trail" });
    
                // Policy-governed services (require approval/allowlist)
                _services.Add(new ClientBackgroundService { Name = "Socket Data", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Real-time data socket communication" });
                _services.Add(new ClientBackgroundService { Name = "Socket Files", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "File transfer socket channel" });
                _services.Add(new ClientBackgroundService { Name = "Screenshot Capture", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Screen capture on request" });
                _services.Add(new ClientBackgroundService { Name = "Remote Assistance (RDP)", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Remote desktop/assistance readiness" });
                _services.Add(new ClientBackgroundService { Name = "Password Change", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Remote password change via governed command" });
                _services.Add(new ClientBackgroundService { Name = "Firewall / Registry", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Policy-controlled system configuration" });
                _services.Add(new ClientBackgroundService { Name = "Image / File Promotion", Category = ServiceCategory.PolicyGoverned, Status = "Stopped", IsAllowedByPolicy = false, Description = "Promotes images/files to ATM destination" });
                _services.Add(new ClientBackgroundService { Name = "Command Receiver", Category = ServiceCategory.Core, Status = "Stopped", Description = "Receives and queues commands from server" });
            }
    
            public IReadOnlyList<ClientBackgroundService> GetAll() => _services.AsReadOnly();
            public IEnumerable<ClientBackgroundService> GetCoreServices() => _services.Where(s => s.Category == ServiceCategory.Core);
            public IEnumerable<ClientBackgroundService> GetPolicyServices() => _services.Where(s => s.Category == ServiceCategory.PolicyGoverned);
    
            public ClientBackgroundService? Find(string name) =>
                _services.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    
            public void SetStatus(string name, string status, string? lastError = null)
            {
                var svc = Find(name);
                if (svc != null)
                {
                    svc.Status = status;
                    svc.LastError = lastError;
                    svc.LastTransitionUtc = DateTime.UtcNow;
                }
            }
        }
    // Enum: ServiceCategory (from 3 sources)
        public partial enum ServiceCategory
        {
        }

    public partial enum ServiceCategory
        {
        }
    public enum ServiceCategory { Core, PolicyGoverned }
}
