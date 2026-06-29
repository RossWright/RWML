# RossWright.MetalGuardian.Server.MFA.TOTP

Server-side TOTP MFA add-on for MetalGuardian. Provides QR code generation, code verification, device-remember, admin reset, and the `IMetalGuardianTotpMfaService` interface. Requires `RossWright.MetalGuardian.Server`.

## Installation

```powershell
dotnet add package RossWright.MetalGuardian.Server.MFA.TOTP
```

The client-side counterpart is `RossWright.MetalGuardian.MFA.TOTP`.

## Quick Start

### Server setup

```csharp
// Program.cs — ASP.NET Core
builder.AddMetalGuardianServer(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();
    options.MapDatabaseAuthentication<MyDbContext, MyUser>(...);
    options.UseTotpMfa<MyUser>(mfa =>
    {
        mfa.SetIssuer("My App");          // label shown in authenticator apps (Google Authenticator, Authy, etc.)
        mfa.SetDeviceRememberDays(30);    // days a trusted device can skip TOTP re-verification
        mfa.UseMetalNexusTotpMfaEndpoints();
    });
});
```

Your user entity must implement `ITotpMfaAuthenticationUser`:

```csharp
public class MyUser : ITotpMfaAuthenticationUser
{
    // IAuthenticationUser members ...
    public string? MfaTotpSecret { get; set; }
    public bool IsMfaTotpEnabled { get; set; }
    public bool IsMfaTotpRequired => true; // or per-user logic
}
```

### Admin reset via IMetalGuardianTotpMfaService

```csharp
public class AdminResetTotpHandler(IMetalGuardianTotpMfaService totpService)
    : IRequestHandler<AdminResetTotpRequest>
{
    public Task Handle(AdminResetTotpRequest request, CancellationToken ct) =>
        totpService.ResetUser(request.UserId, ct);
}
```

### TOTP flow summary

1. **Login** — server issues a *provisional* JWT when `IsMfaTotpRequired` is true.
2. **`SetupTotp`** — if `IsMfaTotpEnabled` is false, client calls `SetupTotp.Request` to get a QR code, user scans it into their authenticator app.
3. **`VerifyTotpMfa`** — client submits the 6-digit code; server validates and returns full (non-provisional) `AuthenticationTokens`.
4. **Device remember** — on success, the device is recorded; on subsequent logins from the same device within `SetDeviceRememberDays`, the provisional-to-full step is skipped automatically.

## Documentation

Full documentation is available in the [MetalGuardian README](../../MetalGuardian/README.md).
