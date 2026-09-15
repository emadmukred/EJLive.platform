using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Shared;

namespace EJLive.Core.Data;

/// <summary>
/// One startup machine for every EJLive process (SS-20 bootstrap). The service, the
/// endpoint console, the enterprise server host and the NOC console all call
/// <see cref="RunAsync"/> before they touch engines or UI, so the sequence is a contract,
/// not six copies:
///
/// <code>
/// dataroot resolve → directories → probe → configuration → database init
///   → schema book (migrations + version guard) → readiness summary
/// </code>
///
/// Every step is idempotent and records a <see cref="BootstrapStepResult"/>; a failing
/// step aborts the dependent ones and yields <c>Success=false</c> so the caller shows one
/// actionable line — the same line the structured log records (SS-14). The headless
/// service treats the result as fatal; the WinForms hosts render it. No step blocks the
/// UI thread: the machine is a single awaitable sequence (SS-12).
/// </summary>
public sealed class PlatformBootstrap
{
    private readonly DataRootLayout _layout;
    private readonly BootstrapOptions _options;

    public PlatformBootstrap(DataRootLayout? layout = null, BootstrapOptions? options = null)
    {
        _layout = layout ?? new DataRootLayout();
        _options = options ?? new BootstrapOptions();
    }

    /// <summary>The resolved layout (paths already ensured when <see cref="RunAsync"/> completed).</summary>
    public DataRootLayout Layout => _layout;

    /// <summary>Configuration document loaded during the last run (null before RunAsync).</summary>
    public AppConfig? Configuration { get; private set; }

    /// <summary>Applied migration count from the last run (-1 before RunAsync).</summary>
    public int MigrationsApplied { get; private set; } = -1;

    public async Task<BootstrapResult> RunAsync(CancellationToken ct = default)
    {
        var started = Stopwatch.StartNew();
        var steps = new List<BootstrapStepResult>();

        // ── 1. dataroot ───────────────────────────────────────────────────────
        steps.Add(await RunStepAsync("dataroot", ct, _ =>
        {
            if (!DataRootPaths.IsUsablePath(DataRootPaths.Root))
                throw new InvalidOperationException(
                    $"Resolved dataroot is not usable: '{DataRootPaths.Root}'. " +
                    $"Set {DataRootPaths.EnvironmentVariable} to an absolute path.");
            if (_options.ExpectedRoot is not null &&
                !string.Equals(_options.ExpectedRoot, DataRootPaths.Root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Expected dataroot '{_options.ExpectedRoot}' but found '{DataRootPaths.Root}'.");
            return Task.CompletedTask;
        }).ConfigureAwait(false));

        // ── 2. directories + 3. writability probe ─────────────────────────────
        steps.Add(await RunStepAsync("directories", ct, _ =>
        {
            _layout.EnsureDirectories();
            return Task.CompletedTask;
        }).ConfigureAwait(false));

        steps.Add(await RunStepAsync("probe", ct, async _ =>
        {
            var probe = await _layout.ProbeAsync().ConfigureAwait(false);
            if (!probe.Success)
                throw new InvalidOperationException(string.Join("; ", probe.Findings));
        }).ConfigureAwait(false));

        // ── 4. configuration ──────────────────────────────────────────────────
        steps.Add(await RunStepAsync("configuration", ct, _ =>
        {
            Configuration = AppConfig.Load(_options.ConfigPath ?? DataRootPaths.ConfigurationFile);
            Configuration.EnsureRuntimeFolders();
            return Task.CompletedTask;
        }).ConfigureAwait(false));

        // ── 5. database ───────────────────────────────────────────────────────
        steps.Add(await RunStepAsync("database", ct, _ =>
        {
            DatabaseManagerAccessor.Initialize(_options.DatabasePathOverride ?? _layout.DatabaseFile);
            return Task.CompletedTask;
        }).ConfigureAwait(false));

        // ── 6. schema book: migrations + version guard ────────────────────────
        steps.Add(await RunStepAsync("schema", ct, _ =>
        {
            var runner = DatabaseMigrationsRunner.FromDatabaseFile(
                _options.DatabasePathOverride ?? _layout.DatabaseFile);
            if (!runner.TryEnsureCurrent(out int current, out int head))
                throw new InvalidOperationException(
                    $"Database schema version {current} is newer than this binary (max {head}); " +
                    "refusing to run against a schema it cannot understand.");
            MigrationsApplied = runner.RunAll();
        }).ConfigureAwait(false));

        steps.Add(steps.Count(s => s.Ok) == steps.Count
            ? new BootstrapStepResult("ready", true,
                $"{steps.Count - 1} steps in {started.ElapsedMilliseconds} ms", TimeSpan.Zero)
            : new BootstrapStepResult("aborted", false,
                "one or more mandatory steps failed", TimeSpan.Zero));

        return new BootstrapResult(steps, started.Elapsed);
    }

    private static async Task<BootstrapStepResult> RunStepAsync(
        string name, CancellationToken ct, Func<CancellationToken, Task> body)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            ct.ThrowIfCancellationRequested();
            await body(ct).ConfigureAwait(false);
            return new BootstrapStepResult(name, true, "ok", sw.Elapsed);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return new BootstrapStepResult(name, false, "cancelled", sw.Elapsed);
        }
        catch (Exception ex)
        {
            return new BootstrapStepResult(name, false, ex.Message, sw.Elapsed);
        }
    }
}

/// <summary>Bootstrap tunables. Defaults resolve everything from the central dataroot.</summary>
public sealed record BootstrapOptions
{
    /// <summary>Fail when the resolved root differs (installer passes the staged root).</summary>
    public string? ExpectedRoot { get; init; }

    /// <summary>Alternate configuration document; null uses <c>&lt;dataroot&gt;/Config/ejlive.config.json</c>.</summary>
    public string? ConfigPath { get; init; }

    /// <summary>Alternate SQLite file; null uses <c>&lt;dataroot&gt;/ejlive.db</c>.</summary>
    public string? DatabasePathOverride { get; init; }
}

/// <summary>One bootstrap machine step.</summary>
public sealed record BootstrapStepResult(string Name, bool Ok, string Detail, TimeSpan Elapsed)
{
    /// <summary>One-line rendering used by status strips and the service event log (SS-14).</summary>
    public string Describe() => $"{Name}: {(Ok ? "OK" : "FAIL")} ({Elapsed.TotalMilliseconds:F0} ms) {Detail}";
}

/// <summary>Aggregate outcome of <see cref="PlatformBootstrap.RunAsync"/>.</summary>
public sealed class BootstrapResult
{
    public BootstrapResult(IReadOnlyList<BootstrapStepResult> steps, TimeSpan elapsed)
    {
        Steps = steps;
        Elapsed = elapsed;
    }

    public IReadOnlyList<BootstrapStepResult> Steps { get; }
    public TimeSpan Elapsed { get; }
    public bool Success => Steps.All(step => step.Ok);

    /// <summary>The first failing step's message — the actionable line callers must show.</summary>
    public string? FailureDetail
    {
        get
        {
            foreach (var step in Steps)
                if (!step.Ok)
                    return $"{step.Name}: {step.Detail}";
            return null;
        }
    }

    /// <summary>Single machine-readable line per step (installer self-test contract, SS-16).</summary>
    public string[] ReportLines() => Steps.Select(step => step.Describe()).ToArray();
}

/// <summary>
/// Seam between bootstrap and the database singleton: the composition root initialises
/// <c>DatabaseManager</c> through this accessor so <see cref="PlatformBootstrap"/> never
/// reaches into another namespace's static state directly and tests can observe the
/// resolved path.
/// </summary>
public static class DatabaseManagerAccessor
{
    public static string? ActiveDatabasePath { get; private set; }

    public static void Initialize(string databasePath)
    {
        Services.DatabaseManager.Instance.Initialize(databasePath);
        ActiveDatabasePath = databasePath;
    }
}
