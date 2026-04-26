# RossWright.MetalInjection.Blazor
Copyright (c) 2023-2026 Pross Co.

Blazor WebAssembly integration for [MetalInjection](https://www.nuget.org/packages/RossWright.MetalInjection/).
Wires MetalInjection's attribute-driven assembly scanning and property injection into a `WebAssemblyHostBuilder`, and automatically supports Blazor's built-in `[Inject]` attribute alongside MetalInjection's own `[Inject]`.

## Installation

```powershell
dotnet add package RossWright.MetalInjection.Blazor
```

## Quick Start

In `Program.cs` of your Blazor WebAssembly project, replace `builder.Services.Add*` with a single `AddMetalInjection` call:

```csharp
using RossWright.MetalInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.AddMetalInjection(_ => _.ScanThisAssembly());

await builder.Build().RunAsync();
```

Services decorated with `[Singleton<T>]`, `[ScopedService<T>]`, or `[TransientService<T>]` in any scanned assembly are registered automatically.

## Key Concepts

### Blazor `[Inject]` Compatibility

`AddMetalInjection` automatically registers `Microsoft.AspNetCore.Components.InjectAttribute` as an alternate inject attribute. Both `@inject` directives in `.razor` files and `[Inject]` property annotations in code-behind files are honoured alongside MetalInjection's own `[Inject]`.

```razor
@inject IMyService MyService
```

```csharp
public partial class MyPage : ComponentBase
{
    [Inject] private IMyService MyService { get; set; } = null!;
}
```

### Configuration Binding

> **Note:** Configuration binding via `[ConfigSection]` is not available in Blazor WebAssembly projects. The WebAssembly runtime does not support the reflection-based configuration binding APIs used by MetalInjection. `[ConfigSection]` attributes on classes in scanned assemblies are silently ignored.

### Custom Options

Pass a delegate to `AddMetalInjection` to configure additional options:

```csharp
builder.AddMetalInjection(options =>
{
    options.ScanThisAssembly();
    options.ScanAssembly(typeof(SharedLibraryMarker).Assembly);
    options.AllowMultipleServicesOf<IMyMultiService>();
});
```

## See Also

- [RossWright.MetalInjection.Abstractions](https://www.nuget.org/packages/RossWright.MetalInjection.Abstractions/) — registration attributes and injection markers
- [RossWright.MetalInjection](https://www.nuget.org/packages/RossWright.MetalInjection/) — core runtime (console / generic-host)
- [RossWright.MetalInjection.Server](https://www.nuget.org/packages/RossWright.MetalInjection.Server/) — ASP.NET Core server integration
- [Full MetalInjection documentation](../README.md)
