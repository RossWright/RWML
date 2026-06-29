# MetalCommand API Index

Primary namespaces: `RossWright.MetalCommand`, `RossWright.MetalCommand.Data`, and `RossWright.MetalCommand.Http`.

## RossWright.MetalCommand.ConsoleApplication

Package: `RossWright.MetalCommand`  
Namespace: `RossWright.MetalCommand`  
Summary: Interactive console application host. Use `CreateBuilder()`, configure commands and services, call `Build()`, then `RunAsync(args)`.

## RossWright.MetalCommand.IConsoleApplicationBuilder

Package: `RossWright.MetalCommand.Abstractions` / `RossWright.MetalCommand`  
Namespace: `RossWright.MetalCommand`  
Summary: Configures services, commands, middleware, logging, prompt behavior, colors, and the service provider factory before the console app is built.

## RossWright.MetalCommand.ICommand

Package: `RossWright.MetalCommand.Abstractions`  
Namespace: `RossWright.MetalCommand`  
Signature: `public interface ICommand`  
Summary: Contract implemented by command classes. The runtime binds arguments before calling `ExecuteAsync(IConsole, CancellationToken)`.

## RossWright.MetalCommand.CommandAttribute

Package: `RossWright.MetalCommand.Abstractions`  
Namespace: `RossWright.MetalCommand`  
Summary: Marks a class as a command and defines its display name, invocations, help text, and category.

## RossWright.MetalCommand.ArgAttribute

Package: `RossWright.MetalCommand.Abstractions`  
Namespace: `RossWright.MetalCommand`  
Summary: Marks a public settable property as a command argument with ordering, required/default behavior, named-argument support, valid values, and help text.

## RossWright.MetalCommand.EnvironmentArgAttribute

Package: `RossWright.MetalCommand.Abstractions`  
Namespace: `RossWright.MetalCommand`  
Summary: Marks a command property as the environment selector and applies `EnvironmentPolicy` checks against registered environment sources.

## RossWright.MetalCommand.CommandResult

Package: `RossWright.MetalCommand.Abstractions`  
Namespace: `RossWright.MetalCommand`  
Summary: Return value from `ICommand.ExecuteAsync`. Use `Ok()`, `Fail(...)`, `Exit()`, or `FailAndExit(...)` to report status and control whether the REPL exits.

## RossWright.MetalCommand.IConsole

Package: `RossWright.MetalCommand.Abstractions`  
Namespace: `RossWright.MetalCommand`  
Summary: Console abstraction used by commands for colored output, input prompts, indentation, and line control.

## RossWright.MetalCommand.ICommandExecutor

Package: `RossWright.MetalCommand.Abstractions`  
Namespace: `RossWright.MetalCommand`  
Summary: Executes registered commands programmatically and exposes the current session context dictionary.

## RossWright.MetalCommand.ICommandMiddleware

Package: `RossWright.MetalCommand.Abstractions`  
Namespace: `RossWright.MetalCommand`  
Summary: Pipeline hook for logic that runs around command execution.

## RossWright.MetalCommand.IConsoleApplicationBuilderExtensions.AddCommands

Package: `RossWright.MetalCommand.Abstractions` / `RossWright.MetalCommand`  
Namespace: `RossWright.MetalCommand`  
Summary: Registers command types through the builder's `ICommandCollection`.

## RossWright.MetalCommand.IConsoleApplicationBuilderExtensions.AddServices

Package: `RossWright.MetalCommand.Abstractions` / `RossWright.MetalCommand`  
Namespace: `RossWright.MetalCommand`  
Summary: Registers application services that commands and middleware can resolve from DI.

## RossWright.MetalCommand.ConsoleApplicationExtensions.Build

Package: `RossWright.MetalCommand`  
Namespace: `RossWright.MetalCommand`  
Summary: Builds a configured `IConsoleApplicationBuilder` into a runnable `ConsoleApplication`.

## RossWright.MetalCommand.Data.AddDatabaseContextFactoryExtensions.AddDatabaseContextFactory

Package: `RossWright.MetalCommand.Data`  
Namespace: `RossWright.MetalCommand.Data`  
Summary: Registers an environment-aware EF Core `IDatabaseContextFactory<TDbContext>` and adds environment middleware.

## RossWright.MetalCommand.Data.IDatabaseContextFactory<TDbContext>

Package: `RossWright.MetalCommand.Data`  
Namespace: `RossWright.MetalCommand.Data`  
Summary: Creates a scoped EF Core `DbContext` for the selected MetalCommand environment.

## RossWright.MetalCommand.Data.IDatabaseContextFactoryBuilder

Package: `RossWright.MetalCommand.Data`  
Namespace: `RossWright.MetalCommand.Data`  
Summary: Registers named database environments and marks defaults or protected environments.

## RossWright.MetalCommand.Data.IDatabaseContextFactoryBuilderExtensions.AddSqlServer

Package: `RossWright.MetalCommand.Data.SqlServer`  
Namespace: `RossWright.MetalCommand.Data`  
Summary: Registers a SQL Server database environment from a literal connection string.

## RossWright.MetalCommand.Data.IDatabaseContextFactoryBuilderExtensions.AddSqlServerByConfigurationName

Package: `RossWright.MetalCommand.Data.SqlServer`  
Namespace: `RossWright.MetalCommand.Data`  
Summary: Registers a SQL Server database environment from a named configuration connection string.

## RossWright.MetalCommand.Data.IDatabaseContextFactoryBuilderExtensions.AddMySql

Package: `RossWright.MetalCommand.Data.MySql`  
Namespace: `RossWright.MetalCommand.Data`  
Summary: Registers a MySQL or MariaDB database environment from a literal connection string.

## RossWright.MetalCommand.Data.IDatabaseContextFactoryBuilderExtensions.AddMySqlByConfigurationName

Package: `RossWright.MetalCommand.Data.MySql`  
Namespace: `RossWright.MetalCommand.Data`  
Summary: Registers a MySQL or MariaDB database environment from a named configuration connection string.

## RossWright.MetalCommand.Data.CsvFile<T>

Package: `RossWright.MetalCommand.Data`  
Namespace: `RossWright.MetalCommand.Data`  
Summary: Reads CSV seed data into typed records for load/reload database workflows.

## RossWright.MetalCommand.Http.AddHttpConnectionsExtensions.AddHttpConnections

Package: `RossWright.MetalCommand.Http`  
Namespace: `RossWright.MetalCommand.Http`  
Summary: Registers environment-aware HTTP connection groups and decorates `IHttpClientFactory` so bare client names resolve through the active environment.

## RossWright.MetalCommand.Http.IHttpConnectionsBuilder

Package: `RossWright.MetalCommand.Http`  
Namespace: `RossWright.MetalCommand.Http`  
Summary: Configures per-environment HTTP base addresses, default/protected flags, optional client configuration, and optional auth handlers.

## RossWright.MetalCommand.IHttpConnectionResolver

Package: `RossWright.MetalCommand.Abstractions` / `RossWright.MetalCommand.Http`  
Namespace: `RossWright.MetalCommand`  
Summary: Resolves the effective `HttpClient` key for an environment-aware HTTP connection group.

## RossWright.MetalCommand.Http.AddPingCommandExtension.AddPingCommand

Package: `RossWright.MetalCommand.Http`  
Namespace: `RossWright.MetalCommand.Http`  
Summary: Adds the built-in HTTP `ping` command for checking configured service environments.
