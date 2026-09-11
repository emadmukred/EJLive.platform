// UnifiedJournalService.cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Services
{
    /// <summary>
    /// خدمة المجلات الموحدة - Unified Journal Service
    /// توفر وظائف متقدمة لإدارة المجلات وتحليلها
    /// </summary>
    public class UnifiedJournalService
    {
        private readonly string _journalPath;
        private readonly DatabaseManager _database;
        private readonly JournalSyncService _syncService;

        public UnifiedJournalService(string journalPath, DatabaseManager database)
        {
            _journalPath = journalPath;
            _database = database;
            _syncService = new JournalSyncService(database);
        }

    // تحليل المجلات
    public async Task<JournalAnalysisResult> AnalyzeJournalsAsync()
    {
        var result = new JournalAnalysisResult();

        try
        {
            // تحميل المجلات من قاعدة البيانات
            var journals = await _database.GetJournalsAsync();

            foreach (var journal in journals)
            {
                var analysis = new JournalAnalysis;
                {
                    JournalId = journal.EntryId,
                    ATMId = journal.ATMId,
                    FileName = journal.FileName,
                    AnalysisDate = DateTime.UtcNow
                };

            // تحليل خصائص الملف
            analysis.LineCount = journal.TransactionCount;
            analysis.FileSize = journal.OriginalSize;
            analysis.IsCompressed = journal.IsCompressed;
            analysis.IsEncrypted = journal.IsEncrypted;

            result.Entries.Add(analysis);
        }

    result.Summary = GenerateSummary(result.Entries);
    return result;
}
catch (Exception ex)
{
    result.Errors.Add($"خطأ في تحليل المجلات: {ex.Message}");
    return result;
}
}

private JournalSummary GenerateSummary(List<JournalAnalysis> analyses)
{
    return new JournalSummary
    {
        TotalJournals = analyses.Count,
        TotalTransactions = analyses.Sum(a => a.LineCount),
        TotalCompressedFiles = analyses.Count(a => a.IsCompressed),
        TotalEncryptedFiles = analyses.Count(a => a.IsEncrypted),
        TotalFileSize = analyses.Sum(a => a.FileSize)
    };
}

// مزامنة المجلات
public async Task<bool> SyncJournalsAsync(string atmId)
{
    try
    {
        var journalDir = Path.Combine(_journalPath, atmId);
        if (!Directory.Exists(journalDir))
        Directory.CreateDirectory(journalDir);

        var journalFiles = Directory.GetFiles(journalDir, "*.jrn");
        .Concat(Directory.GetFiles(journalDir, "*.ej"))
        .Concat(Directory.GetFiles(journalDir, "*.log"))
        .Concat(Directory.GetFiles(journalDir, "*.dat"));

        foreach (var file in journalFiles)
        {
            await ProcessJournalFileAsync(file, atmId);
        }

    return true;
}
catch
{
    return false;
}
}

private async Task ProcessJournalFileAsync(string filePath, string atmId)
{
    try
    {
        // Use synchronous file read for .NET Framework 4.8 compatibility
        var content = await Task.Run(() => File.ReadAllText(filePath));
        var fileInfo = new FileInfo(filePath);

        var journalEntry = new JournalEntry;
        {
            ATMId = atmId,
            FileName = fileInfo.Name,
            OriginalSize = fileInfo.Length,
            CompressedSize = fileInfo.Length, // سيتم تحديثه عند الضغط
            ReceivedAt = DateTime.UtcNow,
            TransactionCount = content.Split('\n').Length
        };

    await _database.SaveJournalAsync(journalEntry);
}
catch (Exception ex)
{
    AppLogger.Instance.Error($"Error processing journal file {filePath}: {ex.Message}", "Journal");
}
}

// فئات داخلية للتحليل
public class JournalAnalysisResult
{
    public List<JournalAnalysis> Entries { get; set; } = new List<JournalAnalysis>();
    public JournalSummary Summary { get; set; } = new JournalSummary();
    public List<string> Errors { get; set; } = new List<string>();
}

public class JournalAnalysis
{
    public string JournalId { get; set; } = string.Empty;
    public string ATMId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public int LineCount { get; set; }
    public long FileSize { get; set; }
    public bool IsCompressed { get; set; }
    public bool IsEncrypted { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
}

public class JournalSummary
{
    public int TotalJournals { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalCompressedFiles { get; set; }
    public int TotalEncryptedFiles { get; set; }
    public long TotalFileSize { get; set; }
    public int TotalErrors { get; set; }
    public int TotalWarnings { get; set; }
    public int TotalInfo { get; set; }
}
}
public partial class UnifiedJournalService
{
    private readonly string _journalPath;
    private readonly DatabaseManager _database;
    private readonly JournalSyncService _syncService;
    public UnifiedJournalService(string journalPath, DatabaseManager database)
    {
        _journalPath = journalPath;
        _database = database;
        _syncService = new JournalSyncService(database);
    }
public async Task<JournalAnalysisResult> AnalyzeJournalsAsync()
{
    var result = new JournalAnalysisResult();
    try
    {
        // تحميل المجلات من قاعدة البيانات
        var journals = await _database.GetJournalsAsync();
        foreach (var journal in journals)
        {
            var analysis = new JournalAnalysis;
            {
                JournalId = journal.EntryId,
                ATMId = journal.ATMId,
                FileName = journal.FileName,
                AnalysisDate = DateTime.UtcNow
            };
        // تحليل خصائص الملف
        analysis.LineCount = journal.TransactionCount;
        analysis.FileSize = journal.OriginalSize;
        analysis.IsCompressed = journal.IsCompressed;
        analysis.IsEncrypted = journal.IsEncrypted;
        result.Entries.Add(analysis);
    }
result.Summary = GenerateSummary(result.Entries);
return result;
}
catch (Exception ex)
{
    result.Errors.Add($"خطأ في تحليل المجلات: {ex.Message}");
    return result;
}
}
private JournalSummary GenerateSummary(List<JournalAnalysis> analyses)
{
    return new JournalSummary
    {
        TotalJournals = analyses.Count,
        TotalTransactions = analyses.Sum(a => a.LineCount),
        TotalCompressedFiles = analyses.Count(a => a.IsCompressed),
        TotalEncryptedFiles = analyses.Count(a => a.IsEncrypted),
        TotalFileSize = analyses.Sum(a => a.FileSize)
    };
}
public async Task<bool> SyncJournalsAsync(string atmId)
{
    try
    {
        var journalDir = Path.Combine(_journalPath, atmId);
        if (!Directory.Exists(journalDir))
        Directory.CreateDirectory(journalDir);
        var journalFiles = Directory.GetFiles(journalDir, "*.jrn");
        .Concat(Directory.GetFiles(journalDir, "*.ej"))
        .Concat(Directory.GetFiles(journalDir, "*.log"))
        .Concat(Directory.GetFiles(journalDir, "*.dat"));
        foreach (var file in journalFiles)
        {
            await ProcessJournalFileAsync(file, atmId);
        }
    return true;
}
catch
{
    return false;
}
}
private async Task ProcessJournalFileAsync(string filePath, string atmId)
{
    try
    {
        // Use synchronous file read for .NET Framework 4.8 compatibility
        var content = await Task.Run(() => File.ReadAllText(filePath));
        var fileInfo = new FileInfo(filePath);
        var journalEntry = new JournalEntry;
        {
            ATMId = atmId,
            FileName = fileInfo.Name,
            OriginalSize = fileInfo.Length,
            CompressedSize = fileInfo.Length, // سيتم تحديثه عند الضغط
            ReceivedAt = DateTime.UtcNow,
            TransactionCount = content.Split('\n').Length
        };
    await _database.SaveJournalAsync(journalEntry);
}
catch (Exception ex)
{
    AppLogger.Instance.Error($"Error processing journal file {filePath}: {ex.Message}", "Journal");
}
}
public class JournalAnalysisResult
{
    public List<JournalAnalysis> Entries { get; set; } = new List<JournalAnalysis>();
    public JournalSummary Summary { get; set; } = new JournalSummary();
    public List<string> Errors { get; set; } = new List<string>();
}
public class JournalAnalysis
{
    public string JournalId { get; set; } = string.Empty;
    public string ATMId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public int LineCount { get; set; }
    public long FileSize { get; set; }
    public bool IsCompressed { get; set; }
    public bool IsEncrypted { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
}
public class JournalSummary
{
    public int TotalJournals { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalCompressedFiles { get; set; }
    public int TotalEncryptedFiles { get; set; }
    public long TotalFileSize { get; set; }
    public int TotalErrors { get; set; }
    public int TotalWarnings { get; set; }
    public int TotalInfo { get; set; }
}
}
public partial public public class UnifiedJournalService
{
    private readonly string _journalPath;
    private readonly DatabaseManager _database;
    private readonly JournalSyncService _syncService;
    public UnifiedJournalService(string journalPath, DatabaseManager database)
    {
        public class JournalAnalysisResult
        {
            public List<JournalAnalysis> Entries { get; set; }
            public JournalSummary Summary { get; set; }
            public List<string> Errors { get; set; }
            public class JournalAnalysis
            {
                public string JournalId { get; set; }
                public string ATMId { get; set; }
                public string FileName { get; set; }
                public DateTime AnalysisDate { get; set; }
                public int LineCount { get; set; }
                public long FileSize { get; set; }
                public bool IsCompressed { get; set; }
                public bool IsEncrypted { get; set; }
                public int ErrorCount { get; set; }
                public int WarningCount { get; set; }
                public int InfoCount { get; set; }
                public class JournalSummary
                {
                    public int TotalJournals { get; set; }
                    public int TotalTransactions { get; set; }
                    public int TotalCompressedFiles { get; set; }
                    public int TotalEncryptedFiles { get; set; }
                    public long TotalFileSize { get; set; }
                    public int TotalErrors { get; set; }
                    public int TotalWarnings { get; set; }
                    public int TotalInfo { get; set; }
                    public List<JournalAnalysis> Entries { get; set; }
                    public string JournalId { get; set; }
                    public int TotalJournals { get; set; }
                    public async Task<JournalAnalysisResult> AnalyzeJournalsAsync()
                    {
                        private JournalSummary GenerateSummary(List<JournalAnalysis> analyses)
                        {
                            public async Task<bool> SyncJournalsAsync(string atmId)
                            {
                                private async Task ProcessJournalFileAsync(string filePath, string atmId)
                                {
                                }

                            public partial public class JournalAnalysisResult
                            {
                                public List<JournalAnalysis> Entries { get; set; }
                                public JournalSummary Summary { get; set; }
                                public List<string> Errors { get; set; }
                                public class JournalAnalysis
                                {
                                    public string JournalId { get; set; }
                                    public string ATMId { get; set; }
                                    public string FileName { get; set; }
                                    public DateTime AnalysisDate { get; set; }
                                    public int LineCount { get; set; }
                                    public long FileSize { get; set; }
                                    public bool IsCompressed { get; set; }
                                    public bool IsEncrypted { get; set; }
                                    public int ErrorCount { get; set; }
                                    public int WarningCount { get; set; }
                                    public int InfoCount { get; set; }
                                    public class JournalSummary
                                    {
                                        public int TotalJournals { get; set; }
                                        public int TotalTransactions { get; set; }
                                        public int TotalCompressedFiles { get; set; }
                                        public int TotalEncryptedFiles { get; set; }
                                        public long TotalFileSize { get; set; }
                                        public int TotalErrors { get; set; }
                                        public int TotalWarnings { get; set; }
                                        public int TotalInfo { get; set; }
                                        public string JournalId { get; set; }
                                        public int TotalJournals { get; set; }
                                        public async Task<JournalAnalysisResult> AnalyzeJournalsAsync()
                                        {
                                            private JournalSummary GenerateSummary(List<JournalAnalysis> analyses)
                                            {
                                                public async Task<bool> SyncJournalsAsync(string atmId)
                                                {
                                                    private async Task ProcessJournalFileAsync(string filePath, string atmId)
                                                    {
                                                    }

                                                public partial public class JournalAnalysis
                                                {
                                                    public string JournalId { get; set; }
                                                    public string ATMId { get; set; }
                                                    public string FileName { get; set; }
                                                    public DateTime AnalysisDate { get; set; }
                                                    public int LineCount { get; set; }
                                                    public long FileSize { get; set; }
                                                    public bool IsCompressed { get; set; }
                                                    public bool IsEncrypted { get; set; }
                                                    public int ErrorCount { get; set; }
                                                    public int WarningCount { get; set; }
                                                    public int InfoCount { get; set; }
                                                    public class JournalSummary
                                                    {
                                                        public int TotalJournals { get; set; }
                                                        public int TotalTransactions { get; set; }
                                                        public int TotalCompressedFiles { get; set; }
                                                        public int TotalEncryptedFiles { get; set; }
                                                        public long TotalFileSize { get; set; }
                                                        public int TotalErrors { get; set; }
                                                        public int TotalWarnings { get; set; }
                                                        public int TotalInfo { get; set; }
                                                        public List<JournalAnalysis> Entries { get; set; }
                                                        public JournalSummary Summary { get; set; }
                                                        public List<string> Errors { get; set; }
                                                        public int TotalJournals { get; set; }
                                                        public async Task<JournalAnalysisResult> AnalyzeJournalsAsync()
                                                        {
                                                            private JournalSummary GenerateSummary(List<JournalAnalysis> analyses)
                                                            {
                                                                public async Task<bool> SyncJournalsAsync(string atmId)
                                                                {
                                                                    private async Task ProcessJournalFileAsync(string filePath, string atmId)
                                                                    {
                                                                    }

                                                                public partial public class JournalSummary
                                                                {
                                                                    public int TotalJournals { get; set; }
                                                                    public int TotalTransactions { get; set; }
                                                                    public int TotalCompressedFiles { get; set; }
                                                                    public int TotalEncryptedFiles { get; set; }
                                                                    public long TotalFileSize { get; set; }
                                                                    public int TotalErrors { get; set; }
                                                                    public int TotalWarnings { get; set; }
                                                                    public int TotalInfo { get; set; }
                                                                    public List<JournalAnalysis> Entries { get; set; }
                                                                    public JournalSummary Summary { get; set; }
                                                                    public List<string> Errors { get; set; }
                                                                    public string JournalId { get; set; }
                                                                    public string ATMId { get; set; }
                                                                    public string FileName { get; set; }
                                                                    public DateTime AnalysisDate { get; set; }
                                                                    public int LineCount { get; set; }
                                                                    public long FileSize { get; set; }
                                                                    public bool IsCompressed { get; set; }
                                                                    public bool IsEncrypted { get; set; }
                                                                    public int ErrorCount { get; set; }
                                                                    public int WarningCount { get; set; }
                                                                    public int InfoCount { get; set; }
                                                                    public async Task<JournalAnalysisResult> AnalyzeJournalsAsync()
                                                                    {
                                                                        private JournalSummary GenerateSummary(List<JournalAnalysis> analyses)
                                                                        {
                                                                            public async Task<bool> SyncJournalsAsync(string atmId)
                                                                            {
                                                                                private async Task ProcessJournalFileAsync(string filePath, string atmId)
                                                                                {
                                                                                }

                                                                        }
                                                                    public partial public class UnifiedJournalService
                                                                    {
                                                                        private readonly string _journalPath;
                                                                        private readonly DatabaseManager _database;
                                                                        private readonly JournalSyncService _syncService;
                                                                        public UnifiedJournalService(string journalPath, DatabaseManager database)
                                                                        {
                                                                            public class JournalAnalysisResult
                                                                            {
                                                                                public List<JournalAnalysis> Entries { get; set; }
                                                                                public JournalSummary Summary { get; set; }
                                                                                public List<string> Errors { get; set; }
                                                                                public class JournalAnalysis
                                                                                {
                                                                                    public string JournalId { get; set; }
                                                                                    public string ATMId { get; set; }
                                                                                    public string FileName { get; set; }
                                                                                    public DateTime AnalysisDate { get; set; }
                                                                                    public int LineCount { get; set; }
                                                                                    public long FileSize { get; set; }
                                                                                    public bool IsCompressed { get; set; }
                                                                                    public bool IsEncrypted { get; set; }
                                                                                    public int ErrorCount { get; set; }
                                                                                    public int WarningCount { get; set; }
                                                                                    public int InfoCount { get; set; }
                                                                                    public class JournalSummary
                                                                                    {
                                                                                        public int TotalJournals { get; set; }
                                                                                        public int TotalTransactions { get; set; }
                                                                                        public int TotalCompressedFiles { get; set; }
                                                                                        public int TotalEncryptedFiles { get; set; }
                                                                                        public long TotalFileSize { get; set; }
                                                                                        public int TotalErrors { get; set; }
                                                                                        public int TotalWarnings { get; set; }
                                                                                        public int TotalInfo { get; set; }
                                                                                        public async Task<JournalAnalysisResult> AnalyzeJournalsAsync()
                                                                                        {
                                                                                            private JournalSummary GenerateSummary(List<JournalAnalysis> analyses)
                                                                                            {
                                                                                                public async Task<bool> SyncJournalsAsync(string atmId)
                                                                                                {
                                                                                                    private async Task ProcessJournalFileAsync(string filePath, string atmId)
                                                                                                    {
                                                                                                    }

                                                                                                public partial public class JournalAnalysisResult
                                                                                                {
                                                                                                    public List<JournalAnalysis> Entries { get; set; }
                                                                                                    public JournalSummary Summary { get; set; }
                                                                                                    public List<string> Errors { get; set; }
                                                                                                }

                                                                                            public partial public class JournalAnalysis
                                                                                            {
                                                                                                public string JournalId { get; set; }
                                                                                                public string ATMId { get; set; }
                                                                                                public string FileName { get; set; }
                                                                                                public DateTime AnalysisDate { get; set; }
                                                                                                public int LineCount { get; set; }
                                                                                                public long FileSize { get; set; }
                                                                                                public bool IsCompressed { get; set; }
                                                                                                public bool IsEncrypted { get; set; }
                                                                                                public int ErrorCount { get; set; }
                                                                                                public int WarningCount { get; set; }
                                                                                                public int InfoCount { get; set; }
                                                                                            }

                                                                                        public partial public class JournalSummary
                                                                                        {
                                                                                            public int TotalJournals { get; set; }
                                                                                            public int TotalTransactions { get; set; }
                                                                                            public int TotalCompressedFiles { get; set; }
                                                                                            public int TotalEncryptedFiles { get; set; }
                                                                                            public long TotalFileSize { get; set; }
                                                                                            public int TotalErrors { get; set; }
                                                                                            public int TotalWarnings { get; set; }
                                                                                            public int TotalInfo { get; set; }
                                                                                        }

                                                                                }

                                                                            // Class: JournalAnalysis (from 2 sources)
                                                                            public partial class JournalAnalysis
                                                                            {
                                                                            }
                                                                        // Class: JournalAnalysisResult (from 2 sources)
                                                                        public partial class JournalAnalysisResult
                                                                        {
                                                                        }
                                                                    // Class: JournalSummary (from 2 sources)
                                                                    public partial class JournalSummary
                                                                    {
                                                                    }
                                                                // Class: UnifiedJournalService (from 1 sources)
                                                                public partial class UnifiedJournalService
                                                                {
                                                                    // --- Constants & Fields ---
                                                                    private readonly string _journalPath;

                                                                    private readonly DatabaseManager _database;

                                                                    private readonly JournalSyncService _syncService;


                                                                    // --- Constructors ---
                                                                    public UnifiedJournalService(string journalPath, DatabaseManager database)
                                                                    {
                                                                        _journalPath = journalPath;
                                                                        _database = database;
                                                                        _syncService = new JournalSyncService(database);
                                                                    }


                                                                // --- Methods ---
                                                                public async Task<JournalAnalysisResult> AnalyzeJournalsAsync()
                                                                {
                                                                    var result = new JournalAnalysisResult();

                                                                    try
                                                                    {
                                                                        // تحميل المجلات من قاعدة البيانات
                                                                        var journals = await _database.GetJournalsAsync();

                                                                        foreach (var journal in journals)
                                                                        {
                                                                            var analysis = new JournalAnalysis;
                                                                            {
                                                                                JournalId = journal.EntryId,
                                                                                ATMId = journal.ATMId,
                                                                                FileName = journal.FileName,
                                                                                AnalysisDate = DateTime.UtcNow
                                                                            };

                                                                        // تحليل خصائص الملف
                                                                        analysis.LineCount = journal.TransactionCount;
                                                                        analysis.FileSize = journal.OriginalSize;
                                                                        analysis.IsCompressed = journal.IsCompressed;
                                                                        analysis.IsEncrypted = journal.IsEncrypted;

                                                                        result.Entries.Add(analysis);
                                                                    }

                                                                result.Summary = GenerateSummary(result.Entries);
                                                                return result;
                                                            }
                                                        catch (Exception ex)
                                                        {
                                                            result.Errors.Add($"خطأ في تحليل المجلات: {ex.Message}");
                                                            return result;
                                                        }
                                                }

                                            private JournalSummary GenerateSummary(List<JournalAnalysis> analyses)
                                            {
                                                return new JournalSummary
                                                {
                                                    TotalJournals = analyses.Count,
                                                    TotalTransactions = analyses.Sum(a => a.LineCount),
                                                    TotalCompressedFiles = analyses.Count(a => a.IsCompressed),
                                                    TotalEncryptedFiles = analyses.Count(a => a.IsEncrypted),
                                                    TotalFileSize = analyses.Sum(a => a.FileSize)
                                                };
                                        }

                                    public async Task<bool> SyncJournalsAsync(string atmId)
                                    {
                                        try
                                        {
                                            var journalDir = Path.Combine(_journalPath, atmId);
                                            if (!Directory.Exists(journalDir))
                                            Directory.CreateDirectory(journalDir);

                                            var journalFiles = Directory.GetFiles(journalDir, "*.jrn");
                                            .Concat(Directory.GetFiles(journalDir, "*.ej"))
                                            .Concat(Directory.GetFiles(journalDir, "*.log"))
                                            .Concat(Directory.GetFiles(journalDir, "*.dat"));

                                            foreach (var file in journalFiles)
                                            {
                                                await ProcessJournalFileAsync(file, atmId);
                                            }

                                        return true;
                                    }
                                catch
                                {
                                    return false;
                                }
                        }

                    private async Task ProcessJournalFileAsync(string filePath, string atmId)
                    {
                        try
                        {
                            // Use synchronous file read for .NET Framework 4.8 compatibility
                            var content = await Task.Run(() => File.ReadAllText(filePath));
                            var fileInfo = new FileInfo(filePath);

                            var journalEntry = new JournalEntry;
                            {
                                ATMId = atmId,
                                FileName = fileInfo.Name,
                                OriginalSize = fileInfo.Length,
                                CompressedSize = fileInfo.Length, // سيتم تحديثه عند الضغط
                                ReceivedAt = DateTime.UtcNow,
                                TransactionCount = content.Split('\n').Length
                            };

                        await _database.SaveJournalAsync(journalEntry);
                    }
                catch (Exception ex)
                {
                    AppLogger.Instance.Error($"Error processing journal file {filePath}: {ex.Message}", "Journal");
                }
        }


    // --- Nested Classes ---
    public class JournalAnalysisResult
    {
        public List<JournalAnalysis> Entries { get; set; } = new List<JournalAnalysis>();
        public JournalSummary Summary { get; set; } = new JournalSummary();
        public List<string> Errors { get; set; } = new List<string>();
    }

public class JournalAnalysis
{
    public string JournalId { get; set; } = string.Empty;
    public string ATMId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public int LineCount { get; set; }
    public long FileSize { get; set; }
    public bool IsCompressed { get; set; }
    public bool IsEncrypted { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
}

public class JournalSummary
{
    public int TotalJournals { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalCompressedFiles { get; set; }
    public int TotalEncryptedFiles { get; set; }
    public long TotalFileSize { get; set; }
    public int TotalErrors { get; set; }
    public int TotalWarnings { get; set; }
    public int TotalInfo { get; set; }
}

}

public partial class JournalAnalysis
{
}
public partial class JournalAnalysisResult
{
}
public partial class JournalSummary
{
}
}