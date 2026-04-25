# RossWright.MetalShout
Copyright (c) 2023-2026 Pross Co.

> **Pre-release version available, will release with 2026.1.**

MetalShout is the server-to-client push complement to MetalNexus. It uses SignalR under the hood to let server code dispatch MetalChain commands directly to connected clients, which handle them with ordinary `IRequestHandler` implementations. The result is real-time server push — event notifications, live data updates, progress reporting — using the same request/handler model as the rest of the Metal stack.

## Table of Contents

- [Packages](#packages)
- [How It Works](#how-it-works)
- [Server Setup](#server-setup)
- [Client Setup](#client-setup)
  - [Blazor WebAssembly](#blazor-webassembly)
- [Pushing to Clients](#pushing-to-clients)
  - [Push via IMediator](#push-via-imediator)
  - [Push via IPushServerService](#push-via-ipushserverservice)
  - [Push to All Connected Clients](#push-to-all-connected-clients)
- [Handling Pushes on the Client](#handling-pushes-on-the-client)
  - [MetalChain Handler](#metalchain-handler)
  - [IPushClientService Subscription](#ipushclientservice-subscription)
- [Subscriptions](#subscriptions)
  - [Server-Side Subscriptions](#server-side-subscriptions)
  - [Client-Side Subscriptions](#client-side-subscriptions)
- [Connection Lifecycle](#connection-lifecycle)
  - [UserConnected and UserDisconnected](#userconnected-and-userdisconnected)
  - [IPushConnectionObserver](#ipushconnectionobserver)
- [Configuration](#configuration)
  - [Custom Hub Name](#custom-hub-name)
  - [Custom JSON Serializer Options](#custom-json-serializer-options)
- [See Also](#see-also)
- [License](#license)

---

## Packages

| Package | NuGet | Description |
|---|---|---|
| `RossWright.MetalShout.Server` | [NuGet](https://www.nuget.org/packages/RossWright.MetalShout.Server) | ASP.NET Core server: SignalR hub setup and `IMediator`-based push dispatch to connected clients |
| `RossWright.MetalShout` | [NuGet](https://www.nuget.org/packages/RossWright.MetalShout) | Client library (Blazor and other .NET clients): connects to the SignalR hub and routes incoming pushes to registered MetalChain handlers |

---

## How It Works

1. The server calls `mediator.Push(...)` or injects `IPushServerService` and calls `Push(...)` directly.
2. MetalShout wraps the request in a `Push<TRequest>` command and dispatches it through MetalChain.
3. The built-in `PushRequestHandler<TRequest>` picks it up and forwards the payload to all target clients over SignalR.
4. On the client, the SignalR connection deserializes the payload and either dispatches it to a registered `IRequestHandler<TMessage>` via `IMediator`, or calls any `IPushSubscriber<TMessage>` subscriptions.

The hub requires authentication (`[Authorize]`) as push messages are routed to users, so each connection must be authenticated with a user. MetalShout integrates with MetalGuardian to provide the bearer token automatically on the client side.

---

## Server Setup

Install the server package:

```powershell
dotnet add package RossWright.MetalShout.Server
```

Register MetalShout in `Program.cs`:

```csharp
// Register services
builder.AddMetalShoutServer();

// Map the SignalR hub (after app.Build())
app.UseMetalShoutServer();
```

`AddMetalShoutServer` registers SignalR, the push connection repository, the subscription repository, and `IPushServerService`. It also registers the open-generic `PushRequestHandler<>` with MetalChain so that `mediator.Push(...)` works automatically.

`UseMetalShoutServer` maps the hub at `/PushHub` (the default). Pass a custom path if needed:

```csharp
app.UseMetalShoutServer("my-hub");
```

---

## Client Setup

### Blazor WebAssembly

Install the client package:

```powershell
dotnet add package RossWright.MetalShout
```

Register MetalShout in `Program.cs`:

```csharp
builder.Services.AddMetalShoutClient();
```

Connect after the host is built — MetalShout authenticates via MetalGuardian before opening the SignalR connection:

```csharp
var app = builder.Build();
await app.Services.UseMetalShoutClient();
await app.RunAsync();
```

`UseMetalShoutClient` calls MetalGuardian to authenticate, then connects to the SignalR hub. The connection is tied to the authenticated session and reconnects automatically if the token changes.

---

## Pushing to Clients

### Push via IMediator

The most idiomatic way to push from server code is through `IMediator` using the `Push` extension method:

```csharp
public class OrderShippedHandler(IMediator mediator) : IRequestHandler<ShipOrder>
{
    public async Task Handle(ShipOrder request, CancellationToken cancellationToken)
    {
        // ... shipping logic ...

        await mediator.Push(
            new OrderShipped { OrderId = request.OrderId },
            refId: null,
            userIds: [request.CustomerId],
            cancellationToken);
    }
}
```

`mediator.Push` wraps the request in `Push<TRequest>` and sends it through MetalChain, which routes it to the built-in `PushRequestHandler<TRequest>` and on to SignalR.

### Push via IPushServerService

Inject `IPushServerService` directly for more control:

```csharp
public class NotificationService(IPushServerService pushService)
{
    public Task NotifyUser(Guid userId, OrderShipped message, CancellationToken ct) =>
        pushService.Push(message, userIds: [userId], ct);

    public Task NotifyWithRef(Guid userId, OrderShipped message, string orderId, CancellationToken ct) =>
        pushService.Push(message, refId: orderId, userIds: [userId], ct);
}
```

`Push` sends to all listed user IDs **plus** any users currently subscribed to that message type (and optional `refId`).

### Push to All Connected Clients

```csharp
await pushService.PushToAll(new SystemAlert { Message = "Maintenance in 5 minutes" }, ct);
```

---

## Handling Pushes on the Client

### MetalChain Handler

If `IMediator` is registered on the client, incoming pushes are automatically dispatched to any registered `IRequestHandler<TMessage>`. This is the primary pattern:

```csharp
public class OrderShippedPushHandler : IRequestHandler<OrderShipped>
{
    public Task Handle(OrderShipped request, CancellationToken cancellationToken)
    {
        // React to the push — update state, trigger a re-render, etc.
        return Task.CompletedTask;
    }
}
```

No extra wiring is needed. MetalShout deserializes the payload by assembly-qualified type name and calls `mediator.Send(value)`.

### IPushClientService Subscription

For ad-hoc or component-scoped subscriptions, inject `IPushClientService` and subscribe with a delegate. The returned `IAsyncDisposable` manages the subscription lifetime:

```csharp
@inject IPushClientService PushService
@implements IAsyncDisposable

@code {
    private IAsyncDisposable? _subscription;

    protected override async Task OnInitializedAsync()
    {
        _subscription = await PushService.Subscribe<OrderShipped>(async msg =>
        {
            // Handle the push
            await InvokeAsync(StateHasChanged);
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_subscription != null)
            await _subscription.DisposeAsync();
    }
}
```

Or implement `IPushSubscriber<TMessage>` directly on a class:

```csharp
public class MyComponent : ComponentBase, IPushSubscriber<OrderShipped>, IAsyncDisposable
{
    [Inject] IPushClientService PushService { get; set; } = null!;

    protected override async Task OnInitializedAsync() =>
        await PushService.Subscribe(this);

    public Task OnPushReceived(OrderShipped message)
    {
        // Handle the push
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync() =>
        await PushService.Unsubscribe<OrderShipped>(this);
}
```

---

## Subscriptions

### Server-Side Subscriptions

MetalShout supports a server-managed subscription model. When a user's client subscribes to a message type, any future push of that type is routed to them automatically — even if their user ID is not in the explicit `userIds` array.

Subscribe or unsubscribe a specific user by ID:

```csharp
// Subscribe user to all pushes of this type
pushService.SubscribeUser<OrderShipped>(userId);

// Subscribe user scoped to a specific reference ID
pushService.SubscribeUser<OrderShipped>(userId, refId: orderId);

// Subscribe and receive pushes for all refs and users of this type
pushService.SubscribeUser<OrderShipped>(userId, forAllRefsAndUsers: true);

// Unsubscribe
pushService.UnsubscribeUser<OrderShipped>(userId);
pushService.UnsubscribeUser<OrderShipped>(userId, refId: orderId);
```

Subscribe or unsubscribe the currently authenticated HTTP user (reads `UserId` from the request claims):

```csharp
pushService.SubscribeCurrentUser<OrderShipped>(refId: orderId);
pushService.UnsubscribeCurrentUser<OrderShipped>(refId: orderId);
```

`SubscribeCurrentUser` throws `MetalShoutException` if no `UserId` claim is present.

### Client-Side Subscriptions

Client-side subscription lifetime is controlled by the `IAsyncDisposable` returned from `IPushClientService.Subscribe`. The connection automatically disconnects when no active subscriptions remain.

---

## Connection Lifecycle

### UserConnected and UserDisconnected

When a client connects or disconnects, MetalShout dispatches MetalChain requests to the server's `IMediator`. Handle them like any other MetalChain request:

```csharp
public class UserConnectedHandler : IRequestHandler<UserConnected>
{
    public Task Handle(UserConnected request, CancellationToken cancellationToken)
    {
        // request.UserId — the connecting user
        // request.IsFirstConnection — true if this is the user's first active connection
        return Task.CompletedTask;
    }
}

public class UserDisconnectedHandler : IRequestHandler<UserDisconnected>
{
    public Task Handle(UserDisconnected request, CancellationToken cancellationToken)
    {
        // request.UserId
        // request.WasLastConnection — true if the user has no remaining connections
        // request.Exception — non-null if the disconnect was abnormal
        return Task.CompletedTask;
    }
}
```

`UserConnected` and `UserDisconnected` use `[AllowNoHandler]` semantics — no handler is required.

### IPushConnectionObserver

For services that need connection events outside of MetalChain, implement `IPushConnectionObserver` and register it with DI:

```csharp
public class MyConnectionObserver : IPushConnectionObserver
{
    public Task OnConnected(Guid? userId, bool isFirstConnection)
    {
        // Called when a client connects
        return Task.CompletedTask;
    }

    public Task OnDisconnected(Guid? userId, Exception? exception, bool wasLastConnection)
    {
        // Called when a client disconnects
        return Task.CompletedTask;
    }
}
```

Register it in DI alongside `AddMetalShoutServer`:

```csharp
builder.Services.AddScoped<IPushConnectionObserver, MyConnectionObserver>();
builder.AddMetalShoutServer();
```

Multiple observers are supported; they are all called on every connection event.

---

## Configuration

### Custom Hub Name

The default hub path is `/PushHub`. Override it consistently on both server and client:

```csharp
// Server
app.UseMetalShoutServer("my-hub");

// Client
builder.Services.AddMetalShoutClient(options =>
    options.SetHubName("my-hub"));
```

### Custom JSON Serializer Options

Override the default `JsonSerializerOptions` used for SignalR serialization on either side:

```csharp
// Server
builder.AddMetalShoutServer(options =>
    options.SetJsonSerializerOptions(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }));

// Client
builder.Services.AddMetalShoutClient(options =>
    options.SetJsonSerializerOptions(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }));
```

The default options on both sides include `AllowNamedFloatingPointLiterals` for numeric handling.

---

## See Also

| Library | Description |
|---|---|
| [MetalChain](../MetalChain/README.md) | Mediator library — `IMediator`, `IRequest`, and `IRequestHandler` |
| [MetalNexus](../MetalNexus/README.md) | Client-to-server request routing over REST |
| [MetalGuardian](../MetalGuardian/README.md) | Authentication and authorization — required for MetalShout clients |
| [MetalCore](../MetalCore/RossWright.MetalCore/README.md) | Core utilities shared across all Metal libraries |

---

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
