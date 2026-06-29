# MetalChain AI Usage Guide

Use this file when generating code that consumes RossWright.MetalChain packages.

## Packages

| Package | Use When |
|---|---|
| `RossWright.MetalChain.Abstractions` | You are defining shared request and handler contracts in a domain/contracts project. |
| `RossWright.MetalChain` | You need the mediator implementation, DI registration, scanning, dispatch, and listener support. |

## Namespace

Most APIs are in:

```csharp
using RossWright;
```

## Common APIs

| Task | API |
|---|---|
| Define a command | `IRequest` |
| Define a query | `IRequest<TResponse>` |
| Handle a command | `IRequestHandler<TRequest>` |
| Handle a query | `IRequestHandler<TRequest, TResponse>` |
| Register handlers | `services.AddMetalChain(options => options.ScanThisAssembly())` |
| Send a request | `mediator.Send(request, cancellationToken)` |
| Allow a missing query handler | `mediator.SendOrDefault(request, cancellationToken)` |
| Allow a missing command handler | `mediator.SendOrIgnore(request, cancellationToken)` |
| Listen at runtime | `mediator.Listen<TRequest>(handler)` |

## Typical setup

```csharp
builder.Services.AddMetalChain(options =>
{
	options.ScanThisAssembly();
	options.ScanAssemblyContaining<MyRequestHandler>();
});
```

## Important notes

- Queries implement `IRequest<TResponse>` and normally require exactly one handler.
- Commands implement `IRequest` and can opt into multicast fan-out behavior.
- If no handler is registered, MetalChain throws by default unless the request type or call site allows no-handler behavior.
- Request and handler assemblies must be scanned or registered explicitly.
