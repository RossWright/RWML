# RossWright.MetalChain.Abstractions
Copyright (c) 2023-2026 Pross Co.

A minimal abstractions-only package for [RossWright.MetalChain](https://www.nuget.org/packages/rosswright.metalchain) — the lightweight mediator library for .NET.

Reference this package in **domain or abstractions projects** that need to define request types and apply handler-behavior attributes without taking a dependency on the full MetalChain implementation.

---

## Installation

```powershell
dotnet add package RossWright.MetalChain.Abstractions
```

---

## Quick Start

Define request types and apply behavior attributes in your abstractions project:

```csharp
// Command (no response)
public class SendNotificationCommand : IRequest
{
    public string UserId { get; set; }
    public string Message { get; set; }
}

// Query (returns a response)
public class GetUserByIdQuery : IRequest<UserDto>
{
    public string UserId { get; set; }
}

// Allow missing handler — returns default(TResponse) / completes silently
[AllowNoHandler]
public class GetCachedThumbnailQuery : IRequest<byte[]?> { }

// Require a handler even when global options are permissive
[RequireHandler]
public class TransferFundsCommand : IRequest { }

// Opt in to multicast fan-out for this command type
[AllowMultipleHandlers(ExecutionMode = MultipleHandlerExecutionMode.SequentialCollectErrors)]
public class AuditableTransferCommand : IRequest { }
```

Implement handlers and wire up MetalChain in the project that references the full package:

```csharp
// In your application/infrastructure project:
builder.Services.AddMetalChain(options => options.ScanThisAssembly());
```

---

## Key Concepts

### `IRequest` and `IRequest<TResponse>`

Two marker interfaces distinguish the two kinds of mediator message:

| Interface | Purpose |
|---|---|
| `IRequest` | Command — fire-and-forget, no response. |
| `IRequest<TResponse>` | Query — returns a `TResponse` produced by the handler. |

### `IRequestHandler<TRequest>` and `IRequestHandler<TRequest, TResponse>`

Implement these in your handler classes. Each `Send` call resolves the handler inside a **newly created DI scope**, so scoped services (e.g., a `DbContext`) receive a fresh instance per dispatch.

### `IMediator`

The dispatcher contract. Inject `IMediator` wherever you need to send requests. Also provides `Listen<TRequest>` for lightweight runtime subscriptions, and `HasHandlerFor` / `HasListenerFor` for conditional dispatch.

### Handler-Count Attributes

All three attributes live in this package so they can be applied without referencing the full MetalChain package.

| Attribute | Effect |
|---|---|
| `[AllowNoHandler]` | Return `default` / complete silently when no handler is registered. |
| `[RequireHandler]` | Throw even when global options would be permissive. |
| `[AllowMultipleHandlers]` | Allow multiple distinct command handler types; dispatch according to `ExecutionMode`. |

### `MultipleHandlerExecutionMode`

Controls how multiple registered command handlers are invoked when multicast fan-out is enabled:

| Value | Behavior |
|---|---|
| `SequentialFailFast` *(default)* | Run in registration order; first exception stops the chain. |
| `SequentialCollectErrors` | All handlers run; exceptions collected into `AggregateException`. |
| `ParallelCollectErrors` | All handlers run concurrently via `Task.WhenAll`; use only when handlers are fully independent. |

---

## API Summary

| Type | Description |
|---|---|
| `IRequest` | Marker interface for commands (no response). |
| `IRequest<TResponse>` | Marker interface for queries (returns `TResponse`). |
| `IRequestHandler<TRequest>` | Handler contract for commands. |
| `IRequestHandler<TRequest, TResponse>` | Handler contract for queries. |
| `IMediator` | Dispatcher — send requests and register listeners. |
| `MetalChainException` | Base exception raised by MetalChain on dispatch errors. |
| `AllowNoHandlerAttribute` | Per-type: allow missing handler without throwing. |
| `RequireHandlerAttribute` | Per-type: require a handler even when global options are permissive. |
| `AllowMultipleHandlersAttribute` | Per-type: enable multicast fan-out for a command type. |
| `MultipleHandlerExecutionMode` | Enum controlling multicast handler execution strategy. |

---

## See Also

- [RossWright.MetalChain](https://www.nuget.org/packages/rosswright.metalchain) — full implementation with DI registration and assembly scanning
- [RossWright.MetalCore](https://www.nuget.org/packages/rosswright.metalcore) — shared foundation utilities
