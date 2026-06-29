# Add MetalGuardian Server Authentication

Use this recipe when an ASP.NET Core server needs JWT login, refresh, logout, current-user access, and MetalNexus authentication endpoints.

## Install

```bash
dotnet add package RossWright.MetalGuardian.Server
dotnet add package RossWright.MetalGuardian.Server.MFA.TOTP
```

Use the TOTP package only when multi-factor authentication is needed.

## Namespace

```csharp
using RossWright;
```

## Setup

```csharp
builder.AddMetalGuardianServer(options =>
{
	options.UseMetalNexusAuthenticationEndpoints();
	options.UseJwtConfigurationSection("MetalGuardian");
	options.MapDatabaseAuthentication<AppDbContext, AppUser>(
		identity => user => user.Email == identity);
});
```

## Current User

```csharp
public sealed class GetProfileHandler(ICurrentUser currentUser)
	: IRequestHandler<GetProfileRequest, ProfileDto>
{
	public Task<ProfileDto> Handle(
		GetProfileRequest request,
		CancellationToken cancellationToken)
	{
		var userId = currentUser.UserId;
		return Task.FromResult(new ProfileDto { UserId = userId });
	}
}
```

## Optional TOTP MFA

```csharp
builder.AddMetalGuardianServer(options =>
{
	options.UseTotpMfa<AppUser>(mfa =>
	{
		mfa.UseMetalNexusTotpMfaEndpoints();
	});
});
```

## Reach For This When

- You need authentication for an ASP.NET Core server.
- You want built-in MetalNexus login, logout, and refresh endpoints.
- You need current-user access in handlers and services.

## Notes For Agents

- 2026.2 exposes authentication, not the deferred authorization framework.
- Keep JWT signing keys in configuration or secrets.
- Pair this with `RossWright.MetalGuardian.Blazor` for Blazor WebAssembly clients.
