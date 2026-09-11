using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EJLive.Client.WinForms.Services
{
    internal sealed class CashTelemetrySnapshot
    {
        public int Cass1 { get; set; }
        public int Cass2 { get; set; }
        public int Cass3 { get; set; }
        public int Cass4 { get; set; }
        public int Remaining { get; set; }
        public int Loaded { get; set; }
        public int DepositIn { get; set; }
        public int DispenseOut { get; set; }
        public int Reject { get; set; }
        public int Retract { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public CashTelemetrySnapshot()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    internal sealed class CashTelemetryService
    {
        private readonly string _stateFilePath;
        private readonly object _sync = new object();

        public CashTelemetryService(string baseDirectory)
        {
            _stateFilePath = Path.Combine(baseDirectory, "cash-counters.ini");
        }

        public CashTelemetrySnapshot GetSnapshot(string sourcePath)
        {
            lock (_sync)
            {
                var snapshot = LoadLocalState();
                if (!string.IsNullOrWhiteSpace(sourcePath) && Directory.Exists(sourcePath))
                {
                    MergeFromJournalFiles(snapshot, sourcePath);
                    Normalize(snapshot);
                    SaveLocalState(snapshot);
                }
                return snapshot;
            }
        }

        public void UpdateFromJournalPayload(string fileName, byte[] data)
        {
            lock (_sync)
            {
                var snapshot = LoadLocalState();
                string text = DecodePayload(data);
                ApplyParsedCounters(snapshot, text);
                ApplyDerivedCounters(snapshot, text);
                Normalize(snapshot);
                snapshot.UpdatedAtUtc = DateTime.UtcNow;
                SaveLocalState(snapshot);
            }
        }

        private void MergeFromJournalFiles(CashTelemetrySnapshot snapshot, string sourcePath)
        {
            var recentFiles = Directory.GetFiles(sourcePath, "*.*", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderByDescending(fi => fi.LastWriteTimeUtc)
                .Take(3)
                .ToList();

            foreach (var file in recentFiles)
            {
                try
                {
                    string text = ReadTailText(file.FullName, 65536);
                    ApplyParsedCounters(snapshot, text);
                    ApplyDerivedCounters(snapshot, text);
                }
                catch
                {
                }
            }
        }

        private void ApplyParsedCounters(CashTelemetrySnapshot snapshot, string text)
        {
            snapshot.Cass1 = KeepIfZero(snapshot.Cass1, MatchNumber(text, new[] { @"cass(?:ette)?\s*1[^\d]{0,10}(\d+)", @"cass1[^\d]{0,10}(\d+)" }));
            snapshot.Cass2 = KeepIfZero(snapshot.Cass2, MatchNumber(text, new[] { @"cass(?:ette)?\s*2[^\d]{0,10}(\d+)", @"cass2[^\d]{0,10}(\d+)" }));
            snapshot.Cass3 = KeepIfZero(snapshot.Cass3, MatchNumber(text, new[] { @"cass(?:ette)?\s*3[^\d]{0,10}(\d+)", @"cass3[^\d]{0,10}(\d+)" }));
            snapshot.Cass4 = KeepIfZero(snapshot.Cass4, MatchNumber(text, new[] { @"cass(?:ette)?\s*4[^\d]{0,10}(\d+)", @"cass4[^\d]{0,10}(\d+)" }));
            snapshot.Remaining = KeepIfZero(snapshot.Remaining, MatchNumber(text, new[] { @"remaining[^\d]{0,10}(\d+)", @"remain[^\d]{0,10}(\d+)" }));
            snapshot.Loaded = KeepIfZero(snapshot.Loaded, MatchNumber(text, new[] { @"loaded[^\d]{0,10}(\d+)" }));
            snapshot.DepositIn = KeepIfZero(snapshot.DepositIn, MatchNumber(text, new[] { @"deposit(?:ed)?[^\d]{0,10}(\d+)" }));
            snapshot.DispenseOut = KeepIfZero(snapshot.DispenseOut, MatchNumber(text, new[] { @"dispens(?:e|ed)[^\d]{0,10}(\d+)" }));
            snapshot.Reject = KeepIfZero(snapshot.Reject, MatchNumber(text, new[] { @"reject(?:ed)?[^\d]{0,10}(\d+)" }));
            snapshot.Retract = KeepIfZero(snapshot.Retract, MatchNumber(text, new[] { @"retract(?:ed)?[^\d]{0,10}(\d+)" }));
        }

        private void ApplyDerivedCounters(CashTelemetrySnapshot snapshot, string text)
        {
            int depositEvents = Regex.Matches(text, "deposit", RegexOptions.IgnoreCase).Count;
            int dispenseEvents = Regex.Matches(text, "withdraw|dispens", RegexOptions.IgnoreCase).Count;
            int rejectEvents = Regex.Matches(text, "reject", RegexOptions.IgnoreCase).Count;
            int retractEvents = Regex.Matches(text, "retract", RegexOptions.IgnoreCase).Count;

            if (snapshot.DepositIn == 0 && depositEvents > 0)
                snapshot.DepositIn += depositEvents;
            if (snapshot.DispenseOut == 0 && dispenseEvents > 0)
                snapshot.DispenseOut += dispenseEvents;
            if (snapshot.Reject == 0 && rejectEvents > 0)
                snapshot.Reject += rejectEvents;
            if (snapshot.Retract == 0 && retractEvents > 0)
                snapshot.Retract += retractEvents;
        }

        private static void Normalize(CashTelemetrySnapshot snapshot)
        {
            if (snapshot.Loaded <= 0)
                snapshot.Loaded = snapshot.Cass1 + snapshot.Cass2 + snapshot.Cass3 + snapshot.Cass4;

            if (snapshot.Remaining <= 0)
                snapshot.Remaining = Math.Max(0, snapshot.Loaded - snapshot.DispenseOut + snapshot.DepositIn - snapshot.Reject - snapshot.Retract);

            if (snapshot.Cass1 <= 0 && snapshot.Loaded > 0) snapshot.Cass1 = Math.Max(0, snapshot.Loaded / 4);
            if (snapshot.Cass2 <= 0 && snapshot.Loaded > 0) snapshot.Cass2 = Math.Max(0, snapshot.Loaded / 4);
            if (snapshot.Cass3 <= 0 && snapshot.Loaded > 0) snapshot.Cass3 = Math.Max(0, snapshot.Loaded / 4);
            if (snapshot.Cass4 <= 0 && snapshot.Loaded > 0) snapshot.Cass4 = Math.Max(0, snapshot.Loaded - snapshot.Cass1 - snapshot.Cass2 - snapshot.Cass3);
        }

        private CashTelemetrySnapshot LoadLocalState()
        {
            var snapshot = new CashTelemetrySnapshot();
            if (!File.Exists(_stateFilePath))
                return snapshot;

            foreach (string rawLine in File.ReadAllLines(_stateFilePath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || !line.Contains("=")) continue;
                int idx = line.IndexOf('=');
                string key = line.Substring(0, idx).Trim();
                string value = idx < line.Length - 1 ? line.Substring(idx + 1).Trim() : string.Empty;
                int parsed;
                switch (key)
                {
                    case "Cass1": if (int.TryParse(value, out parsed)) snapshot.Cass1 = parsed; break;
                    case "Cass2": if (int.TryParse(value, out parsed)) snapshot.Cass2 = parsed; break;
                    case "Cass3": if (int.TryParse(value, out parsed)) snapshot.Cass3 = parsed; break;
                    case "Cass4": if (int.TryParse(value, out parsed)) snapshot.Cass4 = parsed; break;
                    case "Remaining": if (int.TryParse(value, out parsed)) snapshot.Remaining = parsed; break;
                    case "Loaded": if (int.TryParse(value, out parsed)) snapshot.Loaded = parsed; break;
                    case "DepositIn": if (int.TryParse(value, out parsed)) snapshot.DepositIn = parsed; break;
                    case "DispenseOut": if (int.TryParse(value, out parsed)) snapshot.DispenseOut = parsed; break;
                    case "Reject": if (int.TryParse(value, out parsed)) snapshot.Reject = parsed; break;
                    case "Retract": if (int.TryParse(value, out parsed)) snapshot.Retract = parsed; break;
                }
            }
            return snapshot;
        }

        private void SaveLocalState(CashTelemetrySnapshot snapshot)
        {
            File.WriteAllLines(_stateFilePath, new[]
            {
                "Cass1=" + snapshot.Cass1,
                "Cass2=" + snapshot.Cass2,
                "Cass3=" + snapshot.Cass3,
                "Cass4=" + snapshot.Cass4,
                "Remaining=" + snapshot.Remaining,
                "Loaded=" + snapshot.Loaded,
                "DepositIn=" + snapshot.DepositIn,
                "DispenseOut=" + snapshot.DispenseOut,
                "Reject=" + snapshot.Reject,
                "Retract=" + snapshot.Retract,
                "UpdatedAtUtc=" + snapshot.UpdatedAtUtc.ToString("o")
            });
        }

        private static string ReadTailText(string filePath, int maxBytes)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int length = (int)Math.Min(maxBytes, fs.Length);
                fs.Seek(-length, SeekOrigin.End);
                byte[] buffer = new byte[length];
                fs.Read(buffer, 0, length);
                return DecodePayload(buffer);
            }
        }

        private static string DecodePayload(byte[] data)
        {
            if (data == null || data.Length == 0) return string.Empty;
            try { return Encoding.UTF8.GetString(data); }
            catch { return Encoding.Default.GetString(data); }
        }

        private static int MatchNumber(string text, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                int parsed;
                if (match.Success && match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out parsed))
                    return parsed;
            }
            return 0;
        }

        private static int KeepIfZero(int target, int value) => value > 0 ? value : target;
    }
}
