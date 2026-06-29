# Ross Wright's Metal Command
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Defining Commands](#defining-commands)
  - [[Command]](#command)
  - [[Arg]](#arg)
  - [[EnvironmentArg]](#environmentarg)
  - [CommandResult](#commandresult)
- [Registering Commands](#registering-commands)
- [Middleware](#middleware)
- [Running the Application](#running-the-application)
- [The IConsole API](#the-iconsole-api)
  - [Announce helpers](#announce-helpers)
  - [Interactive helpers](#interactive-helpers)
  - [Progress Indicators](#progress-indicators)
- [ICommandExecutor](#icommandexecutor)
- [Database Tooling](#database-tooling)
  - [IDatabaseContextFactory](#idatabasecontextfactory)
  - [Built-in Data Commands](#built-in-data-commands)
  - [CsvFile](#csvfile)

- [HTTP Connections](#http-connections)
  - [Setup](#setup)
  - [Writing a command that calls the API](#writing-a-command-that-calls-the-api)
  - [MetalNexus integration](#metalnexus-integration)
  - [Authentication](#authentication)
  - [Ping command](#ping-command)
  - [IHttpConnectionsBuilder API](#ihttpconnectionsbuilder-api)
  - [IHttpConnectionResolver API](#ihttpconnectionresolver-api)
- [Esoterica](#esoterica)
  - [DupCon — Duplicate Console Window](#dupcon--duplicate-console-window)
  - [SetServiceProviderFactory — MetalInjection Integration](#setserviceproviderfactory--metalinjection-integration)
  - [TryParseEnvironment](#tryparseenvironment)
  - [EnvironmentArgMiddleware Internals](#environmentargmiddleware-internals)
  - [Multiple Connection Groups](#multiple-connection-groups)
  - [Bootstrap Logging](#bootstrap-logging)
- [See Also](#see-also)
- [License](#license)

---

## Overview

MetalCommand gives .NET developers a host-builder pattern for interactive console tools — the same comfortable `CreateBuilder` / `Build` / `RunAsync` idiom used in ASP.NET Core, with a command-dispatch loop, argument binding, dependency injection, and middleware built in. Commands are plain classes; a pair of attributes is all the wiring required.

| Feature | Description |
|---|---|
| Command dispatch | Attribute-driven `ICommand` with typed `[Arg]` property binding; case-insensitive invocation aliases |
| Argument resolution | Positional and named (`--flag value`) binding; required/optional; context-key fallbacks; enum and bool flag support |
| Progress indicators | `Spinner`, `ProgressBar`, and `Percentage` indicators via the `IConsole` extension API |
| Middleware | `ICommandMiddleware` pipeline for cross-cutting concerns (logging, timing, error handling) |
| DI & configuration | Standard `IServiceCollection` + `IConfiguration` loaded from `appsettings.json`; MetalInjection-compatible via `SetServiceProviderFactory` |
| Database tooling | `IDatabaseContextFactory` for scoped EF Core contexts; built-in migrate, load, reload, obliterate, and clear commands |
| HTTP connections | Named `HttpClient` connections per environment; Bearer token auth; MetalNexus integration |
| MetalNexus integration | `Mediator.Send` routes over HTTP when MetalNexus is configured — the same request types work locally and remotely |

A typical session looks like this:

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

Add the core package to your console project:

```powershell
dotnet add package RossWright.MetalCommand
```

For database tooling commands, also add the data package and your database provider:

```powershell
dotnet add package RossWright.MetalCommand.Data
```

```powershell
dotnet add package RossWright.MetalCommand.Data.SqlServer
```

```powershell
dotnet add package RossWright.MetalCommand.Data.MySql
```

---

## Quick Start

After installing `RossWright.MetalCommand`, you'll have a running interactive loop in a few lines. By the end of this section you'll have a console app that accepts a `greet` command and responds to `help` and `exit` automatically.

Define your command class — `[Command]` names it and `[Arg]` binds the argument:

```csharp
[Command("Greet", HelpBrief = "Print a greeting")]
public class GreetCommand : ICommand
{
    [Arg(IsRequired = true, HelpDetail = "The name to greet")]
    public string Name { get; set; } = "";

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        console.WriteLine($"Hello, {Name}!");
        return Task.FromResult(CommandResult.Ok());
    }
}
```

Wire it up in `Program.cs`:

```csharp
var app = ConsoleApplication.CreateBuilder()
    .AddCommands(cmds => cmds.Add<GreetCommand>())
    .Build();

await app.RunAsync(args);
```

Run the app and type `greet Alice` at the prompt. The framework handles `help`, `exit`, argument validation, Ctrl-C abort, and completion timing automatically.

---

## Defining Commands

A command is a class that implements `ICommand` and is decorated with `[Command]`. Arguments are public settable properties decorated with `[Arg]`. MetalCommand reads the attributes at startup, binds resolved values to the properties before each invocation, and calls `ExecuteAsync`.

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

### `[Command]`

The `[Command]` attribute declares the display name and, optionally, the invocation tokens — the strings the user types at the prompt to run the command. Matching is always case-insensitive.

```csharp
[Command("Import", "import", "imp", HelpBrief = "Import records from a CSV file",
    Category = "Data")]
public class ImportCommand : ICommand { ... }
```

| Property | Description |
|---|---|
| `Name` | Display name shown in help listings and run/completion messages |
| `Invocations` | One or more tokens accepted at the prompt (case-insensitive). Defaults to `name` lowercased when omitted |
| `HelpBrief` | Short description shown in the command list |
| `HelpDetail` | Longer description shown when `help <command>` is called |
| `Category` | Groups commands in help output. The built-in "Console" category always appears first; commands with no category appear under "Uncategorized" at the end |

### `[Arg]`

Each `[Arg]`-decorated property represents one argument. Arguments are bound positionally by default — in declaration order — and optionally also by name with `--PropertyName value` syntax.

```csharp
[Arg(IsRequired = true, HelpDetail = "Target environment (dev, staging, prod)")]
public string Env { get; set; } = "";

[Arg(DefaultValue = "100", AllowNamed = true, HelpDetail = "Maximum rows to process")]
public int Limit { get; set; } = 100;

[Arg(AllowNamed = true, HelpDetail = "Suppress progress output")]
public bool Quiet { get; set; }
```

The `Quiet` property above can be set positionally (`greet false`), named with a value (`--quiet false`), or as a bare flag (`--quiet`) which the binder treats as `true`.

**Supported property types:** `string`, `int`, `double`, `bool`, `Guid`, `DateTime`, and any `enum`.

| Property | Description |
|---|---|
| `Name` | Display name used in help and error messages. Defaults to the property name |
| `Order` | Zero-based positional index. Defaults to declaration order when `-1` |
| `IsRequired` | Aborts execution with an error when no value is resolved and no fallback applies |
| `DefaultValue` | Literal string fallback when no value is supplied; converted to the property type by the binder |
| `ContextKey` | Key into the session context dictionary used as a fallback. When a context value is used, it is echoed to the console before execution |
| `ValidValues` | Restricts accepted values to an explicit set (case-insensitive). Applies to `string` and `enum` properties; values outside the set abort execution |
| `HelpDetail` | Description shown in `help <command>` output |
| `AllowNamed` | Accepts `--PropertyName value` in addition to positional order. Boolean properties also accept a bare `--PropertyName` flag as `true` |

### `[EnvironmentArg]`

`[EnvironmentArg]` marks the property that selects the active connection environment (dev, staging, prod). It works alongside `EnvironmentArgMiddleware` — the middleware validates the resolved environment against the declared policy before `ExecuteAsync` is called. Commands that connect to a database or remote API typically declare one.

```csharp
[EnvironmentArg(EnvironmentPolicy.Dangerous, HelpDetail = "Target environment")]
public string Env { get; set; } = "";
```

For multi-connection commands that reference more than one environment source, set `EnvironmentSourceType` on each property to identify which `IEnvironmentSource` to resolve from DI.

For the full setup — defining environments, registering middleware, and the `EnvironmentPolicy` options — see [HTTP Connections](#http-connections).

### CommandResult

`ExecuteAsync` returns a `CommandResult` to report the outcome and control whether the interactive loop continues. `Success` is informational; only `ExitApplication` exits the loop.

```csharp
// Succeeded — loop continues
return CommandResult.Ok();

// Failed with a message — loop continues
return CommandResult.Fail("No records matched the filter.");

// Succeeded and exits — same as the user typing "exit"
return CommandResult.Exit();

// Failed and exits — message is written as an error before the goodbye line
return CommandResult.FailAndExit("Fatal configuration error.");
```

An implicit conversion from `bool` is provided: `true` → `Ok()`, `false` → `Fail()`.

| Factory | Description |
|---|---|
| `CommandResult.Ok()` | Succeeded; loop continues |
| `CommandResult.Fail(message?)` | Failed with an optional message; loop continues |
| `CommandResult.Exit()` | Succeeded; loop exits cleanly |
| `CommandResult.FailAndExit(message?)` | Failed; loop exits after writing the message as an error |

## Registering Commands

Commands are registered on the builder via `AddCommands`. You can list types explicitly or let MetalCommand scan an assembly and register every `ICommand` it finds.

Use explicit registration when you want precise control over which commands are available — useful for conditional registration based on configuration, or in larger solutions where you want to include commands from multiple projects selectively:

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
    .Build();

await app.RunAsync(args);
```

Use assembly scanning when all commands in a project should be registered automatically. This is the common case for a single-project tool:

```csharp
var app = ConsoleApplication.CreateBuilder()
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .Build();

await app.RunAsync(args);
```

To scan a project other than the entry assembly — for example, a shared commands library — use `ScanAssembly(typeof(SomeType))` instead.

### `IConsoleApplicationBuilder` members

| Member | Description |
|---|---|
| `Commands` | `ICommandCollection` — register or scan for `ICommand` implementations |
| `Services` | `IServiceCollection` — register DI services consumed by commands |
| `Logging` | `ILoggingBuilder` — configure the logging pipeline; providers registered here are available via `ILoggerFactory` and `ILogger<T>` in DI |
| `Configuration` | `IConfiguration` — pre-built from `appsettings.json` and `appsettings.dev.json` |
| `Console` | `IConsole` — the shared console instance |
| `AddServices(Action<IServiceCollection>)` | Fluent helper to register DI services |
| `AddCommands(Action<ICommandCollection>)` | Fluent helper to register commands |
| `AddCommands(Action<ICommandCollection, IConfiguration>)` | Fluent helper with access to configuration |
| `SetColors(introOutro?, help?, warning?, error?, errorBg?)` | Customise per-role console colors |
| `SetTabWidth(int)` | Set the number of spaces per indent level (default: 5) |
| `PromptFactory` | Assign a `Func<IDictionary<string, string>, string>` to build a dynamic prompt from the current session context |
| `AddMiddleware<TMiddleware>()` | Register a middleware type to participate in the command pipeline |
| `SetServiceProviderFactory(IServiceProviderFactory)` | Plug in an alternate `IServiceProvider` implementation (e.g. MetalInjection) |

---

## Middleware

Middleware lets you inject cross-cutting logic — logging, timing, error handling, audit trails — around every command execution without touching the command itself. Each middleware wraps the next in a pipeline; calling `next(context)` passes control inward. The innermost step calls `ExecuteAsync` and writes `context.Result`.

Register middleware on the builder before calling `Build()`. Middleware is executed in registration order — the first registered is the outermost wrapper:

```csharp
var app = ConsoleApplication.CreateBuilder()
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .AddMiddleware<LoggingMiddleware>()
    .AddMiddleware<TimingMiddleware>()
    .Build();
```

Implement `ICommandMiddleware` and call `next(context)` to continue the pipeline:

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

Middleware is resolved from DI per execution, so constructor injection of scoped or transient services works normally.

### `CommandContext` properties

| Property | Description |
|---|---|
| `Command` | The `ICommand` instance being executed |
| `Console` | The `IConsole` for the current session |
| `SessionContext` | The shared `IDictionary<string, string>` context |
| `CancellationToken` | The cancellation token for the current command |
| `BoundArgs` | `IReadOnlyDictionary<string, object?>` of bound argument values, populated before `ExecuteAsync` is called |
| `Result` | The `CommandResult` set by `ExecuteAsync`; readable after `next()` returns |

`EnvironmentArgMiddleware` is the built-in middleware that validates `[EnvironmentArg]` properties against their declared policy before execution. Register it with `.AddMiddleware<EnvironmentArgMiddleware>()` on any app that uses environment-tagged commands. For details on environment policies see [HTTP Connections](#http-connections).

## Running the Application

`Build()` returns a `ConsoleApplication`. Call `RunAsync` to start the interactive read-evaluate loop:

```csharp
await app.RunAsync(args);
```

If command-line `args` are provided, MetalCommand joins them into a single invocation string and executes that command first before dropping into the interactive loop. This lets you launch the app with a pre-queued command from a shell script or CI step.

The loop runs until the user types `exit` (or an alias), a command returns `CommandResult.Exit()`, or the process receives a termination signal. `Ctrl-C` while a command is running cancels it via `CancellationToken` — the loop continues. A second `Ctrl-C` with no command running terminates the process.

Built-in loop commands are always registered and cannot be removed:

| Input | Aliases | Description |
|---|---|---|
| `help [command]` | `man`, `h` | List all commands, or show detailed help for one |
| `exit` | `bye`, `quit` | End the session cleanly |

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

Commands use `IConsole` for all output and user input instead of `System.Console` directly. This matters for two reasons: `IConsole` tracks indentation and cursor position so the framework can produce consistent formatting, and it's injected — which means commands that write through it are fully testable without a real terminal. `IConsole` is passed as the first parameter to `ExecuteAsync` and is also available via constructor injection.

```csharp
public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
{
    console.WriteLine("Starting import...");
    using (console.Indent())
    {
        console.WriteLine($"Reading from: {filePath}");
    }
    return CommandResult.Ok();
}
```

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

### Announce helpers

The `Announce` family writes a `"task description... "` header, runs a callback with indented output, then writes `"Done!"` (or a custom conclusion) on the same line. It's the idiomatic way to communicate that a step is happening without flooding the console with interleaved text.

```csharp
var count = await console.AnnounceAsync("Importing records",
    async () =>
    {
        var rows = await importer.RunAsync(cancellationToken);
        console.WriteLine($"Processed {rows} rows.");
        return rows;
    },
    n => $"{n} imported.");
```

| Method | Description |
|---|---|
| `Announce(message, action, conclusion?)` | Write `"message... "`, run `action`, then write `"Done!"` (or custom conclusion) on the same line |
| `AnnounceAsync(message, action, conclusion?)` | Async version of `Announce` |
| `Announce<TResult>(message, func, conclusion?)` | Like `Announce` but captures and returns the result of `func` |
| `AnnounceAsync<TResult>(message, func, conclusion?)` | Async version with return value |
| `WriteLineIndented(text)` | Split multi-line text and write each line at the current indent level |
| `DumpJson(json)` | Pretty-print a JSON string at the current indent |
| `DumpJson(obj)` | Serialize an object to JSON and pretty-print it |

### Interactive helpers

When a command needs a decision or value from the user at runtime — rather than as a pre-bound argument — the interactive helpers handle prompting, re-prompting on invalid input, and returning a typed result.

```csharp
if (!console.Confirm("Are you sure you want to obliterate the database?"))
    return CommandResult.Fail("Aborted.");

var env = console.Choose("Select environment:", new[] { "dev", "staging", "prod" });
var name = console.Prompt("Enter a name", defaultValue: "World");
```

| Method | Description |
|---|---|
| `Confirm(prompt, defaultYes?)` | Yes/no prompt; accepts `y`/`yes`/`n`/`no` (case-insensitive); re-prompts on invalid input. When `defaultYes` is `true`, pressing Enter counts as yes |
| `Prompt(prompt, defaultValue?)` | Writes a prompt and returns the user's input, or `defaultValue` when Enter is pressed with no input |
| `Choose<T>(prompt, options[])` | Presents a numbered list of options and returns the chosen value; re-prompts on invalid input. `T` can be any type — `string`, `enum`, etc. |

### Progress Indicators

When a command runs work that takes more than a second or two, `ShowProgress` keeps the user informed without requiring the command to manage cursor positions or threading. It renders one or more `IProgressIndicator` instances inline on the current console line, updating them as the body callback calls `report(0.0–1.0)`. The body can be synchronous or async and may return a value.

Use `Spinner` when you don't know how far along the work is. Use `ProgressBar` when you can compute a fraction — it overlays the percentage inside a Unicode block-fill bar. Use `Percentage` when you want numeric-only output in a narrow space.

```csharp
var count = await console.ShowProgress(async report =>
{
    for (int i = 0; i < items.Count; i++)
    {
        await ProcessAsync(items[i], cancellationToken);
        report((double)(i + 1) / items.Count);
    }
    return items.Count;
});
```

| Type | Width | Description |
|---|---|---|
| `Spinner` | 1 | Rotating `\`, `\|`, `/`, `—` character — use when progress fraction is unknown |
| `ProgressBar` | 52 | Unicode block-fill bar with percentage overlaid — use when fraction is known |
| `Percentage` | 4 | Plain `" 42%"` numeric display — use when space is constrained |

By default `ShowProgress` uses `[Spinner, ProgressBar]`. Pass a custom `IProgressIndicator[]` to use different indicators or to combine them in a different order. Implement `IProgressIndicator` to supply a fully custom renderer.

---

## ICommandExecutor

`ICommandExecutor` lets you dispatch commands programmatically — either from within another command (e.g. a `reload` that sequences `migrate` then `load`), or from a test that needs to exercise a command without a real interactive session. It's registered as a singleton and available via constructor injection.

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

Arguments are passed as positional strings in the same order they would appear on the command line. The full argument-binding pipeline runs exactly as it does during interactive execution, including `[Arg]` required checks and type conversion.

| Member | Description |
|---|---|
| `Execute(invocation, args)` | Dispatch a command by its invocation token; `args` are bound positionally |
| `Execute<TCommand>(args)` | Dispatch a command by type; `args` are bound positionally |
| `Context` | The shared session context dictionary, readable and writable |

---

## Database Tooling

`RossWright.MetalCommand.Data` solves a specific problem: getting a correctly scoped EF Core `DbContext` inside a long-running console loop, plus a suite of ready-made database management commands so you don't have to write `migrate`, `load`, `reload`, `obliterate`, and `clear` yourself. Add `RossWright.MetalCommand.Data.SqlServer` or `RossWright.MetalCommand.Data.MySql` for the matching provider registration helpers.

### IDatabaseContextFactory

`IDatabaseContextFactory<TDbContext>` lets you define named environments — `dev`, `staging`, `prod` — each with its own `DbContextOptions`. The factory is registered as scoped and injected into both the built-in data commands and any `ICommand` you write.

Call `AddDatabaseContextFactory<TDbContext>` on the builder, then declare your environments. `AddDatabaseContextFactory` also automatically registers `EnvironmentArgMiddleware`, so environment argument validation is wired up with no extra steps.

```csharp
ConsoleApplication.CreateBuilder()
    .AddDatabaseContextFactory<AppDbContext>(db =>
    {
        db.AddDefault("dev",  opts => opts.UseSqlServer(config["Dev:ConnectionString"]));
        db.Add("staging",     opts => opts.UseSqlServer(config["Staging:ConnectionString"]));
        db.AddProtected("prod", opts => opts.UseSqlServer(config["Prod:ConnectionString"]));
    })
    .AddMigrateCommand<AppDbContext>()
    .AddLoadDataCommand<AppDbContext>(opts => opts.LoadData = SeedDataAsync)
    .AddObliterateCommand<AppDbContext>()
    .AddClearDataCommand<AppDbContext>(opts => opts.TableNames = ["Orders", "Users"])
    .AddReloadDatabaseCommand<AppDbContext>()
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .Build();
```

To use the factory in your own commands, inject `IDatabaseContextFactory<AppDbContext>` and call `GetContext(env)`:

```csharp
[Command("Report", HelpBrief = "Generate a summary report")]
public class ReportCommand(IDatabaseContextFactory<AppDbContext> dbFactory) : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign, HelpDetail = "Target environment")]
    public string? Env { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        using var db = dbFactory.GetContext(Env);
        var count = await db.Orders.CountAsync(cancellationToken);
        console.WriteLine($"Total orders: {count}");
        return CommandResult.Ok();
    }
}
```

| `IDatabaseContextFactoryBuilder` method | Description |
|---|---|
| `Add(env, opts, isDefault?, isProtected?)` | Register a named environment |
| `AddDefault(env, opts)` | Register and mark as the default (used when no env argument is supplied) |
| `AddProtected(env, opts)` | Register a protected environment — commands that enforce `EnvironmentPolicy.Forbidden` will not accept it |
| `AddDefaultProtected(env, opts)` | Register as both default and protected |

### Built-in Data Commands

Each command is registered independently so you include only what you need. All accept an optional configure delegate to customise invocation tokens or help text.

| Extension method | Invocation | Description |
|---|---|---|
| `AddMigrateCommand<TDbCtx>(configure?)` | `migrate [env]` | Runs `Database.MigrateAsync()` against the selected environment. Accepts optional `PreMigration` and `PostMigration` callbacks |
| `AddLoadDataCommand<TDbCtx>(configure)` | `loaddata [env]` | Calls your `LoadData` delegate with a `LoadDataCommandContext` containing the `DbContext` and CSV-loading helpers. `LoadData` is required |
| `AddClearDataCommand<TDbCtx>(configure)` | `cleardata [env]` | Deletes all data without dropping the schema. Supply either a `ClearData` callback or a `TableNames` array — the command builds the DELETE statements from the array if no callback is provided |
| `AddObliterateCommand<TDbCtx>(configure?)` | `obliterate env` | Drops all FK constraints, tables, and stored procedures. The environment argument is required (not optional) and protected environments are blocked |
| `AddReloadDatabaseCommand<TDbCtx>(configure?)` | `reload [env]` | Sequences `migrate` → `cleardata` → `loaddata` via `ICommandExecutor`. Requires all three of those commands to also be registered |

> **`obliterate` is destructive.** It requires the environment name to be typed in full and rejects protected environments by policy.

### CsvFile

`CsvFile<T>` reads a CSV file into a typed collection using [CsvHelper](https://joshclose.github.io/CsvHelper/). It's the standard way to supply seed data to a `LoadData` callback, but it can also be used standalone in any command.

Column headers in the CSV are matched to `T` properties by name (case-insensitive). Extra columns, blank lines, and comment lines are silently ignored. Missing fields fall back to the property's default value.

```csharp
.AddLoadDataCommand<AppDbContext>(opts => opts.LoadData = async ctx =>
{
    var users = ctx.LoadFromCsv<UserSeedRow>("data/users.csv");
    await ctx.DbContext.Users.AddRangeAsync(users.Select(r => r.ToEntity()));

    // or use CsvFile<T> directly for files outside the load pipeline
    var extras = new CsvFile<TagRow>("data/tags.csv");
    await ctx.DbContext.Tags.AddRangeAsync(extras.Rows.Select(r => r.ToEntity()));
})
```

`LoadDataCommandContext<TDbCtx>.LoadFromCsv<TEntity>` is the preferred API inside a load callback — it respects the `LoadFilepath` prefix configured on the command options and adds the loaded entities to the context automatically. `CsvFile<T>` is the lower-level helper for cases where you need the rows but want to control what happens to them.

When no file path is supplied, `CsvFile<T>` resolves `data\{TypeName}.csv` relative to `AppContext.BaseDirectory`.

---

## HTTP Connections

`RossWright.MetalCommand.Http` adds environment-aware HTTP client management to MetalCommand. Register one or more named environments — local, test, prod — against an HTTP service and MetalCommand routes `IHttpClientFactory.CreateClient` calls to whichever environment the user selected at runtime. Your commands stay clean and environment-agnostic; the routing is invisible.

Install the package:

```powershell
dotnet add package RossWright.MetalCommand.Http
```

### Setup

Register connection environments with `AddHttpConnections` in `Program.cs`:

```csharp
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

`AddDefault` marks the environment used when the user doesn't supply one. `AddProtected` marks an environment as production-grade — commands that declare `EnvironmentPolicy.Dangerous` will prompt for confirmation before running against it, and those with `EnvironmentPolicy.Forbidden` will refuse entirely.

### Writing a command that calls the API

Inject `IHttpClientFactory` and declare an `[EnvironmentArg]` property. `CreateClient()` automatically routes to the environment the user selected — no client name, no lookup:

```csharp
[Command("Fetch", HelpBrief = "Fetch data from the API")]
public class FetchCommand(IHttpClientFactory httpFactory) : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign, HelpDetail = "Target environment")]
    public string? Environment { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        var client = httpFactory.CreateClient();
        var response = await client.GetAsync("/health", cancellationToken);
        console.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase}");
        return CommandResult.Ok();
    }
}
```

The `EnvironmentAwareHttpClientFactory` decorator intercepts `CreateClient()` and resolves the correct named client for the active environment.

### MetalNexus integration

When commands dispatch requests via `IMediator.Send` and MetalNexus is configured, the same decorator is already in place. `IHttpClientFactory.CreateClient(name)` with a bare connection name (e.g. `"payments"`) is automatically mapped to the environment-qualified key `"MetalCommand:payments:prod"`. No changes are needed to existing MetalNexus handlers or request types.

```csharp
// Program.cs — wire MetalNexus on top of HTTP connections
ConsoleApplication.CreateBuilder()
    .AddHttpConnections(cfg =>
    {
        cfg.AddDefault("local", "http://localhost:5100");
        cfg.AddProtected("prod","https://api.example.com");
    })
    .AddMetalNexusClient(opts =>
    {
        opts.ScanAssembly(typeof(MyRequest));  // shared request types
        opts.ScanThisAssembly();
    })
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .Build();
```

```csharp
// MyCommand.cs — Mediator.Send routes over HTTP transparently
[Command("Sync", HelpBrief = "Sync records via the API")]
public class SyncCommand(IMediator mediator) : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Dangerous, HelpDetail = "Target environment")]
    public string? Environment { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SyncRecords.Request(), cancellationToken);
        console.WriteLine($"Synced {result.Count} records.");
        return CommandResult.Ok();
    }
}
```

### Authentication

Pass an `authHandlerFactory` delegate when registering the environment. The factory receives the DI `IServiceProvider` and returns any `DelegatingHandler`.

If you use MetalGuardian for auth, it handles token acquisition, refresh, and caching transparently:

```csharp
cfg.AddProtected("prod", "https://api.example.com",
    authHandlerFactory: sp => new AuthenticationDelegatingHandler(
        sp.GetRequiredService<IMetalGuardianAuthenticationClient>(),
        connectionName: "my-api"));
```

For a simpler API-key scenario:

```csharp
cfg.AddProtected("prod", "https://api.example.com",
    authHandlerFactory: sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new ApiKeyHandler(config["ApiKey"]!);
    });
```

### Ping command

`AddPingCommand()` registers a built-in `ping` command. It sends `GET /` to the active HTTP environment and reports the status code and round-trip latency — a one-liner connectivity check:

```csharp
ConsoleApplication.CreateBuilder()
    .AddHttpConnections(cfg => { /* ... */ })
    .AddPingCommand()
    .Build();
```

```
> ping
Pinging [MetalCommand:local] (http://localhost:5100/)...
  Status : 200 OK
  Latency: 12 ms
```

The command accepts an optional environment argument and a timeout (seconds, default 10). It's listed under the `HTTP` category in help output.

### `IHttpConnectionsBuilder` API

| Method | Description |
|---|---|
| `AddDefault(env, baseAddress, configure?, authHandlerFactory?)` | Register an environment and mark it as the default when no explicit value is supplied |
| `Add(env, baseAddress, configure?, authHandlerFactory?)` | Register a standard non-default, non-protected environment |
| `AddProtected(env, baseAddress, configure?, authHandlerFactory?)` | Register a protected environment (e.g. production) |

All three accept an optional `configure` delegate (`Action<HttpClient>`) for extra client setup and an optional `authHandlerFactory` for DI-resolved auth handlers.

### `IHttpConnectionResolver` API

`IHttpConnectionResolver` is available for cases where a single command needs to address more than one environment at once — for example, comparing responses across environments. For the common case of one `[EnvironmentArg]` per command, you don't need it.

| Member | Description |
|---|---|
| `GetClientName(environment?, baseConnectionName?)` | Returns the `IHttpClientFactory` key for the given environment and optional connection group |
| `GetClient(environment?, baseConnectionName?)` | Convenience wrapper — resolves and returns the `HttpClient` directly |

For using multiple independent HTTP services (e.g. `"payments"` and `"notifications"`) as separate connection groups, see [Multiple Connection Groups](#multiple-connection-groups) in Esoterica.

---

## Esoterica

The above covers 90% of typical MetalCommand usage. Below are more specialized capabilities.

### DupCon — Duplicate Console Window

`dupcon` is a built-in command (registered automatically) that launches a second instance of the current executable in a new terminal window. It's useful when you want two sessions running side by side — for example, one monitoring and one executing.

```
> dupcon
```

Pass `--context` to forward the current session context to the new window, or `--cmd "greet Alice"` to pre-queue a command in the new session.

Configure `DupCon` behavior via the builder's `BuiltInCommands.DupCon` options:

```csharp
var app = ConsoleApplication.CreateBuilder()
    .ConfigureBuiltInCommands(opts =>
    {
        opts.DupCon.DefaultForwardContext = true;          // always forward context
        opts.DupCon.WindowsLaunchMode = WindowsTerminalLaunchMode.WindowsTerminal;
    })
    .Build();
```

| Option | Description |
|---|---|
| `DefaultForwardContext` | When `true`, context is forwarded without the user needing `--context` |
| `WindowsLaunchMode` | Controls which terminal emulator is used on Windows (`Auto`, `WindowsTerminal`, `Cmd`) |
| `TerminalLauncher` | Full override — supply any `IDupConTerminalLauncher` implementation |

### SetServiceProviderFactory — MetalInjection Integration

By default MetalCommand uses the standard `IServiceCollection.BuildServiceProvider()`. To use MetalInjection's container instead, call `SetServiceProviderFactory` before `Build()`:

```csharp
ConsoleApplication.CreateBuilder()
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .SetServiceProviderFactory(new MetalInjectionServiceProviderFactory())
    .Build();
```

This enables MetalInjection's property injection, covariant generic resolution, and deterministic disposal for all services resolved by commands.

### TryParseEnvironment

`TryParseEnvironment` is an `IConsole` extension method for code that needs manual environment validation. It validates the supplied environment name against an `IEnvironmentSource`, enforces the given `EnvironmentPolicy`, and returns the resolved name — or `null` if validation fails or the user declines the confirmation prompt.

```csharp
var env = console.TryParseEnvironment(dbFactory, rawEnvArg, EnvironmentPolicy.Dangerous);
if (env is null) return CommandResult.Fail();

using var db = dbFactory.GetContext(env);
```

This is the manual equivalent of what `EnvironmentArgMiddleware` does automatically for `ICommand` classes. If you're writing new commands, use `[EnvironmentArg]` instead.

### EnvironmentArgMiddleware Internals

`EnvironmentArgMiddleware` validates `[EnvironmentArg]` properties against their declared `EnvironmentPolicy` before `ExecuteAsync` is called. For `EnvironmentPolicy.Dangerous` environments, it prompts the user to type `yes` before proceeding.

Confirmation is scoped to a single top-level invocation via a `__runId` key in `SessionContext`. When an aggregate command (like `reload`) calls sub-commands via `ICommandExecutor`, all sub-commands share the same run ID and the user is prompted only once — not once per sub-command.

Register it before `Build()` on any app that uses `[EnvironmentArg]` on commands not covered by `AddDatabaseContextFactory` (which registers it automatically):

```csharp
ConsoleApplication.CreateBuilder()
    .AddMiddleware<EnvironmentArgMiddleware>()
    .Build();
```

### Multiple Connection Groups

When a MetalCommand app connects to more than one independent HTTP service, register each service as a named connection group by passing a `connectionName` to the overload of `AddHttpConnections`:

```csharp
ConsoleApplication.CreateBuilder()
    .AddHttpConnections("payments", cfg =>
    {
        cfg.AddDefault("local", "http://localhost:5200");
        cfg.AddProtected("prod","https://payments.example.com");
    })
    .AddHttpConnections("notifications", cfg =>
    {
        cfg.AddDefault("local", "http://localhost:5300");
        cfg.AddProtected("prod","https://notify.example.com");
    })
    .Build();
```

In a command that needs to fan out across groups, use `IHttpConnectionResolver.GetClient` with the connection group name:

```csharp
var payClient  = resolver.GetClient(environment, "payments");
var notifClient = resolver.GetClient(environment, "notifications");
```

When using MetalNexus's `SendVia` to target a specific connection group per-request, call `resolver.GetClientName(environment, "payments")` to get the qualified key and pass it to `SendVia`.

For commands that only use one connection group, `IHttpClientFactory.CreateClient()` (with no arguments) routes to the unnamed default group automatically — no change required.

### Bootstrap Logging

During `CreateBuilder()` and before DI is fully constructed, MetalCommand writes startup messages through `IConsole` directly. If you need structured logging of these startup events (e.g. for diagnostics in CI), configure `ILoggingBuilder` on the builder:

```csharp
ConsoleApplication.CreateBuilder()
    .AddCommands(cmds => cmds.ScanThisAssembly())
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    })
    .Build();
```

Log providers registered here are available from the fully-built `IServiceProvider` as `ILoggerFactory` and `ILogger<T>`, injected into commands normally.

---

## See Also

| Library | Description |
|---|---|
| [MetalChain](https://www.nuget.org/packages/RossWright.MetalChain) | The mediator core — `IMediator`, `IRequest`, `IRequestHandler`; the foundation everything else builds on |
| [MetalNexus](../MetalNexus/README.md) | Bridges MetalChain across HTTP — `[ApiRequest]`, `AddMetalNexusServer`, `AddMetalNexusClient` |
| [MetalInjection](../MetalInjection/README.md) | Attribute-driven DI registration and property injection — `[Singleton]`, `[Scoped]`, `[Inject]` |
| [MetalGuardian](../MetalGuardian/README.md) | Authentication and authorization — JWT, MFA, device fingerprinting, `[Authenticated]` |

---

## License

All **Ross Wright Metal Libraries** including this one are licensed under **Apache License 2.0 with Commons Clause**.

You are free to use the libraries in any project (personal or commercial), modify them, and include them in products or services you sell. You may not sell the libraries themselves, nor repackage them with minimal changes and sell them as a standalone product — their primary value must not be the libraries themselves.

Full legal text: [LICENSE.md](./LICENSE.md)
