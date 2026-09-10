using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>Produces explainable fleet health scores from live ATM telemetry.</summary>
public sealed class FleetPredictionEngine
{
    public int PredictHealthScore(ATMInfo atm)
    {
        ArgumentNullException.ThrowIfNull(atm);
        atm.RecalculateHealthScore();
        return atm.HealthScore;
    }

    public FleetPredictionResult Predict(ATMInfo atm, DateTime? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(atm);
        var now = NormalizeUtc(nowUtc ?? DateTime.UtcNow);
        atm.RecalculateHealthScore();
        var heartbeatAge = MinutesSince(atm.LastHeartbeatUtc, now);
        var syncAge = MinutesSince(atm.LastSyncUtc, now);
        var risk = Math.Max(0, 100 - atm.HealthScore);

        if (atm.ConnectionStatus == ConnectionStatus.Disconnected) risk += 35;
        if (atm.ConnectionStatus == ConnectionStatus.Syncing) risk -= 8;
        if (heartbeatAge > AppConstants.AlertDisconnectWarningMin) risk += 20;
        if (heartbeatAge > AppConstants.AlertDisconnectCriticalMin) risk += 20;
        if (syncAge > AppConstants.AlertNoDataWarningMin) risk += 15;
        if (syncAge > AppConstants.AlertNoDataCriticalMin) risk += 20;
        risk += (int)Math.Min(30L, Math.Max(0L, (long)atm.ConsecutiveSyncFailures) * 10L);
        if (atm.Latency_ms > 2000) risk += 10;
        if (!string.IsNullOrWhiteSpace(atm.LastErrorCode)) risk += 12;
        risk = Math.Clamp(risk, 0, 100);

        var level = risk switch { >= 75 => "Critical", >= 40 => "Watch", _ => "Healthy" };
        var action = risk switch
        {
            >= 75 => "Check connectivity, request a status pulse, synchronize journals, then escalate if telemetry does not recover.",
            >= 40 => "Review the latest journal and active alerts, then schedule a synchronization if data is stale.",
            _ => "Continue normal monitoring."
        };
        var impact = risk switch
        {
            >= 75 => "Service interruption or journal synchronization loss is likely without intervention.",
            >= 40 => "Telemetry delay or connection degradation is possible.",
            _ => "No immediate operational impact detected."
        };

        return new FleetPredictionResult(
            level,
            risk,
            $"Health={atm.HealthScore}%, Connection={atm.ConnectionStatus}, HeartbeatAgeMin={FormatAge(heartbeatAge)}, SyncAgeMin={FormatAge(syncAge)}, Failures={atm.ConsecutiveSyncFailures}.",
            action,
            impact);
    }

    private static double MinutesSince(DateTime timestampUtc, DateTime nowUtc) =>
        timestampUtc == default ? double.PositiveInfinity : Math.Max(0, (nowUtc - NormalizeUtc(timestampUtc)).TotalMinutes);

    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static string FormatAge(double minutes) =>
        double.IsPositiveInfinity(minutes)
            ? "unknown"
            : minutes.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
}

public sealed record FleetPredictionResult(
    string Level,
    int RiskScore,
    string Reason,
    string RecommendedAction,
    string EstimatedImpact);
