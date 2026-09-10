using System;
using System.Collections.Generic;
using System.Linq;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Business
{
    /// <summary>
    /// Analyzes journal evidence across multiple sources to produce
    /// confidence-scored transaction interpretations.
    /// </summary>
    public sealed class UnifiedJournalEvidenceAnalyzer
    {
        private readonly List<string> _analysisLog = new();

        public JournalAnalysisResult Analyze(string atmId, string journalText)
        {
            var result = new JournalAnalysisResult
            {
                AtmId = atmId,
                AnalyzedAtUtc = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(journalText))
            {
                result.Errors.Add("Empty journal text provided");
                return result;
            }

            try
            {
                var lines = journalText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                result.TotalLines = lines.Length;

                // Count evidence patterns
                result.ApprovedCount = lines.Count(l => l.Contains("APPROVED", StringComparison.OrdinalIgnoreCase));
                result.DeclinedCount = lines.Count(l => l.Contains("DECLINED", StringComparison.OrdinalIgnoreCase));
                result.ReversalCount = lines.Count(l => l.Contains("REVERSAL", StringComparison.OrdinalIgnoreCase));
                result.ErrorCount = lines.Count(l => l.Contains("ERROR", StringComparison.OrdinalIgnoreCase) || l.Contains("FAULT", StringComparison.OrdinalIgnoreCase));
                result.SuccessCount = lines.Count(l => l.Contains("NOTES PRESENTED", StringComparison.OrdinalIgnoreCase));

                result.Confidence = result.ApprovedCount > 0 ? "High" : "Medium";
                result.Summary = $"Analyzed {result.TotalLines} lines: {result.SuccessCount} successes, {result.DeclinedCount} declined, {result.ErrorCount} errors";

                Log($"Evidence analysis complete for ATM {atmId}");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Analysis error: {ex.Message}");
            }

            return result;
        }

        public IReadOnlyList<string> GetAnalysisLog() => _analysisLog;

        private void Log(string message)
        {
            _analysisLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
            AppLogger.Instance.Info(message, "Evidence");
        }
    }

    /// <summary>
    /// Result of evidence analysis for a journal.
    /// </summary>
    public sealed class JournalAnalysisResult
    {
        public string AtmId { get; set; } = string.Empty;
        public int TotalLines { get; set; }
        public int SuccessCount { get; set; }
        public int DeclinedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int ReversalCount { get; set; }
        public int ErrorCount { get; set; }
        public string Confidence { get; set; } = "Low";
        public string Summary { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public DateTime AnalyzedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
