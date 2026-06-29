# MetalNexus API Index

Primary namespace: `RossWright`.

## RossWright.ApiRequestAttribute

Package: `RossWright.MetalNexus.Abstractions`  
Namespace: `RossWright`  
Summary: Marks a MetalChain request as an HTTP endpoint that MetalNexus can expose on the server and call from clients.

## RossWright.AuthenticatedAttribute

Package: `RossWright.MetalNexus.Abstractions`  
Namespace: `RossWright`  
Summary: Requires an authenticated caller for an API request endpoint.

## RossWright.AnonymousAttribute

Package: `RossWright.MetalNexus.Abstractions`  
Namespace: `RossWright`  
Summary: Allows anonymous access to an API request endpoint.

## RossWright.FromHeaderAttribute

Package: `RossWright.MetalNexus.Abstractions`  
Namespace: `RossWright`  
Summary: Sends or receives a request property through an HTTP header.

## RossWright.ProducesErrorAttribute

Package: `RossWright.MetalNexus.Abstractions`  
Namespace: `RossWright`  
Summary: Documents expected error responses for generated Swagger/OpenAPI output.

## RossWright.MetalNexusServerExtensions.AddMetalNexusServer

Package: `RossWright.MetalNexus.Server`  
Namespace: `RossWright`  
Summary: Registers MetalNexus server endpoint generation and MetalChain dispatch for ASP.NET Core.

## RossWright.MetalNexusServerExtensions.UseMetalNexusServer

Package: `RossWright.MetalNexus.Server`  
Namespace: `RossWright`  
Summary: Adds MetalNexus endpoint middleware to an ASP.NET Core pipeline.

## RossWright.MetalNexusBlazorExtensions.AddMetalNexusClient

Package: `RossWright.MetalNexus.Blazor`  
Namespace: `RossWright`  
Summary: Registers Blazor WebAssembly client-side HTTP dispatch handlers for ApiRequest types.

## RossWright.MetalNexusExtensions.AddMetalNexusClient

Package: `RossWright.MetalNexus`  
Namespace: `RossWright`  
Summary: Registers non-Blazor client-side HTTP dispatch handlers for ApiRequest types.

## RossWright.IMetalNexusUrlHelper

Package: `RossWright.MetalNexus.Blazor` / `RossWright.MetalNexus`  
Namespace: `RossWright`  
Summary: Builds direct endpoint URLs for ApiRequest instances without sending the request.

## RossWright.FileInput

Package: `RossWright.MetalNexus.Blazor`  
Namespace: `RossWright`  
Summary: Blazor file input component that creates MetalNexus-compatible browser file values.
