# Add MetalCommand Database Tooling

Use this recipe when a MetalCommand app needs EF Core database environments and built-in commands such as migrate, load, reload, clear, or obliterate.

## Install

```bash
dotnet add package RossWright.MetalCommand.Data
dotnet add package RossWright.MetalCommand.Data.SqlServer
```

Use `RossWright.MetalCommand.Data.MySql` for MySQL or MariaDB projects. In 2026.2, the MySQL package targets .NET 8 and .NET 9 only.

## Namespaces

```csharp
using RossWright.MetalCommand;
using RossWright.MetalCommand.Data;
```

## Setup

```csharp
builder.AddDatabaseContextFactory<AppDbContext>(databases =>
{
	databases.AddSqlServerDefaultByConfigurationName("dev", "DevDatabase");
	databases.AddSqlServerProtectedByConfigurationName("prod", "ProdDatabase");
});

builder
	.AddMigrateCommand<AppDbContext>()
	.AddLoadDataCommand<AppDbContext>()
	.AddReloadDatabaseCommand<AppDbContext>()
	.AddClearDataCommand<AppDbContext>();
```

## Use An Environment Argument

```csharp
[Command("Import Customers", "import-customers")]
public sealed class ImportCustomersCommand(
	IDatabaseContextFactory<AppDbContext> databaseFactory) : ICommand
{
	[EnvironmentArg]
	public string Environment { get; set; } = null!;

	public async Task<CommandResult> ExecuteAsync(
		IConsole console,
		CancellationToken cancellationToken)
	{
		await using var db = databaseFactory.GetContext(Environment);
		return CommandResult.Ok();
	}
}
```

## Reach For This When

- A console app needs safe dev/test/prod database selection.
- You need repeatable EF Core maintenance commands.
- You want protected environments to require policy checks before destructive work.

## Notes For Agents

- Register database environments before adding commands that depend on them.
- Use protected environments for production-like databases.
- Do not invent SQLite-only behavior for SQL Server-specific operations.
