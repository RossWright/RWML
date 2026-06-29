# Add MetalGuardian Blazor Authentication

Use this recipe when a Blazor WebAssembly app needs login/logout/refresh behavior, authentication state, and authenticated MetalNexus calls.

## Install

```bash
dotnet add package RossWright.MetalGuardian.Blazor
```

Use shared request/response contracts from:

```bash
dotnet add package RossWright.MetalGuardian.Abstractions
```

## Namespace

```csharp
using RossWright;
```

## Setup

```csharp
builder.AddMetalGuardianClient(options =>
{
	options.UseMetalNexusAuthenticationEndpoints();
	options.AddAuthenticatedHttpClient();
	options.UseBlazorAuthentication();
});
```

## Login And Logout

```csharp
await authClient.Login(username, password, cancellationToken);

await authClient.Logout(cancellationToken);
```

## Reach For This When

- A Blazor WebAssembly app calls authenticated MetalNexus endpoints.
- You need authentication state integrated into Blazor.
- You want token storage and refresh behavior handled by the client library.

## Notes For Agents

- Use the Blazor package in WebAssembly clients.
- Use `RossWright.MetalGuardian.Server` on the ASP.NET Core server.
- A missing decoded access token usually indicates an invalid or unusable token, not authorization failure.
