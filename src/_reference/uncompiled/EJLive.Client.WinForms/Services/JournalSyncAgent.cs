using System;
using System.IO;
using System.Security.Cryptography;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Client.WinForms.Services
{
    public sealed class JournalSyncAgent
    {
        private readonly string _atmId;
        private readonly string _atmType;
        private readonly string _sourcePath;
        private readonly JournalSyncStateService _stateService;

        public event Action<string>? OnLog;

        public JournalSyncAgent(string atmId, string atmType, string sourcePath, string serverStateRoot)
        {
            _atmId = string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim();
            _atmType = string.IsNullOrWhiteSpace(atmType) ? string.Empty : atmType.Trim();
            _sourcePath = sourcePath ?? string.Empty;
            _stateService = new JournalSyncStateService(serverStateRoot);
        }

        public void RecordConnected()
        {
            _stateService.UpdateConnectivity(_atmId, _atmType, true);
            _stateService.UpdateHeartbeat(_atmId, _atmType);
            Log("Journal Sync Agent connected.");
        }

        public void RecordDisconnected()
        {
            _stateService.UpdateConnectivity(_atmId, _atmType, false);
            Log("Journal Sync Agent disconnected.");
        }

        public void RecordHeartbeat()
        {
            _stateService.UpdateHeartbeat(_atmId, _atmType);
        }

        public string? RecordDetected(string filePath, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(filePath) || data == null)
                return null;

            string checksum = ComputeChecksum(data);
            _stateService.RecordDetected(_atmId, _atmType, filePath, data.LongLength, checksum);
            Log("Detected journal data: " + Path.GetFileName(filePath));
            return checksum;
        }

        public void RecordSent(string checksum)
        {
            _stateService.RecordSent(_atmId, checksum);
            Log("Journal data sent: " + checksum);
        }

        public void RecordFailure(string checksum, string reason)
        {
            _stateService.RecordFailure(_atmId, _atmType, checksum, reason);
            Log("Journal sync failed: " + reason);
        }

        private string ComputeChecksum(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private void Log(string message)
        {
            OnLog?.Invoke("[JournalSync] " + message);
        }
    }
}
