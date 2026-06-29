# Add MetalChain Mediator Dispatch

Use this recipe when application code should send commands and queries to handlers without directly depending on handler implementations.

## Install

```bash
dotnet add package RossWright.MetalChain
```

Use `RossWright.MetalChain.Abstractions` in shared contracts projects that only define request and handler types.

## Namespace

```csharp
using RossWright;
```

## Setup

```csharp
builder.Services.AddMetalChain(options =>
{
	options.ScanThisAssembly();
	options.ScanAssemblyContaining<CreateCustomerHandler>();
});
```

## Send Requests

```csharp
await mediator.Send(new CreateCustomerRequest
{
	Name = "Ada"
}, cancellationToken);

var customer = await mediator.Send(
	new GetCustomerRequest { Id = customerId },
	cancellationToken);
```

## Reach For This When

- You want command/query dispatch in a server, console app, or Blazor app.
- You want request handlers discovered through DI scanning.
- You want the same request contracts to later travel over MetalNexus.

## Avoid This When

- You only need HTTP transport. Use MetalNexus on top of MetalChain.
- You only need DI attribute scanning. Use MetalInjection.

## Notes For Agents

- Commands implement `IRequest`.
- Queries implement `IRequest<TResponse>`.
- Handlers must be registered or scanned before `IMediator.Send` can dispatch.
