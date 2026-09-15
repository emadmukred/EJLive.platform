using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using EJLive.Core;

namespace EJLive.Shared
{

    public static class Logger
    {
        public static event Action<string> MessageWritten;

        public static void Info(string message)
        {
            MessageWritten?.Invoke($"INFO {DateTime.UtcNow:O} {message}");
        }

        public static void Error(string message, Exception exception = null)
        {
            var detail = exception == null ? message : message + " :: " + exception.Message;
            MessageWritten?.Invoke($"ERROR {DateTime.UtcNow:O} {detail}");
        }
    }


    // RetryPolicy: removed (C-27). This Core-local copy claimed the EJLive.Shared
    // namespace while the real Shared assembly shipped none, so bare `RetryPolicy`
    // references resolved differently per project. The single canonical owner is
    // EJLive.Shared.RetryPolicy (src/EJLive.Shared/RetryPolicy.cs), whose GetDelay
    // preserves this exact linear-delay semantics.

    public sealed class MonitoringState
    {
        public string AtmId { get; set; } = string.Empty;
        public AtmRuntimeStatus Status { get; set; } = AtmRuntimeStatus.Unknown;
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
        public string Summary { get; set; } = string.Empty;
    }

    public sealed class MonitoringStateStore
    {
        private readonly ConcurrentDictionary<string, MonitoringState> _states =
            new ConcurrentDictionary<string, MonitoringState>(StringComparer.OrdinalIgnoreCase);

        public void Upsert(MonitoringState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.AtmId))
            {
                return;
            }

            _states[state.AtmId] = state;
        }

        public MonitoringState[] Snapshot()
        {
            var values = _states.Values;
            var result = new MonitoringState[values.Count];
            values.CopyTo(result, 0);
            return result;
        }
    }
}
