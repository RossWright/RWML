# RossWright.MetalGuardian.MFA.TOTP

Client-side TOTP MFA add-on for MetalGuardian. Provides the MetalNexus request types for the TOTP setup/verify/reset flow and `IAuthenticationInformation` extension helpers for reading TOTP claim state. Add this package to both the client project and any shared contracts project.

## Installation

```powershell
dotnet add package RossWright.MetalGuardian.MFA.TOTP
```

The server-side counterpart is `RossWright.MetalGuardian.Server.MFA.TOTP`.

## Quick Start

### Register the client-side endpoints

```csharp
// Blazor WASM — Program.cs
builder.AddMetalGuardianClient(options =>
{
    options.UseMetalNexusAuthenticationEndpoints();
    options.UseMetalNexusTotpMfaEndpoints(); // add this
    // ...
});
```

### TOTP setup flow

```razor
@inject IMediator Mediator
@inject IMetalGuardianAuthenticationClient AuthClient

@if (authInfo.NeedsToSetupTotpMfa())
{
    <img src="@qrCodeDataUri" alt="Scan with your authenticator app" />
    <input @bind="code" placeholder="Enter 6-digit code" />
    <button @onclick="VerifyAsync">Verify</button>
}

@code {
    IAuthenticationInformation? authInfo;
    string? qrCodeDataUri;
    string code = "";

    protected override async Task OnInitializedAsync()
    {
        authInfo = AuthClient.GetUser();
        var setup = await Mediator.Send(new SetupTotp.Request());
        qrCodeDataUri = setup.QrCodeDataUri;
    }

    async Task VerifyAsync()
    {
        // VerifyTotpMfa returns a full (non-provisional) AuthenticationTokens on success
        await Mediator.Send(new VerifyTotpMfa.Request { Code = code });
    }
}
```

### Check TOTP state from `IAuthenticationInformation`

```csharp
bool needsSetup = authInfo.NeedsToSetupTotpMfa();  // user must enroll
bool hasEnabled  = authInfo.HasTotpMfaEnabled();    // TOTP already enrolled
```

### Built-in MetalNexus endpoints

| Request type | Endpoint | Description |
|---|---|---|
| `SetupTotp.Request` | `GET /Authentication/SetupTotp` | Returns QR code data URI; requires provisional auth |
| `VerifyTotpMfa.Request` | `POST /Authentication/VerifyTotp` | Verifies code; returns full tokens on success |
| `ResetTotpMfa.Request` | `POST /Authentication/ResetTotp` | Admin reset of TOTP enrollment; requires full auth |

## Documentation

Full documentation is available in the [MetalGuardian README](../../MetalGuardian/README.md).
