# Ross Wright's Metal Guardian
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Packages](#packages)
- [Installation](#installation)
- [Server Setup](#server-setup)
  - [JWT Configuration](#jwt-configuration)
  - [Database Authentication](#database-authentication)
  - [Custom Claims](#custom-claims)
  - [Device Fingerprinting](#device-fingerprinting)
  - [One-Time Passwords](#one-time-passwords)
  - [TOTP Multi-Factor Authentication](#totp-multi-factor-authentication)
- [Client Setup](#client-setup)
  - [Blazor WebAssembly](#blazor-webassembly)
  - [Console App (MetalCommand)](#console-app-metalcommand)
- [Authentication API](#authentication-api)
  - [`IMetalGuardianAuthenticationClient`](#imetalguardianauthenticationclient)
  - [Built-in MetalNexus Endpoints](#built-in-metalnexus-endpoints)
  - [Custom Authentication API Service](#custom-authentication-api-service)
- [Authorization](#authorization)
  - [Endpoint-Level Authorization](#endpoint-level-authorization)
  - [Role-Only Authorization](#role-only-authorization)
  - [Role-and-Permission Authorization](#role-and-permission-authorization)
  - [Entity Authorization](#entity-authorization)
  - [Hierarchical Authorization](#hierarchical-authorization)
  - [Authorization Cache Busting](#authorization-cache-busting)
  - [Client-Side Authorization](#client-side-authorization)
- [ICurrentUser](#icurrentuser)
- [Password Validation](#password-validation)
- [Blazor Utilities](#blazor-utilities)
- [See Also](#see-also)
- [License](#license)
- [Changelog](CHANGELOG.md)

---

## Overview

MetalGuardian is a complete authentication and authorization system purpose-built for the Metal stack. It provides JWT-based login/logout/refresh flows as ready-made [MetalNexus](../MetalNexus/README.md) endpoints and [MetalChain](../MetalChain/README.md) handlers, role/permission authorization at multiple levels of granularity, TOTP multi-factor authentication, one-time password (OTP) email/SMS support, and Blazor `AuthenticationStateProvider` integration — all wired up with a few builder calls.

| Feature | Description |
|---|---|
| JWT authentication | Login, logout, token refresh via MetalNexus endpoints; configurable access and refresh token lifetimes |
| Provisional tokens | Provisional JWTs issued at login when MFA is required; full token issued after verification |
| Device fingerprinting | Optional known-device tracking to skip MFA for trusted devices |
| Role-only authorization | Inject `IGlobalAuthorizationService<TPrivilege>` and call `MayUserDo` |
| Role-and-permission authorization | Per-user permission overrides on top of role-based grants |
| Entity authorization | Per-entity role and permission checks scoped to a `Guid` entity id |
| Hierarchical authorization | Entity authorization that walks an entity ancestry chain for inherited permissions |
| TOTP MFA | QR code setup, code verification, device-remember, admin reset — compatible with Google Authenticator and Authy |
| One-time passwords | OTP generation and verification via email or SMS; configurable digit count and expiry |
| Password validation | Configurable strength rules; injectable `IPasswordValidator` |
| Blazor integration | `AuthenticationStateProvider`, cascading auth state, device fingerprinting via JS |
| MetalCommand integration | `AddMetalGuardianClient` on `IConsoleApplicationBuilder` |

---

## Packages

| Package | NuGet | Description |
|---|---|---|
| `RossWright.MetalGuardian.Abstractions` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Abstractions) | Auth contracts, `IMetalGuardianAuthenticationClient`, built-in MetalNexus request types — add to shared projects |
| `RossWright.MetalGuardian.Server` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Server) | ASP.NET Core server: JWT issuance, login/logout/refresh handlers, role/permission/entity authorization engines, OTP service, `ICurrentUser` |
| `RossWright.MetalGuardian` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian) | Client library for non-Blazor .NET projects: `IMetalGuardianAuthenticationClient`, authorization services, password validator |
| `RossWright.MetalGuardian.Blazor` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Blazor) | Blazor WebAssembly: `AddMetalGuardianClient`, `AuthenticationStateProvider`, device fingerprinting, `<RedirectTo>` component |
| `RossWright.MetalGuardian.MFA.TOTP` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.MFA.TOTP) | TOTP client requests (`SetupTotp`, `VerifyTotpMfa`, `ResetTotpMfa`) and `IAuthenticationInformation` extension helpers |
| `RossWright.MetalGuardian.Server.MFA.TOTP` | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Server.MFA.TOTP) | Server-side TOTP: QR code generation, code verification, device-remember, `IMetalGuardianTotpMfaService` |

---

## Installation

**Shared contracts project:**

```powershell
dotnet add package RossWright.MetalGuardian.Abstractions
```

**ASP.NET Core server:**

```powershell
dotnet add package RossWright.MetalGuardian.Server
```

**Blazor WebAssembly client:**

```powershell
dotnet add package RossWright.MetalGuardian.Blazor
```

**Console / other .NET client:**

```powershell
dotnet add package RossWright.MetalGuardian
```

**TOTP MFA (add to both client and server projects):**

```powershell
dotnet add package RossWright.MetalGuardian.MFA.TOTP
dotnet add package RossWright.MetalGuardian.Server.MFA.TOTP
```

---

## Server Setup

### JWT Configuration

Call `AddMetalGuardianServer` on `WebApplicationBuilder` and provide JWT settings either inline or from a configuration section:

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
        JwtIssuerSigningKey = "your-secret-key",
        JwtAccessTokenExpireMins = 60,         // default 60 minutes
        RefreshTokenExpireMins = 24 * 60 * 60, // default 60 days
        ProvisionalAccessTokenExpireMins = 5,  // default 5 minutes (for MFA flows)
    });

    // or load from appsettings.json:
    // options.UseJwtConfigurationSection("MetalGuardian");

    options.MapDatabaseAuthentication<MyDbContext, MyUser>(
        identity => user => user.Email == identity);
});
```

`MapDatabaseAuthentication` expects your `DbContext` to implement `IMetalGuardianDbContext<TUser, TRefreshToken>`:

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

Use the `SetPassword` / `IsPassword` extension methods on `IAuthenticationUser` to hash and verify passwords:

```csharp
user.SetPassword("s3cur3P@ss!");       // sets PasswordSalt + PasswordHash
bool ok = user.IsPassword("s3cur3P@ss!");
```

### Database Authentication

Use the device-tracking overload to enable known-device fingerprinting:

```csharp
options.MapDatabaseAuthenticationWithDevices<MyDbContext, MyUser>(
    identity => user => user.Email == identity);
```

Your `DbContext` must additionally implement `IMetalGuardianDbContext<TUser, TRefreshToken, TUserDevice>`, adding a `DbSet<TUserDevice>` for `UserDevice<TUser>` (or your own `IUserDevice` implementation).

For full control, supply a custom repository:

```csharp
options.UseAuthenticationRepository<MyAuthenticationRepository>();
options.UseUserDeviceRepository<MyUserDeviceRepository>();
```

### Custom Claims

Add claims from user properties, or inject a full `IUserClaimsProvider`:

```csharp
// Map individual properties to claim types
options.AddUserClaimMapping<MyUser>("tenant", user => user.TenantId.ToString());
options.AddUserClaimsArrayMapping<MyUser>("roles", user => user.Roles.ToArray());

// Or provide a full custom provider
options.UseUserClaimsProvider<MyUserClaimsProvider>();
```

```csharp
public class MyUserClaimsProvider : IUserClaimsProvider
{
    public Task<IEnumerable<(string, string)>?> GetClaims(
        IAuthenticationUser user, CancellationToken cancellationToken)
    {
        var myUser = (MyUser)user;
        return Task.FromResult<IEnumerable<(string, string)>?>(
        [
            ("tenant", myUser.TenantId.ToString()),
        ]);
    }
}
```

### Device Fingerprinting

MetalGuardian tracks device fingerprints to detect whether a login is from a known device. On the server this is handled automatically when `MapDatabaseAuthenticationWithDevices` is used. The `IMultifactorAuthenticationProvider` receives `isKnownDevice` at login time and decides whether to issue a provisional or full token.

### One-Time Passwords

Enable an in-memory OTP store (useful for email/SMS MFA flows):

```csharp
options.UseOneTimePassword(otp =>
{
    otp.NumberOfDigits = 6;         // default 6
    otp.ExpirationInMinutes = 10;   // default 10
});
```

Inject `IOtpService` in handlers to send and verify OTPs:

```csharp
public class SendOtpHandler : IRequestHandler<SendOtpRequest>
{
    public SendOtpHandler(IOtpService otp) => _otp = otp;
    private readonly IOtpService _otp;

    public Task Handle(SendOtpRequest request, CancellationToken ct) =>
        _otp.SendOtpViaEmail(request.Email,
            code => new MyEmail(request.Email, $"Your code: {code}"), ct);
}
```

`IOtpService` members:

| Method | Description |
|---|---|
| `SendOtpViaEmail(userIdentifier, makeEmail, ct)` | Generates a code and sends it via `IAddressedEmail` (MetalCore.Server) |
| `SendOtpViaSms(userIdentifier, makeSms, ct)` | Generates a code and sends it via `IAddressedSmsMessage` |
| `VerifyOtp(userIdentifier, otp)` | Returns `OtpVerifyResult` (Valid / Invalid / Expired) |
| `RemoveOtp(userIdentifier, otp)` | Removes the OTP after use |

### TOTP Multi-Factor Authentication

Add TOTP support to the server with `AddMetalGuardianTotpMfa` (from `RossWright.MetalGuardian.Server.MFA.TOTP`):

```csharp
builder.AddMetalGuardianServer(options =>
{
    // ... standard setup ...
    options.MapDatabaseAuthentication<MyDbContext, MyUser>(...);
});

builder.Services.AddMetalGuardianTotpMfa<MyDbContext, MyUser>(mfa =>
{
    mfa.SetIssuer("My App");          // label shown in authenticator apps
    mfa.SetDeviceRememberDays(30);    // days before a known device must re-verify
    mfa.UseMetalNexusTotpMfaEndpoints();
});
```

Your user entity must implement `ITotpMfaAuthenticationUser` (extends `IAuthenticationUser`):

```csharp
public class MyUser : ITotpMfaAuthenticationUser
{
    // ... IAuthenticationUser members ...
    public string? MfaTotpSecret { get; set; }
    public bool IsMfaTotpEnabled { get; set; }
    public bool IsMfaTotpRequired => true; // or per-user logic
}
```

Built-in TOTP MetalNexus endpoints (registered by `UseMetalNexusTotpMfaEndpoints`):

| Endpoint | Path | Description |
|---|---|---|
| `SetupTotp.Request` | `GET /Authentication/SetupTotp` | Returns a QR code for registering in an authenticator app; requires provisional auth |
| `VerifyTotpMfa.Request` | `POST /Authentication/VerifyTotp` | Verifies the TOTP code; returns full `AuthenticationTokens` on success; requires provisional auth |
| `ResetTotpMfa.Request` | `POST /Authentication/ResetTotp` | Admin reset of TOTP enrollment for a user; requires full auth |

`IAuthenticationInformation` extension helpers from `RossWright.MetalGuardian.MFA.TOTP`:

```csharp
bool needsSetup = authInfo.NeedsToSetupTotpMfa();  // user must enroll
bool hasEnabled = authInfo.HasTotpMfaEnabled();     // TOTP already enrolled
```

---

## Client Setup

### Blazor WebAssembly

```csharp
// Program.cs — Blazor WASM
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.AddMetalGuardianClient(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();   // register Login/Logout/Refresh client handlers
    options.AddAuthenticatedHttpClient();             // defaults BaseAddress to HostEnvironment.BaseAddress
    options.UseBlazorAuthentication();               // wires up AuthenticationStateProvider
    options.UseDeviceFingerprinting();               // JS-based device fingerprint
});

await builder.Build().RunAsync();
```

Use a named connection for multi-server setups:

```csharp
options.AddAuthenticatedHttpClient("https://api.example.com", connectionName: "api");
```

For TOTP, also call:

```csharp
options.UseMetalNexusTotpMfaEndpoints();
```

### Console App (MetalCommand)

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

---

## Authentication API

### `IMetalGuardianAuthenticationClient`

Inject `IMetalGuardianAuthenticationClient` anywhere on the client to manage authentication state:

```csharp
// Login with credentials
IAuthenticationInformation? info = await authClient.Login("user@example.com", "p@ssword!", ct);

// Login with persisted tokens (e.g. on app start)
IAuthenticationInformation? info = await authClient.Login(savedTokens, ct);

// Refresh (called automatically by MetalNexus; call explicitly if needed)
IAuthenticationInformation? info = await authClient.Authenticate(ct: ct);

// Check status without hitting the server
bool isAuth = authClient.IsAuthenticated();
IAuthenticationInformation? user = authClient.GetUser();

// Logout
await authClient.Logout(ct);
```

`IAuthenticationInformation` properties:

| Property | Description |
|---|---|
| `Token` | Raw JWT access token string |
| `ExpiresOn` | Token expiry as `DateTimeOffset` |
| `UserId` | User's `Guid` |
| `UserName` | Display name |
| `IsProvisional` | `true` when MFA has not yet been verified |
| `IsKnownDevice` | `true` if the device was fingerprint-matched |
| `GetAdditionalClaim(type)` | Retrieve any claim by type |
| `AsClaimsIdentity()` | Convert to `ClaimsIdentity` |

Subscribe to auth state changes:

```csharp
authClient.AuthenticationChanged += async (connectionName, info, ct) =>
{
    // react to login/logout/refresh on any connection
};
```

### Built-in MetalNexus Endpoints

These request types are defined in `RossWright.MetalGuardian.Abstractions` and handled server-side by `RossWright.MetalGuardian.Server` when `UseMetalNexusAuthenticationEndpoints()` is called on both sides:

| Request | Path | Description |
|---|---|---|
| `Login.Request` | `POST /Authentication/Login` | Returns `AuthenticationTokens`; `[Anonymous]` |
| `Logout.Request` | `POST /Authentication/Logout` | Revokes the refresh token; `[Authenticated(AllowProvisional = true)]` |
| `Refresh.Request` | `POST /Authentication/Refresh` | Exchanges a refresh token for new tokens; `[Anonymous]` |

`IMetalGuardianAuthenticationClient` calls these endpoints automatically — you rarely need to invoke them directly.

### Custom Authentication API Service

If your backend does not use MetalGuardian server, implement `IAuthenticationApiService` and register it:

```csharp
options.UseAuthenticationApiService<MyCustomAuthService>();
```

```csharp
public class MyCustomAuthService : IAuthenticationApiService
{
    public Task<AuthenticationTokens?> Login(string userIdentity, string password,
        string connectionName, CancellationToken ct) { ... }
    public Task Logout(AuthenticationTokens tokens, string connectionName, CancellationToken ct) { ... }
    public Task<AuthenticationTokens?> Refresh(AuthenticationTokens tokens, string connectionName, CancellationToken ct) { ... }
}
```

---

## Authorization

MetalGuardian's authorization system is independent of ASP.NET Core's `IAuthorizationService`. You define a `TPrivilege` type (an `enum`, `int`, or `string`) representing what actions a user may perform, and a `TRole` type that groups privileges. Authorization is evaluated via `IGlobalAuthorizationService<TPrivilege>` or `IEntityAuthorizationService<TPrivilege>`, which are scoped per HTTP request and internally cache results for performance.

### Endpoint-Level Authorization

MetalNexus `[Authenticated]` / `[Anonymous]` attributes handle coarse-grained access. All endpoints require authentication by default unless `MakeEndpointsAnonymousByDefault` is called.

```csharp
[Authenticated("Admin")]             // roles checked from JWT claims
[ApiRequest]
public class DeleteUserRequest : IRequest { }
```

### Role-Only Authorization

Add role-only authorization on the server (roles control all privilege grants; no per-user overrides):

```csharp
// Server — Program.cs
builder.Services.AddMetalGuardianRoleOnlyAuthorization<MyPrivilege, MyRole, MyRoleRepository>();
```

```csharp
public enum MyPrivilege { ViewReport, EditReport, DeleteReport }
public enum MyRole { Viewer, Editor, Admin }

public class MyRoleRepository : IRoleOnlyAuthorizationRepository<MyPrivilege, MyRole>
{
    // Returns which privileges each role grants
    public Task<IDictionary<MyRole, MyPrivilege[]>> GetRolePrivileges() { ... }
    // Returns which roles a user has
    public Task<MyRole[]> GetUserRoles(Guid userId) { ... }
}
```

Inject and use on the client:

```csharp
// Client setup
services.AddMetalGuardianGlobalAuthorization<MyPrivilege, MyGlobalAuthApiService>();
```

```csharp
// In a handler or Blazor component
@inject IGlobalAuthorizationService<MyPrivilege> AuthService

var ctx = await AuthService.GetContext();
if (ctx.MayUserDo(MyPrivilege.DeleteReport)) { ... }

// Convenience extensions
bool can = await AuthService.MayUserDo(MyPrivilege.DeleteReport);
bool canAny = await AuthService.MayUserDoAny(MyPrivilege.EditReport, MyPrivilege.DeleteReport);
bool canAll = await AuthService.MayUserDoAll(MyPrivilege.ViewReport, MyPrivilege.EditReport);
```

### Role-and-Permission Authorization

Adds per-user permission overrides (`Permission<TPrivilege>`) on top of role-based grants:

```csharp
builder.Services.AddMetalGuardianRoleAndPermissionAuthorization<MyPrivilege, MyRole, MyRoleAndPermRepo>();
```

```csharp
public class MyRoleAndPermRepo : IRoleAndPermissionAuthorizationRepository<MyPrivilege, MyRole>
{
    // from IRoleOnlyAuthorizationRepository:
    public Task<IDictionary<MyRole, MyPrivilege[]>> GetRolePrivileges() { ... }
    public Task<MyRole[]> GetUserRoles(Guid userId) { ... }
    // per-user overrides:
    public Task<Permission<MyPrivilege>[]> GetUserPermissions(Guid userId) { ... }
}
```

A `Permission<TPrivilege>` has `Privilege` and `IsAllowed`. A user-level `IsAllowed = false` overrides a role grant; `IsAllowed = true` grants access even without the role.

### Entity Authorization

Scope authorization checks to a specific entity (e.g. a workspace, project, or folder) identified by a `Guid`:

```csharp
builder.Services.AddMetalGuardianEntityAuthorization<MyPrivilege, MyRole, MyEntityAuthRepo>();
```

```csharp
public class MyEntityAuthRepo : IEntityAuthorizationRepository<MyPrivilege, MyRole>
{
    // global role/permission methods (from IRoleAndPermissionAuthorizationRepository)...
    // entity-scoped overrides:
    public Task<MyRole[]> GetUserRoles(Guid securedEntityId, Guid userId) { ... }
    public Task<Permission<MyPrivilege>[]> GetUserPermissions(Guid securedEntityId, Guid userId) { ... }
}
```

```csharp
@inject IEntityAuthorizationService<MyPrivilege> AuthService

var ctx = await AuthService.GetContext(entityId);
bool can = ctx.MayUserDo(MyPrivilege.EditReport);

// Convenience extensions
bool can = await AuthService.MayUserDo(entityId, MyPrivilege.EditReport);
```

### Hierarchical Authorization

For tree-structured entities (e.g. folder hierarchies) where permissions are inherited from ancestors:

```csharp
builder.Services.AddMetalGuardianHierarchialAuthorization<MyPrivilege, MyRole, MyHierarchyRepo>();
```

```csharp
public class MyHierarchyRepo : IHierarchialAuthorizationRepository<MyPrivilege, MyRole>
{
    // ...entity methods...
    // Returns ancestor ids ordered root-first
    public Task<Guid[]> GetAncestry(Guid securedEntityId) { ... }
}
```

The hierarchical engine walks the ancestry chain and merges permissions, so a grant on a parent folder flows down to all children.

### Authorization Cache Busting

All authorization engines cache results in a singleton `IAuthorizationCache` to avoid repeated database queries within a request. Bust the cache when permissions change:

```csharp
@inject IAuthorizationCache AuthCache

// Reset cache for a specific user or entity
AuthCache.BustCache(userId: changedUserId);
AuthCache.BustCache(entityId: changedEntityId);

// Reset everything
AuthCache.BustCache();
```

### Client-Side Authorization

Register the corresponding client-side service using `AddMetalGuardianGlobalAuthorization` or `AddMetalGuardianEntityAuthorization` with your `IGlobalAuthorizationApiService` / `IEntityAuthorizationApiService` implementation (typically a MetalNexus API call to the server's authorization endpoint):

```csharp
// Client services registration
services.AddMetalGuardianGlobalAuthorization<MyPrivilege, MyGlobalAuthApiService>();
services.AddMetalGuardianEntityAuthorization<MyPrivilege, MyEntityAuthApiService>();
```

Both accept an optional `connectionName` to target a specific named `HttpClient`.

---

## ICurrentUser

Inject `ICurrentUser` in server-side handlers to read the authenticated user's identity and claims:

```csharp
public class GetMyProfileHandler : IRequestHandler<GetMyProfileRequest, Profile>
{
    public GetMyProfileHandler(ICurrentUser currentUser) => _currentUser = currentUser;
    private readonly ICurrentUser _currentUser;

    public Task<Profile> Handle(GetMyProfileRequest request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated) throw new UnauthorizedAccessException();
        var tenantId = _currentUser.GetGuidClaim("tenant");
        return Task.FromResult(new Profile
        {
            UserId = _currentUser.UserId,
            Name = _currentUser.UserName,
        });
    }
}
```

`ICurrentUser` members:

| Member | Description |
|---|---|
| `IsAuthenticated` | `true` when `UserId != Guid.Empty` |
| `UserId` | Authenticated user's `Guid` |
| `UserName` | Authenticated user's display name |
| `HasRole(role)` | Check a role claim |
| `GetClaim(name)` | Get a single claim value by type |
| `GetClaimValues(name)` | Get all values for a multi-value claim |
| `GetGuidClaim(name)` / `GetGuidClaims(name)` | Parse claim(s) as `Guid` |

---

## Password Validation

Enable the built-in password validator:

```csharp
options.UsePasswordValidator(req =>
{
    req.MinimumLength = 10;
    req.RequireSymbol = true;
    req.RequireUpperCase = true;
    req.RequireLowerCase = true;
    req.RequireDigit = true;
});
```

Inject `IPasswordValidator` wherever needed:

```csharp
public class RegisterHandler : IRequestHandler<RegisterRequest>
{
    public RegisterHandler(IPasswordValidator passwordValidator) =>
        _passwordValidator = passwordValidator;
    private readonly IPasswordValidator _passwordValidator;

    public Task Handle(RegisterRequest request, CancellationToken ct)
    {
        if (!_passwordValidator.ValidatePassword(request.Password, request.UserName))
            throw new ArgumentException(_passwordValidator.GetPasswordRequirementsMessage(request.UserName));
        // ...
        return Task.CompletedTask;
    }
}
```

`GetPasswordRequirementsMessage` and `ValidatePassword` both accept `params string?[] forbiddenFragments` — pass values like the user's name or email to prevent them appearing in the password.

---

## Blazor Utilities

### `<RedirectTo>` Component

A component that immediately performs a hard navigation to a URL — useful for protecting routes:

```razor
@if (!authClient.IsAuthenticated())
{
    <RedirectTo Url="/login" />
    return;
}
```

### `AuthenticationStateProvider`

`UseBlazorAuthentication()` registers `MetalGuardianAuthenticationStateProvider` as the `AuthenticationStateProvider`. It bridges `IMetalGuardianAuthenticationClient` events to the Blazor auth state cascade, enabling `<AuthorizeView>`, `[Authorize]`, and `AuthenticationStateTask` to work without any extra wiring.

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalNexus`](../MetalNexus/README.md) | HTTP mediator bridge — MetalGuardian's endpoints use MetalNexus |
| [`RossWright.MetalChain`](../MetalChain/README.md) | Mediator pattern: `IRequest` / `IRequestHandler` / `IMediator` |
| [`RossWright.MetalCore`](../MetalCore/RossWright.MetalCore/README.md) | Foundation utilities, SMTP/SMS messaging contracts |
| [`RossWright.MetalInjection`](../MetalInjection/README.md) | Ground-up `IServiceProvider` with attribute/interface-based registration |
| [`RossWright.MetalCommand`](../MetalCommand/README.md) | Interactive console application host |
| [`RossWright.MetalShout`](../MetalShout/README.md) | Server-to-client push via SignalR |

---

## License

All **Ross Wright Metal Libraries** including this one are licensed under **Apache License 2.0 with Commons Clause**.

**You are free to**:
- Use the libraries in any project (personal or commercial)
- Modify them
- Include them in products or services you sell

**You may not**:
- Sell the libraries themselves (or any product/service whose *primary* value comes from the libraries)

See [LICENSE](LICENSE) for the full text.
Docs go here. Update ToC as you add sections.

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
