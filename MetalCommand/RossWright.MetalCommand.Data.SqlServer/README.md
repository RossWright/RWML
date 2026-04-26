# RossWright.MetalCommand.Data.SqlServer
Copyright (c) 2023-2026 Pross Co.

## Overview

SQL Server provider registration helpers for `RossWright.MetalCommand.Data`. Adds `AddSqlServer*` extension methods to `IDatabaseContextFactoryBuilder` so you can register SQL Server environments with a single call.

## Installation

Add the `RossWright.MetalCommand.Data.SqlServer` package to your project (it transitively brings in `RossWright.MetalCommand.Data`).

## Quick start

```csharp
.AddDatabaseContextFactory<AppDbContext>(db =>
{
    db.AddSqlServerDefault("dev",  "Server=localhost;Database=myapp_dev;Trusted_Connection=True;");
    db.AddSqlServerProtected("prod", "Server=db.example.com;Database=myapp;User Id=app;Password=...;");
})
```

Or resolve the connection string from `appsettings.json`:

```csharp
db.AddSqlServerDefaultByConfigurationName("dev",  "Dev");
db.AddSqlServerProtectedByConfigurationName("prod", "Prod");
```

## API summary

| Method | Description |
|---|---|
| `AddSqlServer(env, connectionString, opts?, isDefault?, isProtected?)` | Register a SQL Server environment with a literal connection string |
| `AddSqlServerDefault(env, connectionString, opts?)` | Register as the default environment |
| `AddSqlServerProtected(env, connectionString, opts?)` | Register as a protected environment |
| `AddSqlServerDefaultProtected(env, connectionString, opts?)` | Register as both default and protected |
| `AddSqlServerByConfigurationName(env, configKey, opts?, ...)` | Register using `ConnectionStrings:{configKey}` from configuration |
| `AddSqlServerDefaultByConfigurationName(env, configKey, opts?)` | Config-key variant, marked as default |
| `AddSqlServerProtectedByConfigurationName(env, configKey, opts?)` | Config-key variant, marked as protected |
| `AddSqlServerDefaultProtectedByConfigurationName(env, configKey, opts?)` | Config-key variant, default and protected |

All methods set a default command timeout of 300 seconds. Pass an `Action<ISqlServerConnectionBuilder>` to apply additional EF Core or SQL Server options.

## See also

- [MetalCommand.Data](../RossWright.MetalCommand.Data/README.md)
- [MetalCommand (core)](../README.md)
