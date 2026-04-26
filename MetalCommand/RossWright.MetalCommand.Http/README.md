# RossWright.MetalCommand.Http
Copyright (c) 2023-2026 Pross Co.

## Overview

Environment-aware HTTP connection management for MetalCommand. Register one or more named environments against an HTTP service; the `EnvironmentAwareHttpClientFactory` decorator automatically routes `IHttpClientFactory.CreateClient` calls to the environment the user selected — no extra plumbing required in your commands.

## Installation

Add the `RossWright.MetalCommand.Http` package to your project.

## Quick start

```csharp
// Program.cs
var app = ConsoleApplication.CreateBuilder()
    .AddHttpConnections(cfg =>
    {
        cfg.AddDefault("local", "http://localhost:5100");
        cfg.Add("test",          "https://api-test.example.com");
        cfg.AddProtected("prod", "https://api.example.com");
    })
    .AddPingCommand()
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .Build();

await app.RunAsync(args);
```

```csharp
// FetchCommand.cs
[Command("Fetch", HelpBrief = "Fetch data from the API")]
public class FetchCommand(IHttpClientFactory httpFactory) : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign, HelpDetail = "Target API environment")]
    public string? Environment { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        // CreateClient() is automatically routed to the selected environment
        var client = httpFactory.CreateClient();
        var response = await client.GetAsync("/health", ct);
        console.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase}");
        return CommandResult.Ok();
    }
}
```

## Key concepts

### Environment routing
`AddHttpConnections` registers each `(environment, baseAddress)` pair as a named `HttpClient` under the key `"MetalCommand:{env}"` (or `"MetalCommand:{groupName}:{env}"` for named groups). The `EnvironmentAwareHttpClientFactory` decorator intercepts bare `CreateClient()` calls and resolves the key for the currently active environment — commands stay environment-agnostic.

### Multiple connection groups
Use a named group when your tool communicates with more than one independent HTTP service:

```csharp
.AddHttpConnections("payments", cfg =>
{
    cfg.AddDefault("local", "http://localhost:5200");
    cfg.AddProtected("prod", "https://payments.example.com");
})
.AddHttpConnections("notifications", cfg =>
{
    cfg.AddDefault("local", "http://localhost:5300");
    cfg.AddProtected("prod", "https://notify.example.com");
})
```

### `IHttpConnectionResolver`
Use when a single command needs to address multiple environments or groups simultaneously:

```csharp
var localClient = resolver.GetClient("local");
var prodClient  = resolver.GetClient("prod");
```

### Authentication
Pass `authHandlerFactory` when registering an environment to add a `DelegatingHandler` to that environment's HTTP pipeline:

```csharp
cfg.AddProtected("prod", "https://api.example.com",
    authHandlerFactory: sp => new AuthenticationDelegatingHandler(
        sp.GetRequiredService<IMetalGuardianAuthenticationClient>(),
        connectionName: "my-api"));
```

### Ping command
`AddPingCommand()` registers a built-in `ping` command that sends a `GET /` request to the active environment and reports the status code and latency.

## API summary

### `IHttpConnectionsBuilder`

| Method | Description |
|---|---|
| `Add(env, baseAddress, configure?, authHandlerFactory?, isDefault?, isProtected?)` | Registers an environment entry |
| `AddDefault(env, baseAddress, configure?, authHandlerFactory?)` | Registers the default environment |
| `Add(env, baseAddress, configure?, authHandlerFactory?)` | Registers a non-default, non-protected environment |
| `AddProtected(env, baseAddress, configure?, authHandlerFactory?)` | Registers a protected environment |

### `HttpConnectionEntry` properties

| Property | Description |
|---|---|
| `Environment` | The environment name |
| `IsDefault` | Used when no explicit environment is supplied |
| `IsProtected` | Triggers environment-policy enforcement |
| `BaseAddress` | The HTTP service base URL |
| `ConfigureClient` | Optional `Action<HttpClient>` for extra client setup |
| `AuthHandlerFactory` | Optional factory producing a `DelegatingHandler` for this environment |

### `IHttpConnectionResolver`

| Member | Description |
|---|---|
| `GetClientName(environment?, baseConnectionName?)` | Returns the `IHttpClientFactory` key for the given environment and group |
| `GetClient(environment?, baseConnectionName?)` | Convenience wrapper — returns the resolved `HttpClient` |

## See also

- [MetalCommand (core)](../README.md)
- [MetalCommand.Abstractions](../RossWright.MetalCommand.Abstractions/README.md)
