using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace EJLive.Shared
{

    public class LogEntry
    {
        public DateTime        Timestamp { get; set; }
        public AppLogger.Level Level     { get; set; }
        public string          Source    { get; set; }
        public string          Message   { get; set; }

        public string Format() =>
            $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";

        public string FormattedForUI =>
            $"[{Timestamp.ToLocalTime():HH:mm:ss}] [{LevelTag}] {Message}";

        private string LevelTag => Level switch
        {
            AppLogger.Level.Debug    => "DBG",
            AppLogger.Level.Info     => "INF",
            AppLogger.Level.Warning  => "WRN",
            AppLogger.Level.Error    => "ERR",
            AppLogger.Level.Critical => "CRT",
            _                        => "---"
        };
    }
}
