# MetalGuardian AI Usage Guide

Use this file when generating code that consumes RossWright.MetalGuardian packages.

## Packages

| Package | Use When |
|---|---|
| `RossWright.MetalGuardian.Abstractions` | You need shared auth contracts and built-in MetalNexus request types. |
| `RossWright.MetalGuardian.Server` | You are adding authentication to an ASP.NET Core server. |
| `RossWright.MetalGuardian.Blazor` | You are adding authentication state and authenticated API calls to Blazor WebAssembly. |
| `RossWright.MetalGuardian` | You are adding non-Blazor client auth services. |
| `RossWright.MetalGuardian.MFA.TOTP` | You need shared TOTP MFA request/response contracts. |
| `RossWright.MetalGuardian.Server.MFA.TOTP` | You need server-side TOTP enrollment, verification, QR codes, and recovery flows. |

## Namespace

Most APIs are in:

```csharp
using RossWright;
```

## Common APIs

| Task | API |
|---|---|
| Register server auth | `builder.AddMetalGuardianServer(...)` |
| Register Blazor client auth | `builder.AddMetalGuardianClient(...)` |
| Use built-in MetalNexus auth endpoints | `UseMetalNexusAuthenticationEndpoints()` |
| Configure JWT | `UseJwtConfiguration(...)` / `UseJwtConfigurationSection(...)` |
| Access current server user | `ICurrentUser` |
| Call login/logout/refresh APIs | `IMetalGuardianAuthenticationClient` |
| Add Blazor auth state | `UseBlazorAuthentication()` |
| Add authenticated HTTP client | `AddAuthenticatedHttpClient()` |
| Add TOTP MFA server support | `UseTotpMfa<TUser>(...)` / `UseMetalNexusTotpMfaEndpoints()` |

## Typical ASP.NET Core server setup

```csharp
builder.AddMetalGuardianServer(options =>
{
	options.UseMetalNexusAuthenticationEndpoints();
	options.UseJwtConfigurationSection("MetalGuardian");
	options.MapDatabaseAuthentication<MyDbContext, MyUser>(identity => user => user.Email == identity);
});
```

## Typical Blazor WebAssembly client setup

```csharp
builder.AddMetalGuardianClient(options =>
{
	options.UseMetalNexusAuthenticationEndpoints();
	options.AddAuthenticatedHttpClient();
	options.UseBlazorAuthentication();
});
```

## Important notes

- MetalGuardian is designed to pair with MetalNexus for built-in auth endpoints.
- Server projects use `RossWright.MetalGuardian.Server`.
- Blazor WebAssembly projects use `RossWright.MetalGuardian.Blazor`.
- Shared request/response projects use `RossWright.MetalGuardian.Abstractions`.
- Keep JWT signing keys in configuration or secrets, not source code.
