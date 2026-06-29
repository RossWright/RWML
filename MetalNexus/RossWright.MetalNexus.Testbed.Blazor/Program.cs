using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using RossWright;
using RossWright.MetalInjection;
using RossWright.MetalNexus;
using RossWright.MetalNexus.Testbed.Blazor;
using RossWright.MetalNexus.Testbed.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Runtime logging — available throughout the app lifetime via ILogger<T>
builder.Logging
    .ClearProviders()
    .AddMetalConsoleLogger()
    .SetMinimumLevel(LogLevel.Information);

// ── Resolve API base URL from appsettings.json (wwwroot) or fall back to host ─
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

// ── MetalInjection — replaces the default service provider and auto-registers  ─
// classes decorated with [Singleton], [ScopedService], or [TransientService]
builder.AddMetalInjection(options =>
{
    options.ScanAssemblyContaining<Program>();
    // Bootstrap logging captures discovery/registration diagnostics during startup
    options.UseBootstrapLogger(logging =>
    {
        logging.ClearProviders();
        logging.AddDebug();
        logging.AddMetalConsoleLogger();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
});

// ── Register MetalNexus client (sets up the default IHttpClientFactory entry) ─
builder
    .AddHttpClient()
    .AddMetalNexusClient(options =>
    {
        options.ScanAssemblyContaining<GetCustomersRequest>();
        options.UseBootstrapLogger(logging =>
        {
            logging.ClearProviders();
            logging.AddDebug();
            logging.AddMetalConsoleLogger();
            logging.SetMinimumLevel(LogLevel.Debug);
        });
    });

// ── Re-wrap the default HttpClient to inject the auth handler ─────────────────
// MetalNexus resolves HttpClient by name Options.DefaultName (""); we override
// that same registration to add our delegating handler on top.
HttpClientFactoryServiceCollectionExtensions
    .AddHttpClient(builder.Services, Options.DefaultName, client =>
        client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthDelegatingHandler>();

await builder.Build().RunAsync();

