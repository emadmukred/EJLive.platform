using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EJLive.Core;

namespace EJLive.Business
{
    /// <summary>
    /// Manages file bindings between terminal configurations and their
    /// associated journal/log paths, ensuring consistency across the fleet.
    /// </summary>
    public sealed class UnifiedFileBindingService
    {
        private readonly Dictionary<string, FileBinding> _bindings = new(StringComparer.OrdinalIgnoreCase);

        public FileBinding Register(string terminalId, string sourcePath, string backupPath, string atmType)
        {
            var binding = new FileBinding
            {
                TerminalId = terminalId,
                SourcePath = sourcePath,
                BackupPath = backupPath,
                AtmType = AppConstants.NormalizeATMType(atmType),
                RegisteredAtUtc = DateTime.UtcNow
            };
            _bindings[terminalId] = binding;
            return binding;
        }

        public FileBinding? Resolve(string terminalId)
        {
            _bindings.TryGetValue(terminalId, out var binding);
            return binding;
        }

        public bool ValidatePaths(string terminalId)
        {
            var binding = Resolve(terminalId);
            if (binding == null) return false;

            binding.IsSourceValid = Directory.Exists(binding.SourcePath);
            binding.IsBackupValid = string.IsNullOrWhiteSpace(binding.BackupPath) || Directory.Exists(binding.BackupPath);
            binding.LastValidatedUtc = DateTime.UtcNow;
            return binding.IsSourceValid;
        }

        public IReadOnlyList<FileBinding> GetAll() => _bindings.Values.OrderBy(b => b.TerminalId).ToList();
    }

    public sealed class FileBinding
    {
        public string TerminalId { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string BackupPath { get; set; } = string.Empty;
        public string AtmType { get; set; } = "NCR";
        public bool IsSourceValid { get; set; }
        public bool IsBackupValid { get; set; }
        public DateTime RegisteredAtUtc { get; set; }
        public DateTime LastValidatedUtc { get; set; }
    }
}
