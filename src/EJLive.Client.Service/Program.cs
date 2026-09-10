using EJLive.Client.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddEventLog(settings =>
{
    settings.SourceName = "EJLive-Client-Source";
    settings.LogName = "Application";
});

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "EJLive Client Agent Service";
});

builder.Services.AddSingleton<RuntimeAgentConfigResolver>();
builder.Services.AddHostedService<ClientAgentWindowsService>();

var host = builder.Build();
await host.RunAsync();
