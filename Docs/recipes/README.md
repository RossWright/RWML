# RWML Task Recipes

These recipes are task-first entry points for developers and AI coding agents.

Use them when you know the job you want to accomplish but do not yet know which Ross Wright Metal Library package to install.

## Package Families

| Task | Start Here |
|---|---|
| Use shared extension methods, validation, mapping, Blazor helpers, server helpers, or EF Core utilities | [Use MetalCore shared utilities](metalcore-shared-utilities.md) |
| Add Blazor browser storage or script loading helpers | [Use MetalCore in Blazor WebAssembly](metalcore-blazor-browser-helpers.md) |
| Add mediator-style command/query dispatch | [Add MetalChain mediator dispatch](metalchain-mediator-dispatch.md) |
| Define shared request/handler contracts | [Define MetalChain requests and handlers](metalchain-requests-handlers.md) |
| Build an interactive console application | [Build a MetalCommand console app](metalcommand-console-app.md) |
| Add database management commands to a console app | [Add MetalCommand database tooling](metalcommand-database-tooling.md) |
| Use attribute-driven dependency injection in ASP.NET Core | [Use MetalInjection in ASP.NET Core](metalinjection-aspnetcore.md) |
| Use attribute-driven dependency injection in Blazor WebAssembly | [Use MetalInjection in Blazor WebAssembly](metalinjection-blazor.md) |
| Expose MetalChain requests as HTTP endpoints | [Connect Blazor to ASP.NET Core with MetalNexus](metalnexus-blazor-server.md) |
| Upload files through generated MetalNexus endpoints | [Upload files with MetalNexus](metalnexus-file-upload.md) |
| Add JWT authentication to an ASP.NET Core server | [Add MetalGuardian server authentication](metalguardian-server-authentication.md) |
| Add login state and authenticated calls to Blazor WebAssembly | [Add MetalGuardian Blazor authentication](metalguardian-blazor-authentication.md) |

## Agent Rules

- Prefer the smallest package that matches the task.
- Use the namespace shown in the recipe before inventing API names.
- Scan the matching `AI-USAGE.md` and `Docs/api-index/*.md` file when you need more symbols.
- Do not use unreleased Metal libraries unless the user explicitly asks for local experimental code.
