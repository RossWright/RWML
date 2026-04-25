# Ross Wright's Metal Core Blazor Library
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Services](#services)
  - [`IBrowserLocalStorage`](#ibrowserlocalstorage)
  - [`IJsScriptLoaderService`](#ijsscriptloaderservice)
- [IServiceCollection Extensions](#iservicecollection-extensions)
- [App Builder Fluent Syntax](#app-builder-fluent-syntax)
  - [`WebAssemblyHostBuilder`](#webassemblyhostbuilder)
- [Installation](#installation)
- [See Also](#see-also)
- [License](#license)

---

## Services

### `IBrowserLocalStorage`

`IBrowserLocalStorage` provides async access to the browser's `localStorage` API via JavaScript interop. All methods return `ValueTask` so they integrate naturally with `await` in Razor components.

```csharp
// Store and retrieve a value
await localStorage.Set("theme", "dark");
var theme = await localStorage.Get("theme");

// Remove one key or clear everything
await localStorage.Remove("theme");
await localStorage.Clear();
```

| Method | Description |
|---|---|
| `Set(string key, string value)` | Writes or overwrites `value` under `key` in `localStorage` |
| `Get(string key)` | Returns the stored string, or `null` if the key does not exist |
| `Remove(string key)` | Deletes the entry for `key`; no-op if the key is absent |
| `Clear()` | Removes all entries from `localStorage` |

---

### `IJsScriptLoaderService`

`IJsScriptLoaderService` lazily loads external JavaScript files via interop, ensuring each script is injected only once per application lifetime.

```csharp
// Load a script; subsequent calls for the same path are no-ops
await jsLoader.EnsureLoaded("/js/chart.min.js", "Chart");

// Optionally supply a hash for cache-busting
await jsLoader.EnsureLoaded("/js/chart.min.js", "Chart", fileHash: "abc123");
```

| Method | Description |
|---|---|
| `EnsureLoaded(string path, string existenceObject, string? fileHash = null)` | Injects `<script src="path">` if `existenceObject` is not already defined on `window`; `fileHash` is appended as a query-string cache-buster |

---

## IServiceCollection Extensions

| Method | Lifetime | Description |
|---|---|---|
| `AddBrowserLocalStorage()` | Transient | Registers `IBrowserLocalStorage` for injection into components and services |
| `AddJsScriptLoader()` | Singleton | Registers `IJsScriptLoaderService`; singleton lifetime ensures each script is loaded at most once |

```csharp
builder.Services.AddBrowserLocalStorage();
builder.Services.AddJsScriptLoader();
```

---

## App Builder Fluent Syntax

MetalCore provides extension methods on `WebAssemblyHostBuilder` and `WebAssemblyHost` that enable a single fluent chain from `CreateDefault(args)` through to `RunAsync()`, eliminating the intermediate variable assignments that standard WASM startup requires.

`RunAsync(butFirst:)` accepts an async delegate that runs after the host is built but before it starts. This is the right place to pre-load services — for example, fetching content or initializing state — before the first Blazor component render.

```csharp
var app = WebAssemblyHostBuilder
    .CreateDefault(args)
    .AddRootComponents(_ => _.Add<App>("#app"))
    .AddMetalInjection(_ => _.ScanThisAssembly())
    .AddServices(_ => _.AddMudServices())
    .Build()
    .RunAsync(butFirst: async host => {
        await host.Services
            .GetRequiredService<IContentService>()
            .Initialize();
    });
```

### `WebAssemblyHostBuilder`

| Method | Description | Example |
|---|---|---|
| `AddRootComponents(Action<RootComponentMappingCollection>)` | Fluent wrapper for registering Blazor root components | `builder.AddRootComponents(r => r.Add<App>("#app"))` |
| `AddServices(Action<IServiceCollection>)` | Fluent wrapper for registering DI services on the WASM host builder | `builder.AddServices(s => s.AddScoped<IFoo, Foo>())` |
| `UseApp(Action<WebAssemblyHost>)` | Post-build configuration callback on the built host | `builder.UseApp(h => ...)` |
| `RunAsync(Func<WebAssemblyHost, Task>)` | Runs an async initialization step before starting the WASM host | `builder.RunAsync(async h => await h.InitAsync())` |

---

## Installation

```powershell
dotnet add package RossWright.MetalCore.Blazor
```

Or add directly to your project file:

```xml
<PackageReference Include="RossWright.MetalCore.Blazor" Version="*" />
```

> **Note:** `IBrowserLocalStorage` and `IJsScriptLoaderService` require a live browser JavaScript context. They are not suitable for server-side Blazor or SSR rendering modes.

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalCore`](https://www.nuget.org/packages/RossWright.MetalCore) | Core extensions, utilities, options builders, load logging, exceptions, signing |
| [`RossWright.MetalCore.Data`](https://www.nuget.org/packages/RossWright.MetalCore.Data) | Entity Framework extensions, GeoCoder, database timing interceptor |
| [`RossWright.MetalCore.Server`](https://www.nuget.org/packages/RossWright.MetalCore.Server) | ASP.NET Core messaging contracts, SMTP email service |
| [`RossWright.MetalCore.Populi`](https://www.nuget.org/packages/RossWright.MetalCore.Populi) | Zero-dependency test-data generator: names, addresses, emails, coordinates, dates, prices, and lorem ipsum |

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
