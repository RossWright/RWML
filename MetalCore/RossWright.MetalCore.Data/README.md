# Ross Wright's Metal Core Data Library
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Entity Framework Extensions](#entity-framework-extensions)
  - [Database Context](#database-context)
  - [SaveChanges with FK Errors](#savechanges-with-fk-errors)
  - [RefreshTable](#refreshtable)
  - [Database Timing Interceptor](#database-timing-interceptor)
- [GeoCoder](#geocoder)
- [Installation](#installation)
- [See Also](#see-also)
- [License](#license)

---

## Entity Framework Extensions

### Database Context

`DbContextExtensions` adds three utility methods to any `DbContext` that targets a relational database.

| Method | Description |
|---|---|
| `DatabaseExists()` | Returns `true` if the underlying relational database has been created |
| `CheckForChangesToAny<T>()` | Returns `true` if the change tracker holds pending changes for entity type `T` |
| `Obliterate()` | Drops all FK constraints, tables, and stored procedures — SQL Server only; intended for test teardown |

> **`Obliterate()` is destructive.** It is designed for integration test teardown where a fresh schema is required between runs. Do not call it in production code.

---

### SaveChanges with FK Errors

`DbContextExtensions.SaveChangesAsyncWithFkErrors` wraps `SaveChangesAsync` and provides structured handling for foreign key constraint violations. When EF throws a `DbUpdateException` caused by an FK constraint, the callback receives the parsed constraint name rather than requiring you to parse the exception message yourself.

```csharp
await context.SaveChangesAsyncWithFkErrors(report =>
    throw new InvalidOperationException($"Cannot delete: referenced by '{report.ConstraintName}'"));
```

| Method | Description |
|---|---|
| `SaveChangesAsyncWithFkErrors(onError)` | Calls `SaveChangesAsync`; on FK violation invokes `onError` with a `ForeignKeyErrorReport`; always re-throws the original exception |

`ForeignKeyErrorReport` properties:

| Property | Type | Description |
|---|---|---|
| `EntityName` | `string` | Name of the entity type involved in the violation |
| `ConstraintName` | `string?` | Database constraint name parsed from the exception message |
| `Values` | `Dictionary<string, object?>` | Current property values of the violating entity, keyed by property name |

---

### RefreshTable

`RefreshTableExtensions.RefreshTable` synchronizes a `DbSet<DBENTITY>` against an incoming collection of `INENTITY` objects in a single call, performing the insert/update/delete logic automatically. It returns an `IRefreshResult` reporting how many records were added, updated, and deleted.

```csharp
var result = await context.Users.RefreshTable(incomingUsers);
// result.Adds, result.Updates, result.Deletes
```

By default, records absent from the incoming data are **not** deleted. Pass `deleteSourceEntities: true` to enable removal of absent records.

**Record matching** — overloads constrained to `IHasId` match by `Guid Id`. Overloads without that constraint accept a custom `Func<DBENTITY, INENTITY, bool> isSame` predicate.

**Entity copying** — by default, matching members are copied using `CopyTo`. Pass a custom `Action<DBENTITY, INENTITY> update` delegate to control exactly which fields are updated.

**Data fetching** — by default, `ToListAsync()` loads the current set. Pass a custom `Func<DbSet<DBENTITY>, Task<List<DBENTITY>>> fetchOldData` delegate to apply filters (e.g. restrict to a specific parent record).

The seven overloads cover combinations of these three axes:

| Overload | Match | Update | Fetch |
|---|---|---|---|
| `(dbSet, newData, deleteSourceEntities?)` | `IHasId` | auto | default |
| `(dbSet, newData, Action update, deleteSourceEntities?)` | `IHasId` | custom | default |
| `(dbSet, fetchOldData, newData, Action update, deleteSourceEntities?)` | `IHasId` | custom | custom |
| `(dbSet, newData, Func isSame, deleteSourceEntities?)` | custom | auto | default |
| `(dbSet, newData, Func isSame, Action update, deleteSourceEntities?)` | custom | custom | default |
| `(dbSet, fetchOldData, newData, Func isSame, Action update, deleteSourceEntities?)` | custom | custom | custom |
| `(dbSet, fetchOldData, newData, Func isSame, Func add, Action update, deleteSourceEntities)` | custom | custom | custom + custom add |

`IHasId` is a core MetalCore contract (`RossWright` namespace) that requires a `Guid Id` property. Most database entity base classes implement it.

#### `IRefreshResult`

| Property | Description |
|---|---|
| `Adds` | Number of entities inserted |
| `Updates` | Number of entities updated |
| `Deletes` | Number of entities deleted |

---

### Database Timing Interceptor

`IDatabaseTimingInterceptor` accumulates the total EF command execution time for the current DI scope, enabling per-request database timing without external tooling.

Register in `Program.cs` and attach to your `DbContext` options:

```csharp
// Program.cs
services.AddDatabaseTimingInterceptor();
services.AddDbContext<AppDbContext>((sp, opts) =>
    opts.UseSqlServer(connectionString)
        .UseDatabaseTimingInterceptor(sp));

// Inject IDatabaseTimingInterceptor anywhere in the same scope
var ms = timingInterceptor.RunTimeInMilliseconds;
```

| Method / Member | Description |
|---|---|
| `AddDatabaseTimingInterceptor(IServiceCollection)` | Registers `IDatabaseTimingInterceptor` as a scoped EF interceptor |
| `UseDatabaseTimingInterceptor(DbContextOptionsBuilder, IServiceProvider)` | Attaches the interceptor to a `DbContext` options builder |
| `IDatabaseTimingInterceptor.RunTimeInMilliseconds` | Cumulative EF command execution time for the current scope in milliseconds |

> **Registration lifetime:** `AddDatabaseTimingInterceptor` registers the interceptor as **scoped**, which is required for per-request timing to work correctly. Do not register `IDatabaseTimingInterceptor` as a singleton — doing so will cause timing to accumulate across all requests rather than resetting per scope.

---

## GeoCoder

`IGeoCoderService` provides a single method that resolves a US postal code or "City, State" string to a latitude/longitude coordinate pair.

> **Offline lookup only.** The bundled `GeoCoderService` uses an embedded data file — it does not contact any external API. Only US postal codes and "City, State" strings are supported. Unrecognized inputs throw `MetalCoreException`.

```csharp
var coords = geoCoderService.GetCoordinates("90210");        // zip code
var coords = geoCoderService.GetCoordinates("Beverly Hills, CA"); // City, State
// coords.Lat, coords.Lng
```

Register with `AddGeoCoderService()`:

```csharp
services.AddGeoCoderService();
```

### `LatLong`

`LatLong` is a value struct holding a latitude and longitude. It provides two utility methods for geographic calculations:

| Member | Description |
|---|---|
| `Lat` | Latitude in decimal degrees |
| `Lng` | Longitude in decimal degrees |
| `DistanceTo(double lat, double lng)` | Returns the distance in miles to the given coordinate using the Haversine formula, rounded to 2 decimal places |
| `CalcBound(double distance)` | Returns a bounding box `(LatLong min, LatLong max)` for the given radius in miles |

### Service Reference

| Type / Method | Description |
|---|---|
| `IGeoCoderService.GetCoordinates(string address)` | Resolves an address or place string to a `LatLong` |
| `AddGeoCoderService(IServiceCollection)` | Registers `IGeoCoderService` as a singleton |

---

## Installation

```powershell
dotnet add package RossWright.MetalCore.Data
```

Or add directly to your project file:

```xml
<PackageReference Include="RossWright.MetalCore.Data" Version="*" />
```

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalCore`](https://www.nuget.org/packages/RossWright.MetalCore) | Core extensions, utilities, options builders, load logging, exceptions, signing |
| [`RossWright.MetalCore.Blazor`](https://www.nuget.org/packages/RossWright.MetalCore.Blazor) | Blazor WASM utilities: local storage, JS script loader, host builder extensions |
| [`RossWright.MetalCore.Server`](https://www.nuget.org/packages/RossWright.MetalCore.Server) | ASP.NET Core messaging contracts, SMTP email service |
| [`RossWright.MetalCore.Populi`](https://www.nuget.org/packages/RossWright.MetalCore.Populi) | Zero-dependency test-data generator: names, addresses, emails, coordinates, dates, prices, and lorem ipsum |

---

## License

All **Ross Wright Metal Libraries** including this one are licensed under **Apache License 2.0 with Commons Clause**.

**You are free to**:
- Use the libraries in any project (personal or commercial)
- Modify them
- Include them in products or services you sell

**You may not**:
- Sell the libraries themselves (or any product/service whose *primary* value comes from the libraries)
- Repackage them with minimal changes and sell them as your own standalone product

Full legal text: [LICENSE.md](./LICENSE.md)

