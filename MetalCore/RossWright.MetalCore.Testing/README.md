# Ross Wright's Metal Core Testing Library
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [`SetDiscoveredConcreteTypesForTesting`](#setdiscoveredconcretetypesfortesting)
- [`SetupAssemblyWithTypes`](#setupassemblywithtypes)
- [See Also](#see-also)
- [License](#license)
- [Changelog](CHANGELOG.md)

---

## Overview

`RossWright.MetalCore.Testing` provides test-support utilities for code that uses `AssemblyScanningOptionsBuilder` from `RossWright.MetalCore`.

> **This package is intended for use in test projects only.** Do not reference it from production code.

It exposes two helpers:

| Helper | Description |
|---|---|
| `SetDiscoveredConcreteTypesForTesting` | Injects a fixed set of types into an `AssemblyScanningOptionsBuilder`, bypassing real assembly scanning |
| `TestHelper.SetupAssemblyWithTypes` | Creates a mock `Assembly` whose `GetTypes()` returns exactly the types you supply |

The package depends on [NSubstitute](https://nsubstitute.github.io/) to create the mock assembly.

---

## Installation

```powershell
dotnet add package RossWright.MetalCore.Testing
```

```xml
<PackageReference Include="RossWright.MetalCore.Testing" Version="*" />
```

> **Test projects only.** Add this reference to your `*.Tests.csproj`, not to your application or library project.

---

## `SetDiscoveredConcreteTypesForTesting`

`SetDiscoveredConcreteTypesForTesting` is an extension method on `AssemblyScanningOptionsBuilder`. It directly sets the `DiscoveredConcreteTypes` collection, so tests can control exactly which types are visible to the builder without triggering real assembly scanning.

```csharp
// Arrange
var builder = new MyOptionsBuilder();
builder.SetDiscoveredConcreteTypesForTesting(typeof(MyServiceA), typeof(MyServiceB));

// Act
builder.Build(services);

// Assert — only MyServiceA and MyServiceB were registered
```

---

## `SetupAssemblyWithTypes`

`TestHelper.SetupAssemblyWithTypes` creates a mock `System.Reflection.Assembly` (via NSubstitute) whose `GetTypes()` method returns exactly the types you pass. Use this to isolate assembly-scanning logic from the real loaded assemblies.

```csharp
// Arrange
var mockAssembly = TestHelper.SetupAssemblyWithTypes(typeof(MyService), typeof(MyOtherService));

// Act
var types = mockAssembly.GetTypes();

// Assert
Assert.Equal(2, types.Length);
Assert.Contains(typeof(MyService), types);
```

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalCore`](../RossWright.MetalCore/README.md) | Core extensions, utilities, options builders, load logging, exceptions, signing |
| [`RossWright.MetalCore.Data`](../RossWright.MetalCore.Data/README.md) | Entity Framework extensions, GeoCoder, database timing interceptor |
| [`RossWright.MetalCore.Blazor`](../RossWright.MetalCore.Blazor/README.md) | Blazor WebAssembly services: local storage, JS script loader |
| [`RossWright.MetalCore.Server`](../RossWright.MetalCore.Server/README.md) | ASP.NET Core messaging contracts, SMTP email, `WebApplicationBuilder` helpers |
| [`RossWright.MetalCore.Testing`](../RossWright.MetalCore.Testing/README.md) | Test-support utilities for assembly-scanning code (this package) |

---

## License

All **Ross Wright Metal Libraries** including this one are licensed under **Apache License 2.0 with Commons Clause**.

**You are free to**:
- Use the libraries in any project (personal or commercial)
- Modify them
- Include them in products or services you sell

**You may not**:
- Sell the libraries themselves (or any product/service whose *primary* value comes from the libraries)

See [LICENSE](LICENSE) for the full text.
