using System;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Represents the current state of the agent.
    /// </summary>
    public enum AgentState
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Failed,
        Degraded
    }

    /// <summary>
    /// Represents the heartbeat status of the network connection.
    /// </summary>
    public enum HeartbeatStatus
    {
        Online,
        Warning,
        Offline,
        CriticalOffline
    }
}