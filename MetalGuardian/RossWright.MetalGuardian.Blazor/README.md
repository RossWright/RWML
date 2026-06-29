# RossWright.MetalGuardian.Blazor

Blazor WebAssembly integration for MetalGuardian. Provides `AddMetalGuardianClient` for `WebAssemblyHostBuilder`, `AuthenticationStateProvider` wiring, JS-based device fingerprinting, and the `<RedirectTo>` navigation component.

## Installation

```powershell
dotnet add package RossWright.MetalGuardian.Blazor
```

## Quick Start

```csharp
// Program.cs — Blazor WASM
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddMetalGuardianClient(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();    // Login/Logout/Refresh handlers
    options.AddAuthenticatedHttpClient();              // defaults to HostEnvironment.BaseAddress
    options.UseBlazorAuthentication();                // wires AuthenticationStateProvider
    options.UseDeviceFingerprinting();                // JS-based browser signals
});

await builder.Build().RunAsync();
```

Use `<AuthorizeView>` and `[Authorize]` as normal — no extra wiring required once `UseBlazorAuthentication()` is called.

### Login / logout from a component

```razor
@inject IMetalGuardianAuthenticationClient AuthClient

<button @onclick="LoginAsync">Log in</button>

@code {
    async Task LoginAsync()
    {
        await AuthClient.Login("user@example.com", "p@ssword!", CancellationToken.None);
    }
}
```

### Protecting a route

```razor
@if (!AuthClient.IsAuthenticated())
{
    <RedirectTo Url="/login" />
    return;
}

<p>You are logged in as @AuthClient.GetUser()?.UserName</p>
```

### Named API connection (separate API server)

```csharp
options.AddAuthenticatedHttpClient("https://api.example.com", connectionName: "api");
```

## Documentation

Full documentation is available in the [MetalGuardian README](../../MetalGuardian/README.md).
