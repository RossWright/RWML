# Use MetalCore Shared Utilities

Use this recipe when a project needs common .NET helpers: string similarity, readable names, conditional LINQ filters, validation, cloning, copying, signing, bootstrap logging, or shared application utilities.

## Install

```bash
dotnet add package RossWright.MetalCore
```

## Namespace

```csharp
using RossWright;
```

## Common Uses

```csharp
var score = "Robert".CalcLevenshteinDistanceTo("Rob");
var display = "CustomerAccountId".SpaceOut();

var activeCustomers = customers.WhereIf(activeOnly, c => c.IsActive);

model.AssertValid();

var dto = entity.CloneAs<CustomerDto>();
entity.CopyTo(existingEntity);
```

## Reach For This When

- You are writing normal .NET, ASP.NET Core, Blazor, or console code and need shared helpers.
- You want extension methods that work across the Metal libraries.
- You need bootstrap logging or common validation behavior.

## Avoid This When

- You only need mediator dispatch. Use MetalChain.
- You only need dependency injection scanning. Use MetalInjection.
- You need browser-only APIs. Use `RossWright.MetalCore.Blazor`.

## Notes For Agents

- Most extension methods are in `RossWright`.
- `CalcLevenshteinDistanceTo` returns a normalized similarity score, not a raw edit distance.
- Prefer XML docs for exact overloads when generating code against less common helpers.
