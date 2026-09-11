using EJLive.Core.Engine;
using EJLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EJLive.Server.WinForms.Services
{
    public partial class ArchiveManager
    {
        public void Archive(string atmId, string filePath) { }
        public List<string> GetArchivedFiles(string atmId) => new();
        public string? RetrieveAsText(string archivePath) => null;
    }

    public partial class EJServer
    {
        public bool IsRunning { get; set; }
        public int ActiveConnections { get; set; }
        public void Start() => IsRunning = true;
        public void Stop() => IsRunning = false;
    }

    public partial class EJServerService
    {
        public EJServer? EJServer { get; set; }
        public void Initialize() { }
        public Task StartAsync() { EJServer?.Start(); return Task.CompletedTask; }
    }

    public partial class RemoteControlService
    {
        public Task<bool> SendCommand(string atmId, string command) => Task.FromResult(true);
    }

    public partial class FleetPredictionEngine
    {
    }

    public partial class JournalAnalyticsService
    {
        public List<EjTransaction> Analyze(string filePath) => new();
    }

}
