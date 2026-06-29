# Connect Blazor To ASP.NET Core With MetalNexus

Use this recipe when a Blazor WebAssembly client should call ASP.NET Core server handlers through MetalChain request types instead of handwritten HTTP clients.

## Install

Shared contracts:

```bash
dotnet add package RossWright.MetalNexus.Abstractions
dotnet add package RossWright.MetalChain.Abstractions
```

Server:

```bash
dotnet add package RossWright.MetalNexus.Server
dotnet add package RossWright.MetalChain
```

Blazor client:

```bash
dotnet add package RossWright.MetalNexus.Blazor
```

## Namespace

```csharp
using RossWright;
```

## Shared Request

```csharp
[ApiRequest]
public sealed class GetGreetingRequest : IRequest<string>
{
	public string Name { get; set; } = null!;
}
```

## Server Setup

```csharp
builder.AddMetalNexusServer(options =>
{
	options.ScanAssemblyContaining<GetGreetingHandler>();
});

var app = builder.Build();
app.UseMetalNexusServer();
```

## Blazor Setup

```csharp
builder
	.AddHttpClient()
	.AddMetalNexusClient(options =>
	{
		options.ScanAssemblyContaining<GetGreetingRequest>();
	});
```

## Client Call

```csharp
var greeting = await mediator.Send(
	new GetGreetingRequest { Name = "Ada" },
	cancellationToken);
```

## Reach For This When

- You already use or want MetalChain request/handler patterns.
- You want server endpoints generated from request types.
- You want Blazor code to keep calling `IMediator.Send`.

## Notes For Agents

- Request contracts usually live in a shared project.
- The client scans request types; the server scans request types and handlers.
- Use MetalGuardian with MetalNexus when endpoints need authentication.
