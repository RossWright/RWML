# Ross Wright's Metal Injection Library
Copyright (c) 2023-2026 Pross Co.

## Table of Contents
- [Introduction](#introduction)
- [Installation](#installation)
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
- [License](#license)

## Introduction
MetalInjection is a dependency inversion library for building and using a service provider. It supports:
* registering services via reflection via an attribute or interface inheritance on the implementation
* keyed services (as the current default .NET ServiceProviders do)
* property injection via Inject attributes.
* open generic service registration, including open-generic factory delegates
* binding and registration of configuration sections for injection via attribute.
* deterministic disposal of `IDisposable` and `IAsyncDisposable` services across all lifetimes and scope boundaries.

## Installation
Metal Injection can be used on server and client projects of any kind: anywhere you want to use a service provider. 
Note that configuration binding only works for projects where configuration binding normally works, so it does not work for Blazor projects.

### Server Setup
To setup MetalInjection and auto-register services and configurations on an ASP.NET Core project, 
add the [RossWright.MetalInjection.Server](https://www.nuget.org/packages/RossWright.MetalInjection.Server/) nuget package to your project 
and call `AddMetalInjection` on the `WebApplicationBuilder` in your program.cs file:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddMetalInjection(_ => _.ScanThisAssembly());
```

> **MVC Controller Property Injection:** `AddMetalInjection` on `WebApplicationBuilder` automatically registers `MetalInjectionControllerActivator`, which enables `[Inject]` property injection on ASP.NET Core MVC controllers. No additional setup is required — just decorate controller properties with `[Inject]` and they will be populated on every request.

### Blazor Client Setup
To setup MetalInjection and auto-register service on a Blazor project, 
add the [RossWright.MetalInjection.Blazor](https://www.nuget.org/packages/RossWright.MetalInjection.Blazor/) nuget package to your project
and call `AddMetalInjection` on the `WebAssemblyHostBuilder` in your program.cs file:
```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.AddMetalInjection(_ => _.ScanThisAssembly());
```

### MetalCommand Client Setup
To setup MetalInjection and auto-register services and configurations on a MetalCommand Console project
add the [RossWright.MetalInjection](https://www.nuget.org/packages/RossWright.MetalInjection/) nuget package to your project
and call `AddMetalInjection` on the `ConsoleApplicationBuilder` in your program.cs file:
```csharp
var builder = ConsoleApplication.CreateBuilder(args);
builder.AddMetalInjection(_ => _.ScanThisAssembly());
```

### Other Projects Setup
To setup MetalInjection and auto-register services and configurations on a console project or any other project type using IServiceProvider, 
add the [RossWright.MetalInjection](https://www.nuget.org/packages/RossWright.MetalInjection/) nuget package to your project,
construct a ServiceCollection as normal maybe registering some services directly on it
and call `BuildMetalInjectionServiceProvider` on the `IServiceCollection` in your program.cs file (instead of BuildServiceProvider):
```csharp
var serviceCollection = new ServiceCollection();
...
// potentially register services directly instead of via reflection
...
var serviceProvider = serviceCollection
     .BuildMetalInjectionServiceProvider(options => 
     {
          options.ScanThisAssembly();
     });
```
and then use the returned serviceProvider for activation as normal.

---
## Registration
To specify a class should be registered as a service during the reflection scan, decorate the implementation with an attribute:
```csharp
[Singleton<ISampleService>]
public class SampleService : ISampleService
```
or implement an interface:
```csharp
public class SampleService : ISampleService, ISingleton<ISampleService>
```

Attributes and interfaces are provided for:
* `[Singleton<>]` or `ISingleton<>` for singleton lifetime services
* `[ScopedService<>]` or `IScopedService<>` for scoped lifetime services
* `[TransientService<>]` or `ITransientService<>` for transient lifetime services.

Using the interface syntax has the benefit of providing compile-time checking that a class actually implements the service type it is registered to provide. Services registered via attribute will be checked for implementation of the specified service type on initialization resulting in a run-time exception if it does not.

If you are not using an interface for injecting your service and want to register your implementation type as itself, then you can set the type parameter of the attribute or interface to the implementation type itself, but you should really ask yourself why you're using dependency injection in the first place!

### Registering One Implementation Under Multiple Service Types
You can stack multiple registration attributes on a single class to expose it under several service interfaces:
```csharp
[Singleton<IMyReadService>]
[Singleton<IMyWriteService>]
public class MyService : IMyReadService, IMyWriteService
```
MetalInjection guarantees that all stacked registrations for the same implementation resolve to **the same instance** within the appropriate lifetime scope:

| Lifetime | Behavior |
|---|---|
| **Singleton** | All stacked interfaces return the same instance for the lifetime of the container. |
| **Scoped** | All stacked interfaces return the same instance within a scope; a new shared instance is created for each new scope. |
| **Transient** | Each resolution creates a new instance regardless of which interface is used — there is no instance sharing across stacked transient registrations. |

> **Note:** Stacking multiple `[ScopedService<>]` or `[TransientService<>]` attributes on a single class requires `AllowMultiple = true`, which is set on these attributes. `[Singleton<>]` also supports stacking.

---
## Injection
Services can be injected into your components, controllers, services, etc. via constructors or Blazor `@inject` or `[Inject]` exactly the same way you do with normal .NET dependency injection.
MetalInjection also supports for optional injection (inject only if the service is registered),  
multiple injection (inject all registered implementations for a service type) 
and blazor-style property injection for all environments using Inject attributes.

### Property Injection
To inject a service, simply preface a property of the service type with the `[Inject]` attribute.
```
public class MyServiceThatUsesAnotherService
{
   [Inject] private IAnotherService _anotherSvc { get; set; } = null!;
}
```
Both the `InjectAttribute` classes in the `Microsoft.AspNetCore.Components` namespace and `RossWright.MetalInjection` namespaces will work, so use whichever is convenient. If needed you can also specify an additional alternate inject attribute of your choosing using the `SetAlternateInjectAttribute` method on initialization of MetalInjection like this:
```csharp
builder.AddMetalInjection(_ => 
{   
    _.ScanThisAssembly();
    _.SetAlternateInjectAttribute<MyInjectAttribute>();
});
```
You can also optionally provide a `Func<TInjectAttribute, object?>` parameter to provide a key for keyed service injection using your custom attribute instance.

Note if an injected service is needed in the constructor of your class, you need to use constructor injection instead as property injection occurs after the constructor is called.

### Optional Injection
You can specify a service be injected if it's been registered and null otherwise, use the option parameter syntax on your constructor. Like this:
```csharp
public class MyComponent(IOptionalService? optionalService = null)
```
If a service is registered for that type, it is injected otherwise the parameter is null.

If you are using property injection it works much the same:
```csharp
public class MyServiceThatUsesAnotherService
{
   [Inject] private IOptionalService? _anotherSvc { get; set; }
}
```

You can also override nullability inference explicitly using the `Optional` property on `[Inject]`:
```csharp
// Force optional even though the type is non-nullable:
[Inject(Optional = true)] private IRequiredLookingService _svc { get; set; } = null!;

// Force required even though the type is nullable (throws if not registered):
[Inject(Optional = false)] private IOptionalLookingService? _svc { get; set; }
```

### Keyed Service Injection
Standard .NET keyed services are fully supported. Register a keyed service as normal, then inject it in a constructor using the `[FromKeyedServices]` attribute:
```csharp
// Registration
services.AddKeyedSingleton<IMessageService, SmsService>("sms");
services.AddKeyedSingleton<IMessageService, EmailService>("email");

// Constructor injection
public class NotificationService(
    [FromKeyedServices("sms")] IMessageService smsService,
    [FromKeyedServices("email")] IMessageService emailService)
{
    // ...
}
```
For property injection with a keyed service, supply a `Func<TInjectAttribute, object?>` key selector when configuring your alternate inject attribute (see `SetAlternateInjectAttribute` in the Property Injection section above).

### Multiple Service Implementation Injection
By default MetalInjection only allows one implementation to be registered for each service type. This can be altered at initialization by specifying specific types allowed to be registered multiple times:
```csharp
builder.AddMetalInjection(_ => 
{   
    _.ScanThisAssembly();
    _.AllowMultipleServicesOf<MyMultipleService>();
    _.AllowMultipleServicesOf<MyOtherMultipleService>();
});
```
or just turning off this constraint entirely:
```csharp
builder.AddMetalInjection(_ => 
{   
    _.ScanThisAssembly();
    _.AllowMultipleServicesOfAnyType();
});
```

Alternatively, you can place `[AllowMultipleRegistrations]` directly on the service **interface**. This is the most ergonomic option when the interface is owned by your project, as it co-locates the intent with the type declaration and requires no call-site configuration:
```csharp
[AllowMultipleRegistrations]
public interface IMultiService { ... }
```
Any number of implementations of `IMultiService` may then be registered without error. Resolve all of them via `IEnumerable<IMultiService>` as shown below.
To inject all services registered for a service type, use `IEnumerable<>` (as normal) like this:
```csharp
public class MyServiceThatUsesMultipleServicesOfTheSameType(
    IEnumerable<MyMultipleService> multSvcs)
```
The collection will be empty if no implementations are registered.

It works similarly for property injection:
```csharp
public class MyServiceThatUsesMultipleServicesOfTheSameType
{
   [Inject] private IEnumerable<IMultiService> _multiServices { get; set; } = null!;
}
```

---
## Activation
When an object is instantiated using Microsoft's `ActivatorUtilities.CreateInstance`, property injection is not resolved. When using MetalInjection the `ActivatorUtilities` class in the `RossWright.MetalInjection` namespace should be used to ensure property injection happens. Some suggestions on how to handle this cleanly in your project:

* Add a global using to a file (perhaps program.cs) in your project like this: 
```csharp
global using ActivatorUtilities = RossWright.MetalInjection.ActivatorUtilities
```
* Call `RossWright.MetalInjection.ActivatorUtilities.CreateInstance` explicitly.
* If you cannot control the call to ActivatorUtilities.CreateInstance because it is down in some third-party code or something, you can inject the service `IMetalInjectionServiceProvider` and call `InjectProperties` on the instance of an object to resolve any injectable properties. 
* You can even inject `IMetalInjectionServiceProvider` via the constructor of the service using property injection and call it in the constructor - if you're really dedicated to using property injection. Like this:
```csharp
public class ActivatedBeyondMyControl
{
   public ActivatedBeyondMyControl(IMetalInjectionServiceProvider serviceProvider)
   {
      serviceProvider.InjectProperties(this);
   }
 
   [Inject] private INeededService _neededSvc { get; set; } = null!;
   ...
}
```
* MetalInjection also provides an extension method on `IServiceProvider` for `CreateInstance` that will invoke property injection and takes non-injected parameters if needed. Like this:
```csharp
[ScopedService<IMyService>]
class MyService(ISomeInjectedService svc, int someParameter) : IMyService
...
serviceProvider.CreateInstance<MyService>(123);

```
In this example `ISomeInjectedService` is injected from the service provider and 123 is sent for someParameter.

---
## Configuration Sections

> **Note:** Configuration binding is not available in Blazor WebAssembly projects because the WebAssembly runtime does not support the configuration binding APIs used by MetalInjection. `[ConfigSection]` attributes are silently ignored when using `AddMetalInjection` on a `WebAssemblyHostBuilder`.

MetalInjection can automatically bind sections of your app configuration

### Basic Usage

Decorate a class with `[ConfigSection]` and give it the configuration section path. The class will be instantiated, bound to that section, and registered as a singleton injectable by its concrete type:

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

Inject it by the concrete type as normal:

```csharp
public class MyRepository(DatabaseSettings settings) { ... }
```

### Registering as an Interface

To register the config class to be registered as an interface type rather than the concrete class, use the generic form of the attribute:

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

The class must implement the specified interface or a run-time exception is thrown on startup. Inject by the interface:

```csharp
public class MyRepository(IDatabaseSettings settings) { ... }
```

### Validation

To validate configuration values at startup, implement `IValidatingConfigSection` on your settings class and throw an appropriate exception from `ValidateOrDie()` if the configuration is invalid. This prevents the application from starting with bad configuration:

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

`ValidateOrDie()` is called immediately after binding during startup. If it throws, the exception propagates and prevents the application from continuing.

### Non-Hosted Projects

When using `BuildMetalInjectionServiceProvider` directly (see [Other Projects Setup](#other-projects-setup)), pass your `IConfiguration` instance to enable config section binding:

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

A single class can be decorated with multiple `[ConfigSection]` attributes. All sections are bound to the **same instance** in the order the attributes appear. This is useful when a single settings object aggregates values from several configuration paths, or needs to be injectable by multiple interface types:

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

Both `IFeatureFlags` and `ILimitsConfig` will resolve to the same `AppPolicySettings` instance.

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

---

## License

All **Ross Wright Metal Libraries** including this one are licensed under **Apache License 2.0 with Commons Clause**.

**You are free to**:
- Use the libraries in any project (personal or commercial)
- Modify them
- Include them in products or services you sell

**You may not**:
- Sell the libraries themselves (or any product/service whose *primary* value comes from the libraries)
- Repackage them with minimal changes and sell them as your own standalone product

Full legal text: [LICENSE.md](./LICENSE.md)
