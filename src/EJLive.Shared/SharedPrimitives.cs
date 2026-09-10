using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using EJLive.Core;

namespace EJLive.Shared
{
    public static class DateTimeHelper
    {
        public static DateTime UtcNow => DateTime.UtcNow;

        public static string ToIsoUtc(DateTime value)
        {
            return value.ToUniversalTime().ToString("O");
        }
    }

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

    public static class SecurityHelper
    {
        public static string SHA256Hash(string value)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public static string MaskSensitiveValue(string value)
        {
            return SecretRedactor.MaskCard(value);
        }
    }

    public sealed class RetryPolicy
    {
        public int MaxAttempts { get; set; } = 3;
        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(250);

        public TimeSpan GetDelay(int attempt)
        {
            var boundedAttempt = Math.Max(1, attempt);
            return TimeSpan.FromMilliseconds(BaseDelay.TotalMilliseconds * boundedAttempt);
        }
    }

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
