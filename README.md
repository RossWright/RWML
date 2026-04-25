# Ross Wright Metal Libraries
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [MetalCore](#metalcore)
- [MetalChain](#metalchain)
- [MetalInjection](#metalinjection)
- [MetalNexus](#metalnexus)
- [MetalGuardian](#metalguardian)
- [MetalShout](#metalshout)
- [MetalCommand](#metalcommand)
- [License](#license)

---

## Overview

The **Ross Wright Metal Libraries** are a suite of foundational .NET packages for building modern, production-ready applications. They cover dependency injection, mediator-pattern request dispatching, utilities, test-data generation, and more — designed to work independently or together across server, Blazor WebAssembly, and console project types.

All packages target **.NET 8, .NET 9, and .NET 10**. All extension methods live in the `RossWright` namespace (global-usings friendly).

> **Note on pre-release libraries:** MetalNexus, MetalGuardian, MetalShout, and MetalCommand are marked as pre-release. These libraries are functional and have been used in commercial applications. However, documentation may be incomplete or out of sync with recent refactors, unit test coverage is limited, and recent refactors have not been rigorously tested. Use them with that in mind and expect rough edges. I'm working on fully documenting, stabilizing and getting good unit test coverage to include these libraries in the next minor release 2026.1.0

---

## MetalCore

A collection of foundational utilities shared across all Ross Wright Metal libraries. MetalCore provides extensions, tooling, and contracts across five packages — from string and collection helpers to Entity Framework utilities, Blazor services, and test-data generation.

### Major Features

- Extensive `string`, collection, numeric, temporal, and reflection extension methods
- DTO mapping via `CloneAs` / `CopyTo` / `HasChangedFrom` with `[Ignore]` and `[Aka]` attribute support
- `IValidatable` validation contract with `AssertValid` / `IsValid` helpers
- Startup-time diagnostic logging via `ILoadLog` (`ConsoleLoadLog`, `ListLoadLog`, `ThrowExceptionOnLogError`)
- Assembly scanning options builder shared by all scanning-based Metal libraries
- Entity Framework `RefreshTable`, FK-error handling, and per-request timing interceptor
- Offline US GeoCoder (zip code / "City, State" → `LatLong`) with Haversine distance and bounding-box helpers
- Browser `localStorage` access and lazy JS script loading for Blazor WebAssembly
- ASP.NET Core and Blazor fluent host-builder syntax for single-chain startup
- SMTP email service with messaging contract abstractions (`IEmailService`, `ISmsService`)
- Pure-managed ECDSA signing that runs in both server and Blazor WebAssembly
- `Populi` static test-data generator: names, addresses, emails, companies, coordinates, dates, prices, and lorem ipsum

### Libraries

| Package | NuGet | Description |
|---|---|---|
| [`RossWright.MetalCore`](MetalCore/RossWright.MetalCore/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCore) | Core extensions (string, collection, numeric, temporal, reflection), clone/copy mapping, validation, DI helpers, assembly scanning options builder, load logging, security tools, ECDSA signing, and utilities |
| [`RossWright.MetalCore.Data`](MetalCore/RossWright.MetalCore.Data/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCore.Data) | Entity Framework Core extensions: `RefreshTable` bulk sync, FK-error handling, database timing interceptor; offline US GeoCoder |
| [`RossWright.MetalCore.Server`](MetalCore/RossWright.MetalCore.Server/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCore.Server) | ASP.NET Core: messaging contracts (`IEmailService`, `ISmsService`), SMTP email service, fluent `WebApplicationBuilder` / `WebApplication` extensions |
| [`RossWright.MetalCore.Blazor`](MetalCore/RossWright.MetalCore.Blazor/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCore.Blazor) | Blazor WebAssembly: `IBrowserLocalStorage`, `IJsScriptLoaderService`, fluent `WebAssemblyHostBuilder` extensions |
| [`RossWright.MetalCore.Populi`](MetalCore/RossWright.MetalCore.Populi/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCore.Populi) | Zero-dependency test-data generator: names, addresses, emails, companies, US coordinates, dates, prices, lorem ipsum, and random selection helpers |

---

## MetalChain

A lightweight, type-safe mediator library for asynchronously dispatching requests to handlers. MetalChain supports both commands (fire-and-forget) and queries (request/response) with distinct handling semantics, runtime subscriptions, and flexible no-handler behavior.

### Major Features

- `IRequest` (command) and `IRequest<TResponse>` (query) distinction with separate dispatch semantics
- Assembly-scanning handler registration with optional explicit registration via `AddMetalChainHandlers`
- `IMediator.Listen` for runtime subscriptions — returned `IDisposable` controls subscription lifetime
- `SendOrDefault` and `SendOrIgnore` call-site overrides for graceful no-handler scenarios
- Per-type `[AllowNoHandler]`, `[RequireHandler]`, and `[AllowMultipleHandlers]` behavior attributes
- Opt-in multicast fan-out for commands: sequential (fail-fast or collect errors) and parallel execution modes
- Open generic request and handler support — define once, use with any constrained type at runtime
- Minimal abstractions package (`RossWright.MetalChain.Abstractions`) for domain/contracts projects with no full-chain dependency

### Libraries

| Package | NuGet | Description |
|---|---|---|
| [`RossWright.MetalChain`](MetalChain/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalChain) | Full mediator implementation: assembly scanning, DI registration, `IMediator` dispatch, multicast fan-out, open generics, and `Listen` subscriptions |
| [`RossWright.MetalChain.Abstractions`](MetalChain/README.md#abstraction-library) | [NuGet](https://www.nuget.org/packages/RossWright.MetalChain.Abstractions) | Minimal contracts only: `IMediator`, `IRequest`, `IRequest<T>`, `IRequestHandler`, and behavior attributes — for use in domain or abstractions projects |

---

## MetalInjection

A ground-up dependency inversion container for .NET. MetalInjection is a complete `IServiceProvider` implementation — not a wrapper around the default .NET service provider — built from scratch to support attribute- and interface-driven registration, property injection, open-generic services, configuration-section binding, and automatic hosted-service registration. It is fully compatible with the standard `IServiceCollection` / `IServiceProvider` interfaces and integrates transparently with ASP.NET Core, Blazor WebAssembly, and console hosts.

### Major Features

- Attribute (`[Singleton<T>]`, `[ScopedService<T>]`, `[TransientService<T>]`) and interface (`ISingleton<T>`, etc.) registration via assembly scanning
- Stacked multi-interface registration on a single implementation with guaranteed instance sharing per lifetime
- Property injection via `[Inject]` — works in all project types including Blazor
- Optional injection (nullable parameters/properties) and `IEnumerable<T>` multiple-implementation injection
- Standard .NET keyed service injection via `[FromKeyedServices]`
- `[ConfigSection]` / `[ConfigSection<T>]` for automatic config binding and singleton registration with optional `IValidatingConfigSection` startup validation
- `[HostedService]` for automatic `IHostedService` / `BackgroundService` registration (server projects)
- Open generic service registration, including open-generic factory delegates (`AddOpenGenericSingleton`, `AddOpenGenericScoped`, `AddOpenGenericTransient`)
- Deterministic `IDisposable` and `IAsyncDisposable` disposal for transient, scoped, and singleton services across all scope boundaries
- Permissive and strict resolution modes; configurable throw-on-error behavior
- Setup for ASP.NET Core, Blazor WebAssembly, MetalCommand console, and plain `IServiceCollection` projects

### Libraries

| Package | NuGet | Description |
|---|---|---|
| [`RossWright.MetalInjection`](MetalInjection/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalInjection) | Core DI engine: attribute/interface registration scanning, property injection, keyed services, open generics, config section binding, and `BuildMetalInjectionServiceProvider` for plain `IServiceCollection` projects |
| [`RossWright.MetalInjection.Server`](MetalInjection/README.md#server-setup) | [NuGet](https://www.nuget.org/packages/RossWright.MetalInjection.Server) | ASP.NET Core integration: `builder.AddMetalInjection(...)` extension on `WebApplicationBuilder`, automatic `[HostedService]` registration |
| [`RossWright.MetalInjection.Blazor`](MetalInjection/README.md#blazor-client-setup) | [NuGet](https://www.nuget.org/packages/RossWright.MetalInjection.Blazor) | Blazor WebAssembly integration: `builder.AddMetalInjection(...)` extension on `WebAssemblyHostBuilder` |
| [`RossWright.MetalInjection.Abstractions`](MetalInjection/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalInjection.Abstractions) | Registration attributes and interfaces only (`[Singleton<T>]`, `ISingleton<T>`, etc.) — for use in domain or contracts projects without a full-engine dependency |

---

## MetalNexus

MetalNexus bridges MetalChain across the network, transparently routing `IMediator.Send` calls from a client to server-side handlers over a RESTful API — with no separate HTTP client code to write. Decorate a request with `[ApiRequest]` and MetalNexus auto-generates the endpoint, handles serialization, and marshals exceptions back to the caller. It works with any MetalChain-compatible client: Blazor WebAssembly, MetalCommand console apps, or plain .NET.

> **Pre-release version available, will release with 2026.1** — see the [MetalNexus README](MetalNexus/README.md) for full documentation.

### Libraries

| Package | NuGet | Description |
|---|---|---|
| [`RossWright.MetalNexus.Server`](MetalNexus/README.md#server-setup) | [NuGet](https://www.nuget.org/packages/RossWright.MetalNexus.Server) | ASP.NET Core server: auto-generates API endpoints for `[ApiRequest]`-decorated MetalChain requests, Swagger/OpenAPI integration, authentication hooks, file upload support |
| [`RossWright.MetalNexus.Blazor`](MetalNexus/README.md#blazor-webassembly) | [NuGet](https://www.nuget.org/packages/RossWright.MetalNexus.Blazor) | Blazor WebAssembly client: `AddMetalNexusClient`, `AddHttpClient`, and `<FileInput>` upload component |
| [`RossWright.MetalNexus`](MetalNexus/README.md#console-app-metalcommand) | [NuGet](https://www.nuget.org/packages/RossWright.MetalNexus) | Core client for console and other non-Blazor .NET projects |
| [`RossWright.MetalNexus.Abstractions`](MetalNexus/README.md#defining-api-requests) | [NuGet](https://www.nuget.org/packages/RossWright.MetalNexus.Abstractions) | `[ApiRequest]` attribute and MetalNexus contracts only — for shared request-type projects with no runtime dependency |

---

## MetalGuardian

MetalGuardian is a complete authentication and authorization system purpose-built for the Metal stack. It provides ready-made MetalNexus endpoints and MetalChain handlers for login, logout, token refresh, role-based authorization, and multi-factor authentication (TOTP), letting you add a secure auth layer to a Metal application with minimal setup.

> **Pre-release version available, will release with 2026.1** — see the [MetalGuardian README](MetalGuardian/README.md) for full documentation.

### Libraries

| Package | NuGet | Description |
|---|---|---|
| [`RossWright.MetalGuardian.Server`](MetalGuardian/README.md#server-setup) | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Server) | ASP.NET Core server: authentication endpoints, JWT issuance, role-based authorization, MetalNexus integration |
| [`RossWright.MetalGuardian.Blazor`](MetalGuardian/README.md#blazor-webassembly) | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Blazor) | Blazor WebAssembly client: auth state management and secured MetalNexus request handling |
| [`RossWright.MetalGuardian`](MetalGuardian/README.md#client-setup) | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian) | Shared core: auth contracts and client-side logic for non-Blazor projects |
| [`RossWright.MetalGuardian.MFA.TOTP`](MetalGuardian/README.md#totp-multi-factor-authentication) | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.MFA.TOTP) | TOTP multi-factor authentication support (compatible with Google Authenticator, Authy, etc.) |
| [`RossWright.MetalGuardian.Server.MFA.TOTP`](MetalGuardian/README.md#totp-multi-factor-authentication) | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Server.MFA.TOTP) | Server-side TOTP registration, verification, and recovery flow |
| [`RossWright.MetalGuardian.Abstractions`](MetalGuardian/README.md#built-in-metalnexus-endpoints) | [NuGet](https://www.nuget.org/packages/RossWright.MetalGuardian.Abstractions) | Auth contracts only — for shared projects with no runtime dependency |

---

## MetalShout

MetalShout is the server-to-client push complement to MetalNexus. It uses SignalR under the hood to let server code dispatch MetalChain commands directly to connected clients, which handle them with ordinary `IRequestHandler` implementations. The result is real-time server push — event notifications, live data updates, progress reporting — using the same request/handler model as the rest of the Metal stack.

> **Pre-release version available, will release with 2026.1** — see the [MetalShout README](MetalShout/README.md) for full documentation.

### Libraries

| Package | NuGet | Description |
|---|---|---|
| [`RossWright.MetalShout.Server`](MetalShout/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalShout.Server) | ASP.NET Core server: SignalR hub setup and `IMediator`-based push dispatch to connected clients |
| [`RossWright.MetalShout`](MetalShout/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalShout) | Client library (Blazor and other .NET clients): connects to the SignalR hub and routes incoming pushes to registered MetalChain handlers |

---

## MetalCommand

MetalCommand is a framework for building interactive .NET console applications. It provides a `ConsoleApplication` host builder that sets up configuration, DI, and a read-execute loop — independently of MetalChain and MetalInjection, though compatible with both. Commands are plain classes implementing `ICommand` that declare their own name, invocation aliases, argument schema, and help text. The runtime handles argument validation, default values, context-key substitution, Ctrl-C cancellation, and error reporting automatically.

The companion data package adds EF Core integration with named, environment-aware database contexts (dev/staging/prod) and a suite of ready-made database management commands — migrate, load from CSV, obliterate, clear — that can be wired up in a single builder call.

> **Pre-release version available, will release with 2026.1** — see the [MetalCommand README](MetalCommand/README.md) for full documentation.

### Libraries

| Package | NuGet | Description |
|---|---|---|
| [`RossWright.MetalCommand`](MetalCommand/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCommand) | Console host builder, `ICommand` / `CommandDescriptor` model, argument resolution, `IConsole` API with indentation and color, progress indicators (spinner, bar, percentage), and `ICommandExecutor` for inter-command dispatch |
| [`RossWright.MetalCommand.Data`](MetalCommand/README.md#database-tooling) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCommand.Data) | EF Core integration: named environment `IDatabaseContextFactory`, built-in migrate / load / reload / obliterate / clear commands, `CsvFile<T>` seed-data reader |
| [`RossWright.MetalCommand.Data.SqlServer`](MetalCommand/README.md#database-tooling) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCommand.Data.SqlServer) | SQL Server provider helpers for MetalCommand data access |
| [`RossWright.MetalCommand.Data.MySql`](MetalCommand/README.md#database-tooling) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCommand.Data.MySql) | MySQL / MariaDB provider helpers for MetalCommand data access |
| [`RossWright.MetalCommand.Abstractions`](MetalCommand/README.md) | [NuGet](https://www.nuget.org/packages/RossWright.MetalCommand.Abstractions) | `ICommand`, `CommandDescriptor`, `IConsole`, `ICommandExecutor`, and `IConsoleApplicationBuilder` contracts — for shared projects with no runtime dependency |

---

## License

All **Ross Wright Metal Libraries** are licensed under **Apache License 2.0 with Commons Clause**.

**You are free to**:
- Use the libraries in any project (personal or commercial)
- Modify them
- Include them in products or services you sell

**You may not**:
- Sell the libraries themselves (or any product/service whose *primary* value comes from the libraries)
- Repackage them with minimal changes and sell them as your own standalone product

See [LICENSE](LICENSE) for the full text.