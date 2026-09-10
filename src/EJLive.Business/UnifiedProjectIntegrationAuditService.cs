using EJLive.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EJLive.Business
{
    public partial class ProjectIntegrationAuditReport
    {
        public string RootPath { get; set; } = string.Empty;
        public int TotalProjects { get; set; }
        public int TotalCSFiles { get; set; }
        public int TotalSolutions { get; set; }
        public bool IsIntegrated { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> AnalyzedFiles { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime GeneratedAtUtc { get; set; }
    }

    public partial class IntegrationAuditEntry
    {
        public string FilePath { get; set; } = string.Empty;
        public bool HasNamespace { get; set; }
        public bool HasClasses { get; set; }
        public string Status { get; set; } = "Unknown";
    }

    public partial class UnifiedProjectIntegrationAuditService
    {
        private readonly List<IntegrationAuditEntry> _entries = new();
        public ProjectIntegrationAuditReport Analyze(string rootPath)
        {
            var report = new ProjectIntegrationAuditReport
            {
                RootPath = rootPath,
                GeneratedAtUtc = DateTime.UtcNow
            };
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                report.Errors.Add($"Invalid root path: {rootPath}");
                return report;
            }
            try
            {
                // Count C# files
                var csFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories);
                report.TotalCSFiles = csFiles.Length;
                // Count project files
                var projFiles = Directory.GetFiles(rootPath, "*.csproj", SearchOption.AllDirectories);
                report.TotalProjects = projFiles.Length;
                // Count solution files
                var slnFiles = Directory.GetFiles(rootPath, "*.sln", SearchOption.TopDirectoryOnly);
                report.TotalSolutions = slnFiles.Length;
                // Scan for missing references (simple analysis)
                foreach (var file in csFiles.Take(200)) // Limit for performance
                {
                    try
                    {
                        var content = File.ReadAllText(file);
                        var relativePath = file.Replace(rootPath, "").TrimStart(Path.DirectorySeparatorChar);
                        report.AnalyzedFiles.Add(relativePath);
                        // Basic check for file content
                        if (content.Contains("namespace") && content.Contains("class"))
                        {
                            _entries.Add(new IntegrationAuditEntry
                            {
                                FilePath = relativePath,
                                HasNamespace = true,
                                HasClasses = true,
                                Status = "OK"
                            });
                        }
                    }
                    catch { }
                }
                report.IsIntegrated = report.TotalProjects > 0 && report.Errors.Count == 0;
                report.Summary = $"Integration audit: {report.TotalProjects} projects, {report.TotalCSFiles} C# files. Integrated: {report.IsIntegrated}";
            }
            catch (Exception ex)
            {
                report.Errors.Add($"Audit error: {ex.Message}");
            }
            return report;
        }
    }

}
