namespace EJLive.Shared;

public static class DateTimeHelper
{
    public static DateTime UtcNow => DateTime.UtcNow;

    public static string ToIsoUtc(DateTime utc)
    {
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O");
    }

    public static DateTime ParseUtcOrDefault(string? value, DateTime defaultValue)
    {
        return DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed.ToUniversalTime()
            : defaultValue;
    }

    public static string FormatElapsed(DateTime utcReference)
    {
        if (utcReference == DateTime.MinValue)
            return "---";

        var elapsed = DateTime.UtcNow - utcReference;
        if (elapsed.TotalSeconds < 60)
            return $"{(int)elapsed.TotalSeconds}s";
        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes}m";
        if (elapsed.TotalHours < 24)
            return $"{(int)elapsed.TotalHours}h {(int)elapsed.Minutes}m";
        return $"{(int)elapsed.TotalDays}d {(int)elapsed.Hours}h";
    }
}
