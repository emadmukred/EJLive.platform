using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EJLive.Core.Client
{
    /// <summary>
    /// Service gateway contract for the Client Companion UI.
    /// All UI calls go through this facade — never directly to engines, sockets, or file watchers.
    /// At runtime, the EJLive.Client.Service implements this (via IPC or in-process).
    /// </summary>
    public interface IClientServiceGateway
    {
        Task<ClientRuntimeSnapshot> GetSnapshotAsync();
        Task<bool> TestConnectionAsync();
        Task<bool> ValidatePathsAsync();
        Task<bool> RequestForceSyncAsync();
        Task<bool> RequestRestartServiceAsync();
        Task<bool> RequestScreenshotAsync();
        Task<bool> PingServerAsync();
        Task<List<string>> RunDiagnosticsAsync();
    }

    /// <summary>
    /// Full client runtime state snapshot for the Companion UI.
    /// No WinForms dependency — plain data object.
    /// </summary>
    public sealed class ClientRuntimeSnapshot
    {
        public string State { get; set; } = "Stopped";
        public string SessionId { get; set; } = string.Empty;
        public bool NetworkConnected { get; set; }
        public DateTimeOffset? LastHandshake { get; set; }
        public DateTimeOffset? LastHeartbeat { get; set; }
        public DateTimeOffset? LastSync { get; set; }
        public int OutboxCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCommandCount { get; set; }
        public string WatcherState { get; set; } = "Unknown";
        public string ImageInboxState { get; set; } = "Unknown";
        public string HealthScore { get; set; } = "Unknown";
        public string LastError { get; set; } = string.Empty;
        public List<ClientComponentSnapshot> Components { get; set; } = new List<ClientComponentSnapshot>();
    }

    /// <summary>
    /// Status of a single client service component.
    /// </summary>
    public sealed class ClientComponentSnapshot
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Unknown";
        public string LastTransition { get; set; } = string.Empty;
        public string LastError { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// Facade that the Client Companion UI binds to.
    /// Routes all requests through IClientServiceGateway — never directly touches
    /// engines, sockets, file watchers, or parsers.
    /// </summary>
    public sealed class ClientCompanionFacade
    {
        private readonly IClientServiceGateway _gateway;

        public ClientCompanionFacade(IClientServiceGateway gateway)
        {
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        public Task<ClientRuntimeSnapshot> GetSnapshotAsync() => _gateway.GetSnapshotAsync();
        public Task<bool> TestConnectionAsync() => _gateway.TestConnectionAsync();
        public Task<bool> ValidatePathsAsync() => _gateway.ValidatePathsAsync();
        public Task<bool> RequestForceSyncAsync() => _gateway.RequestForceSyncAsync();
        public Task<bool> RequestRestartServiceAsync() => _gateway.RequestRestartServiceAsync();
        public Task<bool> RequestScreenshotAsync() => _gateway.RequestScreenshotAsync();
        public Task<bool> PingServerAsync() => _gateway.PingServerAsync();
        public Task<List<string>> RunDiagnosticsAsync() => _gateway.RunDiagnosticsAsync();
    }

    /// <summary>
    /// In-process fallback gateway that returns stub snapshots.
    /// Used when the real Client.Service is not running (e.g., during development).
    /// Swap with IPC-based gateway for production.
    /// </summary>
    public sealed class InProcessClientServiceGateway : IClientServiceGateway
    {
        public Task<ClientRuntimeSnapshot> GetSnapshotAsync() => Task.FromResult(new ClientRuntimeSnapshot
        {
            State = "Running",
            Components = new List<ClientComponentSnapshot>
            {
                new ClientComponentSnapshot { Name = "AgentController", Status = "Running (in-process)" },
                new ClientComponentSnapshot { Name = "NetworkSession", Status = "Disconnected (stub)" },
                new ClientComponentSnapshot { Name = "FileWatcher", Status = "Idle (stub)" },
                new ClientComponentSnapshot { Name = "Outbox", Status = "Empty (stub)" },
                new ClientComponentSnapshot { Name = "HealthReporter", Status = "Active (stub)" }
            }
        });

        public Task<bool> TestConnectionAsync() => Task.FromResult(false);
        public Task<bool> ValidatePathsAsync() => Task.FromResult(false);
        public Task<bool> RequestForceSyncAsync() => Task.FromResult(false);
        public Task<bool> RequestRestartServiceAsync() => Task.FromResult(false);
        public Task<bool> RequestScreenshotAsync() => Task.FromResult(false);
        public Task<bool> PingServerAsync() => Task.FromResult(false);
        public Task<List<string>> RunDiagnosticsAsync() => Task.FromResult(new List<string> { "Service: In-process stub", "Paths: Not validated" });
    }
}