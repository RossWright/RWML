# Define MetalChain Requests And Handlers

Use this recipe when creating shared request contracts and handler classes for MetalChain.

## Install

```bash
dotnet add package RossWright.MetalChain.Abstractions
```

Use `RossWright.MetalChain` in the executable/server project that registers and dispatches handlers.

## Namespace

```csharp
using RossWright;
```

## Query Example

```csharp
public sealed class GetGreetingRequest : IRequest<string>
{
	public string Name { get; set; } = null!;
}

public sealed class GetGreetingHandler
	: IRequestHandler<GetGreetingRequest, string>
{
	public Task<string> Handle(
		GetGreetingRequest request,
		CancellationToken cancellationToken) =>
		Task.FromResult($"Hello, {request.Name}.");
}
```

## Command Example

```csharp
public sealed class RecalculateTotalsRequest : IRequest
{
	public Guid CustomerId { get; set; }
}

public sealed class RecalculateTotalsHandler
	: IRequestHandler<RecalculateTotalsRequest>
{
	public Task Handle(
		RecalculateTotalsRequest request,
		CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
```

## Reach For This When

- You are designing request/response contracts.
- You want handlers that are easy to unit test.
- You plan to expose requests over MetalNexus later.

## Notes For Agents

- Queries normally have exactly one handler.
- Commands can opt into multicast behavior when the request type allows it.
- Use `SendOrDefault` or `SendOrIgnore` only when missing handlers are expected.
