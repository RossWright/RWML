# Ross Wright's Metal Chain
Copyright (c) 2023-2026 Pross Co.

## Table of Contents
- [Introduction](#introduction)
- [Installation](#installation)
- [Defining Requests](#defining-requests)
- [Implementing Handlers](#implementing-handlers)
- [Sending Requests](#sending-requests)
  - [SendOrDefault](#sendordefault)
  - [SendOrIgnore](#sendorignore)
  - [Untyped Send](#untyped-send)
- [Listening For Requests](#listening-for-requests)
- [Esoterica](#esoterica)
  - [Abstraction Library](#abstraction-library)
  - [Explicit Handler Registration](#explicit-handler-registration)
  - [Unhandled Requests](#unhandled-requests)
  - [Multiple Command Handlers (Multicast Fan-Out)](#multiple-command-handlers-multicast-fan-out)
  - [Per-Type Behavior Attributes](#per-type-behavior-attributes)
  - [Excluding Handlers from Scanning](#excluding-handlers-from-scanning)
  - [Multiple Requests Handled by the Same Handler](#multiple-requests-handled-by-the-same-handler)
  - [Open Generic Requests and Handlers](#open-generic-requests-and-handlers)
  - [Conditional Dispatch: HasHandlerFor / HasListenerFor](#conditional-dispatch-hashandlerfor--haslistenerfor)
  - [DI Scope Per Send](#di-scope-per-send)
- [License](#license)
- [Changelog](CHANGELOG.md)

## Introduction
MetalChain is a lightweight, type-safe mediator-pattern-like library for asynchronously dispatching requests to handlers. 
It supports commands (request-only) and queries (request-response) with distinct handling semantics for each. 

MetalChain is different from other popular Mediator libraries in the following ways:
- Handlers are registered independently of the dependency injection framework
- Supports open generic request definitions and handlers
- Command requests support opt-in multicast fan-out to multiple registered handlers
- Run-time subscription to requests via `IMediator.Listen` instead of relying solely on inheritance

### Default Dispatch Behavior

| Scenario | Default |
|---|---|
| No handler registered for a **query** | Throws `MetalChainException` |
| No handler **and** no listener registered for a **command** | Throws `MetalChainException` |
| Duplicate query handler registered | Throws `MetalChainException` at startup |
| Duplicate command handler (distinct type) registered | Throws `MetalChainException` at startup |

All four defaults can be adjusted — see [Esoterica → Unhandled Requests](#unhandled-requests) and [Multiple Command Handlers](#multiple-command-handlers-multicast-fan-out).

### Libraries that use MetalChain
MetalChain is a base technology upon which much of the Metal suite of libraries is built upon, including:
- [MetalNexus](https://www.nuget.org/packages/rosswright.metalnexus) - send a MetalChain request on your client and handle it on your server with minimal setup.
- [MetalGuardian](https://www.nuget.org/packages/rosswright.metalguardian) - includes MetalChain/MetalNexus hooks to make implementing authentication and authorization for your API effortless
- [MetalShout](https://www.nuget.org/packages/rosswright.metalshout) - send a MetalChain request from your server and handle it on your clients with minimal setup.

---

## Installation
Add MetalChain to your project with NuGet include package [RossWright.MetalChain](https://www.nuget.org/packages/rosswright.metalchain)

In your program.cs, add MetalChain to your dependency injection container (IServiceCollection) using the 
`AddMetalChain` extension method and specifying the assemblies to scan for your IRequestHandler implementations:

```csharp
builder.Services.AddMetalChain(options =>
{
    options.ScanThisAssembly();
    options.ScanAssemblyContaining<SendNotificationHandler>();
});
```

---
## Defining Requests

MetalChain distinguishes between two types of requests:

Commands are fire-and-forget requests with no response that implement the IRequest interface:
```csharp
public class SendNotificationCommand : IRequest
{
    public string UserId { get; set; }
    public string Message { get; set; }
}
```

Queries are requests that return a response and implement the IRequest\<TResponse\> interface:
```csharp
public class GetUserByIdQuery : IRequest<UserDto>
{
    public string UserId { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

---
## Implementing Handlers
For commands request handlers implement the IRequestHandler\<TRequest\> interface.
```csharp
public class SendNotificationHandler(
    INotificationService _notificationService)
    : IRequestHandler<SendNotificationCommand>
{
    public async Task Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        await _notificationService.SendAsync(request.UserId, request.Message, cancellationToken);
    }
}
```

For query request handlers implement the IRequestHandler\<TRequest, TResponse\> interface.
```csharp
public class GetUserByIdHandler(
    IUserRepository _userRepository)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
```

---
## Sending Requests
Inject `IMediator` where you need to send requests. Command don't return a response:

```csharp
public class SomeService(IMediator _mediator)
{
    public async Task DoStuff(CancellationToken cancellationToken)
    {
        await _mediator.Send(new SendNotificationCommand(), cancellationToken);
    }
}
```
While queries return a response
```csharp
public class SomeService(IMediator _mediator)
{
    public async Task DoStuff(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserByIdQuery(), cancellationToken);
        // ... use response ...
    }
}
```

### SendOrDefault

When the caller knows a query handler may not be registered and `null` / `default` is an acceptable outcome at that specific call site, use `SendOrDefault`. It returns `TResponse?` — resolving to `default(TResponse)` if no handler is registered, regardless of global settings or type attributes. If a handler *is* registered and throws, the exception propagates normally; `SendOrDefault` only short-circuits to `default` when no handler is registered.

```csharp
// Returns byte[]? — null if no handler is registered
var thumbnail = await mediator.SendOrDefault(new GetCachedThumbnailQuery { AssetId = id });
if (thumbnail is not null)
    RenderThumbnail(thumbnail);
```

### SendOrIgnore

When a command dispatch is intentionally fire-and-forget and no handler may be registered, use `SendOrIgnore`. It always completes silently on a no-handler/no-listener result, regardless of global settings or `[RequireHandler]` on the type:

```csharp
await mediator.SendOrIgnore(new AssetProcessingProgressUpdated { AssetId = id, PercentComplete = 42 });
```

### Untyped Send

When the concrete request type is not known at compile time, use the `Send(object, CancellationToken)` overload. It dynamically dispatches to the correct handler and returns the result as `object?` (or `null` for commands):

```csharp
object request = ResolveRequestAtRuntime();
object? result = await mediator.Send(request);
```

This overload is used internally by [MetalNexus](https://www.nuget.org/packages/rosswright.metalnexus) to route requests arriving over the wire without compile-time knowledge of the concrete type.

---
## Listening for Requests
The `IMediator.Listen` method allows you to register a handler for requests at runtime, without implementing `IRequestHandler`. The subscription remains active for as long as the returned `IDisposable` is alive — dispose it to stop listening. This makes `Listen` a natural fit for `using` blocks or cleanup inside `IDisposable.Dispose` implementations.

Listen works for both command requests and query requests, but for query requests no response can be returned from the listener for the intercepted request.

Register a listener using a `Func<TRequest, CancellationToken, Task>`:

```csharp
var disposable = mediator.Listen<SendNotificationCommand>(
    async (request, cancellationToken) =>
    {
        Console.WriteLine($"Notification sent to {request.UserId}: {request.Message}");
        await Task.CompletedTask;
    });

// Later, when no longer needed, dispose to stop listening
disposable.Dispose();
```

Or pass an existing `IRequestHandler<TRequest>` instance directly:

```csharp
var disposable = mediator.Listen<SendNotificationCommand>(myHandlerInstance);
```

**Execution ordering:** listener tasks are started concurrently with the registered handler(s) and awaited after all handlers complete. A throwing listener will not interrupt handler execution, but its exception will surface afterward.

---
## Esoterica
The above covers 90% of your typical usage of MetalChain. Below you can find information more specialized capabilities and behaviors of the library.

### Abstraction Library
There is a very minimal library of just MetalChain abstractions you can use in your own abstraction libraries without taking a dependency on the full MetalChain package. It exposes `IMediator`, `IRequest`, `IRequest<TResponse>`, `IRequestHandler<TRequest>`, and `IRequestHandler<TRequest, TResponse>`, along with the behavior-control attributes (`AllowNoHandlerAttribute`, `RequireHandlerAttribute`, `AllowMultipleHandlersAttribute`) and the `MultipleHandlerExecutionMode` enum. This allows request types in domain or abstractions projects to carry handler-behavior metadata without referencing the full `RossWright.MetalChain` package. It can be found at [RossWright.MetalChain.Abstractions](https://www.nuget.org/packages/rosswright.metalchain.abstractions)

### Explicit Handler Registration
You can explicitly specify request handlers to be registered using `AddMetalChainHandlers` rather than - or in addition to - the assembly 
scanning of the base initialization. This can be done on the `IServiceCollection` before or after initialization of MetalChain, and can be called
multiple times with one or more handler types. You can even skip assembly scanning altogether by not specifying any assemblies to scan. 
```csharp
builder.Services.AddMetalChainHandlers(
    typeof(CreateUserCommandHandler),
    typeof(UpdateUserCommandHandler));

builder.Services.AddMetalChain();

builder.Services.AddMetalChainHandlers(typeof(GetUserByIdHandler));
```

### Unhandled Requests

By default, MetalChain throws `MetalChainException` when a query is dispatched with no registered handler, or when a command is dispatched with no registered handler and no active listener. Three modalities control this behavior, from most to least specific:

**1. Per-dispatch — `SendOrDefault` / `SendOrIgnore`**

`SendOrDefault` returns `default(TResponse)` on a query miss. `SendOrIgnore` completes silently on a command miss. The call site always wins, regardless of any attribute or global setting. See [SendOrDefault](#sendordefault) and [SendOrIgnore](#sendorignore) in the Sending Requests section.

**2. Per-type attribute**

Apply `[AllowNoHandler]` to a specific request type. The attribute lives in `RossWright.MetalChain.Abstractions`, so domain/abstractions projects can use it with no dependency on the full MetalChain package:

```csharp
[AllowNoHandler]
public class GetCachedThumbnailQuery : IRequest<byte[]?> { ... }

[AllowNoHandler]
public class AssetProcessingProgressUpdated : IRequest { ... }
```

When the global setting makes unhandled requests permissive, use `[RequireHandler]` to opt a critical request back in to strict behavior:

```csharp
[RequireHandler]
public class TransferFundsCommand : IRequest { ... }
```

**3. Global option**

```csharp
builder.Services.AddMetalChain(options =>
{
    options.ScanThisAssembly();
    options.AllowUnhandledQueries();   // return default(TResponse) instead of throwing
    options.AllowUnhandledCommands();  // complete silently instead of throwing
});
```

**Precedence:** per-dispatch > per-type attribute > global option > built-in default (throw).

### Multiple Command Handlers (Multicast Fan-Out)

By default, registering two distinct handler types for the same command throws `MetalChainException` at startup — the same strict default that applies to query handlers. Unlike queries, commands can meaningfully fan out to multiple handlers when you explicitly opt in. When multicast is enabled, `Send` dispatches to all registered handlers according to the configured execution mode.

> **Query handlers are always one-per-type.** Multiple `IRequestHandler<TRequest, TResponse>` registrations for the same query type always throw at startup with no option to allow them — which handler fires is nondeterministic. Use [Excluding Handlers from Scanning](#excluding-handlers-from-scanning) to resolve a conflict from a scanned assembly.

**Global option:**
```csharp
builder.Services.AddMetalChain(options =>
{
    options.ScanThisAssembly();
    options.AllowMultipleCommandHandlers(); // SequentialFailFast by default
});
```

**Per-type attribute (recommended — applies only to the type that needs it):**
```csharp
[AllowMultipleHandlers]
public class AuditableTransferCommand : IRequest { ... }

public class AuditTransferHandler   : IRequestHandler<AuditableTransferCommand> { ... }
public class NotifyTransferHandler  : IRequestHandler<AuditableTransferCommand> { ... }
// Both handlers are called on Send
```

**Execution modes:**

| Mode | Behavior |
|---|---|
| `SequentialFailFast` *(default)* | Handlers run one at a time in registration order. First exception stops the chain. |
| `SequentialCollectErrors` | All handlers run regardless of failures; exceptions collected into `AggregateException`. |
| `ParallelCollectErrors` | All handlers run concurrently via `Task.WhenAll`. Use only when handlers are fully independent. |

Set the mode globally or per-type:
```csharp
// Global
builder.Services.AddMetalChain(options =>
    options.AllowMultipleCommandHandlers(MultipleHandlerExecutionMode.SequentialCollectErrors));

// Per-type
[AllowMultipleHandlers(ExecutionMode = MultipleHandlerExecutionMode.ParallelCollectErrors)]
public class NotifyTransferCommand : IRequest { ... }
```

> **Tip:** `Listen` remains the preferred pattern for lightweight side-effect observation. Use multicast command handlers when multiple independent services must formally handle the same command. Registering the same handler type twice for the same command is always a silent no-op regardless of settings.

### Per-Type Behavior Attributes

All three behavior attributes live in `RossWright.MetalChain.Abstractions` so they can be applied in projects that do not reference the full `RossWright.MetalChain` package.

| Attribute | Applies to | Effect |
|---|---|---|
| `[AllowNoHandler]` | `IRequest` or `IRequest<TResponse>` | Return `default` / complete silently when no handler is found. |
| `[RequireHandler]` | `IRequest` or `IRequest<TResponse>` | Throw even when the global setting would be permissive. |
| `[AllowMultipleHandlers]` | `IRequest` only | Allow multiple distinct handler types; dispatch according to `ExecutionMode`. |

> **Note:** Applying `[AllowMultipleHandlers]` to an `IRequest<TResponse>` (query) type has no effect. The attribute is only meaningful on command types (`IRequest`). Multiple query handler registrations always throw at startup — see [Multiple Command Handlers](#multiple-command-handlers-multicast-fan-out).

### Excluding Handlers from Scanning

When scanning an assembly that ships a default handler you want to replace, use `IgnoreHandler<T>()` to suppress it. The excluded type is silently skipped in all scanning and explicit registration passes, regardless of which assembly it comes from:

```csharp
builder.Services.AddMetalChain(options =>
{
    options.ScanThisAssembly();
    options.ScanAssembly(typeof(SomeThirdPartyType).Assembly);
    options.IgnoreHandler<DefaultGetUserProfileHandler>(); // exclude the shipped default
});
```

`IgnoreHandler` works for both query and command handlers and is the only supported way to resolve a duplicate query handler conflict arising from assembly scanning.

### Multiple Requests Handled by the Same Handler
You can implement `IRequestHandler` multiple times on one class, specifying a different request for each, potentially mixing command and query 
requests. Note handlers are instantiated and injected upon each and every invocation, so there is no real benefit for this beyond code organization.
```csharp
public class DataQueryHandler(MyDbContext _dbCtx)
    : IRequestHandler<GetUserByIdQuery, UserDto>,
    IRequestHandler<CreateUserCommand>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // return the user using the DbContext
    }

    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // create the user using the DbContext
    }
}

```

### Open Generic Requests and Handlers
You can define requests with open generic parameters
```csharp
public class PostResultCommand<TResult> : IRequest
    where TResult : ISomeThing
{
    public TResult SomeThing { get; set; }
    public string Context { get; set; }
}

public class PostResultCommandHandler<TResult> : IRequestHandler<PostResultCommand<TResult>>
    where TResult : ISomeThing
{
    public async Task Handle(PostResultCommand<TResult> request, CancellationToken cancellationToken)
    {
        await request.SomeThing.Log(request.Context, cancellationToken); //where DoStuff is defined on ISomeThing
    }
}

var someThing = new SomeThingImpl();
var someOtherThing = new OtherSomeThingImpl();
var postResultCommand = new PostResultCommand<SomeThingImpl>
{
    SomeThing = someThing
};
await _mediator.Send(postResultCommand, cancellationToken);

var postOtherResultCommand = new PostResultCommand<OtherSomeThingImpl>
{
    SomeThing = someOtherThing
};
await _mediator.Send(postOtherResultCommand, cancellationToken);
```
This allows you to define a request and request handler scheme once and use it with any type at run-time where in some mediator libraries 
you would need to implement closed generic classes for IRequestHandler for each specific type.

This becomes particularly powerful when used to wrap other requests that are in turn sent via IMediator
```csharp
public class QueryWrapper<TRequest, TResponse> : IRequest<TResponse>
    where TRequest : IRequest<TResponse>
{
    public TRequest Request { get; set; }
    public string UserId { get; set; }
    public string Context { get; set; }
}

public class QueryWrapperRequestHandler<TRequest, TResponse>(IMediator _mediator)
    : IRequestHandler<QueryWrapper<TRequest, TResponse>, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        QueryWrapper<TRequest, TResponse> wrapper,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new SendNotificationCommand()
        {
            UserId = wrapper.UserId,
            Message = wrapper.Context + " Starting..."
        }, cancellationToken);

        TResponse response;
        try
        {
            response = await _mediator.Send(wrapper.Request, cancellationToken);
        }
        catch (Exception exception)
        {
            await _mediator.Send(new SendNotificationCommand()
            {
                UserId = wrapper.UserId,
                Message = wrapper.Context + " Failed with error: " + exception.Message
            }, cancellationToken);
            return default!;
        }

        await _mediator.Send(new SendNotificationCommand()
        {
            UserId = wrapper.UserId,
            Message = wrapper.Context +
                " Completed with response: " +
                (response?.ToString() ?? "<null>")
        }, cancellationToken);

        return response;
    }
}

...
var wrapper = new QueryWrapper<GetUserByIdQuery, UserDto>
{
    Request = new GetUserByIdQuery { UserId = queryUserId },
    UserId = executingUserId,
    Context = "Administaion Page"
};

var userDto = await _mediator.Send(wrapper);
```

### Conditional Dispatch: HasHandlerFor / HasListenerFor

`IMediator` exposes `HasHandlerFor(Type)` and `HasListenerFor(Type)` to check registration state at runtime. Generic extension methods are also available:

```csharp
if (mediator.HasHandlerFor<GetCachedThumbnailQuery>())
{
    var thumbnail = await mediator.Send(new GetCachedThumbnailQuery { AssetId = id });
}

if (mediator.HasListenerFor<AssetProcessingProgressUpdated>())
{
    // at least one active listener is registered
}
```

These are the building blocks `SendOrDefault` and `SendOrIgnore` use internally. Call them directly when you need conditional dispatch logic that goes beyond what those helpers provide.

### DI Scope Per Send

Each call to `Send` (in any overload) resolves its handlers in a **newly created DI scope**. This means:

- Scoped services injected into a handler get a fresh instance per dispatch.
- Handlers are never shared across concurrent or sequential `Send` calls.
- The scope is disposed when the handler returns.

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