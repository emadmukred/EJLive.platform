using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Services;

namespace EJLive.Core.Services;

/// <summary>
/// Wave 5 — REST + upload host for the smart content/value analysis pipeline
/// (SS-27). Owns a <see cref="SmartAnalysisService"/> and a single
/// <see cref="HttpListener"/> listening on the loopback interface so the
/// WinForms tabs, CLI tools, and any future web front-end can upload
/// journal payloads, fetch the canonical analysis report, and list the
/// most recent uploads.
///
/// Endpoints (all JSON unless noted):
///   GET  /api/health                      → { status: "ok", uptimeSeconds }
///   POST /api/analysis/upload             (body = raw text, headers X-Source, X-Vendor, X-Trace)
///   POST /api/analysis/files              (multipart/form-data, field name "file")
///   GET  /api/analysis/recent?n=10        → [traceId, source, vendor, ...]
///   GET  /api/analysis/by-trace/{traceId} → SmartAnalysisReport
///   GET  /api/analysis/by-source/{label}  → SmartAnalysisReport (latest)
///   GET  /api/analysis/categories         → [ { name, count } ] (union of recent uploads)
///
/// The host is intentionally a thin transport layer — all real work lives in
/// <see cref="SmartAnalysisService"/> so the same logic is reachable from
/// unit tests and the desktop UI.
/// </summary>
public sealed class SmartAnalysisHost : IDisposable
{
    private readonly SmartAnalysisService _service;
    private readonly HttpListener _listener;
    private readonly string _prefix;
    private readonly string _uploadRoot;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, SmartAnalysisReport> _byTrace
        = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _latestBySource
        = new(StringComparer.OrdinalIgnoreCase);
    private readonly DateTime _startedAtUtc = DateTime.UtcNow;
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private bool _disposed;

    public event Action<string>? OnLog;

    public SmartAnalysisHost(SmartAnalysisService service, string prefix, string uploadRoot)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        _uploadRoot = uploadRoot ?? throw new ArgumentNullException(nameof(uploadRoot));
        if (!Directory.Exists(_uploadRoot)) Directory.CreateDirectory(_uploadRoot);
        _listener = new HttpListener();
        _listener.Prefixes.Add(_prefix);
    }

    public int Port { get; private set; }
    public int RecentUploadCount => _byTrace.Count;
    public TimeSpan Uptime => DateTime.UtcNow - _startedAtUtc;

    public void Start()
    {
        if (_loop is not null) return;
        _cts = new CancellationTokenSource();
        _listener.Start();
        var portStr = _prefix.Split(':')[^1].TrimEnd('/');
        Port = int.TryParse(portStr, out var p) ? p : 0;
        _loop = Task.Run(() => LoopAsync(_cts.Token));
        OnLog?.Invoke($"[SmartAnalysisHost] listening on {_prefix}");
    }

    public void Stop()
    {
        if (_cts is null) return;
        _cts.Cancel();
        try { _listener.Stop(); } catch { /* ignore */ }
        try { _loop?.Wait(TimeSpan.FromSeconds(2)); } catch { /* ignore */ }
        _cts = null;
        _loop = null;
    }

    public IReadOnlyList<SmartAnalysisReport> SnapshotRecent(int n = 20)
    {
        var ordered = _byTrace.Values
            .OrderByDescending(r => r.AnalyzedAtUtc)
            .Take(Math.Max(1, n))
            .ToList();
        return ordered;
    }

    public SmartAnalysisReport? GetByTrace(string traceId)
        => _byTrace.TryGetValue(traceId ?? string.Empty, out var r) ? r : null;

    public SmartAnalysisReport? GetLatestForSource(string source)
    {
        if (string.IsNullOrEmpty(source)) return null;
        if (_latestBySource.TryGetValue(source, out var trace) && _byTrace.TryGetValue(trace, out var r))
            return r;
        return null;
    }

    // -----------------------------------------------------------------
    // Request loop
    // -----------------------------------------------------------------

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try { ctx = await _listener.GetContextAsync().ConfigureAwait(false); }
            catch { break; }
            _ = Task.Run(() => HandleAsync(ctx), ct);
        }
    }

    private async Task HandleAsync(HttpListenerContext ctx)
    {
        try
        {
            var path = (ctx.Request.Url?.AbsolutePath ?? "/").TrimEnd('/').ToLowerInvariant();
            var method = ctx.Request.HttpMethod.ToUpperInvariant();
            OnLog?.Invoke($"[{method} {path}]");

            switch (path)
            {
                case "/api/health":
                    await WriteJson(ctx, 200, new
                    {
                        status = "ok",
                        uptimeSeconds = (long)Uptime.TotalSeconds,
                        port = Port,
                        recentUploads = RecentUploadCount,
                        uploadRoot = _uploadRoot
                    }).ConfigureAwait(false);
                    return;

                case "/api/analysis/upload" when method == "POST":
                    await HandleAnalyzeUpload(ctx).ConfigureAwait(false);
                    return;

                case "/api/analysis/files" when method == "POST":
                    await HandleMultipartUpload(ctx).ConfigureAwait(false);
                    return;

                case "/api/analysis/recent" when method == "GET":
                    await WriteJson(ctx, 200, SnapshotRecent(GetQueryInt(ctx, "n", 20))).ConfigureAwait(false);
                    return;

                case "/api/analysis/categories" when method == "GET":
                    await WriteCategorySummary(ctx).ConfigureAwait(false);
                    return;

                default:
                    if (path.StartsWith("/api/analysis/by-trace/") && method == "GET")
                    {
                        var trace = WebUtility.UrlDecode(path["/api/analysis/by-trace/".Length..]);
                        var r = GetByTrace(trace);
                        if (r is null) { await WriteJson(ctx, 404, new { error = "trace not found" }).ConfigureAwait(false); return; }
                        await WriteJson(ctx, 200, r).ConfigureAwait(false);
                        return;
                    }
                    if (path.StartsWith("/api/analysis/by-source/") && method == "GET")
                    {
                        var src = WebUtility.UrlDecode(path["/api/analysis/by-source/".Length..]);
                        var r = GetLatestForSource(src);
                        if (r is null) { await WriteJson(ctx, 404, new { error = "source not found" }).ConfigureAwait(false); return; }
                        await WriteJson(ctx, 200, r).ConfigureAwait(false);
                        return;
                    }
                    await WriteJson(ctx, 404, new { error = "route not found", path }).ConfigureAwait(false);
                    return;
            }
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"[SmartAnalysisHost] ERROR {ex.GetType().Name}: {ex.Message}");
            try { await WriteJson(ctx, 500, new { error = ex.Message }).ConfigureAwait(false); }
            catch { /* socket torn */ }
        }
        finally
        {
            try { ctx.Response.Close(); } catch { /* ignore */ }
        }
    }

    private async Task HandleAnalyzeUpload(HttpListenerContext ctx)
    {
        var body = await ReadAllTextAsync(ctx.Request.InputStream, ctx.Request.ContentEncoding ?? Encoding.UTF8)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(body))
        {
            await WriteJson(ctx, 400, new { error = "empty body" }).ConfigureAwait(false);
            return;
        }
        var source = ctx.Request.Headers["X-Source"] ?? "upload-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var vendor = ctx.Request.Headers["X-Vendor"] ?? "";
        var trace = ctx.Request.Headers["X-Trace"] ?? "";

        var report = _service.AnalyzeUpload(body, source, vendor, trace);
        PersistReport(report);
        await WriteJson(ctx, 200, report).ConfigureAwait(false);
    }

    private async Task HandleMultipartUpload(HttpListenerContext ctx)
    {
        var ct = ctx.Request.ContentType ?? string.Empty;
        if (!ct.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase) ||
            !ct.Contains("boundary=", StringComparison.OrdinalIgnoreCase))
        {
            await WriteJson(ctx, 400, new { error = "expected multipart/form-data with boundary" }).ConfigureAwait(false);
            return;
        }

        var boundary = "--" + ct.Split("boundary=", 2)[1].Split(';')[0].Trim();
        var ms = new MemoryStream();
        await ctx.Request.InputStream.CopyToAsync(ms).ConfigureAwait(false);
        ms.Position = 0;

        var savedPath = Path.Combine(_uploadRoot, $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.upl");
        var text = ExtractFirstMultipartField(ms, boundary, "file", savedPath);
        if (string.IsNullOrEmpty(text))
        {
            await WriteJson(ctx, 400, new { error = "no 'file' field in multipart payload" }).ConfigureAwait(false);
            return;
        }

        var report = _service.AnalyzeUpload(text, savedPath);
        PersistReport(report);
        await WriteJson(ctx, 200, new
        {
            traceId = report.TraceId,
            savedPath,
            report.LineCount,
            CriticalCount = report.CriticalCount,
            WarningCount = report.WarningCount,
            InfoCount = report.InfoCount
        }).ConfigureAwait(false);
    }

    private async Task WriteCategorySummary(HttpListenerContext ctx)
    {
        var agg = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in _byTrace.Values)
        {
            foreach (var kv in r.CategoryBreakdown)
                agg[kv.Key] = agg.TryGetValue(kv.Key, out var n) ? n + kv.Value : kv.Value;
        }
        var rows = agg.OrderByDescending(kv => kv.Value)
            .Select(kv => new { name = kv.Key, count = kv.Value })
            .ToList();
        await WriteJson(ctx, 200, rows).ConfigureAwait(false);
    }

    // -----------------------------------------------------------------
    // Persistence helpers
    // -----------------------------------------------------------------

    private void PersistReport(SmartAnalysisReport report)
    {
        _byTrace[report.TraceId] = report;
        _latestBySource[report.SourceLabel] = report.TraceId;
    }

    // -----------------------------------------------------------------
    // Plumbing
    // -----------------------------------------------------------------

    private static int GetQueryInt(HttpListenerContext ctx, string key, int fallback)
    {
        var raw = ctx.Request.QueryString[key];
        return int.TryParse(raw, out var n) ? n : fallback;
    }

    private static async Task<string> ReadAllTextAsync(Stream s, Encoding enc)
    {
        using var reader = new StreamReader(s, enc, false, 8192, leaveOpen: true);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    private static string ExtractFirstMultipartField(Stream src, string boundary, string fieldName, string? saveTo)
    {
        using var ms = new MemoryStream();
        src.CopyTo(ms);
        var bytes = ms.ToArray();
        var enc = Encoding.UTF8;
        var text = enc.GetString(bytes);
        var parts = text.Split(new[] { boundary }, StringSplitOptions.None);
        foreach (var part in parts)
        {
            if (part.Length < 2 || part.StartsWith("--")) continue;
            var sep = part.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            if (sep < 0) continue;
            var header = part[..sep];
            if (!header.Contains($"name=\"{fieldName}\"", StringComparison.OrdinalIgnoreCase)) continue;
            var body = part[(sep + 4)..];
            var trim = body.LastIndexOf("\r\n", StringComparison.Ordinal);
            if (trim > 0) body = body[..trim];
            if (saveTo is not null) File.WriteAllText(saveTo, body, enc);
            return body;
        }
        return string.Empty;
    }

    private static async Task WriteJson(HttpListenerContext ctx, int status, object payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var bytes = Encoding.UTF8.GetBytes(json);
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _listener.Close();
    }
}
