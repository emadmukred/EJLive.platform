using EJLive.Client.Service;
using EJLive.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddEventLog(settings =>
{
    settings.SourceName = "EJLive-Client-Source";
    settings.LogName = "Application";
});

// SS-20 bootstrap — headless variant: dataroot, directories, configuration, database and
// the schema book are prepared before any hosted service starts. A failing step exits the
// process with the actionable line reported (SS-14); the Windows service recovery ladder
// (1 s / 5 s / 60 s per SS-16) restarts the agent once the root is fixed, which is safer
// than a worker spinning against a broken database.
var bootstrap = await new PlatformBootstrap().RunAsync().ConfigureAwait(false);
foreach (var line in bootstrap.ReportLines())
    Console.WriteLine(line);
if (!bootstrap.Success)
{
    Console.Error.WriteLine("EJLive client service startup aborted: " + bootstrap.FailureDetail);
    return 1;
}

builder.Services.AddSingleton<RuntimeAgentConfigResolver>();
builder.Services.AddHostedService<ClientAgentWindowsService>();

var host = builder.Build();
await host.RunAsync();
return 0;
