# MetalNexus AI Usage Guide

Use this file when generating code that consumes RossWright.MetalNexus packages.

## Packages

| Package | Use When |
|---|---|
| `RossWright.MetalNexus.Abstractions` | You are defining shared request types and endpoint metadata. |
| `RossWright.MetalNexus.Server` | You are exposing MetalChain requests as ASP.NET Core endpoints. |
| `RossWright.MetalNexus.Blazor` | You are calling MetalNexus endpoints from Blazor WebAssembly. |
| `RossWright.MetalNexus` | You are calling MetalNexus endpoints from a non-Blazor .NET client. |

## Namespace

Most APIs are in:

```csharp
using RossWright;
```

## Common APIs

| Task | API |
|---|---|
| Mark a request as an HTTP endpoint | `[ApiRequest]` |
| Require an authenticated caller | `[Authenticated]` |
| Allow anonymous calls | `[Anonymous]` |
| Put a property in an HTTP header | `[FromHeader]` |
| Document expected errors | `[ProducesError]` |
| Configure server endpoints | `builder.AddMetalNexusServer(...)` |
| Add server middleware | `app.UseMetalNexusServer()` |
| Configure Blazor client | `builder.AddHttpClient().AddMetalNexusClient(...)` |
| Generate direct endpoint URLs | `IMetalNexusUrlHelper` |
| Upload files from Blazor | `<FileInput>` and `BrowserFile` |

## Typical shared request

```csharp
[ApiRequest]
public class GetGreetingRequest : IRequest<string>
{
	public string Name { get; set; } = null!;
}
```

## Typical ASP.NET Core server setup

```csharp
builder.AddMetalNexusServer(options =>
{
	options.ScanAssemblyContaining<GetGreetingHandler>();
});

var app = builder.Build();
app.UseMetalNexusServer();
```

## Typical Blazor WebAssembly client setup

```csharp
builder
	.AddHttpClient()
	.AddMetalNexusClient(options =>
	{
		options.ScanAssemblyContaining<GetGreetingRequest>();
	});
```

## Important notes

- Request types usually live in a shared contracts project referenced by both client and server.
- The client and server must scan the assemblies containing request types and handlers.
- The calling code still uses `IMediator.Send`; MetalNexus swaps in HTTP-backed handlers.
- Use `RossWright.MetalNexus.Blazor` for Blazor WebAssembly clients.
- Use `RossWright.MetalNexus.Server` for ASP.NET Core servers.
