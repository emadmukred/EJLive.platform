using System;

namespace EJLive.Shared
{
    public partial public static class DateTimeHelper
        {
            public static DateTime UtcNow        => DateTime.UtcNow;
            public static DateTime LocalNow      => DateTime.Now;
            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
            public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
            public const string DateFormat = "yyyyMMdd";
            public static string ToDisplayLocal(DateTime utcTime)
            =>
            public static string ToDisplayShort(DateTime utcTime)
            =>
            public static string ToDateOnly(DateTime dt)
            =>
            public static string ToMonthPartition(DateTime dt)
            =>
            public static string ToFilenameSafe(DateTime dt)
            =>
            public static DateTime ParseUtcOrDefault(string isoStr)
            {
            public static string Elapsed(DateTime fromUtc)
            {
            public static bool IsOlderThan(DateTime utcTime, int minutes)
            =>
            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
            =>
            public static string GetUtcTimestamp() =>
            public static string GetLocalTimestamp() =>
            public static string GetUtcDateString() =>
            public static string ToIso8601(DateTime dateTime) =>
            public static DateTime? ParseIso8601(string? isoString)
            if (string.IsNullOrWhiteSpace(isoString)) return null;
    
    
            return DateTime.TryParse(isoString, out var result) ? result : null;
    
    
            // Variant from:
            public static DateTime? ParseIso8601(string? isoString)
            {
            public static string GetElapsedString(DateTime since)
            {
            public static DateTime? ParseIso8601(string? isoString)
            {
            public static string ToIsoUtc(DateTime utc)
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O");
    
    
            public static string ToDisplayLocal(DateTime utcTime)
            =>
            public static DateTime ToLocalTime(DateTime utcTime, string timezoneId = "Arab Standard Time")
            {
            public static DateTime ToUtcTime(DateTime localTime, string timezoneId = "Arab Standard Time")
            {
            public static string FormatDuration(TimeSpan duration)
            {
            public static string FormatRelativeTime(DateTime utcTime)
            {
            public static DateTime ParseUtcOrDefault(string? value, DateTime defaultValue)
            {
            public static string FormatElapsed(DateTime utcReference)
            {
        }
    
    }
    public partial class DateTimeHelper
        {
            public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
    
    
            public const string DateFormat = "yyyyMMdd";
    
    
            public static DateTime UtcNow        => DateTime.UtcNow;
    
    
            public static DateTime LocalNow      => DateTime.Now;
    
    
            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
    
    
            public static string ToDisplayLocal(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    
    
            public static string ToDisplayShort(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("HH:mm:ss");
    
    
            public static string ToDateOnly(DateTime dt)
                => dt.ToString("yyyy-MM-dd");
    
    
            public static string ToMonthPartition(DateTime dt)
                => dt.ToString("yyyy-MM");
    
    
            public static string ToFilenameSafe(DateTime dt)
                => dt.ToString("yyyyMMdd_HHmmss");
    
    
            public static DateTime ParseUtcOrDefault(string isoStr)
            {
                if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
            }
    
    
            public static string Elapsed(DateTime fromUtc)
            {
                var span = DateTime.UtcNow - fromUtc;
                if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                return $"{(int)span.TotalDays}يوم";
            }
    
    
            public static bool IsOlderThan(DateTime utcTime, int minutes)
                => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
    
    
            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
    
    
            public static string GetUtcTimestamp() => DateTime.UtcNow.ToString(TimestampFormat);
    
    
            public static string GetLocalTimestamp() => DateTime.Now.ToString(TimestampFormat);
    
    
            public static string GetUtcDateString() => DateTime.UtcNow.ToString(DateFormat);
    
    
            public static string ToIso8601(DateTime dateTime) => dateTime.ToString("o");
    
    
            public static DateTime? ParseIso8601(string? isoString)
            if (string.IsNullOrWhiteSpace(isoString)) return null;
    
    
            return DateTime.TryParse(isoString, out var result) ? result : null;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Utils\DateTimeHelper.cs
            public static DateTime? ParseIso8601(string? isoString)
            {
                if (string.IsNullOrWhiteSpace(isoString)) return null;
                return DateTime.TryParse(isoString, out var result) ? result : null;
            }
    
    
            public static string GetElapsedString(DateTime since)
            {
                var elapsed = DateTime.UtcNow - since;
                if (elapsed.TotalSeconds < 60) return $"{(int)elapsed.TotalSeconds}s ago";
                if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}m ago";
                if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}h ago";
                return $"{(int)elapsed.TotalDays}d ago";
            }
    
    
            public static string ToIsoUtc(DateTime utc)
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O");
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\DateTimeHelper.cs
            public static DateTime? ParseIso8601(string? isoString)
            {
                if (string.IsNullOrWhiteSpace(isoString)) return null;
                return DateTime.TryParse(isoString, out var result) ? result : null;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Utils\DateTimeHelper.cs
            public static DateTime? ParseIso8601(string? isoString)
            {
                if (string.IsNullOrWhiteSpace(isoString)) return null;
                return DateTime.TryParse(isoString, out var result) ? result : null;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Utils\DateTimeHelper.cs
            public static DateTime? ParseIso8601(string? isoString)
            {
                if (string.IsNullOrWhiteSpace(isoString)) return null;
                return DateTime.TryParse(isoString, out var result) ? result : null;
            }
    
    
        }

    public partial public public class DateTimeHelper
        {
            public static DateTime UtcNow        => DateTime.UtcNow;
            public static DateTime LocalNow      => DateTime.Now;
            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
            public static string ToDisplayLocal(DateTime utcTime)
            =>
            public static string ToDisplayShort(DateTime utcTime)
            =>
            public static string ToDateOnly(DateTime dt)
            =>
            public static string ToMonthPartition(DateTime dt)
            =>
            public static string ToFilenameSafe(DateTime dt)
            =>
            public static DateTime ParseUtcOrDefault(string isoStr)
            {
            public static string Elapsed(DateTime fromUtc)
            {
            public static bool IsOlderThan(DateTime utcTime, int minutes)
            =>
            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
            =>
            public static string ToIsoUtc(DateTime utc)
            {
            public static DateTime ParseUtcOrDefault(string? value, DateTime defaultValue)
            {
            public static string FormatElapsed(DateTime utcReference)
            {
        }
    
    }
    public partial public class DateTimeHelper
        {
            public static DateTime UtcNow        => DateTime.UtcNow;
            public static DateTime LocalNow      => DateTime.Now;
            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
            public static string ToDisplayLocal(DateTime utcTime)
            =>
            public static string ToDisplayShort(DateTime utcTime)
            =>
            public static string ToDateOnly(DateTime dt)
            =>
            public static string ToMonthPartition(DateTime dt)
            =>
            public static string ToFilenameSafe(DateTime dt)
            =>
            public static DateTime ParseUtcOrDefault(string isoStr)
            {
            public static string Elapsed(DateTime fromUtc)
            {
            public static bool IsOlderThan(DateTime utcTime, int minutes)
            =>
            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
            =>
            public static string ToIsoUtc(DateTime utc)
            {
            public static DateTime ParseUtcOrDefault(string? value, DateTime defaultValue)
            {
            public static string FormatElapsed(DateTime utcReference)
            {
        }
    
    }
    public partial class DateTimeHelper
        {
            public static DateTime UtcNow => DateTime.UtcNow;
    
    
            public static DateTime LocalNow      => DateTime.Now;
    
    
            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
    
    
            public static string ToIsoUtc(DateTime utc)
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O");
    
    
            public static string ToDisplayLocal(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    
    
            public static string ToDisplayShort(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("HH:mm:ss");
    
    
            public static string ToDateOnly(DateTime dt)
                => dt.ToString("yyyy-MM-dd");
    
    
            public static string ToMonthPartition(DateTime dt)
                => dt.ToString("yyyy-MM");
    
    
            public static string ToFilenameSafe(DateTime dt)
                => dt.ToString("yyyyMMdd_HHmmss");
    
    
            public static DateTime ParseUtcOrDefault(string isoStr)
            {
                if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
            }
    
    
            public static string Elapsed(DateTime fromUtc)
            {
                var span = DateTime.UtcNow - fromUtc;
                if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                return $"{(int)span.TotalDays}يوم";
            }
    
    
            public static bool IsOlderThan(DateTime utcTime, int minutes)
                => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
    
    
            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
    
    
        }
    public partial class DateTimeHelper
        {
            public static DateTime UtcNow        => DateTime.UtcNow;
    
    
            public static DateTime LocalNow      => DateTime.Now;
    
    
            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
    
    
            public static string ToDisplayLocal(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    
    
            public static string ToDisplayShort(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("HH:mm:ss");
    
    
            public static string ToDateOnly(DateTime dt)
                => dt.ToString("yyyy-MM-dd");
    
    
            public static string ToMonthPartition(DateTime dt)
                => dt.ToString("yyyy-MM");
    
    
            public static string ToFilenameSafe(DateTime dt)
                => dt.ToString("yyyyMMdd_HHmmss");
    
    
            public static DateTime ParseUtcOrDefault(string isoStr)
            {
                if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
            }
    
    
            public static string Elapsed(DateTime fromUtc)
            {
                var span = DateTime.UtcNow - fromUtc;
                if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                return $"{(int)span.TotalDays}يوم";
            }
    
    
            public static bool IsOlderThan(DateTime utcTime, int minutes)
                => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
    
    
            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
    
    
        }
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

    /// <summary>
        /// مساعد التاريخ والوقت — UTC موحد عبر النظام (T-11)
        /// يضمن توافق الطوابع الزمنية بين العميل والخادم والأرشيف
        /// </summary>
        public static class DateTimeHelper
        {
            // الوقت الحالي UTC — استخدم هذا دائمًا في كل الطوابع الزمنية
            public static DateTime UtcNow        => DateTime.UtcNow;
            public static DateTime LocalNow      => DateTime.Now;
            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
    
            public static string ToDisplayLocal(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    
            public static string ToDisplayShort(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("HH:mm:ss");
    
            public static string ToDateOnly(DateTime dt)
                => dt.ToString("yyyy-MM-dd");
    
            public static string ToMonthPartition(DateTime dt)
                => dt.ToString("yyyy-MM");
    
            public static string ToFilenameSafe(DateTime dt)
                => dt.ToString("yyyyMMdd_HHmmss");
    
            public static DateTime ParseUtcOrDefault(string isoStr)
            {
                if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
            }
    
            public static string Elapsed(DateTime fromUtc)
            {
                var span = DateTime.UtcNow - fromUtc;
                if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                return $"{(int)span.TotalDays}يوم";
            }
    
            public static bool IsOlderThan(DateTime utcTime, int minutes)
                => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
    
            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
        }
    // Class: DateTimeHelper (from 5 sources)
        public static partial class DateTimeHelper
        {
            // --- Properties ---
            public static DateTime UtcNow => DateTime.UtcNow;
    
                    public static DateTime LocalNow      => DateTime.Now;
    
                    public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
    
    
            // --- Methods ---
            public static string ToIsoUtc(DateTime utc)
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O");
    
                    public static string ToDisplayLocal(DateTime utcTime)
                        => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    
                    public static string ToDisplayShort(DateTime utcTime)
                        => utcTime.ToLocalTime().ToString("HH:mm:ss");
    
                    public static string ToDateOnly(DateTime dt)
                        => dt.ToString("yyyy-MM-dd");
    
                    public static string ToMonthPartition(DateTime dt)
                        => dt.ToString("yyyy-MM");
    
                    public static string ToFilenameSafe(DateTime dt)
                        => dt.ToString("yyyyMMdd_HHmmss");
    
                    public static DateTime ParseUtcOrDefault(string isoStr)
                    {
                        if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                        return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
                    }
    
                    public static string Elapsed(DateTime fromUtc)
                    {
                        var span = DateTime.UtcNow - fromUtc;
                        if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                        if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                        if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                        return $"{(int)span.TotalDays}يوم";
                    }
    
                    public static bool IsOlderThan(DateTime utcTime, int minutes)
                        => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
    
                    public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                        => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
    
    
        }
    // Class: DateTimeHelper (from 2 sources)
        public static partial class DateTimeHelper
        {
            // --- Properties ---
                    public static DateTime UtcNow        => DateTime.UtcNow;
    
                    public static DateTime LocalNow      => DateTime.Now;
    
                    public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
    
    
            // --- Methods ---
                    public static string ToDisplayLocal(DateTime utcTime)
                        => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    
                    public static string ToDisplayShort(DateTime utcTime)
                        => utcTime.ToLocalTime().ToString("HH:mm:ss");
    
                    public static string ToDateOnly(DateTime dt)
                        => dt.ToString("yyyy-MM-dd");
    
                    public static string ToMonthPartition(DateTime dt)
                        => dt.ToString("yyyy-MM");
    
                    public static string ToFilenameSafe(DateTime dt)
                        => dt.ToString("yyyyMMdd_HHmmss");
    
                    public static DateTime ParseUtcOrDefault(string isoStr)
                    {
                        if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                        return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
                    }
    
                    public static string Elapsed(DateTime fromUtc)
                    {
                        var span = DateTime.UtcNow - fromUtc;
                        if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                        if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                        if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                        return $"{(int)span.TotalDays}يوم";
                    }
    
                    public static bool IsOlderThan(DateTime utcTime, int minutes)
                        => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
    
                    public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                        => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
    
    
        }
    // Class: DateTimeHelper (from 9 sources)
        public static partial class DateTimeHelper
        {
            // --- Properties ---
                            public static DateTime UtcNow        => DateTime.UtcNow;
    
                            public static DateTime LocalNow      => DateTime.Now;
    
                            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
    
    
            // --- Methods ---
                            public static string ToDisplayLocal(DateTime utcTime)
                                => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    
                            public static string ToDisplayShort(DateTime utcTime)
                                => utcTime.ToLocalTime().ToString("HH:mm:ss");
    
                            public static string ToDateOnly(DateTime dt)
                                => dt.ToString("yyyy-MM-dd");
    
                            public static string ToMonthPartition(DateTime dt)
                                => dt.ToString("yyyy-MM");
    
                            public static string ToFilenameSafe(DateTime dt)
                                => dt.ToString("yyyyMMdd_HHmmss");
    
                            public static DateTime ParseUtcOrDefault(string isoStr)
                            {
                                if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                                return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
                            }
    
                            public static string Elapsed(DateTime fromUtc)
                            {
                                var span = DateTime.UtcNow - fromUtc;
                                if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                                if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                                if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                                return $"{(int)span.TotalDays}يوم";
                            }
    
                            public static bool IsOlderThan(DateTime utcTime, int minutes)
                                => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
    
                            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                                => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
    
    
        }
    // Class: DateTimeHelper (from 5 sources)
        public static partial class DateTimeHelper
        {
            // --- Properties ---
                    public static DateTime UtcNow        => DateTime.UtcNow;
    
                    public static DateTime LocalNow      => DateTime.Now;
    
                    public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
    
    
            // --- Methods ---
                    public static string ToDisplayLocal(DateTime utcTime)
                        => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    
                    public static string ToDisplayShort(DateTime utcTime)
                        => utcTime.ToLocalTime().ToString("HH:mm:ss");
    
                    public static string ToDateOnly(DateTime dt)
                        => dt.ToString("yyyy-MM-dd");
    
                    public static string ToMonthPartition(DateTime dt)
                        => dt.ToString("yyyy-MM");
    
                    public static string ToFilenameSafe(DateTime dt)
                        => dt.ToString("yyyyMMdd_HHmmss");
    
                    public static DateTime ParseUtcOrDefault(string isoStr)
                    {
                        if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                        return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
                    }
    
                    public static string Elapsed(DateTime fromUtc)
                    {
                        var span = DateTime.UtcNow - fromUtc;
                        if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                        if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                        if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                        return $"{(int)span.TotalDays}يوم";
                    }
    
                    public static bool IsOlderThan(DateTime utcTime, int minutes)
                        => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
    
                    public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                        => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
    
    
        }
}

namespace EJLive.Core.Utils
{
    public partial public public static class DateTimeHelper
        {
            public static DateTime UtcNow => DateTime.UtcNow;
            public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
            public const string DateFormat = "yyyyMMdd";
            public static DateTime ToLocalTime(DateTime utcTime, string timezoneId = "Arab Standard Time")
            {
            public static DateTime ToUtcTime(DateTime localTime, string timezoneId = "Arab Standard Time")
            {
            public static string FormatDuration(TimeSpan duration)
            {
            public static string FormatRelativeTime(DateTime utcTime)
            {
            public static string GetUtcTimestamp() =>
            public static string GetLocalTimestamp() =>
            public static string GetUtcDateString() =>
            public static string ToIso8601(DateTime dateTime) =>
            public static DateTime? ParseIso8601(string? isoString)
            {
            public static string GetElapsedString(DateTime since)
            {
        }
    
    }
    public partial public static class DateTimeHelper
        {
            public static DateTime UtcNow => DateTime.UtcNow;
            public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
            public const string DateFormat = "yyyyMMdd";
            public static DateTime ToLocalTime(DateTime utcTime, string timezoneId = "Arab Standard Time")
            {
            public static DateTime ToUtcTime(DateTime localTime, string timezoneId = "Arab Standard Time")
            {
            public static string FormatDuration(TimeSpan duration)
            {
            public static string FormatRelativeTime(DateTime utcTime)
            {
            public static string GetUtcTimestamp() =>
            public static string GetLocalTimestamp() =>
            public static string GetUtcDateString() =>
            public static string ToIso8601(DateTime dateTime) =>
            public static DateTime? ParseIso8601(string? isoString)
            {
            public static string GetElapsedString(DateTime since)
            {
        }
    
    }
    public partial class DateTimeHelper
        {
            public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
    
    
            public const string DateFormat = "yyyyMMdd";
    
    
            public static string GetUtcTimestamp() => DateTime.UtcNow.ToString(TimestampFormat);
    
    
            public static string GetLocalTimestamp() => DateTime.Now.ToString(TimestampFormat);
    
    
            public static string GetUtcDateString() => DateTime.UtcNow.ToString(DateFormat);
    
    
            public static string ToIso8601(DateTime dateTime) => dateTime.ToString("o");
    
    
            public static DateTime? ParseIso8601(string? isoString)
            if (string.IsNullOrWhiteSpace(isoString)) return null;
    
    
            return DateTime.TryParse(isoString, out var result) ? result : null;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\DateTimeHelper.cs
            public static DateTime? ParseIso8601(string? isoString)
            {
                if (string.IsNullOrWhiteSpace(isoString)) return null;
                return DateTime.TryParse(isoString, out var result) ? result : null;
            }
    
    
            public static string GetElapsedString(DateTime since)
            {
                var elapsed = DateTime.UtcNow - since;
                if (elapsed.TotalSeconds < 60) return $"{(int)elapsed.TotalSeconds}s ago";
                if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}m ago";
                if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}h ago";
                return $"{(int)elapsed.TotalDays}d ago";
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\DateTimeHelper.cs
            public static DateTime? ParseIso8601(string? isoString)
            {
                if (string.IsNullOrWhiteSpace(isoString)) return null;
                return DateTime.TryParse(isoString, out var result) ? result : null;
            }
    
    
        }
    // Class: DateTimeHelper (from 4 sources)
        public static partial class DateTimeHelper
        {
            // --- Constants & Fields ---
            public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
    
            public const string DateFormat = "yyyyMMdd";
    
    
            // --- Methods ---
            public static string GetUtcTimestamp() => DateTime.UtcNow.ToString(TimestampFormat);
    
            public static string GetLocalTimestamp() => DateTime.Now.ToString(TimestampFormat);
    
            public static string GetUtcDateString() => DateTime.UtcNow.ToString(DateFormat);
    
            public static string ToIso8601(DateTime dateTime) => dateTime.ToString("o");
    
            public static DateTime? ParseIso8601(string? isoString)
            if (string.IsNullOrWhiteSpace(isoString)) return null;
    
            return DateTime.TryParse(isoString, out var result) ? result : null;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\DateTimeHelper.cs
                public static DateTime? ParseIso8601(string? isoString)
                {
                    if (string.IsNullOrWhiteSpace(isoString)) return null;
                    return DateTime.TryParse(isoString, out var result) ? result : null;
                }
    
                public static string GetElapsedString(DateTime since)
                {
                    var elapsed = DateTime.UtcNow - since;
                    if (elapsed.TotalSeconds < 60) return $"{(int)elapsed.TotalSeconds}s ago";
                    if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}m ago";
                    if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}h ago";
                    return $"{(int)elapsed.TotalDays}d ago";
                }
    
    
        }
    public partial class DateTimeHelper
        {
            public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
            public const string DateFormat = "yyyyMMdd";
            public static DateTime UtcNow        => DateTime.UtcNow;
            public static DateTime LocalNow      => DateTime.Now;
            public static string   UtcNowIso8601 => DateTime.UtcNow.ToString("O");
            public static string GetUtcTimestamp() => DateTime.UtcNow.ToString(TimestampFormat);
            public static string GetLocalTimestamp() => DateTime.Now.ToString(TimestampFormat);
            public static string GetUtcDateString() => DateTime.UtcNow.ToString(DateFormat);
            public static string ToIso8601(DateTime dateTime) => dateTime.ToString("o");
            public static DateTime? ParseIso8601(string? isoString)
            if (string.IsNullOrWhiteSpace(isoString)) return null;
            return DateTime.TryParse(isoString, out var result) ? result : null;
            public static DateTime? ParseIso8601(string? isoString)
            {
                if (string.IsNullOrWhiteSpace(isoString)) return null;
                return DateTime.TryParse(isoString, out var result) ? result : null;
            }
            public static string GetElapsedString(DateTime since)
            {
                var elapsed = DateTime.UtcNow - since;
                if (elapsed.TotalSeconds < 60) return $"{(int)elapsed.TotalSeconds}s ago";
                if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}m ago";
                if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}h ago";
                return $"{(int)elapsed.TotalDays}d ago";
            }
            public static string ToDisplayLocal(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            public static string ToDisplayShort(DateTime utcTime)
                => utcTime.ToLocalTime().ToString("HH:mm:ss");
            public static string ToDateOnly(DateTime dt)
                => dt.ToString("yyyy-MM-dd");
            public static string ToMonthPartition(DateTime dt)
                => dt.ToString("yyyy-MM");
            public static string ToFilenameSafe(DateTime dt)
                => dt.ToString("yyyyMMdd_HHmmss");
            public static DateTime ParseUtcOrDefault(string isoStr)
            {
                if (string.IsNullOrEmpty(isoStr)) return DateTime.MinValue;
                return DateTime.TryParse(isoStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;
            }
            public static string Elapsed(DateTime fromUtc)
            {
                var span = DateTime.UtcNow - fromUtc;
                if (span.TotalSeconds < 60)    return $"{(int)span.TotalSeconds}ث";
                if (span.TotalMinutes < 60)    return $"{(int)span.TotalMinutes}د";
                if (span.TotalHours < 24)      return $"{(int)span.TotalHours}س";
                return $"{(int)span.TotalDays}يوم";
            }
            public static bool IsOlderThan(DateTime utcTime, int minutes)
                => (DateTime.UtcNow - utcTime).TotalMinutes > minutes;
            public static bool IsOlderThanSeconds(DateTime utcTime, int seconds)
                => (DateTime.UtcNow - utcTime).TotalSeconds > seconds;
            public static string ToIsoUtc(DateTime utc)
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O");
        }
    /// <summary>
    /// Utility class for consistent date/time operations across the system.
    /// Provides standardized timestamp formatting and timezone handling.
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>Standard timestamp format used across the system.</summary>
        public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
    
        /// <summary>Date-only format for file naming and archival.</summary>
        public const string DateFormat = "yyyyMMdd";
    
        /// <summary>
        /// Returns the current UTC time formatted as a standard timestamp string.
        /// </summary>
        public static string GetUtcTimestamp() => DateTime.UtcNow.ToString(TimestampFormat);
    
        /// <summary>
        /// Returns the current local time formatted as a standard timestamp string.
        /// </summary>
        public static string GetLocalTimestamp() => DateTime.Now.ToString(TimestampFormat);
    
        /// <summary>
        /// Returns the current UTC date in yyyyMMdd format, suitable for file naming.
        /// </summary>
        public static string GetUtcDateString() => DateTime.UtcNow.ToString(DateFormat);
    
        /// <summary>
        /// Converts a DateTime to a standardized ISO 8601 string.
        /// </summary>
        public static string ToIso8601(DateTime dateTime) => dateTime.ToString("o");
    
        /// <summary>
        /// Safely parses an ISO 8601 string, returning null on failure.
        /// </summary>
        public static DateTime? ParseIso8601(string? isoString)
        {
            if (string.IsNullOrWhiteSpace(isoString)) return null;
            return DateTime.TryParse(isoString, out var result) ? result : null;
        }
    
        /// <summary>
        /// Returns the elapsed time since the specified timestamp in a human-readable format.
        /// </summary>
        public static string GetElapsedString(DateTime since)
        {
            var elapsed = DateTime.UtcNow - since;
            if (elapsed.TotalSeconds < 60) return $"{(int)elapsed.TotalSeconds}s ago";
            if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}m ago";
            if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}h ago";
            return $"{(int)elapsed.TotalDays}d ago";
        }
    }

    public static class DateTimeHelper
    {
        public static DateTime UtcNow => DateTime.UtcNow;
    
        public static DateTime ToLocalTime(DateTime utcTime, string timezoneId = "Arab Standard Time")
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
            }
            catch
            {
                return utcTime.ToLocalTime();
            }
        }
    
        public static DateTime ToUtcTime(DateTime localTime, string timezoneId = "Arab Standard Time")
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                return TimeZoneInfo.ConvertTimeToUtc(localTime, tz);
            }
            catch
            {
                return localTime.ToUniversalTime();
            }
        }
    
        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
                return $"{duration.TotalDays:0.0}d";
            if (duration.TotalHours >= 1)
                return $"{duration.TotalHours:0.0}h";
            if (duration.TotalMinutes >= 1)
                return $"{duration.TotalMinutes:0.0}m";
            return $"{duration.TotalSeconds:0}s";
        }
    
        public static string FormatRelativeTime(DateTime utcTime)
        {
            var diff = DateTime.UtcNow - utcTime;
            if (diff.TotalSeconds < 60)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{diff.TotalMinutes:0}m ago";
            if (diff.TotalHours < 24)
                return $"{diff.TotalHours:0}h ago";
            return $"{diff.TotalDays:0}d ago";
        }
    }
}
