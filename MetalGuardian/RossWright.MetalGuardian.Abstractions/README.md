# RossWright.MetalGuardian.Abstractions

Shared contracts, interfaces, and built-in MetalNexus request types for the MetalGuardian authentication and authorization system. Add this package to any shared project that needs to reference MetalGuardian types without taking a dependency on the full client or server library.

## Installation

```powershell
dotnet add package RossWright.MetalGuardian.Abstractions
```

## Quick Start

Reference this package in a shared contracts project. The types you'll use most often:

```csharp
// Inject IMetalGuardianAuthenticationClient anywhere on the client side
public class MyViewModel(IMetalGuardianAuthenticationClient authClient)
{
    public async Task LoginAsync(string email, string password, CancellationToken ct)
    {
        IAuthenticationInformation? info = await authClient.Login(email, password, ct);
        if (info is null) return; // login failed
        Console.WriteLine($"Welcome, {info.UserName}");
    }
}

// Check authentication state without hitting the server
IAuthenticationInformation? user = authClient.GetUser();
bool isProvisional = user?.IsProvisional ?? false; // true = MFA still pending
bool isKnownDevice = user?.IsKnownDevice ?? false;

// Subscribe to auth state changes
authClient.AuthenticationChanged += (connectionName, info, ct) =>
{
    // react to login / logout / refresh
    return Task.CompletedTask;
};
```

Built-in MetalNexus request types (used by `IMetalGuardianAuthenticationClient` internally):

| Type | Endpoint | Description |
|---|---|---|
| `Login.Request` | `POST /Authentication/Login` | Credential login |
| `Logout.Request` | `POST /Authentication/Logout` | Revokes refresh token |
| `Refresh.Request` | `POST /Authentication/Refresh` | Exchanges refresh token for new tokens |

MetalGuardian-specific claim type constants are defined in `ClaimTypes` (e.g. `ClaimTypes.UserId`, `ClaimTypes.UserName`, `ClaimTypes.IsProvisional`).

## Documentation

Full documentation is available in the [MetalGuardian README](../../MetalGuardian/README.md).
