# RossWright.MetalCommand.Data
Copyright (c) 2023-2026 Pross Co.

## Overview

EF Core integration for MetalCommand. Provides an environment-aware `IDatabaseContextFactory<TDbContext>` and a set of ready-made database management commands (`migrate`, `load`, `reload`, `obliterate`, `clear`) so console tools can manage their databases without writing boilerplate.

## Installation

Add the `RossWright.MetalCommand.Data` package to your project, plus a database-provider package:
- `RossWright.MetalCommand.Data.SqlServer` for SQL Server
- `RossWright.MetalCommand.Data.MySql` for MySQL (Pomelo)

## Quick start

```csharp
var app = ConsoleApplication.CreateBuilder()
    .AddDatabaseContextFactory<AppDbContext>(db =>
    {
        db.AddSqlServerDefault("dev",  config.GetConnectionString("Dev")!);
        db.AddSqlServerProtected("prod", config.GetConnectionString("Prod")!);
    })
    .AddMigrateCommand<AppDbContext>()
    .AddLoadDataCommand<AppDbContext>(opts => opts.LoadData = async ctx =>
    {
        var users = ctx.LoadFromCsv<UserRow>("users.csv");
        await ctx.DbContext.SaveChangesAsync();
    })
    .AddObliterateCommand<AppDbContext>()
    .AddReloadDatabaseCommand<AppDbContext>()
    .Build();

await app.RunAsync(args);
```

## Key concepts

### `IDatabaseContextFactory<TDbContext>`
Registered as scoped. Inject directly into commands that need raw `DbContext` access:

```csharp
[Command("Query", HelpBrief = "Run a query")]
public class QueryCommand(IDatabaseContextFactory<AppDbContext> factory) : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign)]
    public string? Env { get; set; }

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken ct)
    {
        using var db = factory.GetContext(Env);
        var count = await db.Users.CountAsync(ct);
        console.WriteLine($"{count} users");
        return CommandResult.Ok();
    }
}
```

### Built-in commands

| Extension method | Invocation | Description |
|---|---|---|
| `AddMigrateCommand<TDbCtx>()` | `migrate [env]` | Runs `Database.MigrateAsync()` with optional pre/post callbacks |
| `AddLoadDataCommand<TDbCtx>(configure)` | `load [env]` | Calls your delegate with a `LoadDataCommandContext` containing the `DbContext` and CSV helpers |
| `AddReloadDatabaseCommand<TDbCtx>()` | `reload [env]` | Obliterates the database, re-migrates, then loads seed data |
| `AddObliterateCommand<TDbCtx>()` | `obliterate env` | Drops all FK constraints, tables, and stored procedures |
| `AddClearDataCommand<TDbCtx>(configure)` | `clear [env]` | Deletes all rows without dropping the schema |

> `obliterate` is hard-blocked on protected environments. `reload` and `clear` apply `EnvironmentPolicy.Dangerous` for protected environments (user must type `yes`).

### `IDatabaseContextFactoryBuilder`

| Method | Description |
|---|---|
| `Add(env, opts, isDefault?, isProtected?)` | Register a named environment |
| `AddDefault(env, opts)` | Register and mark as the default |
| `AddProtected(env, opts)` | Register as protected |
| `AddDefaultProtected(env, opts)` | Register as both default and protected |

Provider packages add `AddSqlServer*` / `AddMySql*` overloads that accept a connection string or a `ConnectionStrings` configuration key.

### `CsvFile<T>`
Reads a CSV file into a typed collection using [CsvHelper](https://joshclose.github.io/CsvHelper/). The standard way to load seed data inside a `LoadData` callback. Extra columns, blank lines, and missing fields are silently ignored.

```csharp
opts.LoadData = async ctx =>
{
    var products = new CsvFile<ProductRow>("data/products.csv");
    ctx.DbContext.Products.AddRange(products.Rows.Select(r => r.ToEntity()));
    await ctx.DbContext.SaveChangesAsync();
};
```

### `DataCommandContext<TDbCtx>`
Base context passed to all data command callbacks. Exposes `Console`, `Environment`, and `DbContext`.

### `LoadDataCommandContext<TDbCtx>`
Extends `DataCommandContext` with `LoadFromCsv<TEntity>(fileName, resolve?)` — reads a CSV, adds entities to the context, and returns the loaded rows.

### `ClearDataCommandContext<TDbCtx>`
Extends `DataCommandContext` with `ClearTable(tableName)` — executes `DELETE FROM` with progress output.

## See also

- [MetalCommand (core)](../README.md)
- [MetalCommand.Abstractions](../RossWright.MetalCommand.Abstractions/README.md)
- [MetalCommand.Data.SqlServer](../RossWright.MetalCommand.Data.SqlServer/README.md)
- [MetalCommand.Data.MySql](../RossWright.MetalCommand.Data.MySql/README.md)
