# MetalCore AI Usage Guide

Use this file when generating code that consumes RossWright.MetalCore packages.

## Packages

| Package | Use When |
|---|---|
| `RossWright.MetalCore` | You need extension methods, validation, mapping, bootstrap logging, signing, or shared utilities. |
| `RossWright.MetalCore.Blazor` | You are in a Blazor WebAssembly project and need browser storage, JS script loading, or host builder helpers. |
| `RossWright.MetalCore.Server` | You are in an ASP.NET Core server and need messaging contracts, SMTP, or web host helpers. |
| `RossWright.MetalCore.Data` | You need EF Core helpers, RefreshTable, GeoCoder, or database timing diagnostics. |
| `RossWright.MetalCore.Populi` | You need generated test/demo data. |

## Namespaces

Most APIs are in:

```csharp
using RossWright;
```

Messaging contracts are in:

```csharp
using RossWright.Messaging;
```

## Common APIs

| Task | API |
|---|---|
| Compare strings by normalized Levenshtein similarity | `text.CalcLevenshteinDistanceTo(other)` |
| Make identifiers readable | `text.SpaceOut()` |
| Add optional LINQ filters | `query.WhereIf(condition, predicate)` |
| Clone compatible DTOs | `source.CloneAs<T>()` |
| Copy matching property values | `source.CopyTo(target)` |
| Check whether values changed | `source.HasChangedFrom(other)` |
| Validate an object | `model.AssertValid()` / `model.IsValid()` |
| Use browser local storage in Blazor | `IBrowserLocalStorage` |
| Send email from a server package | `IEmailService` |

## Important notes

- `CalcLevenshteinDistanceTo` returns a normalized similarity score. Higher is more similar; it is not a raw edit-count distance.
- Extension methods usually require `using RossWright;`.
- Prefer Blazor-specific APIs only in WebAssembly projects.
- Prefer server APIs only in ASP.NET Core server projects.
