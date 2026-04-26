# RossWright.MetalCommand.Abstractions
Copyright (c) 2023-2026 Pross Co.

## Overview

Contracts and shared types for the MetalCommand console application framework. This package defines the interfaces and attributes consumed by commands, middleware, and host applications — without pulling in any runtime implementation. Reference it from libraries that define commands or middleware but should not depend on the full MetalCommand host.

## Installation

Add the `RossWright.MetalCommand.Abstractions` package to your project.

## Quick start

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

## Key concepts

### `ICommand`
The primary command contract. Decorate the class with `[Command]` and properties with `[Arg]`. The framework binds argument values to the properties before calling `ExecuteAsync`.

### `CommandResult`
Returned by `ExecuteAsync`. Use the factory members to signal the outcome:

| Factory | Description |
|---|---|
| `CommandResult.Ok()` | Succeeded; loop continues |
| `CommandResult.Fail(message?)` | Failed; loop continues |
| `CommandResult.Exit()` | Succeeded and requests a clean loop exit |
| `CommandResult.FailAndExit(message?)` | Failed and requests a loop exit |

### `[Command]` attribute
Declares a class as a command and supplies its display name, invocation tokens, and help text.

### `[Arg]` attribute
Declares a settable property as a command argument. Supports positional and named (`--PropertyName`) binding, required/optional, defaults, context-key fallback, and valid-value restriction.

### `[EnvironmentArg]` + `EnvironmentPolicy`
Marks a property as the environment selector. `EnvironmentArgMiddleware` (in the core package) enforces the declared policy (`Benign`, `Dangerous`, `Forbidden`) before the command runs.

### `ICommandMiddleware`
Participates in the `ICommand` execution pipeline. Implement `InvokeAsync` and call `next(context)` to continue. Middleware is registered via `IConsoleApplicationBuilder.AddMiddleware<T>()` and resolved from DI per invocation.

### `IConsole`
Testable abstraction over the terminal. Supports color, indentation, cursor control, and line input. Extension methods in `IConsoleExtensions` add `Announce`, `Confirm`, `Prompt`, `Choose`, `DumpJson`, and `ShowProgress`.

### `IEnvironmentSource`
Supply environment choices (name + protected flag) to `[EnvironmentArg]` properties. Implement this interface and register it in DI.

## API summary

| Type | Description |
|---|---|
| `ICommand` | Primary command contract |
| `ICommandCollection` | Registers commands on the builder |
| `ICommandExecutor` | Dispatches commands programmatically from within other commands |
| `ICommandMiddleware` | Pipeline middleware for `ICommand` executions |
| `ICommandOptionsRegistry` | Per-command option overrides at registration time |
| `IConsole` | Terminal abstraction |
| `IConsoleApplicationBuilder` | Application builder configuration surface |
| `IEnvironmentSource` | Supplies available environments to `[EnvironmentArg]` |
| `IHttpConnectionResolver` | Resolves `IHttpClientFactory` keys for environment-aware HTTP (from `MetalCommand.Http`) |
| `CommandAttribute` | Declares a class as a command |
| `ArgAttribute` | Declares a property as a command argument |
| `EnvironmentArgAttribute` | Declares a property as the environment selector |
| `CommandContext` | Carries execution context through the middleware pipeline |
| `CommandResult` | Signals command outcome and loop-exit intent |
| `EnvironmentPolicy` | Controls behavior for protected environments |
| `EnvironmentEntry` | Name + protection status of one environment |

## See also

- [MetalCommand (core)](../README.md) — runtime host, `ConsoleApplication`, progress indicators, built-in commands
- [MetalCommand.Data](../RossWright.MetalCommand.Data/README.md) — EF Core integration and built-in database management commands
- [MetalCommand.Http](../RossWright.MetalCommand.Http/README.md) — environment-aware HTTP connection management
