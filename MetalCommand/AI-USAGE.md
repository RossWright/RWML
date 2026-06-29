# MetalCommand AI Usage Guide

Use this file when generating code that consumes RossWright.MetalCommand packages.

## Packages

| Package | Use When |
|---|---|
| `RossWright.MetalCommand.Abstractions` | You are defining commands, command arguments, middleware, or shared command contracts without the full runtime host. |
| `RossWright.MetalCommand` | You are building an interactive .NET console application with command discovery, DI, configuration, middleware, progress output, and a REPL loop. |
| `RossWright.MetalCommand.Data` | You need EF Core environment management, scoped `DbContext` creation, CSV seed data loading, or built-in database commands. |
| `RossWright.MetalCommand.Data.SqlServer` | You need SQL Server provider registration helpers for MetalCommand database environments. |
| `RossWright.MetalCommand.Data.MySql` | You need MySQL or MariaDB provider registration helpers for MetalCommand database environments. |
| `RossWright.MetalCommand.Http` | You need environment-aware HTTP client routing, named connection groups, or the built-in `ping` command. |

## Namespaces

Core command APIs are in:

```csharp
using RossWright.MetalCommand;
```

Database APIs are in:

```csharp
using RossWright.MetalCommand.Data;
```

HTTP APIs are in:

```csharp
using RossWright.MetalCommand.Http;
```

## Common APIs

| Task | API |
|---|---|
| Create a console app builder | `ConsoleApplication.CreateBuilder()` |
| Build and run the app | `builder.Build().RunAsync(args)` |
| Define a command | `ICommand` and `[Command]` |
| Define command arguments | `[Arg]` |
| Return command status | `CommandResult.Ok()`, `Fail()`, `Exit()`, `FailAndExit()` |
| Register commands | `builder.AddCommands(commands => commands.Add<TCommand>())` |
| Register services | `builder.AddServices(services => ...)` |
| Add command middleware | `builder.AddMiddleware<TMiddleware>()` |
| Customize prompt text | `builder.SetPromptFactory(...)` |
| Read/write console output | `IConsole` |
| Dispatch another command | `ICommandExecutor.Execute(...)` |
| Register EF Core environments | `builder.AddDatabaseContextFactory<TDbContext>(...)` |
| Register SQL Server environments | `AddSqlServer(...)`, `AddSqlServerDefault(...)`, `AddSqlServerProtected(...)` |
| Register MySQL environments | `AddMySql(...)`, `AddMySqlDefault(...)`, `AddMySqlProtected(...)` |
| Mark an environment argument | `[EnvironmentArg]` |
| Load CSV seed data | `CsvFile<T>` |
| Register HTTP environments | `builder.AddHttpConnections(...)` |
| Resolve environment-aware HTTP clients | `IHttpClientFactory.CreateClient(...)` or `IHttpConnectionResolver` |
| Add the built-in ping command | `builder.AddPingCommand()` |

## Typical command

```csharp
[Command("Greet", "greet", HelpBrief = "Writes a greeting.")]
public sealed class GreetCommand : ICommand
{
	[Arg(IsRequired = true, HelpDetail = "Name to greet.")]
	public string Name { get; set; } = null!;

	public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
	{
		console.WriteLine($"Hello, {Name}.");
		return Task.FromResult(CommandResult.Ok());
	}
}
```

## Typical app setup

```csharp
var builder = ConsoleApplication.CreateBuilder();

builder
	.AddServices(services =>
	{
		services.AddSingleton<MyService>();
	})
	.AddCommands(commands =>
	{
		commands.Add<GreetCommand>();
	});

await builder.Build().RunAsync(args);
```

## Typical database setup

```csharp
builder.AddDatabaseContextFactory<MyDbContext>(databases =>
{
	databases.AddSqlServerDefaultByConfigurationName("dev", "DevDatabase");
	databases.AddSqlServerProtectedByConfigurationName("prod", "ProdDatabase");
});
```

## Typical HTTP setup

```csharp
builder
	.AddHttpConnections(connections =>
	{
		connections.AddDefault("local", "https://localhost:5001");
		connections.AddProtected("prod", "https://api.example.com");
	})
	.AddPingCommand();
```

## Important notes

- MetalCommand namespaces are package-specific. Use `RossWright.MetalCommand` for core APIs, `RossWright.MetalCommand.Data` for database APIs, and `RossWright.MetalCommand.Http` for HTTP APIs.
- Commands must be registered explicitly or discovered through the command collection builder before `Build()`.
- `[Arg]` supports strings, numbers, booleans, `Guid`, `DateTime`, and enum values.
- Use `[EnvironmentArg]` on commands that need a selected database or HTTP environment.
- Protected environments trigger `EnvironmentPolicy` checks before destructive commands run.
- For 2026.2, `RossWright.MetalCommand.Data.MySql` targets .NET 8 and .NET 9 only because the Pomelo EF Core provider has not caught up to EF Core 10.
