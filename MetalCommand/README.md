# Ross Wright's Metal Command
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Defining Commands](#defining-commands)
- [Registering Commands](#registering-commands)
- [Running the Application](#running-the-application)
- [The IConsole API](#the-iconsole-api)
  - [Progress Indicators](#progress-indicators)
- [ICommandExecutor](#icommandexecutor)
- [Database Tooling](#database-tooling)
  - [IDatabaseContextFactory](#idatabasecontextfactory)
  - [Built-in Data Commands](#built-in-data-commands)
  - [CsvFile](#csvfile)
- [License](#license)

---

## Overview

MetalCommand is a framework for building interactive .NET console applications. It provides a host-builder pattern (`ConsoleApplication.CreateBuilder`) that sets up configuration, dependency injection, and a command-dispatch loop — all without depending on MetalChain or MetalInjection (though it is compatible with both via `SetServiceProviderFactory`).

Commands are plain classes that implement `ICommand`. Each command declares its own name, invocation aliases, argument schema, and help text via a `CommandDescriptor`, then executes against a typed `IConsole` and a `string[]` of resolved arguments. The runtime handles argument validation, default values, context-key substitution, and error reporting automatically.

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
> migrate dev
Executing Run Migrations. Use Ctrl-C to abort (and Ctrl-C again to abort program).
  Running migrations for dev... Done!
Run Migrations completed in 2 seconds.
> exit
Goodbye!
```

---

## Installation

```powershell
dotnet add package RossWright.MetalCommand
```

```xml
<PackageReference Include="RossWright.MetalCommand" Version="*" />
```

For database tooling commands, also add the data package and your database provider:

```powershell
dotnet add package RossWright.MetalCommand.Data
dotnet add package RossWright.MetalCommand.Data.SqlServer   # or .MySql
```

---

## Defining Commands

Implement `ICommand`. The `Descriptor` property declares everything the runtime needs — name, invocation aliases, argument schema, and help text. `Execute` receives the resolved argument values in the same order as `Descriptor.Args`.

```csharp
public class GreetCommand : ICommand
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
| `SetServiceProviderFactory(IServiceProviderFactory)` | Plug in an alternate `IServiceProvider` implementation (e.g. MetalInjection) |

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
public class ReloadCommand(ICommandExecutor executor) : ICommand
{
    public CommandDescriptor Descriptor => new() { /* ... */ };

    public async Task Execute(IConsole console, string[] args, CancellationToken cancellationToken)
    {
        await executor.Execute<MigrateCommand>(args[0]);
        await executor.Execute<LoadDataCommand>(args[0]);
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