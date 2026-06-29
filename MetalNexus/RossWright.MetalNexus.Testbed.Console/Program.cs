using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RossWright;
using RossWright.MetalCommand;
using RossWright.MetalInjection;
using RossWright.MetalNexus;
using RossWright.MetalNexus.Testbed.Console;
using RossWright.MetalNexus.Testbed.Console.Commands;
using RossWright.MetalNexus.Testbed.Shared;

var builder = ConsoleApplication.CreateBuilder();

// Runtime logging — available throughout the app lifetime via ILogger<T>
builder.Logging
    .ClearProviders()
    .AddMetalConsoleLogger()
    .SetMinimumLevel(LogLevel.Information);

// HTTP clients
HttpClientFactoryServiceCollectionExtensions
    .AddHttpClient(builder.Services, Microsoft.Extensions.Options.Options.DefaultName,
        client => client.BaseAddress = new Uri("https://localhost:58386"))
    .AddHttpMessageHandler<AuthDelegatingHandler>();

HttpClientFactoryServiceCollectionExtensions
    .AddHttpClient(builder.Services, "connection-b", client =>
    {
        client.BaseAddress = new Uri("https://localhost:58386");
        client.DefaultRequestHeaders.Add("X-Connection", "B");
    })
    .AddHttpMessageHandler<AuthDelegatingHandler>();

// MetalInjection — replaces the default service provider and auto-registers
// classes decorated with [Singleton], [ScopedService], or [TransientService]
builder.AddMetalInjection(options =>
{
    options.ScanAssemblyContaining<Program>();
    // Bootstrap logging captures discovery/registration diagnostics during startup
    options.UseBootstrapLogger(logging =>
    {
        logging.ClearProviders();
        logging.AddMetalConsoleLogger();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
});

builder
    .AddMetalNexusClient(options =>
    {
        options.ScanAssemblyContaining<GetCustomersRequest>();
        options.UseBootstrapLogger(logging =>
        {
            logging.ClearProviders();
            logging.AddMetalConsoleLogger();
            logging.SetMinimumLevel(LogLevel.Debug);
        });
    })
    .AddCommands(commands =>
    {
        commands.ScanAssemblyContaining<TestAllCommand>();
    });

await builder.Build().RunAsync(args);

