# Ross Wright's Metal Nexus
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Packages](#packages)
- [Namespaces](#namespaces)
- [Common APIs](#common-apis)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Defining API Requests](#defining-api-requests)
  - [HTTP Protocol Selection](#http-protocol-selection)
  - [Path and Tag](#path-and-tag)
  - [Authentication](#authentication)
- [Server Setup](#server-setup)
  - [Registering the Server](#registering-the-server)
  - [Swagger / OpenAPI](#swagger--openapi)
- [Client Setup](#client-setup)
  - [Blazor WebAssembly](#blazor-webassembly)
  - [Console App (MetalCommand)](#console-app-metalcommand)
- [Sending Requests](#sending-requests)
- [File Upload](#file-upload)
  - [Server Side](#server-side)
  - [Named File Slots](#named-file-slots)
  - [Blazor Client: `<FileInput>`](#blazor-client-fileinput)
- [Exception Handling](#exception-handling)
- [Esoterica](#esoterica)
  - [AllowProvisional — MFA and Multi-step Auth](#allowprovisional--mfa-and-multi-step-auth)
  - [Per-request Upload Size Limits](#per-request-upload-size-limits)
  - [Server-wide Upload Size Limit](#server-wide-upload-size-limit)
  - [File Validation Attributes](#file-validation-attributes)
  - [Per-request HTTP Timeout](#per-request-http-timeout)
  - [Header Properties](#header-properties)
  - [Raw Request and Response Bodies](#raw-request-and-response-bodies)
  - [Documenting Error Responses](#documenting-error-responses)
  - [Marking an Endpoint as Deprecated](#marking-an-endpoint-as-deprecated)
  - [Endpoint Schema Options](#endpoint-schema-options)
  - [Path Strategies](#path-strategies)
  - [Custom Endpoint Schema](#custom-endpoint-schema)
  - [Multiple Connections](#multiple-connections)
  - [SendVia — Targeting a Specific Server](#sendvia--targeting-a-specific-server)
  - [Direct Download Links in Blazor](#direct-download-links-in-blazor)
  - [Custom Success Status Codes](#custom-success-status-codes)
  - [Advanced Registration](#advanced-registration)
  - [Bootstrap Logging](#bootstrap-logging)
- [See Also](#see-also)
- [License](#license)
- [Changelog](CHANGELOG.txt)
---

## Overview

MetalNexus connects [MetalChain](https://www.nuget.org/packages/RossWright.MetalChain) to HTTP. Just decorate your request types with `[ApiRequest]` and implement a request handler on the server as you normally would. On the client, `Mediator.Send` works as usual — MetalNexus silently routes it over HTTP to the server endpoint. No controllers, no routing attributes, no Swagger annotations, no `HttpClient` code — it's all handled automatically. You can also use the server independently, exposing standard REST endpoints consumable by any HTTP client, or use the client alone to call any external HTTP API.

| Feature | Description |
|---|---|
| Zero-boilerplate HTTP | `[ApiRequest]` + a MetalChain handler is all that's needed on the server |
| Flexible HTTP protocol | Auto, GET, POST (body/query), PUT, PATCH, DELETE — per request |
| Shared request types | Request classes live in a shared project referenced by both client and server |
| Endpoint schema customization | Configurable path prefix, casing, suffix trimming, path strategies, or full custom schema |
| Authentication & authorization | `[Authenticated]` / `[Anonymous]` attributes; compatible with MetalGuardian or any ASP.NET Core auth |
| Swagger / OpenAPI | One `UseMetalNexus()` call wires up JWT security definition and document filter |
| File upload | `MetalNexusFileRequest`, named `[FileSlot]` properties, `[UploadLimit]` / `[NoUploadLimit]`, `[MaxFileSize]` / `[MaxFileCount]` / `[AllowedFileTypes]` validation, and Blazor `<FileInput>` component |
| Exception marshalling | Server exceptions are rethrown on the client (or `MetalNexusException` if type is not visible) |
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

## Namespaces

Most MetalNexus setup methods, request attributes, endpoint schema types, and client/server options builders are available from:

```csharp
using RossWright;
```

---

## Common APIs

| Task | API | Package | Namespace |
|---|---|---|---|
| Mark a MetalChain request as an HTTP endpoint | `[ApiRequest]` | `RossWright.MetalNexus.Abstractions` | `RossWright` |
| Require an authenticated caller | `[Authenticated]` | `RossWright.MetalNexus.Abstractions` | `RossWright` |
| Allow anonymous endpoint access | `[Anonymous]` | `RossWright.MetalNexus.Abstractions` | `RossWright` |
| Register generated endpoints on an ASP.NET Core server | `builder.AddMetalNexusServer(...)` | `RossWright.MetalNexus.Server` | `RossWright` |
| Add MetalNexus middleware | `app.UseMetalNexusServer()` | `RossWright.MetalNexus.Server` | `RossWright` |
| Register a Blazor WebAssembly client | `builder.AddMetalNexusClient(...)` | `RossWright.MetalNexus.Blazor` | `RossWright` |
| Register a non-Blazor client | `builder.AddMetalNexusClient(...)` | `RossWright.MetalNexus` | `RossWright` |
| Upload files from Blazor | `<FileInput>` / `BrowserFile` | `RossWright.MetalNexus.Blazor` | `RossWright` |

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

## Quick Start

By the end of this section you'll have a working request that travels from a Blazor component over HTTP to an ASP.NET Core handler and back — with no controllers, no routing config, and no `HttpClient` code.

**1. Define the request in your shared contracts project** (referenced by both server and client):

```csharp
// MyApp.Contracts — references RossWright.MetalNexus.Abstractions
[ApiRequest]
public class GetGreetingRequest : IRequest<string>
{
    public string Name { get; set; } = null!;
}
```

**2. Implement the handler in your ASP.NET Core server project** and wire MetalNexus in `Program.cs`:

```csharp
// MyApp.Server
public class GetGreetingHandler : IRequestHandler<GetGreetingRequest, string>
{
    public Task<string> Handle(GetGreetingRequest request, CancellationToken ct)
        => Task.FromResult($"Hello, {request.Name}!");
}
```

```csharp
// Program.cs — ASP.NET Core server
var builder = WebApplication.CreateBuilder(args);

builder.AddMetalNexusServer(options =>
{
    options.ScanAssemblyContaining(typeof(Program)); // discovers handlers and their [ApiRequest] types
});

var app = builder.Build();
app.UseMetalNexusServer();
app.Run();
```

**3. Register the client in your Blazor WebAssembly `Program.cs`**:

```csharp
// Program.cs — Blazor WebAssembly
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder
    .AddHttpClient()             // defaults BaseAddress to HostEnvironment.BaseAddress
    .AddMetalNexusClient(options =>
    {
        options.ScanAssemblyContaining<GetGreetingRequest>(); // shared contracts
    });

await builder.Build().RunAsync();
```

**4. Send the request from a Blazor component** — exactly as you would for a local handler:

```razor
@inject IMediator Mediator

<p>@_greeting</p>

@code {
    string? _greeting;

    protected override async Task OnInitializedAsync()
    {
        _greeting = await Mediator.Send(new GetGreetingRequest { Name = "World" });
    }
}
```

That's it. MetalNexus picks up the `[ApiRequest]` attribute at startup, generates the endpoint on the server, and registers the HTTP dispatch handler on the client — the `Mediator.Send` call is identical whether the handler runs locally or over the wire.

---

## Defining API Requests

### HTTP Protocol Selection

Decorate any `IRequest` or `IRequest<TResponse>` with `[ApiRequest]` to expose it as an HTTP endpoint. The only required decision is which HTTP method to use — everything else is derived automatically.

```csharp
[ApiRequest]
public class GetProductRequest : IRequest<Product>
{
    public int Id { get; set; }
}
```

The default `HttpProtocol.Auto` inspects the request type's properties at startup: if the request has few simple (scalar) properties, MetalNexus uses GET with query-string parameters; if it has more than `MaximumRequestParameters` properties, or any complex or collection property, it uses POST with a JSON body. Override explicitly when the default doesn't fit:

| Value | HTTP Method | Parameters |
|---|---|---|
| `Auto` (default) | GET or POST — chosen at startup based on request shape | GET -> query string; POST -> JSON body |
| `Get` | GET | Query string |
| `PostViaBody` | POST | JSON body |
| `PostViaQuery` | POST | Query string |
| `PutViaBody` | PUT | JSON body |
| `PutViaQuery` | PUT | Query string |
| `PatchViaBody` | PATCH | JSON body |
| `PatchViaQuery` | PATCH | Query string |
| `Delete` | DELETE | Query string |
| `DeleteViaBody` | DELETE | JSON body |

```csharp
[ApiRequest(HttpProtocol.PostViaBody)]
public class CreateOrderRequest : IRequest<OrderResult>
{
    public string ProductId { get; set; } = null!;
    public int Quantity { get; set; }
}
```

### Path and Tag

By default MetalNexus derives the endpoint path from the request type's namespace and name using the configured path strategy. Override with the `path` parameter when you need a specific URL shape, and use `tag` to control how the endpoint is grouped in Swagger:

```csharp
[ApiRequest(path: "products/{Id}", tag: "Products")]
public class GetProductRequest : IRequest<Product>
{
    public int Id { get; set; }
}
```

Curly-brace segments are matched to request properties by name and substituted into the URL path automatically. Any properties not matched to a path segment are sent as query-string parameters (GET) or included in the JSON body (POST/PUT/PATCH).

For full control over how namespaces translate to path segments, see [Path Strategies](#path-strategies) in Esoterica.

### Authentication

`[Authenticated]` and `[Anonymous]` are gateway attributes — they answer "is there a logged-in user?" before the handler is ever invoked. They don't evaluate what that user is allowed to do; privilege evaluation belongs in the handler itself (via [MetalGuardian](../MetalGuardian/README.md) or a custom `IAuthorizationService`).

```csharp
// any authenticated user
[Authenticated]
public class GetAccountRequest : IRequest<Account> { }

// only users who hold at least one of these roles
[Authenticated("Admin", "SuperUser")]
public class DeleteAccountRequest : IRequest { }

// enum values are also accepted — ToString() is called automatically
[Authenticated(UserRole.Admin, UserRole.SuperUser)]
public class PromoteUserRequest : IRequest { }

// opt out when RequiresAuthenticationByDefault is true
[Anonymous]
public class GetPublicCatalogRequest : IRequest<Catalog> { }
```

MetalNexus defaults to requiring authentication on every endpoint. Apply `[Anonymous]` to opt individual endpoints out, or flip the global default via `ConfigureEndpointSchema` (see [Endpoint Schema Options](#endpoint-schema-options) in Esoterica).

You can also delegate to a named ASP.NET Core authorization policy instead of listing roles: set `Policy = "MyPolicy"` on `[Authenticated]`.

For MFA and multi-step auth flows where you need to allow a provisionally-authenticated caller, see [AllowProvisional](#allowprovisional--mfa-and-multi-step-auth) in Esoterica.

---

## Server Setup

### Registering the Server

Setting up MetalNexus on the server requires two calls: `AddMetalNexusServer` during service registration and `UseMetalNexusServer` in the middleware pipeline. Together they discover your `[ApiRequest]` types, generate the corresponding endpoints, and wire up the MetalNexus request-routing middleware.

```csharp
// Program.cs — ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

builder.AddMetalNexusServer(options =>
{
    options.ScanAssemblyContaining(typeof(Program)); // server project — handler types
});

var app = builder.Build();
app.UseMetalNexusServer();
app.Run();
```

`AddMetalNexusServer` scans for `IRequestHandler<TRequest>` and `IRequestHandler<TRequest, TResponse>` implementations. For each handler it finds, it walks the generic argument to locate the `[ApiRequest]`-decorated request type — so **only the assembly containing the handlers needs to be scanned**. The shared request project does not need to be scanned separately; MetalNexus reaches the request types through the handlers automatically.

If your handlers are spread across multiple assemblies, call `ScanAssemblyContaining` once per assembly:

```csharp
builder.AddMetalNexusServer(options =>
{
    options.ScanAssemblyContaining(typeof(Program));          // main handler assembly
    options.ScanAssemblyContaining<PluginModuleHandler>();    // additional handler assembly
});
```

> **Tip:** Scanning an assembly more than once is safe — MetalNexus deduplicates registrations automatically.

`UseMetalNexusServer` registers the MetalNexus middleware in the ASP.NET Core pipeline. It also calls `UseAuthentication` automatically when ASP.NET Core authentication services are present — you don't need to add that call yourself.

### Swagger / OpenAPI

One call inside `AddSwaggerGen` gives you complete, accurate API documentation for all MetalNexus endpoints.

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.UseMetalNexus(); // adds JWT bearer security definition + document filter
});
```

`UseMetalNexus` does two things. It registers a JWT bearer security definition named `MetalGuardian` so that authenticated endpoints display the lock icon and allow token entry in the Swagger UI. It also adds `MetalNexusApiDocumentFilter`, which inspects the registered endpoint schema at startup and inserts all MetalNexus endpoints — with their paths, HTTP methods, request bodies or query parameters, response shapes, tags, and security requirements — into the generated OpenAPI document automatically. No `[ProducesResponseType]` attributes or manual OpenAPI annotations are needed.

For documenting expected error responses in Swagger, see [Documenting Error Responses](#documenting-error-responses) in Esoterica. For customizing how type names resolve to endpoint paths, see [Path Strategies](#path-strategies) and [Endpoint Schema Options](#endpoint-schema-options) in Esoterica.

---

## Client Setup

MetalNexus ships two client packages: one for Blazor WebAssembly and one for console apps built with MetalCommand. Both use the same `AddMetalNexusClient` options builder — the only difference is the host type they attach to.

On the client side, MetalNexus needs to scan the assembly that contains your `[ApiRequest]`-decorated types — the shared contracts project. It uses that scan to register the HTTP dispatch handlers for each request type. You don't scan the client project itself unless it also defines `[ApiRequest]` types.

### Blazor WebAssembly

```csharp
// Program.cs — Blazor WebAssembly
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder
    .AddHttpClient()                          // sets BaseAddress to HostEnvironment.BaseAddress
    .AddMetalNexusClient(options =>
    {
        options.ScanAssemblyContaining<GetProductRequest>(); // shared contracts project
    });

await builder.Build().RunAsync();
```

`AddHttpClient()` registers a default `HttpClient` pre-configured with `HostEnvironment.BaseAddress`. For Blazor apps hosted by an ASP.NET Core server this is the correct base URL with no further setup. If your API server is on a different origin, pass a configuration delegate to override the address:

```csharp
builder.AddHttpClient(client => client.BaseAddress = new Uri("https://api.example.com"));
```

`AddMetalNexusClient` also registers the JavaScript interop loader required by the `<FileInput>` component — you don't need a separate call for that.

If your request types are spread across more than one assembly, call `ScanAssemblyContaining` once per assembly:

```csharp
options.ScanAssemblyContaining<GetProductRequest>();  // primary contracts
options.ScanAssemblyContaining<ReportRequest>();      // additional contracts
```

### Console App (MetalCommand)

```csharp
// Program.cs — MetalCommand console app
ConsoleApplication.CreateBuilder(args)
    .AddHttpClient(client => client.BaseAddress = new Uri("https://api.example.com"))
    .AddMetalNexusClient(options =>
    {
        options.ScanAssemblyContaining<GetProductRequest>(); // shared contracts project
    })
    .Build()
    .Run();
```

The options available on `IMetalNexusClientOptionsBuilder` are identical to the Blazor builder. The only practical difference is that `AddHttpClient` on `IConsoleApplicationBuilder` requires an explicit `BaseAddress` — there is no host environment to infer it from.

For connecting to multiple API servers from the same client, see [Multiple Connections](#multiple-connections) in Esoterica.

---

## Sending Requests

MetalNexus is transparent at the call site. The same `Mediator.Send` call you use for local handlers works identically for remote endpoints — MetalNexus silently registers HTTP-backed handlers for every `[ApiRequest]` type it discovers, so your components don't need to know or care whether the handler runs in-process or across the network.

```csharp
// Blazor component — identical whether the handler is local or remote
@inject IMediator Mediator

var product = await Mediator.Send(new GetProductRequest { Id = 42 });
await Mediator.Send(new DeleteProductRequest { Id = 42 });
```

When you need a URL rather than a dispatched result — for example, to set the `src` of an `<img>` tag or pass a pre-built download link to a component — inject `IMetalNexusUrlHelper` instead:

```csharp
@inject IMetalNexusUrlHelper UrlHelper

var url = UrlHelper.GetUrlFor(new GetAvatarRequest { UserId = currentUserId });
// url: "https://api.example.com/users/123/avatar"
```

`IMetalNexusUrlHelper.GetUrlFor<TRequest>` populates path and query parameters from the request instance and applies the configured base address for the endpoint's connection, but does not send any HTTP request.

---

## File Upload

MetalNexus handles the full multipart lifecycle: the client serializes selected files into a multipart form post, and the server deserializes them into strongly-typed properties on your request object. Your handler just receives the request and reads the files — no multipart parsing, no `IFormFile`, no `HttpContext`.

### Server Side

Derive your request from `MetalNexusFileRequest` and implement a handler as usual. MetalNexus populates the `Files` array before your handler is invoked.

```csharp
[ApiRequest(HttpProtocol.PostViaQuery)]
public class UploadAvatarRequest : MetalNexusFileRequest, IRequest { }
```

```csharp
public class UploadAvatarHandler(IStorageService _storage) : IRequestHandler<UploadAvatarRequest>
{
    public async Task Handle(UploadAvatarRequest request, CancellationToken ct)
    {
        var file = request.Files[0];
        await _storage.SaveAsync(file.FileName, file.DataStream!, ct);
    }
}
```

Each uploaded file exposes:

| Member | Description |
|---|---|
| `FileName` | Original filename as reported by the browser |
| `ContentType` | MIME type |
| `DataStream` | Live stream over the uploaded bytes — no intermediate buffer |
| `Data` | Buffered byte array (set by the server for download responses; `null` on uploads) |
| `IsAttachment` | Controls `Content-Disposition` when returning a download (`true` = save dialog, `false` = inline) |

> **Note:** `DataStream` is a live stream from the ASP.NET Core multipart reader. Read or copy it once inside the handler and don't store the `MetalNexusFile` reference after the handler returns.

To return a file download from a handler, return a `MetalNexusFile` with `Data` set:

```csharp
public class DownloadReportHandler : IRequestHandler<DownloadReportRequest, MetalNexusFile>
{
    public Task<MetalNexusFile> Handle(DownloadReportRequest request, CancellationToken ct)
        => Task.FromResult(new MetalNexusFile
        {
            FileName     = "report.pdf",
            ContentType  = "application/pdf",
            Data         = _pdfService.Generate(request.ReportId),
            IsAttachment = true
        });
}
```

For per-request upload size limits and server-wide limits, see [Per-request Upload Size Limits](#per-request-upload-size-limits) and [Server-wide Upload Size Limit](#server-wide-upload-size-limit) in Esoterica. For restricting accepted MIME types, file counts, or individual file sizes, see [File Validation Attributes](#file-validation-attributes) in Esoterica.

### Named File Slots

The anonymous `Files` array works well when all uploads are equivalent. When an endpoint receives semantically distinct files — an avatar and a banner, or a document and its signature — use `[FileSlot]` to bind each file to a named property instead.

```csharp
[ApiRequest(HttpProtocol.PostViaQuery)]
public class UpdateProfileRequest : MetalNexusFileRequest, IRequest
{
    public int UserId { get; set; }

    [FileSlot("avatar")] public MetalNexusFile? Avatar { get; set; }
    [FileSlot("banner")] public MetalNexusFile? Banner { get; set; }
}
```

```csharp
public class UpdateProfileHandler(IStorageService _storage) : IRequestHandler<UpdateProfileRequest>
{
    public async Task Handle(UpdateProfileRequest request, CancellationToken ct)
    {
        if (request.Avatar is not null)
            await _storage.SaveAsync("avatar", request.Avatar.DataStream!, ct);

        if (request.Banner is not null)
            await _storage.SaveAsync("banner", request.Banner.DataStream!, ct);
    }
}
```

The server matches uploaded files to slots by the multipart form-field name (case-insensitive). Files whose name doesn't match any declared slot are still collected in `Files[]`. You can mix both approaches on the same request.

Validation attributes (`[AllowedFileTypes]`, `[MaxFileSize]`) can be applied at the class level to set defaults for all files, then overridden on individual slots:

```csharp
[AllowedFileTypes("image/jpeg", "image/png")]
[MaxFileSize(5 * 1024 * 1024)]
public class UpdateProfileRequest : MetalNexusFileRequest, IRequest
{
    public int UserId { get; set; }

    [FileSlot("avatar")]
    public MetalNexusFile? Avatar { get; set; }

    [FileSlot("document")]
    [AllowedFileTypes("application/pdf")]    // overrides the class-level image types
    [MaxFileSize(20 * 1024 * 1024)]          // overrides the class-level 5 MB cap
    public MetalNexusFile? Document { get; set; }
}
```

### Blazor Client: `<FileInput>`

`<FileInput>` is a headless Blazor component that wires the browser's native file picker to MetalNexus upload calls. It renders no visible UI of its own — you style and trigger it however you like.

```razor
<FileInput @ref="_fileInput"
           AllowMultipleFiles="false"
           Accept="image/*"
           FilesPicked="OnFilesPicked"
           FilePickerCanceled="OnCanceled"
           FilesUploaded="OnUploaded"
           UploadFailed="OnFailed" />

<button @onclick="() => _fileInput.OpenFilePicker()">Choose image</button>
```

```csharp
@code {
    FileInput _fileInput = null!;

    async Task OnFilesPicked(FileInput.IFilesPickedArgs args)
    {
        // optional: preview without uploading
        await args.ShowImage("preview-img", args.Files[0]);

        // upload — MetalNexus routes to the server handler automatically
        await args.UploadFiles<UploadAvatarRequest>(args.Files);
    }
}
```

`<FileInput>` parameters:

| Parameter | Description |
|---|---|
| `AllowMultipleFiles` | When `true`, the browser picker allows selecting more than one file |
| `Accept` | Passed to the browser input's `accept` attribute, e.g. `"image/*"` or `".pdf,.docx"` |
| `FilesPicked` | Raised when the user confirms a selection; receives `IFilesPickedArgs` |
| `FilePickerCanceled` | Raised when the picker is dismissed without a selection |
| `FilesUploaded` | Raised when `UploadFiles` completes successfully |
| `UploadFailed` | Raised when `UploadFiles` throws |

`IFilesPickedArgs` members:

| Member | Description |
|---|---|
| `Files` | Array of `BrowserFile` objects (FileName, ContentType, Size) |
| `ShowImage(imgId, file)` | Previews the browser file in an `<img>` or CSS background-image element without uploading |
| `ShowImage(imgId, url)` | Sets an `<img>` src or CSS background-image from a URL |
| `UploadFiles<TRequest>(files)` | Uploads files to the server endpoint for `TRequest` |
| `UploadFiles<TRequest>(request, files)` | Same, with a pre-populated request instance carrying extra properties |
| `UploadFiles<TRequest>(request, slots, files)` | Uploads named slot files and optional extra files; `slots` is `Dictionary<string, BrowserFile>` keyed by slot name |

When the server request uses `[FileSlot]` properties, use the `slots` overload so each file is sent under its declared form-field name:

```csharp
async Task OnFilesPicked(FileInput.IFilesPickedArgs args)
{
    await args.UploadFiles<UpdateProfileRequest>(
        new UpdateProfileRequest { UserId = _currentUserId },
        slots: new Dictionary<string, BrowserFile>
        {
            ["avatar"]   = args.Files[0],
            ["document"] = args.Files[1]
        });
}
```

---

## Exception Handling

When a server handler throws, MetalNexus serializes the exception — including its type's assembly-qualified name and message — into the error response body. On the client, it reconstructs the original exception type by resolving that name against the loaded assemblies. If the type is defined in a shared project that both client and server reference, the `catch` block sees the exact same exception type that was thrown on the server.

If the exception type can't be resolved on the client (for example, an internal server-only exception), MetalNexus falls back to throwing a `MetalNexusException` carrying the original message. Catching `MetalNexusException` is therefore the generic fallback for any unrecognised server error.

```csharp
try
{
    var order = await Mediator.Send(new GetOrderRequest { Id = id });
}
catch (NotFoundException ex)
{
    // caught as the exact shared type when NotFoundException is in the shared project
    ShowError(ex.Message);
}
catch (MetalNexusException ex)
{
    // fallback for server exception types not visible to this client
    ShowError(ex.Message);
}
```

Any unhandled server exception that is not an already-mapped type surfaces on the client as `InternalServerErrorException`. Two opt-in client options control the behaviour for debugging and hardening:

```csharp
// Include the server stack trace in the exception message (useful in development)
options.IncludeServerStackTraceOnExceptions();

// Treat all unhandled server exceptions as InternalServerErrorException
// rather than rethrowing their original type
options.TreatUnknownExceptionsAsInternalServiceError();
```

> **Note:** To document which exception types a given endpoint can produce in the Swagger UI, see [Documenting Error Responses](#documenting-error-responses) in Esoterica.

---

## Esoterica

The above covers the 80% everyday usage of MetalNexus. Below are the more specialized capabilities and behaviors.

### AllowProvisional — MFA and Multi-step Auth

During MFA and multi-step authentication flows there's a window where a user has passed their first factor but hasn't yet completed the second. Their identity is known but their authentication is not yet complete — MetalGuardian (and any compliant auth stack) marks this state as *provisional*. By default, `[Authenticated]` rejects provisional callers. Set `AllowProvisional = true` on the specific endpoints that need to be reachable in that window — typically the endpoint that receives the OTP or MFA confirmation code.

```csharp
[Authenticated(AllowProvisional = true)]
public class SubmitMfaCodeRequest : IRequest<AuthResult> { }
```

> **Note:** `AllowProvisional` only widens the *authentication* gate. Role and policy checks on `[Authenticated]` still apply normally.

### Per-request Upload Size Limits

ASP.NET Core enforces a server-wide multipart body size limit. Most endpoints need the default, but some legitimately need more — or no limit at all (video processing, raw data ingestion). Apply `[UploadLimit]` or `[NoUploadLimit]` to a `MetalNexusFileRequest`-derived class to override the server-wide limit for that endpoint alone.

```csharp
[UploadLimit(50 * 1024 * 1024)]   // 50 MB for this endpoint
public class UploadDocumentRequest : MetalNexusFileRequest { }

[NoUploadLimit]                    // no cap — use with caution in production
public class UploadLargeVideoRequest : MetalNexusFileRequest { }
```

> **Warning:** `[NoUploadLimit]` removes all size protection for that endpoint. Use it only where the calling context is trusted and bounded by other controls.

### Server-wide Upload Size Limit

Set the default limit applied to all upload endpoints that don't carry their own `[UploadLimit]` or `[NoUploadLimit]`:

```csharp
builder.AddMetalNexusServer(options =>
{
    options.SetMultipartBodyLengthLimit(100 * 1024 * 1024); // 100 MB
    options.ScanAssemblyContaining(typeof(Program));
});
```

Per-request `[UploadLimit]` / `[NoUploadLimit]` take precedence over this setting.

### File Validation Attributes

Three attributes enforce file constraints on the server before the handler is called. They can be applied at the class level (applies to all files in the request) or at the property level on a `[FileSlot]` (applies only to that slot and overrides any class-level value).

**`[MaxFileSize(bytes)]`** — rejects any individual file that exceeds the byte limit:

```csharp
[MaxFileSize(5 * 1024 * 1024)]   // 5 MB per file
public class UploadImagesRequest : MetalNexusFileRequest { }
```

**`[AllowedFileTypes(mimeType, ...)]`** — rejects any file whose `ContentType` is not in the list:

```csharp
[AllowedFileTypes("image/jpeg", "image/png", "image/webp")]
public class UploadAvatarRequest : MetalNexusFileRequest { }
```

**`[MaxFileCount(count)]`** — rejects the request when the total number of uploaded files (slot files plus `Files[]` entries) exceeds the limit. Class-level only.

```csharp
[MaxFileCount(5)]
public class UploadGalleryRequest : MetalNexusFileRequest { }
```

Violations throw `System.ComponentModel.DataAnnotations.ValidationException` and map to a 400 response. When multiple files violate a constraint, all violations are reported together in a single exception.

Property-level attributes on a `[FileSlot]` override the corresponding class-level attribute for that slot only:

```csharp
[AllowedFileTypes("image/jpeg", "image/png")]
[MaxFileSize(2 * 1024 * 1024)]
public class UploadProfileRequest : MetalNexusFileRequest
{
    [FileSlot("avatar")]
    public MetalNexusFile? Avatar { get; set; }

    // PDF allowed for this slot even though the class-level type restriction says images only
    [FileSlot("cv")]
    [AllowedFileTypes("application/pdf")]
    [MaxFileSize(10 * 1024 * 1024)]
    public MetalNexusFile? Cv { get; set; }
}
```

### Per-request HTTP Timeout

Long-running operations — report generation, bulk imports, large file processing — often need a longer timeout than the default registered on the `HttpClient`. Apply `[HttpClientTimeout]` to the request type to set a per-endpoint timeout on the client side without raising the global default for all requests.

```csharp
[HttpClientTimeout(120)]   // seconds
public class GenerateReportRequest : IRequest<ReportResult> { }
```

The timeout value is applied to the underlying `HttpClient.Timeout` for that single send and restored afterward. When the timeout elapses the request is cancelled and the client throws `TaskCanceledException`.

### Header Properties

Some APIs and middleware layers expect metadata — tenant identifiers, correlation IDs, API keys, feature flags — in HTTP headers rather than the request body or query string. Marking a request property with `[FromHeader]` tells MetalNexus to read and write it as a named header instead of routing it through normal parameter binding.

On the **client**, MetalNexus serializes the property value into the named request header before sending. On the **server**, MetalNexus reads the incoming header by name and populates the property before invoking the handler.

```csharp
[ApiRequest]
public class GetSecureDataRequest : IRequest<SecureData>
{
    [FromHeader("X-Tenant-Id")]
    public string TenantId { get; set; } = null!;

    public int ResourceId { get; set; }   // sent via query string or body as normal
}
```

`[FromHeader]` properties are excluded from the Swagger query-string / body schema and documented as header parameters in the OpenAPI document instead.

### Raw Request and Response Bodies

**Raw request body** — Webhook receivers and signature-verification endpoints need the exact bytes that arrived, not a deserialized object. Implementing `IMetalNexusRawRequest` (or `IMetalNexusRawRequest<TResponse>` for endpoints that return a value) tells MetalNexus to skip JSON deserialization and instead populate `RawRequestBody` with the raw request body string before calling the handler.

```csharp
[ApiRequest(HttpProtocol.PostViaBody)]
public class StripeWebhookRequest : IMetalNexusRawRequest
{
    public string? RawRequestBody { get; set; }   // populated by MetalNexus before the handler runs
}

public class StripeWebhookHandler(IStripeSignatureVerifier _verifier)
    : IRequestHandler<StripeWebhookRequest>
{
    public Task Handle(StripeWebhookRequest request, CancellationToken ct)
    {
        _verifier.Verify(request.RawRequestBody!);
        // now parse and process
        return Task.CompletedTask;
    }
}
```

**Raw response body** — When a handler needs to return non-JSON content — CSV, XML, a custom binary format, or a pre-serialized JSON string with non-default options — implement `IMetalNexusRawResponse` on the return type. MetalNexus writes the content directly to the HTTP response, bypassing its own JSON serializer.

```csharp
public sealed class CsvExport : IMetalNexusRawResponse
{
    public string ContentType => "text/csv";
    public byte[]? Data { get; init; }
    public Stream? DataStream => null;
}

public class ExportOrdersHandler : IRequestHandler<ExportOrdersRequest, CsvExport>
{
    public Task<CsvExport> Handle(ExportOrdersRequest request, CancellationToken ct)
    {
        var csv = BuildCsv(request);
        return Task.FromResult(new CsvExport { Data = Encoding.UTF8.GetBytes(csv) });
    }
}
```

Either `Data` or `DataStream` must be non-null. When both are set, `DataStream` takes precedence and is disposed after writing. For the common case of returning a POCO as JSON, the normal handler return type is always preferred over `IMetalNexusRawResponse`.

### Documenting Error Responses

By default MetalNexus only documents `200 OK` in the Swagger/OpenAPI output for each endpoint. When your handler intentionally throws well-known exceptions — `NotFoundException`, `ValidationException`, and so on — those error scenarios are invisible to API consumers browsing the Swagger UI. Apply `[ProducesError<TException>]` to declare them explicitly. MetalNexus maps each exception type to its HTTP status code and adds the corresponding response entry to the generated document. Multiple attributes may be stacked.

```csharp
[ApiRequest]
[ProducesError<NotFoundException>]     // -> 404
[ProducesError<ValidationException>]   // -> 422
public class UpdateOrderRequest : IRequest<Order>
{
    public int Id { get; set; }
    public string Status { get; set; } = null!;
}
```

The built-in exception -> status code mappings are:

| Exception | Status code |
|---|---|
| `NotAuthenticatedException` | `401 Unauthorized` |
| `NotAuthorizedException` | `403 Forbidden` |
| `NotFoundException` | `404 Not Found` |
| `ValidationException` | `422 Unprocessable Entity` |
| `NotImplementedException` | `501 Not Implemented` |
| `InternalServerErrorException` | `500 Internal Server Error` |
| Any other exception | `400 Bad Request` |

Subclasses inherit the parent's mapping. `500 Internal Server Error` is always included in the document automatically — you don't need to declare it.

### Marking an Endpoint as Deprecated

Apply the standard .NET `[Obsolete]` attribute to a request type to mark the corresponding Swagger operation as deprecated. No MetalNexus-specific attribute is needed.

```csharp
[ApiRequest]
[Obsolete("Use GetOrderV2Request instead.")]
public class GetOrderRequest : IRequest<Order>
{
public int Id { get; set; }
}
```

The endpoint will appear with a strikethrough in Swagger UI and the `deprecated: true` flag in the generated OpenAPI document.

### Endpoint Schema Options

Control path generation globally via `ConfigureEndpointSchema`:

```csharp
builder.AddMetalNexusServer(options =>
{
    options.ScanAssemblyContaining(typeof(Program)); // server — handler types
    options.ConfigureEndpointSchema(schema =>
    {
        schema.ApiPathPrefix = "api";                    // prepend /api/ to all paths
        schema.ApiPathToLower = true;                    // lowercase all paths
        schema.RequestSuffixesToTrim = ["Request"];      // trim "Request" from type name
        schema.RequiresAuthenticationByDefault = true;
        schema.MaximumRequestParameters = 5;             // Auto threshold: >N simple props -> POST body
        schema.DefaultHttpProtocol = HttpProtocol.Get;   // fallback when Auto stays under the threshold
    });
});
```

### Path Strategies

A path strategy controls how a request type's namespace is converted into its URL path. The five built-in strategies all produce different results from the same type — here shown for `GetUserRequest` in the namespace `MyCorp.MyApp.Endpoints.Users`:

| Strategy | Result | When to use |
|---|---|---|
| `TrimDefaultNamespacePathStrategy` (default) | `/Endpoints/Users/GetUser` | The common root (`MyCorp.MyApp`) is shared by 80%+ of all types in the assembly |
| `TrimRequestNamespacePathStrategy` | `/Endpoints/Users/GetUser` | Same as above, but the root is detected by scanning only `[ApiRequest]` types — more accurate when non-request types have a different namespace root |
| `TrimFixedPreamblePathStrategy("MyCorp.MyApp.Endpoints")` | `/Users/GetUser` | The common prefix is known in advance; avoids the reflection scan at startup |
| `UseFullNameSpacePathStrategy` | `/MyCorp/MyApp/Endpoints/Users/GetUser` | You want the full namespace reflected in every URL |
| `NoNamespacePathStrategy` | `/GetUser` | Flat URL space; type name only |

`TrimDefaultNamespacePathStrategy` and `TrimRequestNamespacePathStrategy` produce identical URLs when all request types share the same root as the rest of the assembly. Prefer `TrimRequestNamespacePathStrategy` when non-request types would skew the auto-detected root, and `TrimFixedPreamblePathStrategy` when you want to eliminate the startup scan entirely.

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

### SendVia — Targeting a Specific Server

`SendVia` wraps a request with a connection name, overriding the default at call time:

```csharp
// Without response
await Mediator.SendVia("orders", new CancelOrderRequest { OrderId = 99 }, ct);

// With response
var result = await Mediator.SendVia<OrderResult>("orders", new PlaceOrderRequest { ... }, ct);
```

You can also use the `SendVia<TRequest>` / `SendVia<TRequest, TResponse>` request wrappers directly as MetalChain requests.

### Direct Download Links in Blazor

`IMetalNexusUrlHelper` resolves the full absolute URL for any `[ApiRequest]` — including its base address and query-string parameters — without making an HTTP call. Inject it into a Blazor component to generate direct download links (e.g. anchor `href`, `window.open`) for GET endpoints that return files.

```csharp
@inject IMetalNexusUrlHelper UrlHelper

<a href="@_downloadUrl" target="_blank" download>Download Report</a>

@code {
private string _downloadUrl = string.Empty;

protected override void OnInitialized()
{
_downloadUrl = UrlHelper.GetUrlFor(new GetReportRequest { ReportId = 42 });
}
}
```

The URL is built from the registered `HttpClient` base address for the endpoint's connection and the path/query derived from the request properties — the same URL the MetalNexus middleware will answer on the server. For authenticated endpoints, include the bearer token manually in the link or use a short-lived signed URL approach, since the browser will not attach the `Authorization` header for anchor navigations.

### Custom Success Status Codes

By default MetalNexus returns `200 OK` when a handler completes successfully. There are two ways to change this.

**Fixed code via attribute** — use when the endpoint always returns the same code:

```csharp
[ApiRequest(SuccessStatusCode = HttpStatusCode.Created)]
public class CreateWidgetRequest : IRequest<Widget> { ... }
```

**Dynamic code via `IMetalNexusResponseContext`** — inject into the handler when the code needs to be chosen at runtime:

```csharp
public class CreateWidgetHandler(
AppDbContext db,
IMetalNexusResponseContext response) : IRequestHandler<CreateWidgetRequest, Widget>
{
public async Task<Widget> Handle(CreateWidgetRequest request, CancellationToken ct)
{
var widget = new Widget { Name = request.Name };
db.Widgets.Add(widget);
await db.SaveChangesAsync(ct);

response.StatusCode = HttpStatusCode.Created;
response.Location  = $"/widgets/{widget.Id}";
return widget;
}
}
```

`IMetalNexusResponseContext` is registered automatically by `AddMetalNexusServer`. Handlers that do not inject it receive `200 OK`. If both the attribute and the runtime context are set, **the runtime value wins**. If the handler throws, both values are ignored and the normal MetalNexus exception-to-status-code mapping applies.

**NoContent (204)** — return `HttpStatusCode.NoContent` to signal that there is no response body. The MetalNexus client handles this transparently: when the server returns `204` with an empty body, `Mediator.Send` returns `default` for the response type (or completes silently for commands) without throwing. If the server returns `204` with a body it is still deserialized normally.

### Advanced Registration

Use `AddMetalNexusEndpoints` to register additional request types from a separate assembly after setup:

```csharp
services.AddMetalNexusEndpoints(typeof(GetProductRequest), typeof(CreateOrderRequest));
```

This can be called before or after `AddMetalNexusServer` / `AddMetalNexusClient` — MetalNexus resolves the registration order automatically.

### MetalNexus Server with Non-MetalNexus HTTP Clients

The MetalNexus server is a standard REST API. Any HTTP client — `curl`, Postman, a mobile app, another service using plain `HttpClient` — can call a MetalNexus endpoint without using the MetalNexus client library.

#### Error response negotiation

The MetalNexus client automatically adds an `X-MetalNexus-Client: 1` header to every outgoing request. The server uses this header to decide which error format to return:

- **Header present** -> MetalNexus JSON envelope (preserves typed exception reconstruction on the MetalNexus client)
- **Header absent** -> RFC 7807 `ProblemDetails` with `Content-Type: application/problem+json`

```json
// Non-MetalNexus client error response (no header)
{
  "status": 404,
  "title": "NotFoundException",
  "detail": "Product not found.",
  "type": "https://httpstatuses.com/404"
}
```

Existing MetalNexus Server + Client pairs are completely unaffected — the header is always present when going through the MetalNexus client.

#### Raw (non-JSON) responses

By default MetalNexus serializes handler return values as JSON. To return a different content type — XML, CSV, a custom binary format, a pre-serialized JSON string — implement `IMetalNexusRawResponse` on the return type:

```csharp
public interface IMetalNexusRawResponse
{
    string ContentType { get; }
    byte[]? Data { get; }
    Stream? DataStream { get; }
}
```

Either `Data` or `DataStream` must be non-null; if both are set `DataStream` takes precedence and is disposed after writing. Example:

```csharp
public sealed class CsvReport : IMetalNexusRawResponse
{
    public string ContentType => "text/csv";
    public byte[]? Data { get; init; }
    public Stream? DataStream => null;
}

// In the handler:
public Task<CsvReport> Handle(GetReportRequest request, CancellationToken ct)
{
    var csv = BuildCsv(request);
    return Task.FromResult(new CsvReport { Data = Encoding.UTF8.GetBytes(csv) });
}
```

The zero-boilerplate JSON model for regular POCO return types is unchanged.

#### Content negotiation via `IMetalNexusRequestContext`

Handlers that need to inspect the inbound `Accept` header (or other request headers) can inject `IMetalNexusRequestContext`:

```csharp
public class GetReportHandler(
    IMetalNexusRequestContext requestContext) : IRequestHandler<GetReportRequest, IMetalNexusRawResponse>
{
    public Task<IMetalNexusRawResponse> Handle(GetReportRequest request, CancellationToken ct)
    {
        if (requestContext.AcceptHeader?.Contains("text/csv") == true)
            return Task.FromResult<IMetalNexusRawResponse>(new CsvReport { Data = BuildCsv(request) });

        return Task.FromResult<IMetalNexusRawResponse>(new JsonReport { Data = BuildJson(request) });
    }
}
```

`IMetalNexusRequestContext` is registered automatically by `AddMetalNexusServer`. It follows the same ambient pattern as `IMetalNexusResponseContext`: a thin, testable interface that keeps handler code decoupled from `HttpContext`.

| Member | Description |
|---|---|
| `AcceptHeader` | Value of the inbound `Accept` header, or `null` |
| `ContentType` | Value of the inbound `Content-Type` header, or `null` |
| `RequestHeaders` | All inbound request headers as `IReadOnlyDictionary<string, string?>` |

### MetalNexus Client with Non-MetalNexus API Servers

The MetalNexus client is driven entirely by `[ApiRequest]` metadata and works against any standard JSON REST API. Declare a request type that models the external endpoint and call it via `Mediator.Send` — no MetalNexus server required.

#### Declaring requests for external endpoints

```csharp
// GET https://api.example.com/products/{Id}
[ApiRequest(HttpProtocol.Get, connectionName: "external-api")]
public class GetExternalProductRequest : IRequest<ExternalProduct>
{
    public int Id { get; set; }   // becomes a path parameter: /products/42
}

// POST https://api.example.com/orders  (body serialized as JSON)
[ApiRequest(HttpProtocol.Post, connectionName: "external-api")]
public class CreateExternalOrderRequest : IRequest<ExternalOrderResult>
{
    public string ProductCode { get; set; } = null!;
    public int Quantity { get; set; }
}

// GET with query string: /search?q=widget&page=2
[ApiRequest(HttpProtocol.Get, connectionName: "catalog-api")]
public class SearchCatalogRequest : IRequest<SearchResult>
{
    public string Q { get; set; } = null!;
    public int Page { get; set; }
}
```

Register the named `HttpClient` and point the MetalNexus client at it:

```powershell
# Installing the MetalNexus client package
dotnet add package RossWright.MetalNexus
```

```csharp
builder
    .AddHttpClient("external-api", c =>
        c.BaseAddress = new Uri("https://api.example.com"))
    .Services
    .AddMetalNexusClient(options =>
    {
        options.ScanAssemblyContaining<GetExternalProductRequest>();
        options.SetDefaultConnection("external-api");
    });
```

Then dispatch exactly as you would against a MetalNexus server:

```csharp
var product = await mediator.Send(new GetExternalProductRequest { Id = 42 }, ct);
```

#### Authentication / API keys for external APIs

MetalNexus does not inject auth headers automatically. Use a delegating handler on the `HttpClient` registration:

```csharp
public class ApiKeyHandler(IConfiguration config) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        request.Headers.Add("X-Api-Key", config["ExternalApi:Key"]);
        return base.SendAsync(request, ct);
    }
}

builder.Services.AddTransient<ApiKeyHandler>();
builder
    .AddHttpClient("external-api", c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddHttpMessageHandler<ApiKeyHandler>();
```

For per-request headers (tenant IDs, correlation IDs, etc.) use `[FromHeader]` on the request type — MetalNexus sends those properties as HTTP request headers automatically:

```csharp
[ApiRequest(HttpProtocol.Get, connectionName: "external-api")]
public class GetTenantDataRequest : IRequest<TenantData>
{
    [FromHeader("X-Tenant-Id")]
    public string TenantId { get; set; } = null!;

    public int ResourceId { get; set; }
}
```

#### Error handling for non-MetalNexus error shapes

When the external API returns a non-2xx status code, the MetalNexus client attempts to deserialize the body as a MetalNexus `ExceptionResponse`. If the body is not in that format (RFC 7807, a plain string, HTML, etc.) deserialization falls back to a `MetalNexusException` whose `Message` contains the raw response body:

```csharp
try
{
    var result = await mediator.Send(new GetExternalProductRequest { Id = 42 }, ct);
}
catch (MetalNexusException ex)
{
    // ex.Message contains the raw response body from the external API
    // parse it in whatever format the external API uses
    var problem = JsonSerializer.Deserialize<ProblemDetails>(ex.Message);
}
```

#### Non-default JSON naming conventions

If the external API uses non-default naming (e.g. `snake_case` or `PascalCase` responses), register a global custom `JsonSerializerOptions` via `IMetalNexusClientOptions` or add a custom converter.

### Bootstrap Logging

`AddMetalNexusServer` and `AddMetalNexusClient` support startup-time diagnostic logging via `UseBootstrapLogger`. Because the standard `ILogger` pipeline isn't available during DI registration, you supply an `ILoggerFactory` before the container is built. MetalNexus uses it to report which handlers, routes, and registrations were found or rejected during assembly scanning.

To enable console output during startup, use `AddMetalConsoleLogger` from `RossWright.MetalCore`:

```csharp
// Server
builder.AddMetalNexusServer(options =>
{
    options.UseBootstrapLogger(logging =>
    {
        logging.ClearProviders();
        logging.AddMetalConsoleLogger();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
    options.ScanAssemblyContaining(typeof(Program));
});

// Client
builder.AddMetalNexusClient(options =>
{
    options.UseBootstrapLogger(logging =>
    {
        logging.ClearProviders();
        logging.AddMetalConsoleLogger();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
});
```

Call `options.DoNotUseLogger()` to suppress all bootstrap output (the default in Release builds if no factory is supplied).

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalChain`](../MetalChain/README.md) | Mediator pattern: `IRequest` / `IRequestHandler` / `IMediator` |
| [`RossWright.MetalCore`](../MetalCore/RossWright.MetalCore/README.md) | Foundation utilities, assembly scanning, extension methods |
| [`RossWright.MetalGuardian`](../MetalGuardian/README.md) | Authentication and authorization for the Metal stack |
| [`RossWright.MetalInjection`](../MetalInjection/README.md) | Ground-up `IServiceProvider` with attribute/interface-based registration |
| [`RossWright.MetalCommand`](../MetalCommand/README.md) | Interactive console application host |

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
