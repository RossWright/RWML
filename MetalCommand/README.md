# Ross Wright's Metal Command
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Defining Commands](#defining-commands)
  - [CommandResult](#commandresult)
  - [Migrating from ILegacyCommand](#migrating-from-ilegacycommand)
  - [ILegacyCommand (legacy style)](#ilegacycommand-legacy-style)
- [Registering Commands](#registering-commands)
- [Middleware](#middleware)
- [Running the Application](#running-the-application)
- [The IConsole API](#the-iconsole-api)
  - [IConsole Helpers](#iconsole-helpers)
  - [Progress Indicators](#progress-indicators)
- [ICommandExecutor](#icommandexecutor)
- [Database Tooling](#database-tooling)
  - [IDatabaseContextFactory](#idatabasecontextfactory)
  - [Built-in Data Commands](#built-in-data-commands)
  - [CsvFile](#csvfile)
- [HTTP Connections](#http-connections)
  - [Installation](#installation-1)
  - [Quick start](#quick-start)
  - [MetalNexus integration](#metalnexus-integration)
  - [Multiple environments per command](#multiple-environments-per-command)
  - [Authentication](#authentication)
  - [IHttpConnectionResolver API](#ihttpconnectionresolver-api)
  - [IHttpConnectionsBuilder API](#ihttpconnectionsbuilder-api)
  - [Ping command](#ping-command)
- [License](#license)

---

## Overview

MetalCommand is a framework for building interactive .NET console applications. It provides a host-builder pattern (`ConsoleApplication.CreateBuilder`) that sets up configuration, dependency injection, and a command-dispatch loop — all without depending on MetalChain or MetalInjection (though it is compatible with both via `SetServiceProviderFactory`).

Commands are plain classes that implement `ICommand`. Decorate the class with `[Command]` and any argument-carrying properties with `[Arg]` — the framework binds raw input to those properties before calling `ExecuteAsync`. Commands return a `CommandResult` to signal success, failure, or loop-exit intent.

A session looks like this:

```
Found 6 commands. Type "Help" to get help.
> help
  Help / Man / H [command] - Display all available commands or detailed help for a command
  Exit / Bye / Quit - End program
  migrate [env] - Run EF Core migrations
  load [env] - Load seed data from CSV files
  reload [env] - Obliterate and reload the database
  obliterate env - Drop all tables, FKs, and stored procedures
> greet Alice
Executing Greet. Use Ctrl-C to abort (and Ctrl-C again to abort program).
  Hello, Alice!
Greet completed in less than a second.
> exit
Goodbye!
```

---

## Installation

```powershell
dotnet add package RossWright.MetalCommand
```

For database tooling commands, also add the data package and your database provider:

```powershell
dotnet add package RossWright.MetalCommand.Data

# SQL Server:
dotnet add package RossWright.MetalCommand.Data.SqlServer

# MySQL / MariaDB:
dotnet add package RossWright.MetalCommand.Data.MySql
```

---

## Defining Commands

Implement `ICommand` and decorate the class with `[Command]`. Declare each argument as a public property decorated with `[Arg]`. The framework reads the attributes via reflection, binds argument values to the properties, and calls `ExecuteAsync`.

```csharp
[Command("Greet", HelpBrief = "Print a greeting")]
public class GreetCommand(IGreetingService service) : ICommand
{
    [Arg(IsRequired = true, HelpDetail = "The name to greet")]
    public string Name { get; set; } = "";

    [Arg(DefaultValue = "Hello", HelpDetail = "The greeting word to use")]
    public string Greeting { get; set; } = "Hello";

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        var message = await service.BuildGreeting(Greeting, Name, cancellationToken);
        console.WriteLine(message);
        return CommandResult.Ok();
    }
}
```

### `[Command]` attribute

| Property | Description |
|---|---|
| `Name` | Display name shown in help and run/completion messages |
| `Invocations` | One or more strings the user can type to invoke the command (case-insensitive). Defaults to `[name.ToLower()]` when omitted |
| `HelpBrief` | Short one-line description shown in the command list |
| `HelpDetail` | Optional longer description shown when `help <command>` is called |

### `[Arg]` attribute

| Property | Type | Description |
|---|---|---|
| `Name` | `string?` | Display name used in help and error messages. Defaults to the property name |
| `Order` | `int` | 0-based positional order. When `-1` (default), declaration order is used |
| `IsRequired` | `bool` | Execution is aborted with an error if no value is resolved |
| `DefaultValue` | `string?` | Literal fallback when no value is supplied |
| `ContextKey` | `string?` | Session-context dictionary key used as a fallback; resolved value is echoed to the console |
| `ValidValues` | `string[]?` | Restricts accepted values (case-insensitive); works for `string` and `enum` properties |
| `HelpDetail` | `string?` | Description shown in `help <command>` output |
| `AllowNamed` | `bool` | Also accepts `--PropertyName value` syntax; boolean properties also accept bare `--PropertyName` as `true` |

**Supported property types:** `string`, `int`, `double`, `bool`, `Guid`, `DateTime`, and any `enum`.

---

### CommandResult

`CommandResult` is returned by `ExecuteAsync` to signal the outcome. `ExitApplication` controls whether the interactive loop exits after the command completes — `Success` is informational only.

| Factory | Description |
|---|---|
| `CommandResult.Ok()` | Command succeeded; loop continues |
| `CommandResult.Fail(message?)` | Command failed with an optional message; loop continues |
| `CommandResult.Exit()` | Command succeeded and requests a clean loop exit — equivalent to the user typing `exit` |
| `CommandResult.FailAndExit(message?)` | Command failed and requests a loop exit; the message is written as an error before the goodbye line |

An implicit conversion from `bool` is also provided: `true` → `Ok()`, `false` → `Fail()`.

---

### Migrating from ILegacyCommand

Existing `ILegacyCommand` implementations continue to work without any changes. To migrate a command to the new style:

1. Replace `: ILegacyCommand` with `: ICommand`.
2. Remove the `Descriptor` property and replace it with `[Command]` on the class and `[Arg]` on argument properties.
3. Rename `Execute(IConsole, string[], CancellationToken)` to `ExecuteAsync(IConsole, CancellationToken)`, update the body to read from the typed properties instead of `args[n]`, and return a `CommandResult`.

---

### ILegacyCommand (legacy style)

The original descriptor-based interface is still fully supported and will not be removed. New code should use `ICommand` instead.

```csharp
public class GreetCommand : ILegacyCommand
{
    public CommandDescriptor Descriptor => new()
    {
        Name = "Greet",
        Invocations = ["greet", "hi"],
        HelpBrief = "Print a greeting",
        Args =
        [
            ArgumentDescriptor.Required("name", "The name to greet"),
            ArgumentDescriptor.Optional("greeting", "Hello", "The greeting word to use")
        ]
    };

    public async Task Execute(IConsole console, string[] args, CancellationToken cancellationToken)
    {
        var name     = args[0];
        var greeting = args[1];
        console.WriteLine($"{greeting}, {name}!");
        await Task.CompletedTask;
    }
}
```

### `CommandDescriptor`

| Property | Description |
|---|---|
| `Name` | Display name shown in help and run/completion messages |
| `Invocations` | One or more strings the user can type to invoke the command (case-insensitive) |
| `HelpBrief` | Short one-line description shown in the command list |
| `HelpDetail` | Optional longer description shown when `help <command>` is called |
| `Args` | Ordered array of `ArgumentDescriptor` entries |

### `ArgumentDescriptor` factory methods

> New commands should use `[Arg]` properties instead. These factory methods remain for use with `ILegacyCommand`.

| Factory | Description |
|---|---|
| `Required(name, help?)` | Positional required argument; execution is aborted if not supplied |
| `Optional(name, defaultValue, help?)` | Positional optional argument with a literal default |
| `RequiredWithContext(name, contextKey, help?)` | Required argument that falls back to a named context value |
| `OptionalWithContext(name, contextKey, help?)` | Optional argument that falls back to a named context value |
| `RequiredWithValidValues(name, validValues[], help?)` | Required argument restricted to an explicit set of values |
| `OptionalWithValidValues(name, validValues[], defaultValue, help?)` | Optional argument restricted to a set of values |

Argument values drawn from context echo the resolved value to the console as a warning before execution. Values not in `ValidValues` are rejected before `Execute` is called.

---

## Registering Commands

Commands are registered on `IConsoleApplicationBuilder.Commands` during startup. Use `AddCommands` for a fluent call:

```csharp
var app = ConsoleApplication.CreateBuilder()
    .AddCommands(cmds =>
    {
        cmds.Add<GreetCommand>();
        cmds.Add<ReportCommand>();
    })
    .AddServices(services =>
    {
        services.AddScoped<IReportService, ReportService>();
    })
    .SetColors(introOutroColor: ConsoleColor.DarkCyan, helpColor: ConsoleColor.Cyan)
    .Build();

await app.RunAsync(args);
```

Commands can also be discovered automatically via assembly scanning:

```csharp
.AddCommands(cmds => cmds.ScanThisAssembly())
```

### `IConsoleApplicationBuilder` members

| Member | Description |
|---|---|
| `Commands` | `ICommandCollection` — register or scan for `ICommand` implementations |
| `Services` | `IServiceCollection` — register DI services consumed by commands |
| `Configuration` | `IConfiguration` — pre-built from `appsettings.json` and `appsettings.dev.json` |
| `Console` | `IConsole` — the shared console instance |
| `AddServices(Action<IServiceCollection>)` | Fluent helper to register DI services |
| `AddCommands(Action<ICommandCollection>)` | Fluent helper to register commands |
| `AddCommands(Action<ICommandCollection, IConfiguration>)` | Fluent helper with access to configuration |
| `SetColors(introOutro?, help?, warning?, error?, errorBg?)` | Customise per-role console colors |
| `SetTabWidth(int)` | Set the number of spaces per indent level (default: 5) |
| `SetPromptFactory(Func<context, string>?)` | Supply a delegate that builds the prompt string from the current context |
| `AddMiddleware<TMiddleware>()` | Register a middleware type to participate in the new-style command pipeline |
| `SetServiceProviderFactory(IServiceProviderFactory)` | Plug in an alternate `IServiceProvider` implementation (e.g. MetalInjection) |

---

## Middleware

Middleware participates in the new-style (`ICommand`) execution pipeline. Each middleware wraps the next — allowing you to add logging, timing, error handling, or other cross-cutting concerns.

Register middleware on the builder before calling `Build()`:

```csharp
var app = ConsoleApplication.CreateBuilder()
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .AddMiddleware<LoggingMiddleware>()
    .AddMiddleware<TimingMiddleware>()
    .Build();
```

Implement `ICommandMiddleware`. Call `next(context)` to continue the pipeline. Middleware is executed in registration order (first registered = outermost wrapper).

```csharp
public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : ICommandMiddleware
{
    public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
    {
        logger.LogInformation("Executing {Command}", context.Command.GetType().Name);
        await next(context);
        logger.LogInformation("Completed {Command} — Success: {Success}",
            context.Command.GetType().Name, context.Result.Success);
    }
}
```

Middleware instances are resolved from DI per execution, so constructor injection works normally.

### `CommandContext` properties

| Property | Description |
|---|---|
| `Command` | The `ICommand` instance being executed |
| `Console` | The `IConsole` for the current session |
| `SessionContext` | The shared `IDictionary<string, string>` context |
| `CancellationToken` | The cancellation token for the current command |
| `BoundArgs` | `IReadOnlyDictionary<string, object?>` of bound argument values (set after `ArgBinder` runs) |
| `Result` | The `CommandResult` written by `ExecuteAsync` (readable after `next()` returns) |

> Middleware only applies to new-style `ICommand` implementations. `ILegacyCommand` executions bypass the pipeline.

---

## Running the Application

`ConsoleApplication.Build()` returns a `ConsoleApplication`. Call `RunAsync` to start the interactive loop.

```csharp
await app.RunAsync(args);
```

If `args` contains a command string, that command runs immediately and the loop then waits for further input. When running non-interactively (e.g. in CI), pipe commands via stdin or pass them as command-line arguments.

Built-in loop commands (always available):

| Input | Aliases | Description |
|---|---|---|
| `help [command]` | `man`, `h` | List all commands, or show detailed help for one |
| `exit` | `bye`, `quit` | End the session |

`Ctrl-C` cancels the running command (passes `CancellationToken`). A second `Ctrl-C` while no command is running terminates the process.

### `ConsoleApplication` members

| Member | Description |
|---|---|
| `RunAsync(args)` | Start the interactive read-execute loop |
| `Execute(invocation, args)` | Programmatically dispatch a command by invocation string |
| `Execute<TCommand>(args)` | Programmatically dispatch a command by type |
| `Context` | `IDictionary<string, string>` — session-wide key/value store shared with all commands |
| `Console` | The `IConsole` instance |
| `Services` | The built `IServiceProvider` |
| `AddContext(key, value)` | Fluent extension to pre-populate the context before `RunAsync` |

---

## The IConsole API

`IConsole` is the abstraction commands use for all output and input. It supports color, indentation, and cursor control, and is injected into every `Execute` call.

| Method | Description |
|---|---|
| `Write(message, textColor?, bgColor?)` | Write text at the current position without a newline |
| `WriteLine(message?, textColor?, bgColor?)` | Write a line (or a blank line if message is null/empty) |
| `WriteError(message)` | Write error-colored text without a newline |
| `WriteErrorLine(message?)` | Write an error-colored line prefixed with `ERROR:` |
| `ReadLine()` | Read a line of input from the user |
| `Indent()` | Increase indent level; returns `IDisposable` — dispose to restore |
| `ResetIndent()` | Reset indent level to zero |
| `ResetLine()` | If not at the start of a line, emit a newline |
| `HideCursor()` | Hide the terminal cursor; returns `IDisposable` — dispose to restore |

### Extension methods on `IConsole`

| Method | Description |
|---|---|
| `Announce(message, action, conclusion?)` | Write `"message... "`, run `action`, then write `"Done!"` (or custom conclusion) on the same line |
| `AnnounceAsync(message, action, conclusion?)` | Async version of `Announce` |
| `Announce<TResult>(message, func, conclusion?)` | Like `Announce` but captures and returns the result of `func` |
| `AnnounceAsync<TResult>(message, func, conclusion?)` | Async version with return value |
| `WriteLineIndented(text)` | Split multi-line text and write each line at the current indent level |
| `DumpJson(json)` | Pretty-print a JSON string at the current indent |
| `DumpJson(obj)` | Serialize an object to JSON and pretty-print it |

### IConsole Helpers

| Method | Description |
|---|---|
| `Confirm(prompt, defaultYes?)` | Writes a yes/no prompt; accepts `y`/`yes`/`n`/`no` (case-insensitive); re-prompts on invalid input. Returns `true` for yes. When `defaultYes` is `true`, pressing Enter without input counts as yes |
| `Prompt(prompt, defaultValue?)` | Writes a prompt and returns the user's input, or `defaultValue` if the user presses Enter without typing anything |
| `Choose<T>(prompt, options[])` | Presents a numbered list of options and returns the chosen value; re-prompts on invalid input. `T` can be any type — `string`, `enum`, etc. |

```csharp
if (!console.Confirm("Are you sure you want to obliterate the database?"))
    return CommandResult.Fail("Aborted.");

var env = console.Choose("Select environment:", new[] { "dev", "staging", "prod" });
var name = console.Prompt("Enter a name", defaultValue: "World");
```

### Progress Indicators

`ShowProgress` renders one or more `IProgressIndicator` instances inline on the current line, updating them as the body callback reports progress via an `Action<double>` (0.0–1.0).

```csharp
var result = await console.ShowProgress(async report =>
{
    for (int i = 0; i < items.Count; i++)
    {
        await ProcessAsync(items[i], cancellationToken);
        report((double)(i + 1) / items.Count);
    }
    return items.Count;
});
```

Three built-in indicator types:

| Type | Width | Description |
|---|---|---|
| `Spinner` | 1 | Rotating `\`, `\|`, `/`, `—` character |
| `ProgressBar` | 52 | Unicode block-fill bar with percentage text overlaid |
| `Percentage` | 4 | Plain `" 42%"` numeric display |

By default `ShowProgress` uses `[Spinner, ProgressBar]`. Pass a custom `IProgressIndicator[]` to override.

---

## ICommandExecutor

`ICommandExecutor` is registered as a singleton in the DI container. Inject it into commands that need to dispatch other commands programmatically (e.g. a `reload` command that calls `migrate` then `load`).

```csharp
[Command("Reload", HelpBrief = "Obliterate and reload the database")]
public class ReloadCommand(ICommandExecutor executor) : ICommand
{
    [Arg(IsRequired = true, HelpDetail = "Target environment")]
    public string Env { get; set; } = "";

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        await executor.Execute<MigrateCommand>(Env);
        await executor.Execute<LoadDataCommand>(Env);
        return CommandResult.Ok();
    }
}
```

| Member | Description |
|---|---|
| `Execute(invocation, args)` | Dispatch a command by invocation string |
| `Execute<TCommand>(args)` | Dispatch a command by type |
| `Context` | The shared session context dictionary |

---

## Database Tooling

`RossWright.MetalCommand.Data` adds EF Core integration and a set of ready-made database management commands. Add `RossWright.MetalCommand.Data.SqlServer` or `RossWright.MetalCommand.Data.MySql` for the matching provider registration helpers.

### IDatabaseContextFactory

Register one or more named database environments (e.g. `dev`, `staging`, `prod`) against a `DbContext` type. The factory is then injected into data commands and your own `ICommand` implementations.

```csharp
ConsoleApplication.CreateBuilder()
    .AddDatabaseContextFactory<AppDbContext>(db =>
    {
        db.AddDefault("dev",     opts => opts.UseSqlServer(config["Dev:ConnectionString"]));
        db.AddProtected("prod",  opts => opts.UseSqlServer(config["Prod:ConnectionString"]));
    })
    // ...
```

| `IDatabaseContextFactoryBuilder` method | Description |
|---|---|
| `Add(env, opts, isDefault?, isProtected?)` | Register a named environment |
| `AddDefault(env, opts)` | Register and mark as the default (used when no env argument is supplied) |
| `AddProtected(env, opts)` | Register a protected environment (excluded from commands that accept only unprotected targets) |
| `AddDefaultProtected(env, opts)` | Register as both default and protected |

`IDatabaseContextFactory<TDbContext>` is registered as scoped. Inject it directly in commands that need raw `DbContext` access. The helper `GetEnvironmentArg` and `GetUnprotectedEnvironmentArg` extension methods produce ready-made `ArgumentDescriptor` entries whose `ValidValues` are drawn from the registered environments.

### Built-in Data Commands

Add any of these to your builder instead of writing the commands yourself:

| Extension method | Invocation | Description |
|---|---|---|
| `AddMigrateCommand<TDbCtx>(pre?, post?)` | `migrate [env]` | Runs `Database.MigrateAsync()` against the selected environment, with optional pre/post callbacks |
| `AddLoadDataCommand<TDbCtx>(loadFunc, path?)` | `load [env]` | Calls your `loadFunc` delegate with a `LoadDataCommandContext` containing the `DbContext` and a `CsvFile<T>` helper for the target path |
| `AddReloadDatabaseCommand<TDbCtx>()` | `reload [env]` | Obliterates and re-migrates the database, then runs the load function |
| `AddObliterateCommand<TDbCtx>()` | `obliterate env` | Drops all FK constraints, tables, and stored procedures — protected environments require explicit naming |
| `AddClearDataCommand<TDbCtx>()` | `clear [env]` | Deletes all data without dropping the schema |

> **`obliterate` is destructive.** It requires the environment name to be typed explicitly and is blocked on protected environments by default.

All built-in commands accept a `CommandDescriptor` override to customise the invocation name, aliases, or help text.

### CsvFile

`CsvFile<T>` reads a CSV file into a typed collection using [CsvHelper](https://joshclose.github.io/CsvHelper/). It is the standard way to load seed data inside a `loadFunc` callback.

```csharp
.AddLoadDataCommand<AppDbContext>(async ctx =>
{
    var users = new CsvFile<UserRow>("data/users.csv");
    await ctx.DbContext.Users.RefreshTable(users.Rows.Select(r => r.ToEntity()));
    await ctx.DbContext.SaveChangesAsync();
})
```

By default (no path argument) `CsvFile<T>` resolves `data\{TypeName}.csv` relative to the application base directory. Extra columns and blank lines are silently ignored; missing columns fall back to default values.

---

## HTTP Connections

`RossWright.MetalCommand.Http` adds environment-aware HTTP client management to MetalCommand.
Register one or more named environments against an HTTP service; the framework automatically
routes `IHttpClientFactory.CreateClient` calls to the environment the user selected — no extra
plumbing in your commands.

### Quick start

Register connection environments with `AddHttpConnections` and inject `IHttpClientFactory`
or `IHttpConnectionResolver` into your command:

```csharp
// Program.cs
ConsoleApplication.CreateBuilder()
    .AddHttpConnections(cfg =>
    {
        cfg.AddDefault("local", "http://localhost:5100");
        cfg.Add("test",         "https://api-test.example.com");
        cfg.AddProtected("prod","https://api.example.com");
    })
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .Build();
```

```csharp
// MyApiCommand.cs
[Command("Fetch", HelpBrief = "Fetch data from the API")]
public class FetchCommand(IHttpClientFactory httpFactory) : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign,
        HelpDetail = "The API environment to target")]
    public string? Environment { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        // IHttpClientFactory.CreateClient() automatically routes to the selected environment
        var client = httpFactory.CreateClient();
        var response = await client.GetAsync("/health", cancellationToken);
        console.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase}");
        return CommandResult.Ok();
    }
}
```

The `EnvironmentAwareHttpClientFactory` decorator intercepts `CreateClient()` and resolves
the correct named client for the active environment — your command code stays clean and
environment-agnostic.

### MetalNexus integration

When your commands dispatch requests via `IMediator.Send` (MetalNexus), the decorator is
already transparent: `IHttpClientFactory.CreateClient(name)` with a bare service name (e.g.
`"payments"`) is mapped to the environment-qualified key `"MetalCommand:payments:prod"`
automatically. No changes are needed to existing MetalNexus handlers.

For scenarios where you need an explicit environment qualifier per-request (e.g. fanning out
across environments), use `IHttpConnectionResolver.GetClientName` to build the key and pass it
to `SendVia`.

### Multiple environments per command

Use `IHttpConnectionResolver` directly when a single command needs to address more than one
environment at once:

```csharp
[Command("Compare", HelpBrief = "Compare responses across environments")]
public class CompareCommand(IHttpConnectionResolver resolver) : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        var localClient = resolver.GetClient("local", null);
        var prodClient  = resolver.GetClient("prod",  null);

        var localResponse = await localClient.GetAsync("/api/status", cancellationToken);
        var prodResponse  = await prodClient.GetAsync("/api/status",  cancellationToken);

        console.WriteLine($"local : {(int)localResponse.StatusCode}");
        console.WriteLine($"prod  : {(int)prodResponse.StatusCode}");
        return CommandResult.Ok();
    }
}
```

### Authentication

Two patterns are supported — choose whichever fits your setup.

#### Option 1 — MetalGuardian `AuthenticationDelegatingHandler`

If you use MetalGuardian for auth, pass an `authHandlerFactory` when registering the environment:

```csharp
cfg.AddProtected("prod", "https://api.example.com",
    authHandlerFactory: sp => new AuthenticationDelegatingHandler(
        sp.GetRequiredService<IMetalGuardianAuthenticationClient>(),
        connectionName: "my-api"));
```

MetalGuardian handles token acquisition, refresh, and caching transparently.

#### Option 2 — Custom `DelegatingHandler`

Any `DelegatingHandler` is accepted. For a simple API-key header:

```csharp
cfg.AddProtected("prod", "https://api.example.com",
    authHandlerFactory: sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new ApiKeyHandler(config["ApiKey"]!);
    });
```

```csharp
public class ApiKeyHandler(string apiKey) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Api-Key", apiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
```

### `IHttpConnectionResolver` API

| Member | Description |
|---|---|
| `GetClientName(environment?, baseConnectionName?)` | Returns the `IHttpClientFactory` key for the given environment and optional connection group. Pass `null` for either to use the active/default value. |
| `GetClient(environment?, baseConnectionName?)` | Convenience wrapper — calls `GetClientName` and returns the resolved `HttpClient`. |

### `IHttpConnectionsBuilder` API

| Method | Description |
|---|---|
| `AddDefault(env, baseAddress, configure?, authHandlerFactory?)` | Register an environment and mark it as the default (used when no explicit `--env` is supplied). |
| `Add(env, baseAddress, configure?, authHandlerFactory?)` | Register a standard non-default, non-protected environment. |
| `AddProtected(env, baseAddress, configure?, authHandlerFactory?)` | Register a protected environment (e.g. production). |

All three methods accept an optional `configure` delegate (`Action<HttpClient>`) for extra client setup (e.g. default headers or timeout) and an optional `authHandlerFactory` delegate for DI-resolved authentication handlers.

### Ping command

Call `AddPingCommand()` to add the built-in `ping` command. It sends a `GET /` request to
the active HTTP environment and reports the status code and round-trip latency:

```powershell
> ping
Pinging [MetalCommand:local] (http://localhost:5100/)...
  Status : 200 OK
  Latency: 12 ms
```

```csharp
ConsoleApplication.CreateBuilder()
    .AddHttpConnections(cfg => { /* ... */ })
    .AddPingCommand()
    // ...
```

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