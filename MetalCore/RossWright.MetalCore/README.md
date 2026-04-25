# Ross Wright's Metal Core Library
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [String Extensions](#string-extensions)
- [Collection Extensions](#collection-extensions)
- [Numeric & Temporal Extensions](#numeric--temporal-extensions)
- [Reflection Extensions](#reflection-extensions)
- [Clone Extensions](#clone-extensions)
- [Validation](#validation)
- [DI & Service Collection Extensions](#di--service-collection-extensions)
- [Options Builders](#options-builders)
- [Load Log](#load-log)
- [Utilities](#utilities)
- [Signing](#signing)
- [Installation](#installation)
- [See Also](#see-also)
- [License](#license)
- [Changelog](CHANGELOG.md)

---

## Overview

MetalCore is a collection of foundational utilities shared across all Ross Wright Metal libraries. It provides extensions, tooling, and contracts spanning five packages.

| Package | Purpose |
|---|---|
| `RossWright.MetalCore` | Core extensions, utilities, options builders, load logging, exceptions, signing |
| `RossWright.MetalCore.Server` | ASP.NET Core messaging contracts, SMTP, and `WebApplicationBuilder` helpers |
| `RossWright.MetalCore.Data` | Entity Framework Core extensions, RefreshTable, GeoCoder, timing interceptor |
| `RossWright.MetalCore.Blazor` | Blazor/WASM services and `WebAssemblyHostBuilder` helpers |
| `RossWright.MetalCore.Populi` | Zero-dependency static test-data generator: names, addresses, emails, coordinates, dates, prices, and lorem ipsum |

All extension methods live in the `RossWright` namespace (global-usings friendly). Messaging types live in `RossWright.Messaging`.

---

## String Extensions

#### `string`

| Method | Description | Example |
|---|---|---|
| `SpaceOut()` | Converts PascalCase, camelCase, and Snake_Case to space-separated words | `"PascalCase".SpaceOut()` → `"Pascal Case"` |
| `CapFirst()` | Capitalizes the first character | `"hello".CapFirst()` → `"Hello"` |
| `TitleCase()` | Converts each word to Title Case | `"hello world".TitleCase()` → `"Hello World"` |
| `Clip(maxChars)` | Truncates to at most `maxChars` characters | `"Hello!".Clip(3)` → `"Hel"` |
| `Filter(char[])` | Keeps only characters present in the allowed set | `"hello".Filter('h', 'e')` → `"he"` |
| `Filter(Func<char,bool>)` | Keeps only characters matching a predicate | `"abc123".Filter(char.IsDigit)` → `"123"` |
| `Without(char[])` | Removes the specified characters from the string | `"hello".Without('l')` → `"heo"` |
| `NullIfEmptyOrWhitespace()` | Returns `null` if the string is null, empty, or whitespace | `"   ".NullIfEmptyOrWhitespace()` → `null` |
| `MakeSafeFileName()` | Replaces invalid filename characters and spaces with underscores | `"my file.txt".MakeSafeFileName()` → `"my_file.txt"` |
| `Split(Func<char,bool>)` | Splits using a character predicate instead of a fixed delimiter | `"a1b2".Split(char.IsDigit)` → `["a","b"]` |
| `SplitAroundQuotes(...)` | Splits on delimiters while treating quoted regions as single tokens | `"a,\"b,c\"".SplitAroundQuotes()` → `["a","b,c"]` |
| `EndSentence(punctuation)` | Appends punctuation if the string doesn't already end with a sentence terminator | `"Hello".EndSentence()` → `"Hello."` |
| `ButAll(char)` | Returns a string of the same length filled with a single repeated char | `"Hello".ButAll('*')` → `"*****"` |
| `ToOnlyDigits()` | Strips all non-digit characters | `"(555) 123-4567".ToOnlyDigits()` → `"5551234567"` |
| `IsValidEmail()` | Validates an email address using RFC-compliant regex with Unicode domain normalization | `"user@example.com".IsValidEmail()` → `true` |
| `IsValidPhoneNumber()` | Returns `true` for a valid 10-digit US phone number (tolerates formatting chars) | `"(555) 123-4567".IsValidPhoneNumber()` → `true` |
| `ToFormattedPhoneNumber()` | Formats a digit string as `(555) 123-4567`, with optional country code | `"5551234567".ToFormattedPhoneNumber()` → `"(555) 123-4567"` |
| `ToNormalizedPhoneNumber()` | Normalizes to E.164 format (`+1XXXXXXXXXX`) | `"(555) 123-4567".ToNormalizedPhoneNumber()` → `"+15551234567"` |

### `IEnumerable<string>`

| Method | Description | Example |
|---|---|---|
| `JoinWithQuotes(delimiter, quote)` | Joins a string sequence, wrapping each element in quote chars | `new[]{"a","b"}.JoinWithQuotes()` → `"\"a\",\"b\""` |
| `CommaListJoin(conjunction)` | Joins a collection into a natural-language list | `new[]{"a","b","c"}.CommaListJoin()` → `"a, b and c"` |

### `IEnumerable<T>`

| Method | Description | Example |
|---|---|---|
| `ZeroOneOrMany(many, one, zero)` | Returns different strings for empty, single, or multiple items | `items.ZeroOneOrMany(x => $"{x.Count()} found", x => x.First(), "none")` |

### `object?`

| Method | Description | Example |
|---|---|---|
| `ToStringIfPresent(Func)` | Applies a formatter to a value only when its string representation is non-empty | `value.ToStringIfPresent(v => $"({v})")` → `"(42)"` or `""` |
| `PreSpaceIfPresent()` | Prepends a space to a value's string if it is non-empty | `value.PreSpaceIfPresent()` → `" 42"` or `null` |

### `string[]`

| Method | Description | Example |
|---|---|---|
| `GetTwo()` … `GetTen()` | Destructures a `string[]` into a typed value tuple of 2–10 strings | `arr.GetTwo()` → `(arr[0], arr[1])` |

---

## Collection Extensions

### `IEnumerable<T>`

| Method | Description | Example |
|---|---|---|
| `WhereIf(bool, ifTrue, ifFalse?)` | Applies a filter only when a flag is `true`, with optional else filter | `query.WhereIf(isActive, x => x.Active)` |
| `ConcatAllowNull(second)` | Concatenates two sequences, treating either as empty if `null` | `first.ConcatAllowNull(second)` |
| `WithIndex()` | Projects each element paired with its zero-based index | `items.WithIndex()` → `[(item, 0), (item, 1), ...]` |
| `ForEach(action)` | Executes an action for each element | `items.ForEach(Console.WriteLine)` |
| `WithEach(action)` | Executes an action for each element and yields each element | `items.WithEach(Log).ToList()` |
| `ForEachAsync(action)` | Executes an async action for each element sequentially | `await items.ForEachAsync(SaveAsync)` |
| `SelectDeep(selectSubItems, select?)` | Recursively flattens a tree-shaped hierarchy into a sequence | `nodes.SelectDeep(n => n.Children)` |
| `Without(params T[])` | Filters out the specified values from the sequence | `items.Without(3, 5)` → removes 3 and 5 |
| `OrderBy(keySelector, isAscending)` | Sorts ascending or descending based on a `bool` flag | `items.OrderBy(x => x.Name, isAscending)` |
| `ThenBy(keySelector, isAscending)` | Secondary sort ascending or descending based on a `bool` flag | `items.ThenBy(x => x.Date, isAscending)` |
| `FirstIndexWhere(predicate)` | Returns the zero-based index of the first matching element, or `-1` | `items.FirstIndexWhere(x => x.Id == id)` → `2` |
| `ScrambledEquals(list2)` | Compares two sequences for equality regardless of element order | `new[]{1,2,3}.ScrambledEquals(new[]{3,1,2})` → `true` |
| `WhereNotNull()` | Filters out `null` values from a nullable sequence; returns a non-nullable sequence | `items.WhereNotNull()` → non-null elements only |
| `GetAggregateHashCode()` | Computes a combined hash code for the entire sequence | `items.GetAggregateHashCode()` → `int` |
| `AllSame(predicate)` | Returns `true` if all elements produce the same projected value; empty sequence is all-same | `items.AllSame(x => x.Status)` → `true` |
| `ToArray<TIn,TOut>(predicate)` | Projects each element and returns the results as an array | `items.ToArray(x => x.Name)` → `string[]` |
| `ToList<TIn,TOut>(predicate)` | Projects each element and returns the results as a list | `items.ToList(x => x.Name)` → `List<string>` |

### `IQueryable<T>`

| Method | Description | Example |
|---|---|---|
| `Skip(int?)` | Skips elements only when count is non-null and positive | `query.Skip(page.Skip)` |
| `Take(int?)` | Takes elements only when count is non-null and positive | `query.Take(page.PageSize)` |
| `WhereIf(bool, predicate, else?)` | Applies a LINQ expression only when a flag is `true` | `query.WhereIf(hasFilter, x => x.Active)` |
| `WhereIfNotNull(value, predicate)` | Applies a filter only when a reference value is non-null | `query.WhereIfNotNull(status, x => x.Status == status)` |
| `WhereIfNotNullOrEmpty(collection, predicate)` | Applies a filter only when a collection is non-null and non-empty | `query.WhereIfNotNullOrEmpty(ids, x => ids.Contains(x.Id))` |
| `OrderBy(keySelector, isAscending)` | Orders ascending or descending based on a `bool` flag | `query.OrderBy(x => x.Name, isAscending)` |
| `ThenBy(keySelector, isAscending)` | Secondary ascending/descending sort based on a `bool` flag | `query.ThenBy(x => x.Date, isAscending)` |

### `IDictionary<TKey, TValue>`

| Method | Description | Example |
|---|---|---|
| `GetValueOrDefault(key, default?)` | Returns the value for a key, or a default if not found | `dict.GetValueOrDefault("x", 0)` |
| `WithoutKey(TKey)` | Returns a new dictionary without the specified key | `dict.WithoutKey("removeMe")` |
| `WithoutKey(Func<TKey,bool>)` | Returns a new dictionary excluding keys matching a predicate | `dict.WithoutKey(k => k.StartsWith("_"))` |
| `Without(Func<TKey,TValue,bool>)` | Returns a new dictionary excluding entries matching a key+value predicate | `dict.Without((k, v) => v == null)` |
| `ToDictionary()` | Converts `IEnumerable<KeyValuePair<K,V>>` to a `Dictionary<K,V>` | `kvps.ToDictionary()` |
| `CopyTo(target)` | Copies all entries from one dictionary into another | `source.CopyTo(target)` |
| `RemoveWhere(predicate)` | Removes in-place all entries whose key+value match the predicate | `dict.RemoveWhere((k, v) => v == null)` |

### `IDictionary<TKey, IList<TValue>>`

| Method | Description | Example |
|---|---|---|
| `AddToList(key, value)` | Appends a value to the list at a key, creating the list if absent | `dict.AddToList("tag", item)` |
| `GetList(key)` | Returns the list stored at a key, or `null` if absent or empty | `dict.GetList("tag")` |
| `RemoveFromList(key, value)` | Removes a value from the list stored at a key | `dict.RemoveFromList("tag", item)` |

### `HashSet<T>`

| Method | Description | Example |
|---|---|---|
| `AddRange(items)` | Adds multiple items to a `HashSet<T>`; returns `true` if any were new | `set.AddRange(new[]{1, 2, 3})` → `true` |
| `RemoveRange(items)` | Removes multiple items from a `HashSet<T>`; returns `true` if any were present | `set.RemoveRange(new[]{1, 2})` → `true` |

### `T`

| Method | Description | Example |
|---|---|---|
| `In<T>(params T[])` | Returns `true` if the value equals any of the supplied candidates | `status.In(Status.Active, Status.Pending)` → `true` |

---

## Numeric & Temporal Extensions

### `int`

| Method | Description | Example |
|---|---|---|
| `Clamp(min?, max?)` | Clamps an `int` to optional min/max bounds | `(-5).Clamp(0, 100)` → `0` |

### `double`

| Method | Description | Example |
|---|---|---|
| `NullIfNotReal()` | Returns `null` if the value is `NaN` or `Infinity` | `double.NaN.NullIfNotReal()` → `null` |
| `ToAccountingString()` | Formats as `1,234.56` or `(1,234.56)` for negatives | `(-1234.56).ToAccountingString()` → `"(1,234.56)"` |
| `FromDegreesToRadians()` | Converts an angle from degrees to radians | `(180.0).FromDegreesToRadians()` → `Math.PI` |
| `FromRadiansToDegrees()` | Converts an angle from radians to degrees | `Math.PI.FromRadiansToDegrees()` → `180.0` |
| `Clamp(min?, max?)` | Clamps a `double` to optional min/max bounds | `(1.5).Clamp(0.0, 1.0)` → `1.0` |

### `double[]` / `double?[]`

| Method | Description | Example |
|---|---|---|
| `Downsample(sampleCount)` | Down-samples a large array by bucket-averaging to a target length | `largeArray.Downsample(500)` → 500-element array |

### `IEnumerable<double>`

| Method | Description | Example |
|---|---|---|
| `StandardDeviation()` | Computes the standard deviation of a `double` sequence | `values.StandardDeviation()` → `1.41` |

### `bool?`

| Method | Description | Example |
|---|---|---|
| `IsNullOrTrue()` | Returns `true` if the nullable bool is `null` or `true` | `((bool?)null).IsNullOrTrue()` → `true` |
| `IsNullOrFalse()` | Returns `true` if the nullable bool is `null` or `false` | `((bool?)false).IsNullOrFalse()` → `true` |

### `TimeSpan`

| Method | Description | Example |
|---|---|---|
| `ToRelativeTime()` | Formats as a human-readable duration | `TimeSpan.FromHours(3.5).ToRelativeTime()` → `"3 hours"` |

### `DateTime`

| Method | Description | Example |
|---|---|---|
| `ToLocalShortDateTimeString()` | Formats as local short date + short time | `dt.ToLocalShortDateTimeString()` → `"1/1/2025 3:00 PM"` |
| `ToShortDateTimeString()` | Formats as short date + local short time | `dt.ToShortDateTimeString()` → `"1/1/2025 3:00 PM"` |
| `ToRelativeTime()` | Formats as a human-readable relative string | `dt.ToRelativeTime()` → `"Yesterday at 3:00 PM"` |

### `DayOfWeek`

| Method | Description | Example |
|---|---|---|
| `Abbr()` | Returns the 3-letter weekday abbreviation | `DayOfWeek.Monday.Abbr()` → `"Mon"` |

---

## Reflection Extensions

### `MemberInfo`

| Method | Description | Example |
|---|---|---|
| `GetValue(obj)` | Gets the value of a `PropertyInfo` or `FieldInfo` from an object | `propInfo.GetValue(obj)` → property value |
| `SetValue(obj, value)` | Sets the value of a `PropertyInfo` or `FieldInfo` on an object | `propInfo.SetValue(obj, "new value")` |
| `GetReturnType()` | Gets the field/property/return type from any `MemberInfo` | `member.GetReturnType()` → `typeof(string)` |

### `Type`

| Method | Description | Example |
|---|---|---|
| `HasAttribute(attributeType)` | Checks whether a `Type` carries the given attribute | `typeof(MyClass).HasAttribute(typeof(ObsoleteAttribute))` → `true` |
| `HasAttribute<TAttribute>()` | Generic overload of `HasAttribute` | `typeof(MyClass).HasAttribute<ObsoleteAttribute>()` |
| `Parse(string)` | Converts a string to the type using `TypeDescriptor` | `typeof(int).Parse("42")` → `42` |
| `TryConvert(value)` | Attempts type conversion using `TypeDescriptor` or string parsing | `typeof(int).TryConvert("42")` → `42` |
| `IsSimpleType()` | Returns `true` if the type can be converted from a `string` | `typeof(int).IsSimpleType()` → `true` |
| `IsConcrete()` | Returns `true` if the type is neither abstract nor an interface | `typeof(MyService).IsConcrete()` → `true` |
| `GetFullGenericName()` | Returns a human-readable generic type name | `typeof(List<string>).GetFullGenericName()` → `"List<string>"` |

### `FieldInfo`

| Method | Description | Example |
|---|---|---|
| `HasAttribute(attributeType)` | Checks whether a `FieldInfo` carries the given attribute | `field.HasAttribute(typeof(JsonIgnoreAttribute))` |
| `HasAttribute<TAttribute>()` | Generic overload of `HasAttribute` | `field.HasAttribute<JsonIgnoreAttribute>()` → `true` |

### `PropertyInfo`

| Method | Description | Example |
|---|---|---|
| `HasAttribute(attributeType)` | Checks whether a `PropertyInfo` carries the given attribute | `prop.HasAttribute(typeof(RequiredAttribute))` |
| `HasAttribute<TAttribute>()` | Generic overload of `HasAttribute` | `prop.HasAttribute<RequiredAttribute>()` → `true` |

---

## Clone Extensions

Clone extensions use a **shallow copy** model: primitive and value-type members are copied field-by-field, but reference-type members are *not* deep-copied — nested objects share the same instance as the original.

The four single-object methods cover the most common mapping scenarios:

- **`Clone<T>(init?)`** — creates a same-type duplicate; useful for taking a snapshot before allowing edits.
- **`CloneAs<T>(init?)`** — maps into a new instance of a *different* type; members are matched by name (case-insensitive), and unmatched members are left at their default values.
- **`CopyTo(target)`** — writes into an *existing* target instead of allocating a new one; useful when updating an already-tracked entity.
- **`HasChangedFrom(original)`** — dirty detection; compares every member and returns `true` if anything has changed.

Two attributes control mapping behavior:

- **`[Ignore]`** — skip this member entirely during any Clone or CloneAs operation.
- **`[Aka("name")]`** — treat this member as having an alternate name on the source type, enabling mapping between properties with different names.

**Primary use case: DTO mapping.** `CloneAs` removes the boilerplate of manual assignment when translating between database entities and view models:

```csharp
// Single entity → DTO
var dto = dbUser.CloneAs<UserDto>();

// Collection
var dtos = users.CloneAs<UserDto>();

// With per-item initializer to fill computed fields
var dtos = users.CloneAs<UserDto>(dto => dto.DisplayName = $"{dto.First} {dto.Last}");

// Dirty-check before saving
if (editedUser.HasChangedFrom(originalUser)) await SaveAsync(editedUser);

// Async: chain onto an in-flight Task<DBO?>
var dto = await GetUserAsync().ThenCloneAs<UserDbo, UserDto>();

// Async collection: chain onto an in-flight Task<List<DBO>>
List<UserDto> dtos = await GetUsersAsync().ThenCloneAs<UserDbo, UserDto>();
```

> **Limitation:** nested reference-type properties share the same instance as the original. For deeper control, chain `CloneAs` calls or use the initializer delegate.

### `T`

| Method | Description | Example |
|---|---|---|
| `Clone<T>(init?)` | Shallow-copies an object to a new instance of the same type | `order.Clone()` → new `Order` with same values |
| `CloneAs<T>(init?)` | Copies into a new instance of a different type, mapping matching members | `dbo.CloneAs<OrderDto>()` |
| `CopyTo(target)` | Copies all matching properties/fields from source to an existing target object | `source.CopyTo(target)` |
| `HasChangedFrom(original)` | Returns `true` if any property or field differs from the original | `edited.HasChangedFrom(original)` → `true` |

### `IEnumerable<T>`

| Method | Description | Example |
|---|---|---|
| `CloneAs<T>(init?)` | Maps a collection of objects to a new array of type `T` | `dbos.CloneAs<OrderDto>()` → `OrderDto[]` |
| `CloneAs<DBO, DTO>(init?)` | Strongly-typed collection mapping with an optional per-item initializer | `dbos.CloneAs<Order, OrderDto>(dto => dto.Label = "x")` |

### `Task<DBO?>` / `Task<List<DBO>>` / `Task<DBO[]>`

| Method | Description | Example |
|---|---|---|
| `ThenCloneAs<DBO,DTO>(init?)` | Awaits `Task<DBO?>` and maps result to `DTO`; returns `null` if source is `null` | `await userTask.ThenCloneAs<UserDbo, UserDto>()` |
| `ThenCloneAs<DBO,DTO>(init?)` | Awaits `Task<List<DBO>>` and maps each element; returns `List<DTO>` | `await usersTask.ThenCloneAs<UserDbo, UserDto>()` |
| `ThenCloneAs<DBO,DTO>(init?)` | Awaits `Task<DBO[]>` and maps each element; returns `DTO[]` | `await usersArrayTask.ThenCloneAs<UserDbo, UserDto>()` |

| Attribute | Description | Example |
|---|---|---|
| `[Ignore]` | Marks a property or field to be skipped during Clone/CloneAs mapping | `[Ignore] public string Internal { get; set; }` |
| `[Aka(alias)]` | Provides an alternate member name for cross-type Clone/CloneAs mapping | `[Aka("Name")] public string FullName { get; set; }` |

### `IHasId`

`IHasId` (namespace `RossWright`) is a lightweight identity contract that requires a single `Guid Id` property. Implement it on your database entity base classes to unlock the `IHasId`-constrained overloads of `RefreshTableExtensions.RefreshTable` in `RossWright.MetalCore.Data`, which use it for automatic record matching without requiring a custom `isSame` predicate.

```csharp
public class UserEntity : IHasId
{
    public Guid Id { get; set; }
    // ...
}
```

See [`RossWright.MetalCore.Data`](../RossWright.MetalCore.Data/README.md#refreshtable) for full `RefreshTable` usage.

---

## Validation

| Symbol | Description |
|---|---|
| `Tools.Validate<T>(value, checks)` | Runs a set of validation checks; returns combined error message or `null` |
| `Tools.AssertValid<T>(value, checks)` | Runs validation checks; throws `ValidationException` on failure |
| `IValidatable.Validate()` | Contract method; returns a validation error string or `null` if valid |
| `IValidatable.IsValid()` | Extension; returns `true` when `Validate()` returns `null` |
| `IValidatable.AssertValid()` | Extension; throws `ValidationException` when `Validate()` returns a message |

---

## DI & Service Collection Extensions

### `IServiceCollection`

| Method | Description | Example |
|---|---|---|
| `AddScopedAlias<TService, TAliasOf>()` | Registers `TService` as a scoped alias, resolving by casting an existing `TAliasOf` | `services.AddScopedAlias<IFoo, FooImpl>()` |
| `HasService<TService>()` | Returns `true` if `TService` is already registered | `services.HasService<IMyService>()` → `true` |
| `HasService(Type)` | Returns `true` if the given service type is already registered | `services.HasService(typeof(IMyService))` |
| `AddServices(registrations)` | Applies a batch of service registration delegates | `services.AddServices(builder.Services)` |

---

## Options Builders

Many Ross Wright Metal libraries accept a builder callback in their `AddXxx(builder, opts => { ... })` registration call. The callback receives an object implementing `IAssemblyScanningOptionsBuilder`, which instructs the library to scan your assemblies and discover types automatically — no manual wiring of services, handlers, validators, or endpoints.

Libraries that use assembly scanning: **MetalInjection**, **MetalNexus** (client and server), **MetalChain**, and **MetalShout**.

Choose the `ScanXxx` method that matches your project layout:

- **`ScanThisAssembly()`** — for the project that contains the registration call; the most common choice.
- **`ScanAssemblyContaining<T>()`** — for a type in a referenced project you want to include.
- **`ScanAssembliesStartingWith(...)`** — for multi-project solutions with a shared name prefix.
- **`ScanAllAssemblies()` / `ScanAllAssembliesViaFileSystem()`** — for simple setups where all types live in one assembly.

```csharp
builder.AddMetalInjection(opts => {
    opts.ScanThisAssembly();
    opts.ScanAssemblyContaining<MySharedContracts>();
});
```

### `IUsesLoggerOptionsBuilder`

| Member | Description |
|---|---|
| `UseLogger(ILoadLog?)` | Attaches an `ILoadLog` for diagnostic output during options setup |
| `DoNotUseLogger()` | Extension; disables all diagnostic output |

### `IAssemblyScanningOptionsBuilder`

| Member | Description |
|---|---|
| `ScanAssembly(Assembly)` | Adds one assembly to the scan list |
| `ScanAssemblies(params Assembly[])` | Adds multiple assemblies at once |
| `ScanThisAssembly()` | Adds the calling assembly |
| `ScanAllAssemblies()` | Discovers and adds all relevant loaded assemblies |
| `ScanAllAssembliesViaReference()` | Discovers assemblies by walking loaded assembly references |
| `ScanAllAssembliesViaFileSystem()` | Discovers assemblies by scanning the application base directory |
| `ScanAssembliesStartingWith(params string[])` | Adds only assemblies whose names match the given prefixes |
| `ScanReferencedAssembliesStartingWith(params string[])` | Discovers assemblies by walking loaded assembly references, keeping only those matching the given name prefixes |
| `ScanAssembliesInFolderStartingWith(params string[])` | Discovers assemblies by scanning the application base directory, keeping only those matching the given name prefixes |
| `ScanAssemblyContaining(params Type[])` | Adds the assemblies containing each of the supplied types; useful for including multiple referenced projects in one call |
| `DiscoveredConcreteTypes` | All non-abstract, non-interface types found across scanned assemblies |

`Assemblies.BuildList(builder?)` — Convenience factory

### `IOptionsBuilder` / `OptionsBuilder` (library authors)

When building a Metal-compatible library that exposes its own options builder, derive from `OptionsBuilder` (or implement `IOptionsBuilder` directly). The `AddServices(Action<IServiceCollection>)` method queues a service registration delegate for deferred batch application — callers can enqueue registrations during options setup, and the library applies them all at once when it configures the DI container.

```csharp
// Library author: accept a callback, enqueue registrations from user code
public class MyLibraryOptions : OptionsBuilder
{
    public void UseMyFeature() => AddServices(s => s.AddScoped<IMyFeature, MyFeature>());
}
```

---

## Load Log

The standard `ILogger` pipeline is not available during DI registration — the logging infrastructure hasn't been built yet at that point. `ILoadLog` fills that gap by providing diagnostic output *during app startup*, so library auto-registration can report what it found, skipped, or rejected.

Because `IAssemblyScanningOptionsBuilder` inherits `IUsesLoggerOptionsBuilder`, every Metal library that uses assembly scanning automatically supports `UseLogger(...)` and `DoNotUseLogger()`. The affected libraries are **MetalInjection**, **MetalNexus** (client and server), **MetalChain**, and **MetalShout**.

Three implementations are provided:

- **`ConsoleLoadLog`** — writes color-coded output to the console, one line per entry. `ConsoleLoadLog.Default` is a ready-to-use singleton configured for `LogLevel.Trace` (Debug builds) or `LogLevel.Warning` (Release builds). To customize, construct directly: `new ConsoleLoadLog(minLogLevel, traceColor, warningColor, errorColor)` — all parameters are optional and default to `DarkBlue`/`Yellow`/`Red`.
- **`ListLoadLog`** — captures all entries in memory; useful for asserting startup behavior in unit tests.
- **`ThrowExceptionOnLogError`** — wraps an optional inner log and throws `MetalCoreException` if any error (or optionally any warning) is logged; useful for fail-fast startup validation.

```csharp
builder.AddMetalInjection(opts => {
    opts.UseLogger(ConsoleLoadLog.Default); // see startup output in console
    opts.ScanThisAssembly();
});
```

For tests, use `opts.UseLogger(new ListLoadLog())` to capture entries, or `opts.DoNotUseLogger()` to suppress all output.

| Type | Description |
|---|---|
| `ILoadLog` | Diagnostic logging contract used by option builders; supports scopes and three severity levels |
| `ILoadLogExtensions` | Null-safe `LogTrace`, `LogWarning`, `LogError` extension helpers on `ILoadLog?` |
| `ConsoleLoadLog` | Writes to the console with per-level colors; `Default` is a shared pre-configured instance |
| `ListLoadLog` | Buffers all entries to an in-memory list; useful for capturing diagnostics in tests |
| `ThrowExceptionOnLogError` | Wraps an optional inner log and throws `MetalCoreException` on errors (or optionally warnings) |

`ListLoadLog.Entries` is a `List<ListLoadLog.Entry>`. Each `Entry` exposes:

| Property | Type | Description |
|---|---|---|
| `ScopeLevel` | `int` | Nesting depth at which this entry was logged (incremented by `BeginScope()`) |
| `Level` | `LogLevel` | Severity: `Trace`, `Warning`, or `Error` |
| `Message` | `string` | The log message text |

```csharp
var log = new ListLoadLog();
opts.UseLogger(log);
// ... run startup ...
Assert.Empty(log.Entries.Where(e => e.Level == LogLevel.Error));
```

---

## Utilities

### Security Tools

| Method | Description |
|---|---|
| `Hash(text)` | Generates a random salt and returns `(salt, hash)` via SHA-256 |
| `Hash(text, salt)` | Hashes a string with a given salt via SHA-256 |
| `RandomString(length)` | Generates a cryptographically random Base64 string |
| `RandomNumber(length)` | Generates a random numeric digit string of the given length |

### `ParseOrNull`

Static class. All methods accept `string?` and return `null` on parse failure.

| Method | Description |
|---|---|
| `Bool(string?)` | Parses to `bool?` |
| `DateTime(string?)` | Parses to `DateTime?` |
| `DateOnly(string?)` | Parses to `DateOnly?` |
| `Int(string?)` | Parses to `int?` |
| `Guid(string?)` | Parses to `Guid?` |
| `Double(string?)` | Parses to `double?` |

### URL & Color Tools

| Method | Description |
|---|---|
| `CombineUrl(params string[])` | Joins URL path segments, trimming slashes and skipping null/empty parts |
| `BuildQuery(url, params (name, value)[])` | Appends non-null query-string parameters to a URL |
| `GetLighterColor(hex, percent)` | Returns an `#RRGGBB` color lightened by a percentage via HSL conversion |
| `GetDesaturatedColor(hex)` | Returns an `#RRGGBB` color with reduced saturation via HSL conversion |

### Service Provider & Activation

**`MetalActivator`** is a drop-in replacement for `System.Activator` that lives in the `System.Reflection` namespace by design, so it appears alongside the BCL type in code completion. In Release builds, all construction methods are marked `[DebuggerStepThrough]`, keeping the debugger out of framework-level object creation. Use it anywhere you would use `Activator.CreateInstance(...)`.

| Type / Method | Description |
|---|---|
| `MetalActivator` (`System.Reflection`) | Drop-in `Activator` replacement supporting `ObjectHandle` and `[DebuggerStepThrough]` in Release |

### LoadGuard

`LoadGuard` prevents duplicate or concurrent async loads for the same keyed resource. If two callers request the same key simultaneously, only one load runs; the second awaits and receives the same result. An optional `ReloadAfterSeconds` parameter causes the cached value to expire and be re-fetched on the next access.

```csharp
var config = await LoadGuard.Load("appConfig",
    async () => await FetchConfigAsync(),
    ReloadAfterSeconds: 300);
```

| Type / Method | Description |
|---|---|
| `LoadGuard.Load(key, loadFunc)` | Prevents concurrent or duplicate async loads for a key; supports cache expiry via `ReloadAfterSeconds` |

### JSON Utilities

`JsonFormatter.Format(json)` pretty-prints any JSON string with consistent indentation. Handy for logging pipelines, debug display, or generating readable test fixtures.

| Type / Method | Description |
|---|---|
| `JsonFormatter.Format(string)` | Pretty-prints a JSON string with indentation |

### Exception Formatting

`ExceptionExtensions.ToBetterString()` formats the full exception chain — type name, message, all inner exceptions, and stack trace — as a readable multi-line string. More informative than `.ToString()` and well-suited for structured log entries or error reports.

| Type / Method | Description |
|---|---|
| `ExceptionExtensions.ToBetterString()` | Formats the full exception chain (type, message, stack trace, inner exceptions) as a string |

### Disposal Helpers

`OnDispose` and `OnDisposeAsync` wrap a callback as `IDisposable` / `IAsyncDisposable`, invoking it exactly once on first disposal and ignoring any subsequent `Dispose()` calls. The primary use case is **subscription cleanup** — instead of requiring callers to hold a reference and call an explicit `Unsubscribe()` method, wrap the unsubscribe logic in an `OnDispose` and return it to the caller. The caller then uses `using` to clean up automatically, with no separate unsubscription API needed:

```csharp
// Return an IDisposable that unsubscribes when disposed
public IDisposable Subscribe(Action<MyEvent> handler)
{
    _handlers.Add(handler);
    return new OnDispose(() => _handlers.Remove(handler));
}

// Caller uses 'using' — no explicit Unsubscribe needed
using var subscription = eventSource.Subscribe(e => Console.WriteLine(e));
```

| Type | Description |
|---|---|
| `OnDispose(Action)` | Wraps an `Action` as `IDisposable`; the action is called exactly once on first `Dispose()` |
| `OnDisposeAsync(Func<Task>)` | Wraps a `Func<Task>` as `IAsyncDisposable`; the function is called exactly once on first `DisposeAsync()` |

### Exception Types

Three typed exception classes are provided for common application scenarios:

| Type | Description |
|---|---|
| `NotFoundException` | Throw when a requested resource cannot be found (HTTP 404 equivalent) |
| `NotAuthorizedException` | Throw when the current user lacks permission for an operation (HTTP 403 equivalent) |
| `NotAuthenticatedException` | Throw when the current user is not authenticated (HTTP 401 equivalent) |

All three follow the standard four-constructor exception pattern: `()`, `(message)`, `(message, innerException)`, and `(innerException)`.

---

## Signing

`Ecdsa` partial static class in the `RossWright` namespace.

**ECDSA** (Elliptic Curve Digital Signature Algorithm) is a public-key cryptographic algorithm for producing and verifying digital signatures. The *signer* holds a private key; any holder of the matching *public key* can verify the signature without being able to forge one. ECDSA is asymmetric — different keys are used for signing and verifying — and produces compact signatures compared to RSA.

**Why does MetalCore include its own implementation?** The .NET BCL's `System.Security.Cryptography` stack is not fully available in Blazor WebAssembly. This pure-managed implementation allows signing and verification to run *client-side in the browser* without a server round-trip.

```csharp
// One-time: generate and store key pair
Ecdsa.GenerateKeyPair(out var privateKeyPem, out var publicKeyPem);

// Sign (keep private key secure)
var signature = Ecdsa.Sign(privateKeyPem, data);

// Verify (public key can be distributed freely)
bool isValid = Ecdsa.Verify(publicKeyPem, signature, data);
```

| Method | Description |
|---|---|
| `GenerateKeyPair(out privateKeyPem, out publicKeyPem)` | Generates a new ECDSA key pair as PEM-encoded strings |
| `Sign(privateKeyPem, data)` | Signs data with a PEM private key; returns a Base64 signature string |
| `Verify(publicKeyPem, signatureBase64, data)` | Verifies a Base64 signature against data using a PEM public key |

---

## Installation

```powershell
dotnet add package RossWright.MetalCore
```

Or add directly to your project file:

```xml
<PackageReference Include="RossWright.MetalCore" Version="*" />
```

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalCore.Data`](https://www.nuget.org/packages/RossWright.MetalCore.Data) | Entity Framework extensions, GeoCoder, database timing interceptor |
| [`RossWright.MetalCore.Blazor`](https://www.nuget.org/packages/RossWright.MetalCore.Blazor) | Blazor WASM utilities: local storage, JS script loader, host builder extensions |
| [`RossWright.MetalCore.Server`](https://www.nuget.org/packages/RossWright.MetalCore.Server) | ASP.NET Core messaging contracts, SMTP email service |
| [`RossWright.MetalCore.Populi`](https://www.nuget.org/packages/RossWright.MetalCore.Populi) | Zero-dependency static test-data generator: names, addresses, emails, coordinates, dates, prices, and lorem ipsum |

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
