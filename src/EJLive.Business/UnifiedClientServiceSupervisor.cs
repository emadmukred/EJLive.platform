using System;
using System.Collections.Generic;
using EJLive.Shared;

namespace EJLive.Business
{
    /// <summary>
    /// Supervises the client-side agent service lifecycle:
    /// health monitoring, restart orchestration, and configuration distribution.
    /// </summary>
    public sealed class UnifiedClientServiceSupervisor
    {
        private readonly Dictionary<string, ClientServiceState> _clients = new(StringComparer.OrdinalIgnoreCase);

        public ClientServiceState Register(string agentId, string atmId, string serverHost, int serverPort)
        {
            var state = new ClientServiceState
            {
                AgentId = agentId,
                AtmId = atmId,
                ServerHost = serverHost,
                ServerPort = serverPort,
                Status = "Registered",
                RegisteredAtUtc = DateTime.UtcNow,
                LastHeartbeatUtc = DateTime.UtcNow
            };
            _clients[agentId] = state;
            AppLogger.Instance.Info($"Client agent {agentId} registered for ATM {atmId}", "Supervisor");
            return state;
        }

        public bool UpdateHeartbeat(string agentId)
        {
            if (!_clients.TryGetValue(agentId, out var state))
                return false;

            state.LastHeartbeatUtc = DateTime.UtcNow;
            state.Status = "Healthy";
            state.ConsecutiveMissedHeartbeats = 0;
            return true;
        }

        public int CheckHealth(int heartbeatTimeoutSec = 90)
        {
            var now = DateTime.UtcNow;
            var unhealthy = 0;

            foreach (var kvp in _clients)
            {
                var elapsed = (now - kvp.Value.LastHeartbeatUtc).TotalSeconds;
                if (elapsed > heartbeatTimeoutSec)
                {
                    kvp.Value.ConsecutiveMissedHeartbeats++;
                    kvp.Value.Status = kvp.Value.ConsecutiveMissedHeartbeats >= 3 ? "Unhealthy" : "Warning";
                    unhealthy++;
                }
            }

            return unhealthy;
        }

        public ClientServiceState? GetState(string agentId)
        {
            _clients.TryGetValue(agentId, out var state);
            return state;
        }

        public IReadOnlyList<ClientServiceState> GetAllStates()
            => new List<ClientServiceState>(_clients.Values);
    }

    public sealed class ClientServiceState
    {
        public string AgentId { get; set; } = string.Empty;
        public string AtmId { get; set; } = string.Empty;
        public string ServerHost { get; set; } = string.Empty;
        public int ServerPort { get; set; }
        public string Status { get; set; } = "Unknown";
        public int ConsecutiveMissedHeartbeats { get; set; }
        public DateTime RegisteredAtUtc { get; set; }
        public DateTime LastHeartbeatUtc { get; set; }
    }
}
