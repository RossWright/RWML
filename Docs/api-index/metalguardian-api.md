# MetalGuardian API Index

Primary namespace: `RossWright`.

## RossWright.MetalGuardianServerExtensions.AddMetalGuardianServer

Package: `RossWright.MetalGuardian.Server`  
Namespace: `RossWright`  
Summary: Registers MetalGuardian authentication services for ASP.NET Core.

## RossWright.MetalGuardianBlazorExtensions.AddMetalGuardianClient

Package: `RossWright.MetalGuardian.Blazor`  
Namespace: `RossWright`  
Summary: Registers MetalGuardian authentication services for Blazor WebAssembly.

## RossWright.IMetalGuardianAuthenticationClient

Package: `RossWright.MetalGuardian.Abstractions`  
Namespace: `RossWright`  
Summary: Client contract for login, logout, refresh, and authentication-state operations.

## RossWright.ICurrentUser

Package: `RossWright.MetalGuardian.Server`  
Namespace: `RossWright`  
Summary: Server-side abstraction for accessing the current authenticated user.

## RossWright.MetalGuardianServerConfiguration

Package: `RossWright.MetalGuardian.Server`  
Namespace: `RossWright`  
Summary: JWT issuer, audience, signing key, and token lifetime configuration.

## RossWright.UseMetalNexusAuthenticationEndpoints

Package: `RossWright.MetalGuardian.Server` / `RossWright.MetalGuardian.Blazor`  
Namespace: `RossWright`  
Summary: Wires built-in MetalGuardian authentication endpoints through MetalNexus.

## RossWright.UseBlazorAuthentication

Package: `RossWright.MetalGuardian.Blazor`  
Namespace: `RossWright`  
Summary: Registers Blazor authentication state support for MetalGuardian.

## RossWright.AddAuthenticatedHttpClient

Package: `RossWright.MetalGuardian.Blazor`  
Namespace: `RossWright`  
Summary: Adds an HTTP client that sends the current JWT with MetalNexus requests.

## RossWright.IMetalGuardianTotpMfaService

Package: `RossWright.MetalGuardian.Server.MFA.TOTP`  
Namespace: `RossWright`  
Summary: Server-side service for TOTP enrollment, verification, and MFA flows.
