using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EJLive.Client.Service
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "EJLive Client Agent Service";
            });
            builder.Services.AddHostedService<ClientAgentWorker>();

            using var host = builder.Build();
            await host.RunAsync().ConfigureAwait(false);
        }
    }

    public sealed class ClientAgentWorker : BackgroundService
    {
        private readonly ILogger<ClientAgentWorker> _logger;
        private readonly string _healthPath;

        public ClientAgentWorker(ILogger<ClientAgentWorker> logger)
        {
            _logger = logger;
            _healthPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "Agent",
                "health.json");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EJLive client agent worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await WriteHealthSnapshotAsync(stoppingToken).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
            }
        }

        private async Task WriteHealthSnapshotAsync(CancellationToken cancellationToken)
        {
            var directory = Path.GetDirectoryName(_healthPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var snapshot = new
            {
                Service = "EJLive.Client.Service",
                Status = "Running",
                TimestampUtc = DateTimeOffset.UtcNow,
                Capabilities = new[]
                {
                    "heartbeat",
                    "journal-sync-watch",
                    "policy-governed-command-intake",
                    "health-snapshot"
                }
            };

            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_healthPath, json, cancellationToken).ConfigureAwait(false);
        }
    }
}
