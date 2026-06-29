# Use MetalInjection In Blazor WebAssembly

Use this recipe when a Blazor WebAssembly app should discover services through MetalInjection attributes or marker interfaces.

## Install

```bash
dotnet add package RossWright.MetalInjection.Blazor
```

Use `RossWright.MetalInjection.Abstractions` in shared libraries that declare service registrations.

## Namespace

```csharp
using RossWright;
```

## Setup

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddMetalInjection(options =>
{
	options.ScanThisAssembly();
	options.ScanAssemblyContaining<ClientState>();
});

await builder.Build().RunAsync();
```

## Service Example

```csharp
[Singleton<IClientState>]
public sealed class ClientState : IClientState
{
}
```

## Reach For This When

- You want Blazor client services registered by scanning.
- You want the same registration attributes used across server and client projects.
- You are wiring a larger WebAssembly app and want less startup boilerplate.

## Notes For Agents

- Use `RossWright.MetalInjection.Blazor` only in Blazor WebAssembly.
- Avoid server-only services in WebAssembly scans.
- Property injection uses `[Inject]`; constructor injection remains preferred for required dependencies.
