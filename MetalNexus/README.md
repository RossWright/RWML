# Ross Wright's Metal Nexus
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Packages](#packages)
- [Installation](#installation)
- [Defining API Requests](#defining-api-requests)
  - [HTTP Protocol Selection](#http-protocol-selection)
  - [Path and Tag](#path-and-tag)
  - [Authentication Attributes](#authentication-attributes)
  - [Upload Limit Attributes](#upload-limit-attributes)
  - [Per-request HTTP Timeout](#per-request-http-timeout)
  - [Header Properties](#header-properties)
  - [Raw Request Body](#raw-request-body)
- [Server Setup](#server-setup)
  - [Registering the Server](#registering-the-server)
  - [Swagger / OpenAPI](#swagger--openapi)
  - [Endpoint Schema Options](#endpoint-schema-options)
  - [Path Strategies](#path-strategies)
  - [Custom Endpoint Schema](#custom-endpoint-schema)
  - [Upload Size Limits](#upload-size-limits)
- [Client Setup](#client-setup)
  - [Blazor WebAssembly](#blazor-webassembly)
  - [Console App (MetalCommand)](#console-app-metalcommand)
  - [Multiple Connections](#multiple-connections)
- [Sending Requests](#sending-requests)
  - [From Blazor or Client Code](#from-blazor-or-client-code)
  - [SendVia — Targeting a Specific Server](#sendvia--targeting-a-specific-server)
- [File Upload](#file-upload)
  - [Server Side](#server-side)
  - [Blazor Client: `<FileInput>`](#blazor-client-fileinput)
- [Exception Handling](#exception-handling)
- [Advanced Registration](#advanced-registration)
- [See Also](#see-also)
- [License](#license)
- [Changelog](CHANGELOG.md)

---

## Overview

MetalNexus bridges [MetalChain](https://www.nuget.org/packages/RossWright.MetalChain) across the network. Decorate a request with `[ApiRequest]` and MetalNexus automatically generates a RESTful HTTP endpoint on the server, registers the corresponding client-side `IRequestHandler` that calls it, handles serialization, and marshals exceptions back to the caller — with no HTTP client code to write.

Clients can be Blazor WebAssembly, MetalCommand console apps, or any other .NET project that references the client package.

| Feature | Description |
|---|---|
| Zero-boilerplate HTTP | `[ApiRequest]` on a request class is all that's needed |
| Flexible HTTP protocol | Auto, GET, POST (body/query), PUT, PATCH, DELETE — per request |
| Endpoint schema customization | Configurable path prefix, casing, suffix trimming, path strategies, or full custom schema |
| Authentication & authorization | `[Authenticated]` / `[Anonymous]` / `[AllowProvisional]` attributes; compatible with MetalGuardian or any ASP.NET Core auth |
| Swagger / OpenAPI | One `UseMetalNexus()` call wires up JWT security definition and document filter |
| File upload | `MetalNexusFileRequest`, `[UploadLimit]` / `[NoUploadLimit]`, and Blazor `<FileInput>` component |
| Exception marshalling | Throw on the server, catch on the client as `MetalNexusException` or `InternalServerErrorException` |
| Multiple connections | Named `HttpClient` connections route requests to different servers |
| Header properties | `[FromHeader]` sends a request property as an HTTP header |
| Provisional auth | `AllowProvisional = true` on `[Authenticated]` for MFA and multi-step auth flows |

---

## Packages

| Package | NuGet | Description |
|---|---|---|
| `RossWright.MetalNexus.Abstractions` | [NuGet](https://www.nuget.org/packages/RossWright.MetalNexus.Abstractions) | `[ApiRequest]` and all MetalNexus contracts — add to shared request projects |
| `RossWright.MetalNexus.Server` | [NuGet](https://www.nuget.org/packages/RossWright.MetalNexus.Server) | ASP.NET Core server: generates endpoints, Swagger filter, authentication hooks, multipart file upload |
| `RossWright.MetalNexus.Blazor` | [NuGet](https://www.nuget.org/packages/RossWright.MetalNexus.Blazor) | Blazor WebAssembly client: `AddMetalNexusClient`, `AddHttpClient`, and `<FileInput>` component |
| `RossWright.MetalNexus` | [NuGet](https://www.nuget.org/packages/RossWright.MetalNexus) | Core client for console and non-Blazor .NET projects |

---

## Installation

**Shared request project (models only):**

```powershell
dotnet add package RossWright.MetalNexus.Abstractions
```

**ASP.NET Core server:**

```powershell
dotnet add package RossWright.MetalNexus.Server
```

**Blazor WebAssembly client:**

```powershell
dotnet add package RossWright.MetalNexus.Blazor
```

**Console / other .NET client:**

```powershell
dotnet add package RossWright.MetalNexus
```

---

## Defining API Requests

### HTTP Protocol Selection

Decorate any `IRequest` or `IRequest<TResponse>` with `[ApiRequest]` to expose it as an HTTP endpoint.

```csharp
[ApiRequest]
public class GetProductRequest : IRequest<Product>
{
	public int Id { get; set; }
}
```

The `HttpProtocol` parameter controls the HTTP method and how parameters are sent:

| Value | HTTP Method | Parameters |
|---|---|---|
| `Auto` (default) | GET for ≤`MaximumRequestParameters` props, otherwise POST body | GET=query, POST=body |
| `Get` | GET | Query string |
| `PostViaBody` | POST | JSON body |
| `PostViaQuery` | POST | Query string |
| `PutViaBody` | PUT | JSON body |
| `PutViaQuery` | PUT | Query string |
| `PatchViaBody` | PATCH | JSON body |
| `PatchViaQuery` | PATCH | Query string |
| `Delete` | DELETE | Query string |

```csharp
[ApiRequest(HttpProtocol.PostViaBody)]
public class CreateOrderRequest : IRequest<OrderResult>
{
	public string ProductId { get; set; } = null!;
	public int Quantity { get; set; }
}
```

### Path and Tag

By default MetalNexus derives the path from the request type's namespace and name (using the configured path strategy). Override explicitly with the `path` parameter, and group the endpoint in Swagger with `tag`:

```csharp
[ApiRequest(path: "products/{Id}", tag: "Products")]
public class GetProductRequest : IRequest<Product>
{
	public int Id { get; set; }
}
```

Curly-brace segments are matched to request properties by name and substituted into the URL path automatically.

### Authentication Attributes

```csharp
// Requires authentication; optionally restrict to specific roles
[Authenticated]
public class GetAccountRequest : IRequest<Account> { }

[Authenticated("Admin", "SuperUser")]
public class DeleteAccountRequest : IRequest { }

// Explicitly opt out when RequiresAuthenticationByDefault = true
[Anonymous]
public class GetPublicCatalogRequest : IRequest<Catalog> { }
```

`AllowProvisional = true` allows provisionally-authenticated users (e.g. mid-MFA-flow) to reach the endpoint:

```csharp
[Authenticated(AllowProvisional = true)]
public class SubmitMfaCodeRequest : IRequest<AuthResult> { }
```

### Upload Limit Attributes

Apply to file-upload request classes to control the multipart body size limit on the server:

```csharp
[UploadLimit(50 * 1024 * 1024)]   // 50 MB
public class UploadAvatarRequest : MetalNexusFileRequest { }

[NoUploadLimit]                    // remove the limit entirely
public class UploadLargeVideoRequest : MetalNexusFileRequest { }
```

### Per-request HTTP Timeout

Override the `HttpClient` timeout for a specific request on the client side:

```csharp
[HttpClientTimeout(120)]   // seconds
public class GenerateReportRequest : IRequest<ReportResult> { }
```

### Header Properties

Mark a property to be sent as an HTTP request header instead of a body or query parameter:

```csharp
[ApiRequest]
public class GetSecureDataRequest : IRequest<SecureData>
{
	[FromHeader("X-Tenant-Id")]
	public string TenantId { get; set; } = null!;
}
```

### Raw Request Body

Implement `IMetalNexusRawRequest` (or `IMetalNexusRawRequest<TResponse>`) to receive the raw JSON request body string on the server without automatic deserialization:

```csharp
public class WebhookRequest : IMetalNexusRawRequest
{
	public string? RawRequestBody { get; set; }
}
```

---

## Server Setup

### Registering the Server

```csharp
// Program.cs — ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

builder.AddMetalNexusServer(options =>
{
	options.ScanAssemblyContaining<GetProductRequest>();
});

var app = builder.Build();
app.UseMetalNexusServer();
app.Run();
```

`AddMetalNexusServer` uses assembly scanning (via MetalCore's `AssemblyScanningOptionsBuilder`) to discover all `[ApiRequest]`-decorated types. `UseMetalNexusServer` registers the MetalNexus middleware and, if ASP.NET Core authentication is present, calls `UseAuthentication` automatically.

### Swagger / OpenAPI

```csharp
builder.Services.AddSwaggerGen(options =>
{
	options.UseMetalNexus();   // adds JWT security definition + document filter
});
```

### Endpoint Schema Options

Control path generation globally via `ConfigureEndpointSchema`:

```csharp
builder.AddMetalNexusServer(options =>
{
	options.ScanAssemblyContaining<GetProductRequest>();
	options.ConfigureEndpointSchema(schema =>
	{
		schema.ApiPathPrefix = "api";               // prepend /api/ to all paths
		schema.ApiPathToLower = true;               // lowercase all paths
		schema.RequestSuffixesToTrim = ["Request"]; // trim "Request" from type name
		schema.RequiresAuthenticationByDefault = true;
		schema.MaximumRequestParameters = 5;        // Auto protocol threshold
	});
});
```

### Path Strategies

A path strategy controls how a request type's namespace is converted into its URL path. The built-in strategies are:

| Strategy | Behaviour | Example |
|---|---|---|
| `TrimDefaultNamespacePathStrategy` (default) | Auto-detects the root namespace and strips it | `MyCorp.MyApp.Users.GetUserRequest` → `/Users/GetUser` |
| `UseFullNameSpacePathStrategy` | Converts the entire namespace to a path | `MyCorp.MyApp.Users.GetUserRequest` → `/MyCorp/MyApp/Users/GetUser` |
| `TrimFixedPreamblePathStrategy` | Strips a fixed namespace prefix you specify | configurable |
| `TrimRequestNamespacePathStrategy` | Strips only the request type's own namespace | type name only |
| `NoNamespacePathStrategy` | Strips all namespace segments | `/GetUser` |

```csharp
options.ConfigureEndpointSchema(schema =>
{
	schema.PathStrategy = new TrimFixedPreamblePathStrategy("MyCorp.MyApp.Endpoints");
});
```

### Custom Endpoint Schema

For complete control implement `ICustomEndpointSchema` and pass it to `UseCustomEndpointSchema`. Each method receives the request type and the proposal computed from attributes and options, and returns the final value:

```csharp
public class MySchema : ICustomEndpointSchema
{
	public string DeterminePath(Type requestType, string proposal) => proposal;
	public string DetermineTag(Type requestType, string proposal) => proposal;
	public HttpProtocol DetermineHttpProtocol(Type requestType, HttpProtocol proposal) => proposal;
	public bool DetermineRequiresAuthentication(Type requestType, bool proposal) => proposal;
	public string[]? DetermineAuthorizedRoles(Type requestType, string[]? proposal) => proposal;
}

options.UseCustomEndpointSchema(new MySchema());
```

### Upload Size Limits

Override the default multipart body size limit for the entire server:

```csharp
options.SetMultipartBodyLengthLimit(100 * 1024 * 1024); // 100 MB default
```

Per-request overrides using `[UploadLimit]` or `[NoUploadLimit]` take precedence.

---

## Client Setup

### Blazor WebAssembly

```csharp
// Program.cs — Blazor WASM
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder
	.AddHttpClient()          // defaults BaseAddress to HostEnvironment.BaseAddress
	.AddMetalNexusClient(options =>
	{
		options.ScanAssemblyContaining<GetProductRequest>();
	});

await builder.Build().RunAsync();
```

Use the overload accepting `connectionName` for named connections (see [Multiple Connections](#multiple-connections)).

### Console App (MetalCommand)

```csharp
ConsoleApplication.CreateBuilder(args)
	.AddHttpClient(client => client.BaseAddress = new Uri("https://api.example.com"))
	.AddMetalNexusClient(options =>
	{
		options.ScanAssemblyContaining<GetProductRequest>();
	})
	.Build()
	.Run();
```

### Multiple Connections

Register multiple named `HttpClient` instances and route specific requests to them via `connectionName` on `[ApiRequest]` or `SetDefaultConnection` on the builder:

```csharp
builder
	.AddHttpClient("orders", c => c.BaseAddress = new Uri("https://orders.example.com"))
	.AddHttpClient("catalog", c => c.BaseAddress = new Uri("https://catalog.example.com"))
	.AddMetalNexusClient(options =>
	{
		options.ScanAssemblyContaining<GetProductRequest>();
		options.SetDefaultConnection("catalog");
	});
```

```csharp
[ApiRequest(connectionName: "orders")]
public class PlaceOrderRequest : IRequest<OrderResult> { }
```

---

## Sending Requests

### From Blazor or Client Code

Inject `IMediator` from MetalChain and send requests normally — MetalNexus registers the HTTP handlers transparently:

```csharp
@inject IMediator Mediator

var product = await Mediator.Send(new GetProductRequest { Id = 42 });
```

### SendVia — Targeting a Specific Server

`SendVia` wraps a request with a connection name, overriding the default at call time:

```csharp
// Without response
await Mediator.SendVia("orders", new CancelOrderRequest { OrderId = 99 }, ct);

// With response
var result = await Mediator.SendVia<OrderResult>("orders", new PlaceOrderRequest { ... }, ct);
```

You can also use the `SendVia<TRequest>` / `SendVia<TRequest, TResponse>` request wrappers directly as MetalChain requests.

---

## File Upload

### Server Side

Derive your request from `MetalNexusFileRequest`. MetalNexus deserializes the uploaded files into `MetalNexusFile[]` on the server:

```csharp
[ApiRequest(HttpProtocol.PostViaBody)]
[UploadLimit(20 * 1024 * 1024)]
public class UploadAvatarRequest : MetalNexusFileRequest { }
```

```csharp
public class UploadAvatarHandler : IRequestHandler<UploadAvatarRequest>
{
	public Task Handle(UploadAvatarRequest request, CancellationToken ct)
	{
		foreach (var file in request.Files)
		{
			// file.FileName, file.ContentType, file.Data, file.IsAttachment
		}
		return Task.CompletedTask;
	}
}
```

### Blazor Client: `<FileInput>`

`<FileInput>` is a headless Blazor component — it manages a hidden `<input type="file">` in JavaScript and exposes a clean event-based API:

```razor
@* Inject and render the component *@
<FileInput @ref="fileInput"
		   AllowMultipleFiles="false"
		   Accept="image/*"
		   FilesPicked="OnFilesPicked"
		   FilePickerCanceled="OnCanceled"
		   FilesUploaded="OnUploaded"
		   UploadFailed="OnFailed"
		   Progress="OnProgress" />

<button @onclick="() => fileInput.OpenFilePicker()">Choose image</button>
```

```csharp
@code {
	FileInput fileInput = null!;

	async Task OnFilesPicked(FileInput.IFilesPickedArgs args)
	{
		// Preview the first selected file in an <img> tag
		await args.ShowImage("preview-img", args.Files[0]);

		// Upload — MetalNexus routes to the server endpoint automatically
		await args.UploadFiles<UploadAvatarRequest>(args.Files);
	}
}
```

`IFilesPickedArgs` members:

| Member | Description |
|---|---|
| `Files` | Array of `BrowserFile` (FileName, ContentType, Size, FileRefId) |
| `ShowImage(imgId, file)` | Sets an `<img>` src or CSS background-image from the in-browser file reference |
| `ShowImage(imgId, url)` | Sets an `<img>` src or CSS background-image from a URL |
| `UploadFiles<TRequest>(files)` | Uploads files to the MetalNexus server endpoint for `TRequest` |
| `UploadFiles<TRequest>(request, files)` | Same, with a pre-populated request instance |

The `Progress` callback receives `IProgressArgs` with `Loaded`, `Total`, `UpdateLoaded`, and `UpdateSpeedKbps` for real-time upload progress reporting.

---

## Exception Handling

MetalNexus marshals server-side exceptions to the client. On the server, throw `MetalNexusException` (or let unhandled exceptions propagate); on the client they surface as `MetalNexusException` with the original message.

```csharp
// Server handler
throw new MetalNexusException("Order not found.");

// Client
try
{
	var result = await Mediator.Send(new GetOrderRequest { Id = id });
}
catch (MetalNexusException ex)
{
	Console.WriteLine(ex.Message);
}
```

Unhandled server exceptions surface as `InternalServerErrorException` on the client. To include the server stack trace in the exception message (useful in development):

```csharp
options.IncludeServerStackTraceOnExceptions();
```

To treat all unhandled server exceptions as `InternalServerErrorException` rather than rethrowing their original type:

```csharp
options.TreatUnknownExceptionsAsInternalServiceError();
```

---

## Advanced Registration

Use `AddMetalNexusEndpoints` to register additional request types from a separate assembly after setup:

```csharp
services.AddMetalNexusEndpoints(typeof(GetProductRequest), typeof(CreateOrderRequest));
```

This can be called before or after `AddMetalNexusServer` / `AddMetalNexusClient` — MetalNexus resolves the registration order automatically.

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalChain`](../MetalChain/README.md) | Mediator pattern: `IRequest` / `IRequestHandler` / `IMediator` |
| [`RossWright.MetalCore`](../MetalCore/RossWright.MetalCore/README.md) | Foundation utilities, assembly scanning, extension methods |
| [`RossWright.MetalGuardian`](../MetalGuardian/README.md) | Authentication and authorization for the Metal stack |
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
To use MetalNexus, ensure your request types are accessible by both your client and server projects, and then configure MetalNexus in both projects as follows. 
Note that any of the `AddMetalNexus...` methods will add MetalChain if you have not already added it, passing it the same set of assemblies for scanning.

### Server Setup
To setup MetalNexus and auto-detect request handlers on an ASP.NET Core project
add the [RossWright.MetalNexus.Server](https://www.nuget.org/packages/RossWright.MetalNexus.Server/) package to your project, call `AddMetalNexusServer` on the builder in your program.cs file passing the assembly containing your request handlers,
and call `app.UseMetalNexusServer()` on the built app. 
Like this:
```csharp
var builder = WebApplication.CreateBuilder(args);
...
builder.AddMetalNexus(_ => _.ScanThisAssembly());
...
var app = builder.Build();
...
app.UseMetalNexusServer();
...
app.Run();
```

### Blazor Client Setup
To setup MetalNexus and auto-detect requests on an Blazor project 
add the [RossWright.MetalNexus.Blazor](https://www.nuget.org/packages/RossWright.MetalNexus.Blazor/) package
and call `AddMetalNexusClient` on the builder in your program.cs file passing the assembly containing your api reqeusts.
Like this:
```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.AddMetalNexusClient(_ => _.ScanAssemblyContaining<MyRequest>());
```

### MetalCommand Client Setup
To setup MetalNexus and auto-detect requests on a MetalCommand console project,
add the [RossWright.MetalNexus](https://www.nuget.org/packages/RossWright.MetalNexus/) package
and call `AddMetalNexusClient` on the builder in your program.cs file passing the assembly containing your api reqeusts.
Like this:
```csharp
var builder = ConsoleApplication.CreateBuilder(args);
builder.AddMetalNexusClient(_ => _.ScanAssemblyContaining<MyRequest>());
```

### Other Clients Setup
To setup MetalNexus and auto-detect requests on any other kind of client project
add the [RossWright.MetalNexus](https://www.nuget.org/packages/RossWright.MetalNexus/) package
and call `AddMetalNexus` on your service collection passing the assembly containing your api reqeusts.
Like this:
```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddMetalNexus(_ => _.ScanAssemblyContaining<MyRequest>());
```

---
## Defining API Requests
To specify a MetalChain Request should be handled on the server add the 
decorate the request class with the `ApiRequest` attribute. Like this:
```csharp
[ApiRequest]
public class MyRequest : IRequest<MyResponse>
```
Without parameters, an HTTP verb and serialization will be chosen based on the complexity of the Request class 
and a path is generated based on the Request class name and namespace. You can specify the HTTP verb, request 
transmission method (Query or Body) and path explicitly using parameters of the `ApiRequest` attribute like this:
```csharp
[ApiRequest(HttpProtocol.PostViaBody, "/api/request")]
public class MyRequest : IRequest<MyResponse>
```
Note Body is only possible for POST, PUT and PATCH verbs. GET and DELETE verbs are also available. Serializing 
the request as query parameters limits the allowable complexity of the Request object considerably. Note when 
transmiting sensitive data you should always specify a body transmission protocol to keep sensitive data our of the url.

If your project containing your request classes does not reference any of the MetalNexus packages, you can add a reference to the
[RossWright.MetalNexus.Abstractions](https://www.nuget.org/packages/RossWright.MetalInjection.Abstractions/) package
for a super-lightweight dependency that only contains attributes and abstractions used by MetalNexus.

---
## Server-side Handlers
To implement a request handler for a MetalNexus API request, just implement an IRequestHandler for the request 
as you would normally do with MetalChain. Ensure the assembly containing the request handler is included in the
assemblies scanned when MetalNexus is initialized on the server. 
```csharp
public class MyRequestHandler : IRequestHandler<MyRequest, MyResponse>
{
	public async Task<MyResponse> Handle(MyRequest request, CancellationToken cancellationToken)
	{
		// Handle the request as you normally would
	}
}

```

---
## Calling API Requests
MetalNexus API Requests are sent just like normal MetalChain requests.
```csharp
var response = await _mediator.Send(new MyRequest());
```
Assuming `MyRequest` is decorated with the `ApiRequest` attribute, a MetalNexus client-side request handler will handle the request 
by making the HTTP call to the server using whatever HttpClient you've setup using `AddHttpClient` and re-send the request on the 
server to be handled by your RequestHandler.

If you have multiple HTTPClients setup using named clients, you can specify 
which client to use for a specific request using the `ConnectionName` parameter of the `ApiRequest` attribute, like this:
```csharp
[ApiRequest(connectionName: "MyConnection")]
public class MyRequest : IRequest<MyResponse>
```
Or you can use the MetalNexus extension `SendVia` on IMediator to specify the connection name when sending the request, like this:
```csharp
var response = await _mediator.SendVia("MyConnection", new MyRequest());
```

---
## Authentication
MetalNexus does not have it's own authentication system, but it is designed to work with any authentication system that can be used in ASP.NET Core.
Check out [MetalGuardian](https://www.nuget.org/packages/rosswright.metalguardian) for a complete authentication and authorization system 
that is just as easy to setup as MetalNexus and even provides MetalNexus endpoints for common authentication and authorization operations. 

You can specify whether a request requires authentication or not using the `Authenticated` and `Anonymous` attributes. Like this:
```csharp
[Anonymous, ApiRequest(HttpProtocol.Get, "/api/request")]
public class MyRequest : IRequest<MyResponse[]>
```
If a specific role is required, this can be passed as a parameter to the Authenticated attribute, like this:
```csharp
[Authenticated("Admin"), ApiRequest(HttpProtocol.Get, "/api/request")]
public class MyRequest : IRequest<MyResponse[]>
```
Note that MetalNexus assumes authentication is required unless the Anonymous attribute is specified. This default can 
be changed by calling `MakeEndpointsAnonymousByDefault` when initializing MetalNexus on the server (or when configuring the Endpoint Schema).

## Swagger Support
MetalNexus can generate OpenAPI documentation for your API using Swagger. To enable this, call `UseMetalNexus` when initializing Swagger in your ASP.NET Core project, like this:
```csharp
builder.Services.AddSwaggerGen(c =>
{
	...
	c.UseMetalNexus();
});
```
You can specify the tag to be used for your API requests in the generated documentation using the tag parameter of the ApiRequest attribute, like this:
```csharp
[ApiRequest(HttpProtocol.Get, "/api/request", "My Important Requests")]
public class MyRequest : IRequest<MyResponse[]>
```
When you view your api using the Swagger UI, your requests will be grouped under the specified tag. 
If you do not specify a tag, a tag is generated based on the path of the request.

---
## File Upload
To handle file uploads, derive your request from "MetalNexusFileRequest"

---
## Exception Handling
Error handling via exception marshalling - throw on the server, catch on the client

---
## Esoterica
The above covers 90% of your typical usage of MetalNexus. Below you can find information more specialized capabilities and behaviors of the library.

### To be documented:
* Support for specifying request properties be sent as header parameters.
* Support for provisionally authenticated endpoints useful for MFA and other multi-step authentication flows.
* Raw request and response access for maximum flexibility when needed.


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