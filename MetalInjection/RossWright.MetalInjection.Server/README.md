# RossWright.MetalInjection.Server
Copyright (c) 2023-2026 Pross Co.

ASP.NET Core integration for [MetalInjection](https://www.nuget.org/packages/RossWright.MetalInjection/).
Wires MetalInjection's attribute-driven assembly scanning, property injection, hosted-service auto-registration, and MVC controller activation into a `WebApplicationBuilder`.

## Installation

```powershell
dotnet add package RossWright.MetalInjection.Server
```

## Quick Start

In `Program.cs`, replace manual service registration with a single `AddMetalInjection` call on the `WebApplicationBuilder`:

```csharp
using RossWright.MetalInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddMetalInjection(_ => _.ScanThisAssembly());

var app = builder.Build();
// ... configure middleware pipeline ...
app.Run();
```

Services decorated with `[Singleton<T>]`, `[ScopedService<T>]`, `[TransientService<T>]`, or `[ConfigSection]` in any scanned assembly are registered automatically.

## Key Concepts

### MVC Controller Property Injection

`AddMetalInjection` automatically replaces the default controller activator with `MetalInjectionControllerActivator`, which creates a dedicated DI scope per request and runs property injection on each controller instance. No additional setup is required — decorate controller properties with `[Inject]` and they are populated on every request:

```csharp
public class MyController : ControllerBase
{
    [Inject] private IMyService MyService { get; set; } = null!;
}
```

### Hosted Service Auto-Registration

Classes that derive from `BackgroundService` (or implement `IHostedService`) and are decorated with `[HostedService]` are discovered and registered as singleton hosted services automatically:

```csharp
[HostedService]
public class MyBackgroundWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // do work
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

Alternatively, implement the marker interface `IHostedService<T>` instead of the attribute for compile-time verification:

```csharp
public class MyBackgroundWorker : BackgroundService, IHostedService<MyBackgroundWorker>
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) { ... }
}
```

### Configuration Sections

Bind `appsettings.json` sections to POCO classes with `[ConfigSection]` and inject them anywhere:

```csharp
[ConfigSection("MyApp:Database")]
public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
```

Use `[ConfigSection<TInterface>]` to register under an interface, and implement `IValidatingConfigSection` to validate at startup. See the [full MetalInjection documentation](../README.md#configuration-sections) for details.

### Blazor `[Inject]` Compatibility

`AddMetalInjection` on `WebApplicationBuilder` automatically registers `Microsoft.AspNetCore.Components.InjectAttribute` as an alternate inject attribute, so server-side Blazor components can use either `[Inject]` or MetalInjection's own `[Inject]` for property injection.

## API Summary

| Type | Purpose |
|---|---|
| `MetalInjectionServerExtensions.AddMetalInjection` | Wires MetalInjection into `WebApplicationBuilder` |
| `HostedServiceAttribute` | Marks a `BackgroundService` for automatic hosted-service registration |
| `IHostedService<T>` | Marker-interface equivalent of `[HostedService]` |
| `MetalInjectionControllerActivator` | Scoped-per-request controller activator with property injection |

## See Also

- [RossWright.MetalInjection.Abstractions](https://www.nuget.org/packages/RossWright.MetalInjection.Abstractions/) — registration attributes and injection markers
- [RossWright.MetalInjection](https://www.nuget.org/packages/RossWright.MetalInjection/) — core runtime (console / generic-host)
- [RossWright.MetalInjection.Blazor](https://www.nuget.org/packages/RossWright.MetalInjection.Blazor/) — Blazor WebAssembly integration
- [Full MetalInjection documentation](../README.md)
