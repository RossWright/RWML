# Ross Wright's Metal Guardian
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Packages](#packages)
- [Namespaces](#namespaces)
- [Common APIs](#common-apis)
- [Installation](#installation)
- [Server Setup](#server-setup)
  - [JWT Configuration](#jwt-configuration)
  - [Database Authentication](#database-authentication)
  - [Custom Claims](#custom-claims)
  - [One-Time Passwords](#one-time-passwords)
  - [TOTP Multi-Factor Authentication](#totp-multi-factor-authentication)
- [Client Setup](#client-setup)
  - [Blazor WebAssembly](#blazor-webassembly)
  - [Console App (MetalCommand)](#console-app-metalcommand)
- [Authentication API](#authentication-api)
  - [`IMetalGuardianAuthenticationClient`](#imetalguardianauthenticationclient)
  - [Built-in MetalNexus Endpoints](#built-in-metalnexus-endpoints)
- [ICurrentUser](#icurrentuser)
- [Password Validation](#password-validation)
- [Blazor Utilities](#blazor-utilities)
- [Esoterica](#esoterica)
  - [Device Fingerprinting](#device-fingerprinting)
  - [Custom Authentication Repositories](#custom-authentication-repositories)
  - [Custom Claims Provider](#custom-claims-provider)
  - [Custom Authentication API Service](#custom-authentication-api-service)
- [See Also](#see-also)
- [License](#license)
- [Changelog](CHANGELOG.txt)

---

## Overview

MetalGuardian is an authentication system purpose-built for the Metal stack. It provides JWT-based login, logout, and token refresh as ready-made [MetalNexus](../MetalNexus/README.md) endpoints, TOTP multi-factor authentication, one-time password (OTP) support for email and SMS verification flows, and Blazor `AuthenticationStateProvider` integration — all wired up with a few builder calls in `Program.cs`.

| Feature | Description |
|---|---|
| JWT authentication | Login, logout, token refresh via MetalNexus endpoints; configurable access and refresh token lifetimes |
| Provisional tokens | Partial-auth JWTs issued at login when MFA is pending; full token issued after verification |
| Device fingerprinting | Optional known-device tracking to skip MFA for trusted devices |
| TOTP MFA | QR code enrollment, code verification, device-remember, and admin reset — compatible with Google Authenticator and Authy |
| One-time passwords | Short-lived codes delivered via email or SMS; configurable digit count and expiry |
| Password validation | Configurable strength rules with an injectable `IPasswordValidator` |
| Blazor integration | `AuthenticationStateProvider`, cascading auth state, JS-based device fingerprinting, `<RedirectTo>` component |
| MetalCommand integration | `AddMetalGuardianClient` on `IConsoleApplicationBuilder` for CLI apps |

---

## Packages

The library is split across focused packages so each project only takes what it needs.

| Package | NuGet | Description |
|---|---|---|
| `RossWright.MetalGuardian.Abstractions` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Abstractions) | Shared auth contracts, `IMetalGuardianAuthenticationClient`, and built-in MetalNexus request types — add to shared/contracts projects |
| `RossWright.MetalGuardian.Server` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Server) | ASP.NET Core server — JWT issuance, login/logout/refresh handlers, OTP service, `ICurrentUser` |
| `RossWright.MetalGuardian` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian) | Non-Blazor .NET client — `IMetalGuardianAuthenticationClient`, password validator; use for console apps and MetalCommand |
| `RossWright.MetalGuardian.Blazor` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Blazor) | Blazor WebAssembly client — `AddMetalGuardianClient`, `AuthenticationStateProvider`, JS device fingerprinting, `<RedirectTo>` component |
| `RossWright.MetalGuardian.MFA.TOTP` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.MFA.TOTP) | Shared TOTP contracts — client request types (`SetupTotp`, `VerifyTotpMfa`, `ResetTotpMfa`) and `IAuthenticationInformation` extension helpers; add to shared and client projects |
| `RossWright.MetalGuardian.Server.MFA.TOTP` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Server.MFA.TOTP) | Server-side TOTP — QR code generation, code verification, device-remember logic, `IMetalGuardianTotpMfaService`; add to your ASP.NET Core server project |

---

## Namespaces

Most MetalGuardian setup methods, authentication contracts, MFA contracts, and Blazor helpers are available from:

```csharp
using RossWright;
```

---

## Common APIs

| Task | API | Package | Namespace |
|---|---|---|---|
| Register MetalGuardian on an ASP.NET Core server | `builder.AddMetalGuardianServer(...)` | `RossWright.MetalGuardian.Server` | `RossWright` |
| Register MetalGuardian in Blazor WebAssembly | `builder.AddMetalGuardianClient(...)` | `RossWright.MetalGuardian.Blazor` | `RossWright` |
| Use built-in login/logout/refresh endpoints | `UseMetalNexusAuthenticationEndpoints()` | `RossWright.MetalGuardian.Server` / `RossWright.MetalGuardian.Blazor` | `RossWright` |
| Configure JWT settings | `UseJwtConfiguration(...)` / `UseJwtConfigurationSection(...)` | `RossWright.MetalGuardian.Server` | `RossWright` |
| Access the authenticated user on the server | `ICurrentUser` | `RossWright.MetalGuardian.Server` | `RossWright` |
| Call authentication APIs from clients | `IMetalGuardianAuthenticationClient` | `RossWright.MetalGuardian.Abstractions` | `RossWright` |
| Add TOTP MFA server support | `UseTotpMfa<TUser>(...)` | `RossWright.MetalGuardian.Server.MFA.TOTP` | `RossWright` |

---

## Installation

Add the Abstractions package to your shared contracts project — this is where your MetalNexus request types live:

```powershell
dotnet add package RossWright.MetalGuardian.Abstractions
```

Add the server package to your ASP.NET Core project:

```powershell
dotnet add package RossWright.MetalGuardian.Server
```

Add the Blazor package to your Blazor WebAssembly project:

```powershell
dotnet add package RossWright.MetalGuardian.Blazor
```

Add the client package to a console app or other non-Blazor .NET client:

```powershell
dotnet add package RossWright.MetalGuardian
```

Add both TOTP packages when enabling TOTP multi-factor authentication — the shared contracts package goes in your client project and the server package goes in your ASP.NET Core project:

```powershell
dotnet add package RossWright.MetalGuardian.MFA.TOTP
dotnet add package RossWright.MetalGuardian.Server.MFA.TOTP
```

---

## Quick Start

The steps below assume the canonical Metal stack: MetalNexus transport, an ASP.NET Core server, and a Blazor WebAssembly client.

Register MetalGuardian on the server in `Program.cs`, providing your JWT settings and wiring your `DbContext` and user entity:

```csharp
// Program.cs — ASP.NET Core server
var builder = WebApplication.CreateBuilder(args);

builder.AddMetalGuardianServer(options =>
{
    options.UseMetalNexusAuthenticationEndpoints(); // register Login/Logout/Refresh handlers

    options.UseJwtConfiguration(new MetalGuardianServerConfiguration
    {
        JwtIssuer = "https://myapp.example.com",
        JwtAudience = "myapp",
        JwtIssuerSigningKey = builder.Configuration["MetalGuardian:SigningKey"]!,
    });

    options.MapDatabaseAuthentication<MyDbContext, MyUser>(
        identity => user => user.Email == identity);
});

var app = builder.Build();
app.Run();
```

Register MetalGuardian on the Blazor client in `Program.cs`, pointing it at the same server:

```csharp
// Program.cs — Blazor WebAssembly client
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddMetalGuardianClient(options =>
{
    options.UseMetalNexusAuthenticationEndpoints(); // route Login/Logout/Refresh over HTTP
    options.AddAuthenticatedHttpClient();           // HttpClient that sends the JWT automatically
    options.UseBlazorAuthentication();              // wire up <AuthorizeView> and [Authorize]
});

await builder.Build().RunAsync();
```

You now have working login, logout, and token refresh endpoints on the server, an authenticated `HttpClient` on the client, and Blazor's authorization primitives fully wired. The sections below explain every option in detail.

---

## Server Setup

### JWT Configuration

`JwtIssuerSigningKey`, `JwtIssuer`, and `JwtAudience` are required — MetalGuardian won't start without them. The remaining properties have sensible defaults, but understanding what they control helps you tune them for your application.

`JwtAccessTokenExpireMins` controls how long an access token stays valid before the client must refresh it. The default is 60 minutes — meaning the client silently requests a new token once per hour, and a stolen token is usable for at most that window. `RefreshTokenExpireMins` controls how long the user stays logged in without being asked to re-enter credentials; the default of 60 days means a user who visits every few weeks won't be logged out unexpectedly. `ProvisionalAccessTokenExpireMins` only matters when TOTP MFA is enabled — it's the window a partially authenticated user has to complete their MFA challenge before the provisional token expires. The default is 5 minutes, which is generous enough for a user to open their authenticator app.

Supply configuration inline, or bind it from `appsettings.json`:

```csharp
// Program.cs — ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

builder.AddMetalGuardianServer(options =>
{
    options.UseMetalNexusAuthenticationEndpoints(); // register Login/Logout/Refresh handlers

    options.UseJwtConfiguration(new MetalGuardianServerConfiguration
    {
        JwtIssuer = "https://myapp.example.com",
        JwtAudience = "myapp",
        JwtIssuerSigningKey = builder.Configuration["MetalGuardian:SigningKey"]!,
        JwtAccessTokenExpireMins = 60,          // default: 60 minutes
        RefreshTokenExpireMins = 24 * 60 * 60,  // default: 60 days
        ProvisionalAccessTokenExpireMins = 5,   // default: 5 minutes; only relevant when TOTP is enabled
    });

    // alternatively, bind from appsettings.json:
    // options.UseJwtConfigurationSection("MetalGuardian");

    options.MapDatabaseAuthentication<MyDbContext, MyUser>(
        identity => user => user.Email == identity);
});
```

| Property | Default | Controls |
|---|---|---|
| `JwtIssuer` | none (required) | Issuer claim embedded in and validated against every JWT |
| `JwtAudience` | none (required) | Audience claim embedded in and validated against every JWT |
| `JwtIssuerSigningKey` | none (required) | Symmetric key used to sign and verify JWTs |
| `JwtAccessTokenExpireMins` | `60` | Lifetime of access tokens; clients refresh silently at this interval |
| `RefreshTokenExpireMins` | `86400` (60 days) | How long a user stays logged in without re-entering credentials |
| `ProvisionalAccessTokenExpireMins` | `5` | Lifetime of MFA-pending tokens; only relevant when TOTP is enabled |

### Database Authentication

`MapDatabaseAuthentication` wires MetalGuardian's built-in EF Core repositories against your `DbContext` and user entity. You provide a predicate that maps the login identity string (typically an email address) to a user lookup expression, and MetalGuardian takes care of password verification, refresh token storage, and token generation.

Your `DbContext` must implement `IMetalGuardianDbContext<TUser, TRefreshToken>`, and your user entity must implement `IAuthenticationUser`:

```csharp
public class MyDbContext : DbContext,
    IMetalGuardianDbContext<MyUser, RefreshToken<MyUser>>
{
    public DbSet<MyUser> Users { get; set; } = null!;
    public DbSet<RefreshToken<MyUser>> RefreshTokens { get; set; } = null!;
}

public class MyUser : IAuthenticationUser
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDisabled { get; set; }
    public string PasswordSalt { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Email { get; set; } = null!;
}
```

Use `RefreshToken<TUser>` as-is unless you need to add columns to the refresh token table — it's MetalGuardian's built-in implementation of `IRefreshToken`.

Use the `SetPassword` and `IsPassword` extension methods on `IAuthenticationUser` to hash and verify passwords without touching the salt and hash fields directly:

```csharp
user.SetPassword("s3cur3P@ss!");        // generates a new salt and stores the hash
bool ok = user.IsPassword("s3cur3P@ss!");
```

`MapDatabaseAuthenticationWithDevices` is the upgrade path when you want to enable the "remember this device" feature for MFA. It costs one extra `DbSet` on your `DbContext` and enables device-trust tracking so recognized devices can skip MFA challenges:

```csharp
// DbContext adds UserDevices
public class MyDbContext : DbContext,
    IMetalGuardianDbContext<MyUser, RefreshToken<MyUser>, UserDevice<MyUser>>
{
    public DbSet<MyUser> Users { get; set; } = null!;
    public DbSet<RefreshToken<MyUser>> RefreshTokens { get; set; } = null!;
    public DbSet<UserDevice<MyUser>> UserDevices { get; set; } = null!;
}

// Program.cs — swap MapDatabaseAuthentication for the devices overload
options.MapDatabaseAuthenticationWithDevices<MyDbContext, MyUser>(
    identity => user => user.Email == identity);
```

For full control over authentication and device storage, see [Custom Authentication Repositories](#custom-authentication-repositories) in Esoterica.

### Custom Claims

By default, issued JWTs contain the user's `UserId` and `Name`. Any additional data you want available on the server via `ICurrentUser` — a tenant ID, a list of roles, a subscription tier — must be embedded in the token as claims. `AddUserClaimMapping` and `AddUserClaimsArrayMapping` are the straightforward way to do that for properties that already live on your user entity:

```csharp
options.AddUserClaimMapping<MyUser>("tenant", user => user.TenantId.ToString());
options.AddUserClaimsArrayMapping<MyUser>("roles", user => user.Roles.ToArray());
```

`AddUserClaimMapping` embeds a single string claim. Returning `null` from the delegate omits the claim entirely. `AddUserClaimsArrayMapping` embeds an array property as multiple claims of the same type — the standard way to represent roles in a JWT.

Once a claim is in the token, read it back on the server with `ICurrentUser.GetGuidClaim("tenant")` or the appropriate typed accessor. The claim name you pass here is the same one you'll read on the other side.

> **Note:** Claims are embedded at login time and travel in the token until refresh. If a user's tenant or roles can change at runtime, make sure the client refreshes the token after the change to get the updated claims.

When claim logic depends on data beyond the user entity — for example, loading a tenant ID from a separate table — see [Custom Claims Provider](#custom-claims-provider) in Esoterica.

### One-Time Passwords

OTPs are short-lived numeric codes delivered out-of-band — sent to the user's email address or phone number — for flows like email verification, password reset, or phone confirmation. They're distinct from TOTP (authenticator-app codes): an OTP is generated and delivered by your server on demand, and it expires after a single use or a configurable time window. If you're building a "verify your email" or "enter the code we texted you" step, this is the right tool.

OTP state lives in `IDistributedCache`. For a single server, `AddDistributedMemoryCache()` is all you need. For multiple servers, replace it with any `IDistributedCache` provider — Redis, SQL Server, etc. — and nothing else about the OTP setup changes.

Register the cache and enable the OTP service in `Program.cs`:

```csharp
// Program.cs — ASP.NET Core
builder.Services.AddDistributedMemoryCache(); // or any IDistributedCache provider

builder.AddMetalGuardianServer(options =>
{
    // ...
    options.UseOneTimePassword(otp =>
    {
        otp.NumberOfDigits = 6;        // default 6
        otp.ExpirationInMinutes = 10;  // default 10
    });
});
```

The lifecycle is straightforward: call `SendOtpViaEmail` or `SendOtpViaSms` to generate a code and deliver it to the user; the user submits the code in a follow-up request; call `VerifyOtp`; on success the code is consumed and `Valid` is returned. Inject `IOtpService` into your MetalChain handlers:

```csharp
// Handler 1 — generate and send the code
public class SendVerificationEmailHandler(IOtpService _otp) : IRequestHandler<SendVerificationEmailRequest>
{
    public Task Handle(SendVerificationEmailRequest request, CancellationToken ct) =>
        _otp.SendOtpViaEmail(request.Email,
            code => new VerificationEmail(request.Email, code), ct);
}

// Handler 2 — verify the submitted code
public class VerifyEmailHandler(IOtpService _otp) : IRequestHandler<VerifyEmailRequest>
{
    public async Task Handle(VerifyEmailRequest request, CancellationToken ct)
    {
        var result = await _otp.VerifyOtp(request.Email, request.Code, ct: ct);
        if (result != OtpVerifyResult.Valid)
            throw new InvalidOperationException("Invalid or expired code.");
        // code is consumed — proceed with the verified action
    }
}
```

`OtpVerifyResult` values:

| Value | Meaning |
|---|---|
| `Valid` | Code matched and has been removed |
| `NotFound` | No active OTP exists for the given user identifier |
| `WrongUserId` | An OTP exists but was issued for a different user identifier |
| `Expired` | An OTP was found but its expiry time has passed |

For flows where you need to verify a code before committing an action, pass `preserveOtp: true` to keep the code alive after the check, then call `RemoveOtp` explicitly once the action succeeds. A concrete example: a user wants to change their email address. You send a code to the new address, verify ownership with `preserveOtp: true`, commit the address change, then remove the OTP so it can't be reused:

```csharp
// Step 1 — verify ownership but keep the code alive
var result = await _otp.VerifyOtp(request.NewEmail, request.Code, preserveOtp: true, ct: ct);
if (result != OtpVerifyResult.Valid)
    throw new InvalidOperationException("Invalid or expired code.");

// Step 2 — commit the change, then consume the code
await UpdateEmailAsync(request.NewEmail, ct);
await _otp.RemoveOtp(request.NewEmail, request.Code, ct);
```

### TOTP Multi-Factor Authentication

When a user with TOTP MFA enabled submits their credentials, MetalGuardian issues a *provisional* JWT instead of a full one. A provisional token signals that authentication is incomplete — MFA verification is still pending. The client reads that signal and routes the user to the appropriate next step: the QR-code setup page if they've never enrolled, or the code-entry page if they have. Only after a correct TOTP code is submitted does the server issue a full-access JWT.

A provisional token is a safety mechanism, not a soft suggestion. It can only reach endpoints explicitly marked `AllowProvisional = true` — which is exactly what the TOTP endpoints are. There is no way to accidentally treat a provisional login as fully authenticated.

#### Registration

Add TOTP support to the server with `UseTotpMfa<TUser>` (from `RossWright.MetalGuardian.Server.MFA.TOTP`) inside `AddMetalGuardianServer`:

```csharp
// Program.cs — ASP.NET Core
builder.AddMetalGuardianServer(options =>
{
    // ... standard setup ...
    options.MapDatabaseAuthentication<MyDbContext, MyUser>(...);
    options.UseTotpMfa<MyUser>(mfa =>
    {
        mfa.SetIssuer("My App");          // label shown in the authenticator app (e.g. "My App: user@example.com")
        mfa.SetDeviceRememberDays(30);    // pass null to require MFA on every login
        mfa.UseMetalNexusTotpMfaEndpoints();
    });
});
```

On the client, register the matching request types alongside the standard auth endpoints:

```csharp
// Program.cs — Blazor WASM (or MetalCommand)
builder.AddMetalGuardianClient(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();
    options.UseMetalNexusTotpMfaEndpoints(); // adds SetupTotp, VerifyTotpMfa, ResetTotpMfa
    // ...
});
```

#### User Entity

Your user entity must implement `ITotpMfaAuthenticationUser`, which extends `IAuthenticationUser` with three additional members:

```csharp
public class MyUser : ITotpMfaAuthenticationUser
{
    // ... IAuthenticationUser members ...

    // The Base32 TOTP secret, written by MetalGuardian during setup — treat as opaque
    public string? MfaTotpSecret { get; set; }

    // true once the user has completed setup and verified their first code
    public bool IsMfaTotpEnabled { get; set; }

    // Controls whether this user can skip MFA — return true to enforce it for everyone,
    // or drive it from a per-user column for opt-in scenarios
    public bool IsMfaTotpRequired => true;
}
```

`IsMfaTotpRequired` is evaluated at login time. When it returns `false` for a given user, that user can log in without completing MFA even if TOTP is otherwise enabled — useful for service accounts or admin overrides. Return `true` unconditionally to make MFA mandatory for all users.

#### Routing After Login

After calling `Login`, read the two extension helpers from `RossWright.MetalGuardian.MFA.TOTP` on the returned `IAuthenticationInformation` to decide where to send the user:

```csharp
var info = await authClient.Login(email, password, ct);

if (info is null)
{
    // bad credentials — show error
}
else if (info.NeedsToSetupTotpMfa())
{
    // provisional token, user has never enrolled — navigate to QR setup page
    NavigateTo("/mfa/setup");
}
else if (info.HasTotpMfaEnabled())
{
    // provisional token, user is enrolled — navigate to code entry page
    NavigateTo("/mfa/verify");
}
else
{
    // full token — proceed normally
    NavigateTo("/");
}
```

`NeedsToSetupTotpMfa()` returns `true` when the provisional token carries a claim indicating the user has not yet scanned a QR code. `HasTotpMfaEnabled()` returns `true` when TOTP is enrolled and the token is provisional — meaning they need to enter their code before gaining full access.

#### TOTP Endpoints

`UseMetalNexusTotpMfaEndpoints()` registers three endpoints on the server and the matching client request types:

| Request | Path | Flow role |
|---|---|---|
| `SetupTotp.Request` | `GET /Authentication/SetupTotp` | Generates a new TOTP secret and returns a QR code data URI to display for authenticator app scanning; requires provisional token |
| `VerifyTotpMfa.Request` | `POST /Authentication/VerifyTotp` | Submits the TOTP code; returns full `AuthenticationTokens` on success or `null` on an incorrect code; requires provisional token |
| `ResetTotpMfa.Request` | `POST /Authentication/ResetTotp` | Clears a user's TOTP enrollment so they must re-scan a QR code; intended for admin flows; requires full token |

A typical setup page calls `SetupTotp.Request`, renders the returned `QrCode` data URI as an `<img>`, asks the user to type the first code from their authenticator app, and then calls `VerifyTotpMfa.Request` to confirm enrollment and receive the full token. The `DeviceFingerprint` field on `VerifyTotpMfa.Request` is optional — include it when device fingerprinting is enabled and you want to offer "remember this device."

---

## Client Setup

### Blazor WebAssembly

`AddMetalGuardianClient` on `WebAssemblyHostBuilder` is the entry point for a Blazor WASM client. Each option in the builder callback enables a distinct capability — understanding what each one does helps you include only what your app needs.

`UseMetalNexusAuthenticationEndpoints()` registers the client-side handlers that route login, logout, and token refresh over HTTP to the server's MetalNexus endpoints. Call this unless you're supplying a custom `IAuthenticationApiService`.

`AddAuthenticatedHttpClient()` registers an `HttpClient` that automatically attaches the current access token as a `Bearer` header on every outgoing request. When called without arguments on the Blazor builder, the base address defaults to the host environment's base address.

`UseBlazorAuthentication()` wires up the `AuthenticationStateProvider` so Blazor's `<AuthorizeView>`, `[Authorize]` on pages, and `AuthenticationStateTask` all work without any further wiring. The provider subscribes to `IMetalGuardianAuthenticationClient.AuthenticationChanged` and pushes a new `AuthenticationState` through Blazor's cascade on every auth transition.

`UseDeviceFingerprinting()` enables browser-based device fingerprinting using JavaScript browser signals. Enable this when the server has device-based MFA skipping configured so the client can supply a fingerprint at login.

```csharp
// Program.cs — Blazor WASM
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddMetalGuardianClient(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();
    options.AddAuthenticatedHttpClient();
    options.UseBlazorAuthentication();
    options.UseDeviceFingerprinting();              // omit if not using device-based MFA skipping
});

await builder.Build().RunAsync();
```

| Option | Description |
|---|---|
| `UseMetalNexusAuthenticationEndpoints()` | Registers client-side Login, Logout, and Refresh request handlers over MetalNexus |
| `AddAuthenticatedHttpClient()` | Registers a `Bearer`-token-attaching `HttpClient`; base address defaults to host environment |
| `UseBlazorAuthentication()` | Wires `AuthenticationStateProvider`; enables `<AuthorizeView>` and `[Authorize]` |
| `UseDeviceFingerprinting()` | Enables JS-based browser device fingerprinting for MFA device-remember |
| `UsePasswordValidator()` | Registers `IPasswordValidator` for client-side password strength checking |

**Named connections.** When your app communicates with more than one API server, call `AddAuthenticatedHttpClient` with a `connectionName` for each. Use the same name on the server and client to route requests correctly:

```csharp
options.AddAuthenticatedHttpClient("https://api.example.com", connectionName: "api");
options.AddAuthenticatedHttpClient("https://files.example.com", connectionName: "files", isDefault: false);
```

If you're adding TOTP MFA support, also call `UseMetalNexusTotpMfaEndpoints()` inside the same builder callback to register the TOTP request handlers.

### Console App (MetalCommand)

`AddMetalGuardianClient` on `IConsoleApplicationBuilder` registers MetalGuardian for MetalCommand console applications. The option surface is the same `IMetalGuardianClientOptionsBuilder` used by Blazor, minus the Blazor-specific options: there's no `UseBlazorAuthentication()` and no browser-based `UseDeviceFingerprinting()`. If your console app needs device tracking for MFA skipping, use `UseDeviceFingerprinting<MachineDeviceFingerprintService>()` instead — it generates a stable fingerprint from the machine's hardware identifiers.

```csharp
ConsoleApplication.CreateBuilder(args)
    .AddMetalGuardianClient(options =>
    {
        options.UseMetalNexusAuthenticationEndpoints();
        options.AddAuthenticatedHttpClient("https://api.example.com");
    })
    .Build()
    .Run();
```

If you're not using MetalNexus as your transport — e.g. custom controllers, SignalR, JSON-RPC — see [Custom Authentication API Service](#custom-authentication-api-service) in Esoterica.

---

## Authentication API

### `IMetalGuardianAuthenticationClient`

`IMetalGuardianAuthenticationClient` is the single source of truth for authentication state on the client. Logging in, logging out, checking whether a user is authenticated, reading who they are — all of it goes through this one interface. Inject it into any Blazor component, MetalCommand handler, or service that needs to interact with auth state.

`Login` has two meaningful overloads. The credentials overload is the normal login-form path — pass the user's identity string and password and you get back `IAuthenticationInformation` on success, or `null` on bad credentials. The `AuthenticationTokens` overload restores a previously saved session without contacting the server — use it on app startup to reload tokens persisted to localStorage or secure storage.

`Authenticate()` (no-args refresh) is called automatically on every MetalNexus request to ensure the access token is current. You won't call it directly in normal usage, but it's available if you need to force a token refresh or restore a session before making an out-of-band request.

```csharp
// Login with credentials — normal login form path
IAuthenticationInformation? info = await authClient.Login("user@example.com", "p@ssword!", ct);

// Login with saved tokens — restore a persisted session on app startup
IAuthenticationInformation? info = await authClient.Login(savedTokens, ct);

// Check status without hitting the server
bool isAuth = authClient.IsAuthenticated();
IAuthenticationInformation? user = authClient.GetUser();

// Logout — revokes tokens on the server and clears local state
await authClient.Logout(ct);
```

`IAuthenticationInformation` carries all the identity data decoded from the current access token:

| Property | Description |
|---|---|
| `Token` | Raw JWT access token string |
| `ExpiresOn` | Token expiry as `DateTimeOffset` |
| `UserId` | Authenticated user's `Guid` |
| `UserName` | Authenticated user's display name |
| `IsProvisional` | `true` when MFA is still pending — route the user to the MFA flow, not the app |
| `IsKnownDevice` | `true` if the device fingerprint matched a trusted device and MFA was skipped; `null` if fingerprinting isn't enabled |
| `GetAdditionalClaim(type)` | Retrieve any custom claim embedded at login time by its type string |
| `AsClaimsIdentity()` | Convert to `ClaimsIdentity` for use with ASP.NET Core or Blazor auth primitives |

`AuthenticationChanged` fires on every auth state transition: login, logout, token refresh, and session restore. The typical uses are persisting tokens to localStorage so the session survives a page reload, and pushing auth state updates to other parts of the UI that aren't driven by Blazor's `AuthenticationStateProvider` cascade.

```csharp
authClient.AuthenticationChanged += async (connectionName, info, ct) =>
{
    if (info is not null)
        await localStorage.SetItemAsync("tokens", new AuthenticationTokens
        {
            AccessToken = info.Token,
            // RefreshToken is not exposed on IAuthenticationInformation —
            // persist the AuthenticationTokens returned by Login instead
        }, ct);
    else
        await localStorage.RemoveItemAsync("tokens", ct);
};
```

> **Tip:** To persist and restore a full session, capture the `AuthenticationTokens` returned by `Login` (not `IAuthenticationInformation`) and pass them to the `Login(AuthenticationTokens)` overload on next startup. `AuthenticationTokens` carries both the access token and the refresh token.

### Built-in MetalNexus Endpoints

You won't call these directly — `IMetalGuardianAuthenticationClient` handles them automatically. The table below is reference material for developers who need the exact paths, for example to configure a reverse proxy or API gateway.

These request types are defined in `RossWright.MetalGuardian.Abstractions` and registered server-side when `UseMetalNexusAuthenticationEndpoints()` is called on both the server and the client builder:

| Request | Path | Auth requirement |
|---|---|---|
| `Login.Request` | `POST /Authentication/Login` | `[Anonymous]` — accepts `UserIdentity`, `Password`, and optional `DeviceFingerprint` |
| `Logout.Request` | `POST /Authentication/Logout` | `[Authenticated(AllowProvisional = true)]` — revokes the refresh token server-side |
| `Refresh.Request` | `POST /Authentication/Refresh` | `[Anonymous]` — exchanges a refresh token for a new token pair |

---

## ICurrentUser

`ICurrentUser` is the server-side counterpart to `IAuthenticationInformation`. Where `IAuthenticationInformation` is what the *client* knows about the current user, `ICurrentUser` is what *server-side handlers* use to access that same identity. It's registered as a scoped service — one instance per request, resolved from the request's JWT bearer token.

Prefer it over `IHttpContextAccessor` directly: it surfaces identity in application terms rather than raw HTTP claim strings, and it's trivially mockable in unit tests without faking an `HttpContext`.

Inject it into any handler that needs to know who's asking. Claims you mapped with `AddUserClaimMapping` at login time — a tenant ID, a subscription tier — are read back here with the matching typed accessor:

```csharp
public class GetMyProfileHandler(ICurrentUser _currentUser)
    : IRequestHandler<GetMyProfileRequest, Profile>
{
    public Task<Profile> Handle(GetMyProfileRequest request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated) throw new UnauthorizedAccessException();
        return Task.FromResult(new Profile
        {
            UserId = _currentUser.UserId,
            Name = _currentUser.UserName,
            TenantId = _currentUser.GetGuidClaim("tenant"), // mapped via AddUserClaimMapping
        });
    }
}
```

| Member | Description |
|---|---|
| `IsAuthenticated` | `true` when `UserId != Guid.Empty`; `false` for unauthenticated requests |
| `UserId` | Authenticated user's `Guid`; `Guid.Empty` when not authenticated |
| `UserName` | Authenticated user's display name |
| `HasRole(role)` | `true` if the user holds the specified role claim |
| `GetClaim(name)` | First value of the named claim, or `null` if absent |
| `GetClaimValues(name)` | All values for a multi-value claim (e.g. roles), or `null` if absent |
| `GetGuidClaim(name)` | First value of the named claim parsed as a `Guid`, or `null` if absent or not a valid GUID |
| `GetGuidClaims(name)` | All values of the named claim parsed as `Guid`s; non-parseable entries are `null` |

---

## Password Validation

`IPasswordValidator` serves two jobs: validating a candidate password against a set of configured rules, and generating a human-readable requirements message for display in your registration UI. Both methods accept a `forbiddenFragments` parameter — pass the user's email address, username, or any other value you want to exclude from the password. The check is case-insensitive, so `forbiddenFragments: request.Email, request.UserName` prevents passwords like `Alice123!` when the username is `alice`.

Register the validator in your client options and optionally customize the requirements:

```csharp
options.UsePasswordValidator(req =>
{
    req.MinimumLength = 10;
    // defaults: RequireUpperCase, RequireLowerCase, RequireDigit, RequireSymbol all true
    // MaximumLength defaults to no limit
});
```

The defaults — minimum 8 characters, at least one uppercase, lowercase, digit, and symbol — cover most applications without any configuration. Inject `IPasswordValidator` wherever you need it and use both methods together so the validation message the handler returns matches exactly what the UI already told the user:

```csharp
public class RegisterHandler(IPasswordValidator _passwordValidator, LedgertronDbContext _db)
    : IRequestHandler<Register.Request>
{
    public async Task Handle(Register.Request request, CancellationToken ct)
    {
        if (!_passwordValidator.ValidatePassword(request.Password, request.Email, request.UserName))
            throw new ValidationException(
                _passwordValidator.GetPasswordRequirementsMessage(request.Email, request.UserName));

        // hash and persist the user...
    }
}
```

`GetPasswordRequirementsMessage` produces a single sentence describing all active constraints, including the forbidden-fragments note when you pass any. Display it beneath the password field when the form first renders so the user knows the rules before they type.

| Property | Default | Controls |
|---|---|---|
| `MinimumLength` | `8` | Minimum character count |
| `MaximumLength` | no limit | Maximum character count |
| `RequireUpperCase` | `true` | At least one uppercase letter |
| `RequireLowerCase` | `true` | At least one lowercase letter |
| `RequireDigit` | `true` | At least one numeric digit |
| `RequireSymbol` | `true` | At least one symbol from `AllowedSymbols` |
| `AllowedSymbols` | common punctuation | Characters accepted as symbols |

---

## Blazor Utilities

### `AuthenticationStateProvider`

`UseBlazorAuthentication()` in your Blazor client's `Program.cs` registers `MetalGuardianAuthenticationStateProvider` as Blazor's `AuthenticationStateProvider`. The provider subscribes to `IMetalGuardianAuthenticationClient.AuthenticationChanged` and calls `NotifyAuthenticationStateChanged` on every auth transition — login, logout, token refresh, and session restore. After that single call, `<AuthorizeView>`, `[Authorize]` on pages, and `AuthenticationStateTask` all work without any additional wiring.

### `<RedirectTo>` Component

`<RedirectTo>` renders nothing and immediately triggers a hard navigation to the specified `Url` via `NavigationManager.NavigateTo`. Place it at the top of any protected page or component, before any protected content or data-fetching runs — if the user isn't authenticated, they're redirected before anything else executes:

```razor
@inject IMetalGuardianAuthenticationClient AuthClient

@if (!AuthClient.IsAuthenticated())
{
    <RedirectTo Url="/login" />
    return;
}

<!-- protected content below here -->
```

---

## Esoterica

The sections above cover the typical MetalGuardian setup. Below are advanced topics for specific scenarios.

### Device Fingerprinting

Device fingerprinting is for developers enabling the "remember this device" feature that lets MetalGuardian skip the MFA challenge on a device the user has previously verified. It's opt-in on both sides: the client must call `UseDeviceFingerprinting()` and the server must have a device repository registered (via `MapDatabaseAuthenticationWithDevices` or `UseUserDeviceRepository`).

MetalGuardian ships three implementations. The Blazor implementation collects browser and hardware signals via JavaScript — user agent, language, platform, hardware concurrency, screen resolution, color depth, timezone, canvas pixel output, WebGL GPU vendor and renderer, audio processing characteristics, and font rendering metrics — and hashes them into a stable hex string. The `MachineDeviceFingerprintService` is for console and desktop clients; it hashes machine-level signals (machine name, OS description, processor count, architecture) that are stable for the lifetime of the machine. For anything else, implement `IDeviceFingerprintService` and return any stable string that uniquely identifies the device.

```csharp
// Blazor WASM — Program.cs
builder.AddMetalGuardianClient(options =>
{
    // ...
    options.UseDeviceFingerprinting(); // JS-based browser signals
});
```

```csharp
// Console / desktop — Program.cs
builder.AddMetalGuardianClient(options =>
{
    // ...
    options.UseDeviceFingerprinting<MachineDeviceFingerprintService>(); // machine-level signals
});
```

```csharp
// Custom implementation
public class MyDeviceFingerprintService : IDeviceFingerprintService
{
    public Task<string> GetFingerprint() =>
        Task.FromResult(/* your stable device identifier */);
}
```

Both built-in fingerprints are stable across process restarts. The Blazor fingerprint also survives page loads, browser restarts, and cookie or localStorage clears — it doesn't rely on stored state. Privacy-hardened browsers are the exception: Firefox `privacy.resistFingerprinting` and Brave's fingerprint randomization actively alter canvas and WebGL output on every page load, producing a different fingerprint each time and effectively disabling device recognition. Users with those settings will always be prompted for MFA.

> **Compliance notice:** Device fingerprinting collects browser and hardware signals and hashes them into a device identifier used solely for MFA device-trust decisions. The hash is never transmitted to third parties. The server stores only the hash — not the raw signals. You are responsible for ensuring your use of this feature complies with all applicable privacy laws and regulations in your jurisdiction, including any disclosure obligations.

### Custom Authentication Repositories

`MapDatabaseAuthentication` and `MapDatabaseAuthenticationWithDevices` cover the common case: a standard EF Core `DbContext` with your user, refresh token, and (optionally) device entities. When you need full control — a custom database schema, a non-EF ORM, an external identity store, or a multi-tenant setup that routes queries differently — you can supply your own repository implementations.

`UseAuthenticationRepository<T>()` replaces the built-in EF-backed storage for users and refresh tokens. `UseUserDeviceRepository<T>()` replaces the device storage independently. Both are mutually exclusive with their `MapDatabase*` counterparts — pick one per role.

```csharp
builder.AddMetalGuardianServer(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();
    options.UseJwtConfiguration(/* ... */);

    options.UseAuthenticationRepository<MyAuthRepository>();
    options.UseUserDeviceRepository<MyDeviceRepository>(); // optional; enables device trust
});
```

`IAuthenticationRepository` is the contract for user lookup and refresh token lifecycle:

```csharp
public class MyAuthRepository(MyDb _db) : IAuthenticationRepository
{
    public Task<IAuthenticationUser?> LookupUser(string userIdentity, CancellationToken ct) { ... }
    public Task<IAuthenticationUser?> UpdateUser(Guid userId, Func<IAuthenticationUser, bool> update, CancellationToken ct) { ... }
    public Task AddRefreshToken(Action<IRefreshToken> setProperties, CancellationToken ct) { ... }
    public Task<IAuthenticationUser?> UpdateRefreshToken(Guid userId, string refreshToken, Action<IRefreshToken> setProperties, CancellationToken ct) { ... }
    public Task DeleteRefreshToken(Guid userId, string refreshToken, CancellationToken ct) { ... }
}
```

`IUserDeviceRepository` is the contract for trusted-device records:

```csharp
public class MyDeviceRepository(MyDb _db) : IUserDeviceRepository
{
    public Task Add(Action<IUserDevice> setProperties, CancellationToken ct) { ... }
    public Task<IUserDevice?> Get(Guid userId, string deviceFingerprint, CancellationToken ct) { ... }
    public Task Update(Guid userId, string deviceFingerprint, Action<IUserDevice> setProperties, CancellationToken ct) { ... }
}
```

### Custom Claims Provider

`AddUserClaimMapping` and `AddUserClaimsArrayMapping` cover the common case: mapping properties that already exist on the user entity into the JWT. When claim logic depends on data that isn't on the user entity — loading a tenant ID from a separate table, aggregating roles from a many-to-many, or calling an external service — implement `IUserClaimsProvider` instead.

Multiple providers can be registered; MetalGuardian calls all of them during token generation and merges their results. Each provider receives the authenticated `IAuthenticationUser` and can resolve its own dependencies from DI through the constructor.

```csharp
builder.AddMetalGuardianServer(options =>
{
    // ...
    options.UseUserClaimsProvider<TenantClaimsProvider>();
});
```

```csharp
public class TenantClaimsProvider(MyDb _db) : IUserClaimsProvider
{
    public async Task<IEnumerable<(string, string)>?> GetClaims(
        IAuthenticationUser user, CancellationToken ct)
    {
        var tenant = await _db.Tenants
            .Where(t => t.OwnerId == user.UserId)
            .Select(t => t.Id.ToString())
            .FirstOrDefaultAsync(ct);

        if (tenant is null) return null;
        return [("tenant", tenant)];
    }
}
```

Read the custom claim back on the server with `ICurrentUser.GetGuidClaim("tenant")` — see [ICurrentUser](#icurrentuser).

### Custom Authentication API Service

`IAuthenticationApiService` is the client-side transport contract — it's what actually makes the login, logout, and refresh calls to the server. It isn't just for non-MetalGuardian backends; it's the seam between `IMetalGuardianAuthenticationClient` and whatever mechanism your app uses to reach the server. The built-in implementation routes those calls through MetalNexus, which is registered by `UseMetalNexusAuthenticationEndpoints()`. If you're using your own API controllers, SignalR, JSON-RPC, gRPC, or any other transport, implement `IAuthenticationApiService` and register it with `UseAuthenticationApiService<T>()`.

```csharp
options.UseAuthenticationApiService<MyCustomAuthService>();
```

```csharp
public class MyCustomAuthService(HttpClient _http) : IAuthenticationApiService
{
    public async Task<AuthenticationTokens?> Login(string userIdentity, string password,
        string connectionName, CancellationToken ct)
    {
        // call your server, return AuthenticationTokens on success or null on failure
    }

    public async Task Logout(AuthenticationTokens tokens,
        string connectionName, CancellationToken ct)
    {
        // invalidate the tokens on your server
    }

    public async Task<AuthenticationTokens?> Refresh(AuthenticationTokens tokens,
        string connectionName, CancellationToken ct)
    {
        // exchange the refresh token for new tokens, return null if expired or invalid
    }
}
```

| Method | Responsibility |
|---|---|
| `Login` | Authenticate credentials; return new `AuthenticationTokens` on success, `null` on failure |
| `Logout` | Invalidate the provided tokens on the server |
| `Refresh` | Exchange the refresh token for a new token pair; return `null` if the refresh token is expired or invalid |

`connectionName` is always non-null at the call site — it's the named connection configured on the client builder, or `"default"` if none was specified.

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalNexus`](../MetalNexus/README.md) | HTTP mediator bridge — MetalGuardian's endpoints use MetalNexus |
| [`RossWright.MetalChain`](../MetalChain/README.md) | Mediator pattern: `IRequest` / `IRequestHandler` / `IMediator` |
| [`RossWright.MetalCore`](../MetalCore/RossWright.MetalCore/README.md) | Foundation utilities, SMTP/SMS messaging contracts |
| [`RossWright.MetalInjection`](../MetalInjection/README.md) | Ground-up `IServiceProvider` with attribute/interface-based registration |
| [`RossWright.MetalCommand`](../MetalCommand/README.md) | Interactive console application host |

---

## License

All **Ross Wright Metal Libraries** including this one are licensed under **Apache License 2.0 with Commons Clause**.

**You are free to**: use in any project (personal or commercial), modify, and include in products or services you sell.

**You may not**: sell the libraries themselves, or repackage them with minimal changes and sell them as your own standalone product.

Full legal text: [LICENSE.md](./LICENSE.md)
