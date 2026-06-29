# Use MetalCore In Blazor WebAssembly

Use this recipe when a Blazor WebAssembly app needs browser local storage, JavaScript script loading, or WebAssembly host builder helpers.

## Install

```bash
dotnet add package RossWright.MetalCore.Blazor
```

## Namespace

```csharp
using RossWright;
```

## Typical Setup

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBrowserLocalStorage();
builder.Services.AddJsScriptLoader();

await builder.Build().RunAsync();
```

## Common Uses

```csharp
public sealed class PreferencesService(IBrowserLocalStorage storage)
{
	public Task SaveTheme(string theme) =>
		storage.Set("theme", theme).AsTask();

	public Task<string?> LoadTheme() =>
		storage.Get("theme").AsTask();
}
```

## Reach For This When

- You are in a Blazor WebAssembly client.
- You need browser storage behind an injectable service.
- You need shared MetalCore helpers plus Blazor-specific host services.

## Avoid This When

- You are in an ASP.NET Core server. Use `RossWright.MetalCore.Server`.
- You are in a plain console app and do not need browser APIs.

## Notes For Agents

- Keep Blazor-only APIs out of shared contracts projects.
- Pair this with `RossWright.MetalNexus.Blazor` when the Blazor app calls generated MetalNexus endpoints.
