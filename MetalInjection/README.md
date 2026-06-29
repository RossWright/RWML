# Ross Wright's MetalInjection Library
Copyright (c) 2023-2026 Pross Co.

## Table of Contents
- [Overview](#overview)
- [Packages](#packages)
- [Namespaces](#namespaces)
- [Common APIs](#common-apis)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Registration](#registration)
- [Injection](#injection)
- [Activation](#activation)
- [Configuration Sections](#configuration-sections)
- [ASP.NET Core Hosted Services](#asp.net-core-hosted-services)
- [Disposal](#disposal)
- [Esoterica](#esoterica)
  - [SetEntryAssembly](#setentryassembly)
  - [Ignore](#ignore)
  - [SetStrictResolution](#setstrictresolution)
  - [SetThrowExceptionOnError](#setthrowexceptiononerror)
  - [AllowRootScopedResolution](#allowrootscopedresolution)
  - [[AllowRootResolution]](#allowrootresolution)
  - [Covariant Generic Resolution](#covariant-generic-resolution)
  - [Open-Generic Factory Registration](#open-generic-factory-registration)
  - [Bootstrap Logging](#bootstrap-logging)
- [See Also](#see-also)
- [License](#license)
- [Changelog](CHANGELOG.txt)

## Overview

MetalInjection replaces manual `services.AddSingleton/Scoped/Transient` calls with attribute-driven registration — just decorate your class and MetalInjection discovers it automatically during an assembly scan. On top of that it adds property injection for any environment (not just Blazor), `appsettings.json` configuration binding and registration in one step, and deterministic disposal of every `IDisposable` and `IAsyncDisposable` regardless of lifetime — something the standard .NET container doesn't do for singletons.

| Feature | Description |
|---|---|
| Attribute registration | Decorate classes with `[Singleton<T>]`, `[ScopedService<T>]`, or `[TransientService<T>]` — no manual wiring required |
| Interface registration | Alternatively implement `ISingleton<T>`, `IScopedService<T>`, or `ITransientService<T>` for compile-time service-type checking |
| Property injection | `[Inject]` properties populated after construction in any project type, not just Blazor |
| Configuration binding | `[ConfigSection]` binds and registers an options class from `appsettings.json` in one step |
| Deterministic disposal | All `IDisposable`/`IAsyncDisposable` instances — including singletons — are disposed when the container or scope tears down |
| Keyed services | Full support for .NET keyed service registration and injection |
| MVC controller injection | Property injection on ASP.NET Core controllers wired automatically |
| Assembly scanning | Scan one or more assemblies; discovered types are registered without any per-class wiring |

---

## Packages

The library ships three packages. The Abstractions package is included transitively — most projects don't need to reference it directly.

| Package | NuGet | Description |
|---|---|---|
| `RossWright.MetalInjection.Server` | [NuGet](https://www.nuget.org/packages/RossWright.MetalInjection.Server) | ASP.NET Core — `AddMetalInjection` on `WebApplicationBuilder`, controller activator, hosted-service registration |
| `RossWright.MetalInjection.Blazor` | [NuGet](https://www.nuget.org/packages/RossWright.MetalInjection.Blazor) | Blazor WebAssembly — `AddMetalInjection` on `WebAssemblyHostBuilder` |
| `RossWright.MetalInjection` | [NuGet](https://www.nuget.org/packages/RossWright.MetalInjection) | MetalCommand console apps and any other .NET project using `IServiceProvider` |

---

## Installation

Add the package that matches your project type:

```powershell
# ASP.NET Core
dotnet add package RossWright.MetalInjection.Server
```

```powershell
# Blazor WebAssembly
dotnet add package RossWright.MetalInjection.Blazor
```

```powershell
# MetalCommand console / other .NET projects
dotnet add package RossWright.MetalInjection
```

---

## Quick Start

The example below shows an ASP.NET Core server. Call `AddMetalInjection` on `WebApplicationBuilder` and point it at your assembly — everything decorated with a registration attribute is registered automatically.

```csharp
// Program.cs — ASP.NET Core
var builder = WebApplication.CreateBuilder(args);
builder.AddMetalInjection(_ => _.ScanThisAssembly());

var app = builder.Build();
app.Run();
```

Decorate your service implementation — no manual `services.Add...` call needed:

```csharp
[Singleton<IEmailService>]
public class EmailService(IConfiguration config) : IEmailService
{
    public Task SendAsync(string to, string subject, string body) => ...;
}
```

Inject it anywhere via constructor or property:

```csharp
public class WelcomeHandler(IEmailService _email) : IRequestHandler<SendWelcome>
{
    public async Task Handle(SendWelcome request, CancellationToken ct)
        => await _email.SendAsync(request.To, "Welcome!", "Thanks for signing up.");
}
```

For Blazor WebAssembly, the call is the same shape on `WebAssemblyHostBuilder`:

```csharp
// Program.cs — Blazor WASM
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.AddMetalInjection(_ => _.ScanThisAssembly());
await builder.Build().RunAsync();
```

For a MetalCommand console app, use `IConsoleApplicationBuilder`:

```csharp
// Program.cs — MetalCommand
var builder = ConsoleApplication.CreateBuilder(args);
builder.AddMetalInjection(_ => _.ScanThisAssembly());
await builder.Build().RunAsync();
```

> **Note:** For non-host projects that work with a bare `IServiceCollection`, call `BuildMetalInjectionServiceProvider` instead of `BuildServiceProvider`. See [Advanced Registration](#advanced-registration) in Esoterica.

---

## Registration

Telling MetalInjection about a class takes one of two forms: decorating it with a registration attribute, or implementing a marker interface. Both are discovered automatically during the assembly scan — no manual `services.Add...` call is needed.

### Attributes vs. Interfaces

The attribute form is the most common:

```csharp
[Singleton<IEmailService>]
public class EmailService : IEmailService { ... }

[ScopedService<IOrderRepository>]
public class OrderRepository : IOrderRepository { ... }

[TransientService<IPdfRenderer>]
public class PdfRenderer : IPdfRenderer { ... }
```

The interface form is an alternative that gives you a compile-time guarantee that the class actually implements the service type it claims to provide:

```csharp
public class EmailService : IEmailService, ISingleton<IEmailService> { ... }
```

With the attribute form, the same guarantee is enforced at startup — MetalInjection throws if the decorated class doesn't implement the specified service type. Choose whichever form keeps the declaration closest to the code you're reading; there's no behavioral difference.

| Attribute | Interface | Lifetime |
|---|---|---|
| `[Singleton<T>]` | `ISingleton<T>` | Single instance for the lifetime of the container |
| `[ScopedService<T>]` | `IScopedService<T>` | One instance per DI scope |
| `[TransientService<T>]` | `ITransientService<T>` | New instance on every resolution |

### Registering Under Multiple Service Types

Stack multiple attributes to expose one implementation under several interfaces. MetalInjection ensures that all stacked singletons and scoped registrations resolve to the same shared instance — the class is only constructed once per appropriate lifetime boundary.

```csharp
[Singleton<IMyReadService>]
[Singleton<IMyWriteService>]
public class MyService : IMyReadService, IMyWriteService { ... }
```

Transient registrations are the exception: each resolution produces a new instance regardless of which interface was requested.

### Scanning Assemblies

`ScanThisAssembly()` discovers all decorated types in the calling assembly. When your registrations are spread across multiple assemblies — for example, a shared contracts project — scan each one explicitly:

```csharp
builder.AddMetalInjection(_ =>
{
    _.ScanThisAssembly();
    _.ScanAssembly(typeof(SomeTypeInAnotherAssembly).Assembly);
});
```

### Allowing Multiple Implementations

By default MetalInjection enforces one implementation per service type and throws at startup if a duplicate is detected. When you genuinely need multiple implementations of the same interface — for example, a collection of event handlers — you have two options.

Apply `[AllowMultipleRegistrations]` directly to the interface declaration:

```csharp
[AllowMultipleRegistrations]
public interface IEventHandler { ... }
```

Or opt in per-type at registration time:

```csharp
builder.AddMetalInjection(_ =>
{
    _.ScanThisAssembly();
    _.AllowMultipleServicesOf<IEventHandler>();
});
```

Either way, resolve the full set via `IEnumerable<IEventHandler>` — resolving a single instance when multiple are registered remains an error.

> **Note:** For keyed service registration, open-generic factory registration, and global duplicate suppression via `AllowMultipleServicesOfAnyType`, see [Esoterica](#esoterica).

---
## Injection

Constructor injection works exactly as it does in standard .NET DI — no attributes, no configuration. MetalInjection also adds property injection for any project type (not just Blazor) and optional injection driven by nullability, so you can express intent in the type system rather than with try/catch blocks.

### Constructor Injection

Declare dependencies as constructor parameters and they're resolved from the container automatically:

```csharp
public class OrderService(IOrderRepository _repo, IEmailService _email) : IOrderService
{
    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        await _repo.SaveAsync(order, ct);
        await _email.SendConfirmationAsync(order.CustomerEmail, ct);
    }
}
```

### Property Injection

Property injection is useful when you can't or don't want to add a constructor parameter — for example, in base classes, in types activated by a framework that controls construction, or for optional dependencies that would clutter the constructor signature. Decorate any settable property with `[Inject]` and MetalInjection populates it after construction:

```csharp
public class ReportGenerator
{
    [Inject] private IReportFormatter _formatter { get; set; } = null!;

    public string Generate(ReportData data) => _formatter.Format(data);
}
```

Both `RossWright.MetalInjection.InjectAttribute` and Blazor's `Microsoft.AspNetCore.Components.InjectAttribute` are recognized — use whichever is already imported.

> **Note:** Property injection runs after the constructor. If you need a dependency inside the constructor body, use constructor injection instead.

#### MVC Controllers

When you call `AddMetalInjection` on a `WebApplicationBuilder`, MetalInjection automatically replaces the default controller activator with `MetalInjectionControllerActivator`. All ASP.NET Core controllers get property injection with no extra wiring.

### Optional Injection

Mark a constructor parameter or property as nullable and MetalInjection treats it as optional — the dependency is injected if registered, and `null` is passed if it isn't:

```csharp
// constructor — optional parameter
public class NotificationService(ISlackClient? slackClient = null) { ... }

// property — optional property
[Inject] private IAuditLogger? _audit { get; set; }
```

You can override nullability inference explicitly using the `Optional` property on `[Inject]`:

```csharp
// force optional even though the type is non-nullable
[Inject(Optional = true)] private IFallbackService _fallback { get; set; } = null!;

// force required even though the type is nullable
[Inject(Optional = false)] private IRequiredService? _svc { get; set; }
```

### Keyed Service Injection

Standard .NET keyed services are fully supported. Register a keyed service as normal, then inject it by passing the key to `[Inject]`:

```csharp
// registration (standard .NET keyed service API)
services.AddKeyedSingleton<IMessageService, SmsService>("sms");
services.AddKeyedSingleton<IMessageService, EmailService>("email");

// constructor injection — use [FromKeyedServices] or [Inject("key")]
public class NotificationService(
    [Inject("sms")] IMessageService _sms,
    [Inject("email")] IMessageService _email) { ... }

// property injection — same key syntax
[Inject("sms")] private IMessageService _sms { get; set; } = null!;
```

### Injecting All Implementations

When multiple implementations of the same service type are registered (see [Allowing Multiple Implementations](#allowing-multiple-implementations) in Registration), inject `IEnumerable<T>` to receive all of them:

```csharp
public class EventDispatcher(IEnumerable<IEventHandler> _handlers)
{
    public async Task DispatchAsync(Event e, CancellationToken ct)
    {
        foreach (var handler in _handlers)
            await handler.HandleAsync(e, ct);
    }
}
```

The collection is empty if no implementations are registered — it's never null.

---

## Activation

`Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance` creates objects outside the normal DI resolution path, so MetalInjection can't intercept the call to run property injection. Use the MetalInjection equivalents instead and `[Inject]` properties are populated automatically.

### Using MetalInjection's ActivatorUtilities

Drop in `RossWright.MetalInjection.ActivatorUtilities` anywhere you'd use the BCL version. A global using alias keeps the call sites clean:

```csharp
// add once — typically in Program.cs or a global usings file
global using ActivatorUtilities = RossWright.MetalInjection.ActivatorUtilities;
```

Then call it as normal — property injection happens transparently:

```csharp
var instance = ActivatorUtilities.CreateInstance<MyService>(serviceProvider);
```

### IServiceProvider Extension

The `CreateInstance<T>` extension on `IServiceProvider` is a convenient alternative when you have a provider reference and need to pass extra non-injected arguments:

```csharp
[ScopedService<IMyService>]
public class MyService(ISomeInjectedService _svc, int timeout) : IMyService { ... }

// timeout is passed explicitly; ISomeInjectedService is resolved from the provider
var instance = serviceProvider.CreateInstance<MyService>(30);
```

### Manual Property Injection

When an object is created by code you don't control — a third-party framework, a reflection-based factory — inject `IMetalInjectionServiceProvider` and call `InjectProperties` directly:

```csharp
public class FrameworkActivatedType
{
    public FrameworkActivatedType(IMetalInjectionServiceProvider serviceProvider)
    {
        serviceProvider.InjectProperties(this);
    }

    [Inject] private INeededService _neededSvc { get; set; } = null!;
}
```

You can also call `serviceProvider.InjectProperties(obj)` from outside the class if you hold a reference to the instance after construction.

---
## Configuration Sections

Instead of writing `services.Configure<MyOptions>(config.GetSection("MySection"))` for every options class, decorate the class with `[ConfigSection]` and MetalInjection binds it to the matching `appsettings.json` section and registers it as a singleton automatically — no manual wiring needed.

### Basic Usage

Decorate a class with `[ConfigSection]` and give it the configuration section path. The class is bound to that section and registered as a singleton injectable by its concrete type:

```csharp
[ConfigSection("MyApp:Database")]
public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeoutSeconds { get; set; } = 30;
}
```

```json
// appsettings.json
{
  "MyApp": {
    "Database": {
      "ConnectionString": "Server=...;Database=MyDb",
      "CommandTimeoutSeconds": 60
    }
  }
}
```

Inject it by its concrete type as normal:

```csharp
public class MyRepository(DatabaseSettings settings) { ... }
```

### Registering as an Interface

Use the generic form of the attribute to register the settings class under an interface type rather than the concrete class. The class must implement the specified interface or a run-time exception is thrown on startup:

```csharp
public interface IDatabaseSettings
{
    string ConnectionString { get; }
    int CommandTimeoutSeconds { get; }
}

[ConfigSection<IDatabaseSettings>("MyApp:Database")]
public class DatabaseSettings : IDatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeoutSeconds { get; set; } = 30;
}
```

Inject by the interface:

```csharp
public class MyRepository(IDatabaseSettings settings) { ... }
```

### Validation

Incorrect configuration is easier to diagnose when the application refuses to start than when it fails at runtime. Implement `IValidatingConfigSection` on your settings class and throw from `ValidateOrDie()` for any invalid value — MetalInjection calls it immediately after binding, before any services are resolved:

```csharp
[ConfigSection<IDatabaseSettings>("MyApp:Database")]
public class DatabaseSettings : IDatabaseSettings, IValidatingConfigSection
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeoutSeconds { get; set; } = 30;

    public void ValidateOrDie()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("MyApp:Database:ConnectionString is required.");
        if (CommandTimeoutSeconds <= 0)
            throw new InvalidOperationException("MyApp:Database:CommandTimeoutSeconds must be positive.");
    }
}
```

### Non-Hosted Projects

When using `BuildMetalInjectionServiceProvider` directly (see [Advanced Registration](#advanced-registration) in Esoterica), pass your `IConfiguration` instance to enable config section binding:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var serviceProvider = serviceCollection
    .BuildMetalInjectionServiceProvider(
        options => options.ScanThisAssembly(),
        configuration: configuration);
```

Without a configuration instance, `[ConfigSection]` attributes are ignored and no settings classes are bound.

### Advanced: Multiple Sections on One Class

A single class can carry multiple `[ConfigSection]` attributes. All sections are bound to the same instance in the order the attributes appear — useful when a settings object aggregates values from several configuration paths, or needs to be injectable by multiple interface types:

```csharp
[ConfigSection<IFeatureFlags>("MyApp:Features")]
[ConfigSection<ILimitsConfig>("MyApp:Limits")]
public class AppPolicySettings : IFeatureFlags, ILimitsConfig, IValidatingConfigSection
{
    // bound from MyApp:Features
    public bool EnableDarkMode { get; set; }
    // bound from MyApp:Limits
    public int MaxUploadSizeMb { get; set; }

    public void ValidateOrDie()
    {
        if (MaxUploadSizeMb <= 0)
            throw new InvalidOperationException("MyApp:Limits:MaxUploadSizeMb must be positive.");
    }
}
```

Both `IFeatureFlags` and `ILimitsConfig` resolve to the same `AppPolicySettings` instance.

> **Note:** Configuration binding is not available in Blazor WebAssembly projects — the WebAssembly runtime doesn't support the configuration binding APIs MetalInjection uses. `[ConfigSection]` attributes are silently ignored when using `AddMetalInjection` on a `WebAssemblyHostBuilder`.

---
## ASP.NET Core Hosted Services

MetalInjection.Server can automatically register [ASP.NET Core hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) from scanned assemblies, removing the need to call `services.AddHostedService<T>()` manually for each one.

### Basic Usage

Implement `IHostedService` (or derive from `BackgroundService`) as you normally would, then decorate the class with `[HostedService]`:

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

Alternatively, you can use the `IHostedService<T>` marker interface instead of the attribute. This is equivalent to `[HostedService]` and provides compile-time confirmation that your class implements `IHostedService`:

```csharp
public class MyBackgroundWorker : BackgroundService, IHostedService<MyBackgroundWorker>
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

As long as the class is in a scanned assembly, `AddMetalInjection` will register it automatically as a singleton hosted service. No further setup is needed.

### Requirements

Both conditions must be true for a class to be picked up:

1. The class has the `[HostedService]` attribute.
2. The class implements `Microsoft.Extensions.Hosting.IHostedService` (directly or via `BackgroundService`).

A class with `[HostedService]` that does not implement `IHostedService` is silently ignored.

### Injection

Because hosted services are registered as singletons, constructor injection works normally. Inject only singleton or transient services — injecting scoped services into a hosted service is a captive dependency and will throw at startup:

```csharp
[HostedService]
public class ReportingWorker(ILogger<ReportingWorker> logger, IReportingConfig config) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ...
    }
}
```

If you need to consume a scoped service from inside a hosted service, inject `IServiceScopeFactory` and create a scope within `ExecuteAsync`:

```csharp
[HostedService]
public class DataSyncWorker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
            await repo.SyncAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

---
## Disposal

MetalInjection tracks all resolvable instances for deterministic disposal. Both `IDisposable` and `IAsyncDisposable` are fully supported across all lifetimes and scope boundaries.

### What is tracked

| Lifetime | Tracked where | Disposed when |
|---|---|---|
| **Singleton** | Root provider | Root provider is disposed (`Dispose()` or `DisposeAsync()`) |
| **Scoped** | The scope that created it | That scope is disposed |
| **Transient** (`IDisposable` only) | The provider/scope that resolved it | That provider/scope is disposed |
| **Transient** (`IAsyncDisposable` only or dual) | The provider/scope that resolved it | That provider/scope is disposed |

Transients resolved from the root provider are tracked by the root and disposed when the root is disposed. Transients resolved from a scope are tracked by that scope and disposed when the scope is disposed — they are never held by the root.

### Sync vs async disposal paths

When `IDisposable.Dispose()` is called:
- Instances that implement only `IDisposable` — `Dispose()` is called.
- Instances that implement only `IAsyncDisposable` — `DisposeAsync().GetAwaiter().GetResult()` is called (sync-over-async, matching the BCL pattern).
- Instances that implement **both** `IDisposable` and `IAsyncDisposable` — `Dispose()` is called (the sync path is preferred on the sync disposal path).

When `IAsyncDisposable.DisposeAsync()` is called (preferred for async code — use `await using`):
- Instances that implement `IAsyncDisposable` (alone or dual) — `DisposeAsync()` is called.
- Instances that implement only `IDisposable` — `Dispose()` is called.

```csharp
// Sync disposal
using var scope = provider.CreateScope();
// ...use scope...
// scope.Dispose() fires here — all transients and scoped instances disposed

// Async disposal (preferred)
await using var scope = (IAsyncDisposable)provider.CreateScope();
// ...use scope...
// scope.DisposeAsync() fires here
```

---
## Esoterica
The above covers 90% of your typical usage of MetalInjection.

---
### SetEntryAssembly

```csharp
builder.AddMetalInjection(_ =>
{
    _.ScanThisAssembly();
    _.SetEntryAssembly(typeof(MyEntryPoint).Assembly);
});
```

When multiple assemblies are scanned and two or more of them register an implementation for the same service type without keys, MetalInjection uses the **entry assembly** as a tiebreaker in permissive mode (see [SetStrictResolution](#setstrictresolution) below): the implementation from the entry assembly wins and the others are silently dropped with a warning log.

By default the entry assembly is detected via `Assembly.GetEntryAssembly()`. This works correctly for most application types, but it can return `null` or the wrong assembly in unit test runners, generic host scenarios, or when a library is acting as the application root. Call `SetEntryAssembly` to supply the correct assembly explicitly in those cases.

`SetEntryAssembly` has no effect when `SetStrictResolution(true)` has been called, because strict mode never performs dominance resolution — it treats all conflicts as errors immediately.

---

### Ignore

```csharp
builder.AddMetalInjection(_ =>
{
    _.ScanThisAssembly();
    _.Ignore<MyConflictingService>();
    // or by type object:
    _.Ignore(typeof(MyConflictingService));
});
```

Excludes a type from the reflection scan entirely. An ignored type will not be registered even if it carries `[Singleton<>]`, `[ScopedService<>]`, `[TransientService<>]`, `[HostedService]`, or `[ConfigSection]` attributes. Use this when a type in a scanned assembly has registration attributes that conflict with your application's needs and you cannot modify the type directly.

---

### SetStrictResolution

```csharp
builder.AddMetalInjection(_ =>
{
    _.ScanThisAssembly();
    _.SetStrictResolution(); // enables strict mode
});
```

Controls how MetalInjection handles ambiguity. The default is **permissive mode** (`false`). Strict mode (`true`) makes every ambiguity an error.

| Scenario | Permissive (default) | Strict |
|---|---|---|
| Multiple non-keyed implementations for the same service type, startup | Attempts entry-assembly dominance; warns and suppresses losers if one winner found; errors if no winner | Errors immediately — no dominance check attempted |
| Multiple registered implementations at resolution time and AllowMultipleServices was not called in initialization | Logs a warning and uses the first implementation that constructs successfully | Logs an error and throws |
| Service fails to instantiate (no satisfiable constructor) | Silently skips that descriptor and tries the next | Logs an error and throws |

---

### SetThrowExceptionOnError

```csharp
builder.AddMetalInjection(_ =>
{
    _.ScanThisAssembly();
    _.SetThrowExceptionOnError(false); // disable throw-on-error
});
```

Controls whether detected errors throw exceptions (`true`, the default) or are only logged and silently skipped (`false`). This setting applies to **non-fatal** errors — the situations where MetalInjection can continue past the problem by skipping the offending registration or service. Some errors always throw regardless of this setting (see table below).

All exceptions thrown by MetalInjection are `MetalInjectionException`.

---

### AllowRootScopedResolution

By default, resolving a scoped service directly from the root `IServiceProvider` (i.e., outside of any explicit `IServiceScope`) is an error (see the *"A scoped service is requested directly from the root service provider"* row in the `SetThrowExceptionOnError` table above).

To suppress this guard **globally** for all scoped services, call `AllowRootScopedResolution()` during setup:

```csharp
builder.AddMetalInjection(_ =>
{
    _.ScanThisAssembly();
    _.AllowRootScopedResolution();
});
```

> **Caution:** Disabling the root-scope guard globally allows scoped services to escape their intended lifetime. Use this only when you fully understand the lifecycle implications — for example, in test harnesses or CLI tools where a single ambient scope covers the entire run.

### [AllowRootResolution]

When you only need to exempt a **specific type** rather than all scoped services, apply `[AllowRootResolution]` to the implementation class instead of calling `AllowRootScopedResolution()` globally:

```csharp
[ScopedService<IMyService>]
[AllowRootResolution]
public class MyService : IMyService { ... }
```

With this attribute, `MyService` can be resolved from the root provider without error while the root-scope guard remains active for all other scoped services.

**When to prefer each approach:**

| Scenario | Recommended option |
|---|---|
| A single known type needs root-scope access | `[AllowRootResolution]` on the implementation class |
| Many types need root-scope access, or you own the interface only | `AllowRootScopedResolution()` globally |
| Test harness / CLI with no real scope boundary | `AllowRootScopedResolution()` globally |

> **Note:** `[AllowRootResolution]` is a no-op on singleton and transient services — the guard never fires for those lifetimes.

---

#### Errors controlled by `SetThrowExceptionOnError`

The following errors only throw when `ThrowOnError` is `true` (the default):

| When | Circumstances |
|---|---|
| **Startup** | A class decorated with `[Singleton<T>]`, `[ScopedService<T>]`, or `[TransientService<T>]` does not actually implement `T`. The registration is skipped. |
| **Startup** | Multiple non-keyed implementations are registered for the same service type and the conflict cannot be resolved (either strict mode is on, or no single entry-assembly winner exists). All conflicting registrations are skipped. |
| **Startup** | A class decorated with `[ConfigSection<T>]` does not implement the interface `T`. The registration is skipped. |
| **Resolution** | A scoped service is requested directly from the root service provider (i.e., outside of any scope). |
| **Resolution** | A singleton service's constructor requires a scoped service (captive dependency). The singleton fails to instantiate. |
| **Resolution** | Strict mode only: `GetService` is called for a type that has multiple registrations. |
| **Resolution** | Strict mode only: A registered service cannot be instantiated because no constructor can be satisfied. |

#### Errors that always throw

The following always throw regardless of the `ThrowOnError` setting:

| When | Circumstances |
|---|---|
| **Resolution** | `GetRequiredService` or `GetRequiredKeyedService` is called and no service is registered for the requested type. |
| **Resolution** | A `ServiceDescriptor` exists in the container with no implementation type, implementation instance, or implementation factory — this indicates a malformed registration. |

---

### Covariant Generic Resolution

By default, DI registrations are exact: registering `IValidator<Animal>` will not satisfy a request for `IValidator<Dog>`, even though `Dog` derives from `Animal`. MetalInjection's covariant resolution lets a single "wider" registration satisfy requests for "narrower" closed types — including generics with multiple type arguments.

Opt in by setting the `CovariantResolution` property on any registration attribute to one of two strategies from the `Covariance` enum:

| Value | Behaviour |
|---|---|
| `Covariance.Disabled` | Default. Exact match only — identical to standard .NET DI. |
| `Covariance.Covariant` | Every type-argument position uses `IsAssignableFrom`. Registered type is the base; requested type is the same or more derived. Works on any interface, including invariant ones with no `out`/`in` annotations. |
| `Covariance.HonorInOut` | Per-position rules driven by the CLR `out`/`in` annotations on the interface's type parameters. Use when the interface already carries correct variance annotations. |

**General matching rules (both modes):**

- An **exact match** is always preferred over a covariant match. If `IValidator<Dog>` is registered directly, that registration wins regardless of any covariant registrations.
- If more than one covariant registration is a valid match for the requested type, MetalInjection throws a `MetalInjectionException` — the ambiguity cannot be resolved automatically.
- Covariant resolution applies to the type arguments only; the open generic type itself (`IValidator<>`) must match exactly.

---

#### `Covariance.Covariant`

Every type-argument position is treated as covariant: the registered type is the base and the requested type must be the same as or derive from it. This works on any interface regardless of whether it has `out`/`in` annotations.

**Single type argument:**

```csharp
public interface IValidator<T> { }

[Singleton(typeof(IValidator<ILedger>), CovariantResolution = Covariance.Covariant)]
public class LedgerValidator : IValidator<ILedger> { }

// IValidator<ILedger>        → LedgerValidator ✓  (exact match)
// IValidator<AddLedger>      → LedgerValidator ✓  (AddLedger : ILedger)
// IValidator<SubtractLedger> → LedgerValidator ✓  (SubtractLedger : ILedger)
// IValidator<string>         → null              (string is not ILedger)
```

**Multiple type arguments — all positions must satisfy `IsAssignableFrom`:**

```csharp
public interface IRepository<TEntity, TKey> { TEntity? Find(TKey id); }

[Singleton(typeof(IRepository<Animal, int>), CovariantResolution = Covariance.Covariant)]
public class AnimalRepository : IRepository<Animal, int> { }

// IRepository<Animal, int> → AnimalRepository ✓  (exact)
// IRepository<Dog, int>    → AnimalRepository ✓  (Dog : Animal, int == int)
// IRepository<Dog, long>   → null              (int is NOT assignable from long)
// IRepository<string, int> → null              (Animal is not assignable from string)
```

---

#### `Covariance.HonorInOut`

MetalInjection reads the CLR `out`/`in` annotation on each type parameter of the interface definition and applies the correct rule per position:

| Annotation | Rule | Meaning |
|---|---|---|
| `out T` (covariant) | `TReg.IsAssignableFrom(TReq)` | Registered type is base; requested type may be more derived. |
| `in T` (contravariant) | `TReq.IsAssignableFrom(TReg)` | Registered type is more derived; requested type may be wider. |
| *(none)* (invariant) | `TReg == TReq` | Exact match only for that position. |

> **Note:** If the interface has no `out`/`in` annotations at all, every position falls back to exact match and `HonorInOut` behaves identically to `Disabled`. Use `Covariance.Covariant` instead for invariant interfaces.

**Covariant interface (`out T`):**

```csharp
public interface IProducer<out T> { T Produce(); }

[Singleton(typeof(IProducer<Animal>), CovariantResolution = Covariance.HonorInOut)]
public class AnimalProducer : IProducer<Animal> { }

// IProducer<Animal> → AnimalProducer ✓  (exact)
// IProducer<Dog>    → AnimalProducer ✓  (out T: Animal.IsAssignableFrom(Dog))
// IProducer<object> → null              (Animal is not assignable from object)
```

**Contravariant interface (`in T`):**

```csharp
public interface IConsumer<in T> { void Consume(T value); }

[Singleton(typeof(IConsumer<object>), CovariantResolution = Covariance.HonorInOut)]
public class ObjectConsumer : IConsumer<object> { }

// IConsumer<object> → ObjectConsumer ✓  (exact)
// IConsumer<Animal> → ObjectConsumer ✓  (in T: Animal.IsAssignableFrom(object))
// IConsumer<Dog>    → ObjectConsumer ✓  (in T: Dog.IsAssignableFrom(object))
// IConsumer<int>    → null              (value type — int.IsAssignableFrom(object) is false)
```

**Mixed variance (`in TFrom, out TResult`):**

```csharp
public interface IConverter<in TFrom, out TResult> { TResult Convert(TFrom input); }

[Singleton(typeof(IConverter<Dog, Animal>), CovariantResolution = Covariance.HonorInOut)]
public class DogToAnimalConverter : IConverter<Dog, Animal> { }

// IConverter<Animal, Cat> → DogToAnimalConverter ✓
//   in  TFrom:   Animal.IsAssignableFrom(Dog)   ✓  (requested Animal is wider than registered Dog)
//   out TResult: Animal.IsAssignableFrom(Cat)   ✓  (Cat : Animal)

// IConverter<Animal, Dog> → DogToAnimalConverter ✓
//   in  TFrom:   Animal.IsAssignableFrom(Dog)   ✓
//   out TResult: Animal.IsAssignableFrom(Dog)   ✓
```

---

#### Choosing between `Covariant` and `HonorInOut`

| Scenario | Recommended value |
|---|---|
| Interface has no `out`/`in` annotations (invariant) | `Covariance.Covariant` |
| Interface has `out`/`in` annotations and you want DI to honour them | `Covariance.HonorInOut` |
| You want uniform "registered is base, requested is derived" semantics regardless of annotations | `Covariance.Covariant` |
| You need contravariant (`in T`) resolution | `Covariance.HonorInOut` |

---

### Open-Generic Factory Registration

In addition to the standard `services.AddTransient(typeof(IRepo<>), typeof(RepoImpl<>))` syntax, MetalInjection provides extension methods that let you supply a **factory delegate** for open-generic services. The factory receives the `IServiceProvider` and the resolved closed type arguments, so it can branch on the type argument, pull in other services, or construct the instance however it needs to.

```csharp
services.AddOpenGenericSingleton(typeof(IRepo<>),
    (sp, typeArgs) => Activator.CreateInstance(typeof(RepoImpl<>).MakeGenericType(typeArgs))!);

services.AddOpenGenericScoped(typeof(IRepo<>),
    (sp, typeArgs) => Activator.CreateInstance(typeof(RepoImpl<>).MakeGenericType(typeArgs))!);

services.AddOpenGenericTransient(typeof(IRepo<>),
    (sp, typeArgs) => Activator.CreateInstance(typeof(RepoImpl<>).MakeGenericType(typeArgs))!);

// or use the lifetime-parameterised overload directly:
services.AddOpenGenericFactory(typeof(IRepo<>), ServiceLifetime.Singleton,
    (sp, typeArgs) => Activator.CreateInstance(typeof(RepoImpl<>).MakeGenericType(typeArgs))!);
```

All four methods are extension methods on `IServiceCollection` in the `RossWright.MetalInjection` namespace.

**Rules and constraints:**

- The first argument must be an open-generic type definition (`typeof(IRepo<>)`). Passing a closed type (`typeof(IRepo<int>)`) throws `ArgumentException`.
- The factory is invoked lazily on first resolution (same caching semantics as any other descriptor — once for singletons, once per scope for scoped, every time for transients).
- The `sp` argument passed to the factory is the current scope's `IServiceProvider`, so you can call `sp.GetRequiredService<T>()` inside the factory to pull in other dependencies.
- Instances returned by the factory participate in the normal disposal pipeline (see [Disposal](#disposal)).

### Bootstrap Logging

`AddMetalInjection` supports startup-time diagnostic logging via `UseBootstrapLogger`. Because the standard `ILogger` pipeline isn't available during DI registration, you supply an `ILoggerFactory` before the container is built. MetalInjection uses it to report which services were found, registered, skipped, or rejected during assembly scanning.

To enable console output during startup, use `AddMetalConsoleLogger` from `RossWright.MetalCore`:

```csharp
builder.AddMetalInjection(options =>
{
    options.UseBootstrapLogger(logging =>
    {
        logging.ClearProviders();
        logging.AddMetalConsoleLogger();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
    options.ScanThisAssembly();
});
```

Call `options.DoNotUseLogger()` to suppress all bootstrap output (the default in Release builds if no factory is supplied).

---
## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalChain`](../MetalChain/README.md) | Mediator pattern: `IRequest` / `IRequestHandler` / `IMediator` |
| [`RossWright.MetalNexus`](../MetalNexus/README.md) | HTTP mediator bridge — connects MetalChain to ASP.NET Core and Blazor over HTTP |
| [`RossWright.MetalGuardian`](../MetalGuardian/README.md) | Authentication and authorization for the Metal stack |
| [`RossWright.MetalCommand`](../MetalCommand/README.md) | Interactive console application host |
| [`RossWright.MetalCore`](../MetalCore/RossWright.MetalCore/README.md) | Foundation utilities shared across the Metal libraries |

---
## License

All **Ross Wright Metal Libraries**

**You are free to**:
- Use the libraries in any project (personal or commercial)
- Modify them
- Include them in products or services you sell

**You may not**:
- Sell the libraries themselves (or any product/service whose *primary* value comes from the libraries)
- Repackage them with minimal changes and sell them as your own standalone product

Full legal text: [LICENSE.md](./LICENSE.md)

---
[Changelog](CHANGELOG.txt)
