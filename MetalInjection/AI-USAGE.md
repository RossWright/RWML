# MetalInjection AI Usage Guide

Use this file when generating code that consumes RossWright.MetalInjection packages.

## Packages

| Package | Use When |
|---|---|
| `RossWright.MetalInjection.Abstractions` | You need registration attributes or marker interfaces in a shared/domain assembly. |
| `RossWright.MetalInjection` | You need the DI container in a console, MetalCommand, or bare `IServiceCollection` scenario. |
| `RossWright.MetalInjection.Blazor` | You are configuring dependency injection in Blazor WebAssembly. |
| `RossWright.MetalInjection.Server` | You are configuring dependency injection in ASP.NET Core. |

## Namespace

Most APIs are in:

```csharp
using RossWright;
```

## Common APIs

| Task | API |
|---|---|
| Register in ASP.NET Core | `builder.AddMetalInjection(options => options.ScanThisAssembly())` |
| Register in Blazor WebAssembly | `builder.AddMetalInjection(options => options.ScanThisAssembly())` |
| Build a standalone provider | `services.BuildMetalInjectionServiceProvider(...)` |
| Register singleton | `[Singleton<TService>]` or `ISingleton<TService>` |
| Register scoped service | `[ScopedService<TService>]` or `IScopedService<TService>` |
| Register transient service | `[TransientService<TService>]` or `ITransientService<TService>` |
| Request property injection | `[Inject]` |
| Bind configuration | `[ConfigSection]` / `[ConfigSection<T>]` |
| Register hosted service | `[HostedService]` |

## Typical Blazor WebAssembly setup

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.AddMetalInjection(options => options.ScanThisAssembly());
await builder.Build().RunAsync();
```

## Typical ASP.NET Core setup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddMetalInjection(options => options.ScanThisAssembly());
var app = builder.Build();
app.Run();
```

## Important notes

- Scan every assembly that contains decorated services.
- Constructor injection works normally; `[Inject]` is for property injection.
- Use the Blazor package in WebAssembly projects and the Server package in ASP.NET Core projects.
- By default, duplicate single-service registrations are treated as configuration errors unless multiple registrations are explicitly allowed.
