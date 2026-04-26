# RossWright.MetalInjection.Abstractions
Copyright (c) 2023-2026 Pross Co.

Core contracts, attributes, and interfaces for the [MetalInjection](https://www.nuget.org/packages/RossWright.MetalInjection/) dependency-injection library.
Consume this package directly when building libraries that expose MetalInjection registration attributes or injection markers without taking a full runtime dependency on MetalInjection itself.

## Installation

```powershell
dotnet add package RossWright.MetalInjection.Abstractions
```

## Quick Start

```csharp
// Mark a class for auto-registration as a singleton:
[Singleton<IMyService>]
public class MyService : IMyService { }

// Mark a class for auto-registration as a scoped service:
[ScopedService<IMyService>]
public class MyService : IMyService { }

// Mark a class for auto-registration as a transient service:
[TransientService<IMyService>]
public class MyService : IMyService { }

// Bind a configuration section and register it as a singleton:
[ConfigSection("MyApp:Settings")]
public class AppSettings : IValidatingConfigSection
{
    public string ConnectionString { get; set; } = string.Empty;
    public void ValidateOrDie()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("ConnectionString is required.");
    }
}

// Property injection:
public class MyConsumer
{
    [Inject] private IMyService _myService { get; set; } = null!;
}
```

## Key Concepts

### Registration Attributes

| Attribute | Lifetime | Example |
|---|---|---|
| `[Singleton<T>]` | Singleton | `[Singleton<IMyService>]` |
| `[ScopedService<T>]` | Scoped | `[ScopedService<IMyService>]` |
| `[TransientService<T>]` | Transient | `[TransientService<IMyService>]` |

Each has a non-generic overload (`[Singleton(typeof(T))]`) and a marker-interface equivalent (`ISingleton<T>`, `IScopedService<T>`, `ITransientService<T>`). Multiple attributes may be stacked to register a class under several service types.

### `[ConfigSection]`

Binds a POCO class to an `IConfiguration` section and registers it as a singleton. Use the generic form `[ConfigSection<TInterface>]` to register under an interface type. Implement `IValidatingConfigSection` to validate the bound values at startup.

### `[InjectAttribute]`

Marks a property or constructor parameter for property injection by MetalInjection. Supports optional injection (nullable type or `Optional = true`), keyed service injection via the `Key` property, and required enforcement via `Optional = false`.

### `[AllowMultipleRegistrations]`

Applied to a service **interface**: suppresses the duplicate-registration error, allowing multiple implementations to be registered. Resolve all implementations via `IEnumerable<TInterface>`.

### `[AllowRootResolution]`

Applied to a **scoped** implementation class: allows it to be resolved from the root `IServiceProvider` without creating a scope. Use sparingly.

### Covariant Resolution

`AutoServiceAttributeBase.CovariantResolution` (a `Covariance` enum value) controls whether a registered `IFoo<TBase>` can satisfy a request for `IFoo<TDerived>`. See the `Covariance` enum documentation for details on `Covariant` vs `HonorInOut` modes.

### `IValidatingConfigSection`

Implement on a `[ConfigSection]`-decorated class to validate bound configuration at startup. `ValidateOrDie()` is called after `IConfiguration.Bind` completes; throwing any exception aborts startup.

## API Summary

| Type | Purpose |
|---|---|
| `SingletonAttribute` / `SingletonAttribute<T>` | Registers a class as a singleton |
| `ScopedServiceAttribute` / `ScopedServiceAttribute<T>` | Registers a class as a scoped service |
| `TransientServiceAttribute` / `TransientServiceAttribute<T>` | Registers a class as a transient service |
| `ISingleton<T>` / `IScopedService<T>` / `ITransientService<T>` | Marker-interface equivalents of the above |
| `ConfigSectionAttribute` / `ConfigSectionAttribute<T>` | Binds a POCO to a config section and registers as singleton |
| `InjectAttribute` | Marks a property/parameter for property injection |
| `AllowMultipleRegistrationsAttribute` | Permits multiple registrations of a service interface |
| `AllowRootResolutionAttribute` | Allows scoped service resolution from root provider |
| `AutoServiceAttributeBase` | Base class for all auto-registration attributes |
| `IValidatingConfigSection` | Contract for startup config validation |
| `IMetalInjectionServiceProvider` | Extended `IServiceProvider` with `InjectProperties` |
| `ServiceProviderExtensions` | Extension methods for `CreateInstance` and `InjectProperties` |
| `ActivatorUtilities` | MetalInjection-aware replacement for the BCL `ActivatorUtilities` |
| `MetalInjectionException` | Exception thrown on registration or resolution failure |
| `Covariance` | Enum controlling covariant generic resolution behavior |

## See Also

- [RossWright.MetalInjection](https://www.nuget.org/packages/RossWright.MetalInjection/) — runtime package for console/generic-host apps
- [RossWright.MetalInjection.Server](https://www.nuget.org/packages/RossWright.MetalInjection.Server/) — ASP.NET Core integration
- [RossWright.MetalInjection.Blazor](https://www.nuget.org/packages/RossWright.MetalInjection.Blazor/) — Blazor WebAssembly integration
