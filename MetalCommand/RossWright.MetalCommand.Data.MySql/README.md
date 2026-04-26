# RossWright.MetalCommand.Data.MySql
Copyright (c) 2023-2026 Pross Co.

## Overview

MySQL (Pomelo) provider registration helpers for `RossWright.MetalCommand.Data`. Adds `AddMySql*` extension methods to `IDatabaseContextFactoryBuilder` so you can register MySQL environments with a single call.

## Installation

Add the `RossWright.MetalCommand.Data.MySql` package to your project (it transitively brings in `RossWright.MetalCommand.Data`).

## Quick start

```csharp
.AddDatabaseContextFactory<AppDbContext>(db =>
{
    db.AddMySqlDefault("dev",  "Server=localhost;Database=myapp_dev;User=root;Password=secret;");
    db.AddMySqlProtected("prod", "Server=db.example.com;Database=myapp;User=app;Password=...;");
})
```

Or resolve the connection string from `appsettings.json`:

```csharp
db.AddMySqlDefaultByConfigurationName("dev",  "Dev");
db.AddMySqlProtectedByConfigurationName("prod", "Prod");
```

## API summary

| Method | Description |
|---|---|
| `AddMySql(env, connectionString, opts?, isDefault?, isProtected?)` | Register a MySQL environment with a literal connection string |
| `AddMySqlDefault(env, connectionString, opts?)` | Register as the default environment |
| `AddMySqlProtected(env, connectionString, opts?)` | Register as a protected environment |
| `AddMySqlDefaultProtected(env, connectionString, opts?)` | Register as both default and protected |
| `AddMySqlByConfigurationName(env, configKey, opts?, ...)` | Register using `ConnectionStrings:{configKey}` from configuration |
| `AddMySqlDefaultByConfigurationName(env, configKey, opts?)` | Config-key variant, marked as default |
| `AddMySqlProtectedByConfigurationName(env, configKey, opts?)` | Config-key variant, marked as protected |
| `AddMySqlDefaultProtectedByConfigurationName(env, configKey, opts?)` | Config-key variant, default and protected |

All methods set a default command timeout of 300 seconds. Pass an `Action<IMySqlConnectionBuilder>` to apply additional EF Core or Pomelo options.

## See also

- [MetalCommand.Data](../RossWright.MetalCommand.Data/README.md)
- [MetalCommand (core)](../README.md)
