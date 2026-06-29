# Use MetalInjection In ASP.NET Core

Use this recipe when an ASP.NET Core app should discover services through MetalInjection attributes or marker interfaces.

## Install

```bash
dotnet add package RossWright.MetalInjection.Server
```

Use `RossWright.MetalInjection.Abstractions` in shared/domain assemblies that only declare attributes or marker interfaces.

## Namespace

```csharp
using RossWright;
```

## Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddMetalInjection(options =>
{
	options.ScanThisAssembly();
	options.ScanAssemblyContaining<CustomerService>();
});

var app = builder.Build();
app.Run();
```

## Service Example

```csharp
[ScopedService<ICustomerService>]
public sealed class CustomerService : ICustomerService
{
}
```

## Reach For This When

- You want services registered by scanning instead of manual `AddScoped` calls.
- You need property injection through `[Inject]`.
- You want configuration-bound services through `[ConfigSection]`.

## Notes For Agents

- Scan every assembly that contains decorated services.
- Constructor injection still works normally.
- Use the Server package for ASP.NET Core, not the Blazor package.
