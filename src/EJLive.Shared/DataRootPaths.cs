using System;
using System.Collections.Generic;
using System.Threading;
using IO = System.IO;

namespace EJLive.Shared
{
    /// <summary>
    /// The single, fixed, central data-root contract for every EJLive process
    /// (client service, endpoint console, enterprise server, NOC console,
    /// installer). All runtime data — database, logs, config, outbox, inbox,
    /// archive, reports, agent health — hangs off this one root.
    ///
    /// Resolution order (first hit wins, then cached for the process lifetime):
    ///   1. <c>EJLIVE_DATAROOT</c> environment variable (set by the installer or by an
    ///      operator for test/isolated trees);
    ///   2. <c>%PROGRAMDATA%\EJLive</c> (i.e. <see cref="Environment.SpecialFolder.CommonApplicationData"/>),
    ///      which is the shipped default and the only value the packaging contract probes for.
    ///
    /// The class owns no behaviour beyond path arithmetic and directory creation;
    /// consumers never hard-code <c>C:\ProgramData\EJLive</c> again — that literal was
    /// scattered across six assemblies before this consolidation (finding E-19).
    /// </summary>
    public static class DataRootPaths
    {
        /// <summary>Environment variable that overrides the root (install-time and per-test).</summary>
        public const string EnvironmentVariable = "EJLIVE_DATAROOT";

        private static readonly object Gate = new object();
        private static string? _root;

        /// <summary>The absolute data root for this process. Never null, never relative.</summary>
        public static string Root
        {
            get
            {
                var current = Volatile.Read(ref _root);
                if (current is not null)
                    return current;
                lock (Gate)
                    return _root ??= Resolve(Environment.GetEnvironmentVariable(EnvironmentVariable));
            }
        }

        /// <summary>
        /// Pure resolution used by startup code and tests: an absolute-path override wins,
        /// anything else (null/blank/relative/unhealthy) falls back to the shipped default.
        /// </summary>
        public static string Resolve(string? overridePath)
        {
            if (IsUsablePath(overridePath))
                return IO.Path.GetFullPath(overridePath!.TrimEnd('\\', '/'));

            var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (string.IsNullOrWhiteSpace(programData))
                programData = @"C:\ProgramData"; // last-resort fallback; still deterministic
            return IO.Path.Combine(programData, "EJLive");
        }

        /// <summary>True when the value can serve as a data root: non-blank, absolute, no wildcards.</summary>
        public static bool IsUsablePath(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            var trimmed = value.Trim();
            if (trimmed.IndexOfAny(new[] { '*', '?', '"' }) >= 0)
                return false;
            try
            {
                return IO.Path.IsPathRooted(trimmed);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        // ── Sub-tree (fixed layout; every consumer derives its paths from these) ──
        public static string ConfigDirectory  => IO.Path.Combine(Root, "Config");
        public static string LogsDirectory   => IO.Path.Combine(Root, "Logs");
        public static string DataDirectory   => IO.Path.Combine(Root, "Data");
        public static string ArchiveDirectory => IO.Path.Combine(Root, "Archive");
        public static string ReportsDirectory => IO.Path.Combine(Root, "Reports");
        public static string BackupsDirectory => IO.Path.Combine(Root, "Backups");
        public static string ImagesDirectory => IO.Path.Combine(Root, "Images");
        public static string ClientDirectory => IO.Path.Combine(Root, "Client");
        public static string OutboxDirectory => IO.Path.Combine(ClientDirectory, "Outbox");
        public static string InboxDirectory  => IO.Path.Combine(ClientDirectory, "Inbox");
        public static string AgentDirectory  => IO.Path.Combine(Root, "Agent");
        public static string WatchDirectory  => IO.Path.Combine(Root, "Watch");

        // ── Well-known files ──
        public static string DatabaseFile     => IO.Path.Combine(Root, "ejlive.db");
        public static string ConfigurationFile => IO.Path.Combine(ConfigDirectory, "ejlive.config.json");
        public static string HealthSnapshotFile => IO.Path.Combine(AgentDirectory, "health.json");
        public static string JournalStagingDirectory => IO.Path.Combine(ClientDirectory, "Staging");

        // ── Server-side image distribution shares (same root, share-local subtrees) ──
        public static string ShareDirectory => IO.Path.Combine(Root, "Share");
        public static string ShareImagesAllPath => IO.Path.Combine(ShareDirectory, "Images", "All");
        public static string ShareImagesByTypePath => IO.Path.Combine(ShareDirectory, "Images", "ByType");
        public static string ShareImagesStagingPath => IO.Path.Combine(ShareDirectory, "Images", "Staging");

        /// <summary>Creates the fixed layout (idempotent). The installer calls it once; every
        /// process calls it at startup so a manually-placed dataroot is still self-healing.</summary>
        public static IReadOnlyList<string> EnsureDirectories()
        {
            var created = new List<string>();
            foreach (var dir in new[]
                     {
                         Root, ConfigDirectory, LogsDirectory, DataDirectory, ArchiveDirectory,
                         ReportsDirectory, BackupsDirectory, ImagesDirectory, ClientDirectory,
                         OutboxDirectory, InboxDirectory, AgentDirectory, JournalStagingDirectory,
                         ShareImagesAllPath, ShareImagesByTypePath, ShareImagesStagingPath
                     })
            {
                try
                {
                    IO.Directory.CreateDirectory(dir);
                    created.Add(dir);
                }
                catch (Exception)
                {
                    // A read-only or ACL-restricted share still yields a usable root for the
                    // rest of the layout; per-directory failures are surfaced by the bootstrap
                    // report rather than thrown here, so one locked folder cannot brick startup.
                }
            }
            return created;
        }

        /// <summary>Test seam: recomputes the cached root from the current environment.
        /// Production code never calls it; <c>EJLive.Tests</c> and the Verification harness do.</summary>
        public static void Refresh()
        {
            lock (Gate)
                _root = null;
        }
    }
}
