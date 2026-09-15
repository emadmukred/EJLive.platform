using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Server.WinForms;

/// <summary>
/// Bulk folder analysis for the Journal Studio (SS-10.5 "bulk analysis"). Off the UI
/// thread, one pass per file: head sniff for the vendor (<see cref="UnifiedJournalEvidenceAnalyzer"/>)
/// → registry parser resolution (POL-1) → full parse → per-file aggregate. No journal data
/// is persisted here — the bulk surface is read-only analytics over the same parser
/// contracts the single-file path uses, capped at 500 files so a folder drop can never
/// outrun the operator's patience (progress + cancel throughout, SS-15 #5).
/// </summary>
internal static class JournalStudioBulkAnalyzer
{
    public const int MaxFiles = 500;
    private static readonly string[] AcceptedExtensions = { ".log", ".ej", ".txt" };

    /// <summary>One aggregated row per analysed file.</summary>
    internal sealed record BulkRow(
        string FileName,
        string FullPath,
        string Vendor,
        string Parser,
        int Lines,
        int Transactions,
        int SuccessCount,
        int SuspiciousCount,
        decimal TotalAmount,
        string Status);

    public static async Task<IReadOnlyList<BulkRow>> RunAsync(
        string folder,
        IProgress<int>? progress,
        CancellationToken ct)
    {
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Folder not found: {folder}");

        var files = Directory.EnumerateFiles(folder)
            .Where(f => AcceptedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .Take(MaxFiles)
            .ToList();

        if (files.Count == 0)
            return Array.Empty<BulkRow>();

        // The whole sweep is CPU/IO-bound and bound-checked; run it on the pool and pump
        // progress back to the UI thread through the caller's Progress<T>.
        return await Task.Run(() =>
        {
            var registry = EjParserRegistry.Default;
            var rows = new List<BulkRow>(files.Count);
            var done = 0;

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                rows.Add(AnalyseOne(file, registry));
                progress?.Report(++done);
            }

            return (IReadOnlyList<BulkRow>)rows;
        }, ct).ConfigureAwait(false);
    }

    private static BulkRow AnalyseOne(string file, EjParserRegistry registry)
    {
        try
        {
            var lines = File.ReadAllLines(file);
            var head = string.Join("\n", lines.Take(24));
            var vendor = UnifiedJournalEvidenceAnalyzer.DetectVendor(null, head);
            var parser = registry.Resolve(vendor);
            var transactions = parser.Parse(lines.ToList(), Path.GetFileNameWithoutExtension(file));

            return new BulkRow(
                Path.GetFileName(file),
                file,
                vendor,
                parser.GetType().Name,
                lines.Length,
                transactions.Count,
                transactions.Count(t => t.Classification == TransactionClassification.Success),
                transactions.Count(t => t.Classification == TransactionClassification.Suspicious),
                transactions.Where(t => t.Amount.HasValue).Sum(t => t.Amount ?? 0m),
                transactions.Count == 0 && lines.Length > 0 ? "No transactions recognised" : "OK");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A broken file is a row with an error status — the run keeps going; operators
            // triage the failures in the grid (never abort the sweep over one bad capture).
            return new BulkRow(Path.GetFileName(file), file, "?", "?", 0, 0, 0, 0, 0m, "Failed: " + ex.Message);
        }
    }
}
