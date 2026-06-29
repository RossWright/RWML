# Build A MetalCommand Console App

Use this recipe when building an interactive .NET console tool with commands, DI, configuration, middleware, progress output, and a read-execute loop.

## Install

```bash
dotnet add package RossWright.MetalCommand
```

## Namespace

```csharp
using RossWright.MetalCommand;
```

## Define A Command

```csharp
[Command("Greet", "greet", HelpBrief = "Writes a greeting.")]
public sealed class GreetCommand : ICommand
{
	[Arg(IsRequired = true)]
	public string Name { get; set; } = null!;

	public Task<CommandResult> ExecuteAsync(
		IConsole console,
		CancellationToken cancellationToken)
	{
		console.WriteLine($"Hello, {Name}.");
		return Task.FromResult(CommandResult.Ok());
	}
}
```

## Setup

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

## Reach For This When

- You are building a CLI-like interactive tool, admin console, data utility, or test harness.
- You want commands to use DI.
- You want arguments, help text, cancellation, colors, progress, and context storage handled for you.

## Notes For Agents

- Commands are classes implementing `ICommand`.
- Use `[Command]` on the class and `[Arg]` on public settable properties.
- Register command types before calling `Build()`.
- `CommandResult.Exit()` exits the interactive loop after the command completes.
