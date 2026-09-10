using System;

namespace EJLive.Client.Service
{
    /// <summary>
    /// Lightweight snapshot of agent state for the Client Companion UI.
    /// </summary>
    public class AgentRuntimeSnapshot
    {
        public string State { get; set; } = "Stopped";
        public bool NetworkConnected { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public DateTime LastHeartbeat { get; set; }
        public DateTime LastSync { get; set; }
        public int OutboxCount { get; set; }
        public bool WatcherRunning { get; set; }
        public bool OutboxRunning { get; set; }
        public bool HealthReporterRunning { get; set; }
        public int ErrorCount { get; set; }
        public string LastError { get; set; } = string.Empty;
    }
}