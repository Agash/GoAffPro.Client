using GoAffPro.Client;
using GoAffPro.Client.Example;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

builder.Services.Configure<GoAffProClientOptions>(builder.Configuration.GetSection("GoAffPro"));
builder.Services.Configure<ExampleOptions>(builder.Configuration.GetSection("Example"));
builder.Services.AddGoAffProClient();
builder.Services.AddHostedService<Worker>();

using IHost host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
