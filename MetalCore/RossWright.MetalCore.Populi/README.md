# Ross Wright's Metal Core Populi Library
Copyright (c) 2023-2026 Pross Co.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Primitives](#primitives)
- [Strings & Letters](#strings--letters)
- [Selection Helpers](#selection-helpers)
- [Person & Identity](#person--identity)
- [Location](#location)
- [Numbers](#numbers)
- [Dates](#dates)
- [Text](#text)
- [See Also](#see-also)
- [License](#license)

---

## Overview

`RossWright.MetalCore.Populi` is a zero-dependency test-data generator. Every method on the static `Populi` class returns a plausible random value — names, addresses, emails, companies, dates, prices, coordinates, lorem ipsum, and more — so you can populate test databases, seed demo data, and exercise UI layouts without maintaining fixture files.

All members live in the `RossWright` namespace.

```csharp
string name    = Populi.NextName();           // "Sandra Mitchell"
string email   = Populi.NextEmail(name);      // "Sandra.Mitchell@outlook.com"
string address = Populi.NextAddress();        // "7341 Springfield Blvd"
DateTime dob   = Populi.NextBirthdate(30);    // a date ~30 years ago
double   price = Populi.NextPrice(10, 99.99); // e.g. 47.83
```

---

## Installation

```powershell
dotnet add package RossWright.MetalCore.Populi
```

```xml
<PackageReference Include="RossWright.MetalCore.Populi" Version="*" />
```

---

## Primitives

### Booleans

| Method | Description |
|---|---|
| `NextBool(pctBiasToTrue = 50)` | Returns a random `bool`. Pass a percentage (0–100) to skew the result toward `true`. |

### Integers

| Method | Description |
|---|---|
| `NextInt(max = int.MaxValue)` | Random non-negative integer less than `max`. |
| `NextInt(min, max)` | Random integer within [`min`, `max`). |

### Doubles

| Method | Description |
|---|---|
| `NextDouble(max = double.MaxValue)` | Random non-negative double less than `max`. |
| `NextDouble(min, max)` | Random double within [`min`, `max`). |

### Prices

| Method | Description |
|---|---|
| `NextPrice(max = double.MaxValue)` | Random price (two decimal places) between 0 and `max`. |
| `NextPrice(min, max)` | Random price (two decimal places) within [`min`, `max`). |

### Enums

| Method | Description |
|---|---|
| `Next<T>()` | Returns a random value from the enum type `T`. |

```csharp
var status = Populi.Next<OrderStatus>();
var bias   = Populi.NextBool(75); // 75% chance of true
var amount = Populi.NextPrice(5.00, 150.00);
```

---

## Strings & Letters

| Method | Description |
|---|---|
| `NextLetter()` | A single random ASCII letter (A–Z or a–z). |
| `NextWord()` | A single random common English word. |
| `NextWords(count)` | `count` random words joined by spaces. |

---

## Selection Helpers

Use these to pick random elements from your own collections.

| Method | Description |
|---|---|
| `OneOf<T>(IEnumerable<T>)` | Random element from any sequence. |
| `OneOf<T>(params T[])` | Random element from an inline list of values. |
| `OneOf<T>(params (T, int)[])` | Weighted random pick — each tuple is `(value, weight)`. Higher weight = higher probability. |
| `SomeOf<T>(count, params T[])` | Returns exactly `count` randomly chosen elements from the array. Returns all elements if the array is smaller than `count`. |
| `OutOf<T>(ICollection<T>)` | Random element from an `ICollection<T>`. |
| `OutOf<T>(IList<T>)` | Random element from an `IList<T>`. |
| `OutOf<T>(T[])` | Random element from an array. |

```csharp
// Uniform pick
var color = Populi.OneOf("Red", "Green", "Blue");

// Weighted pick — "Active" is 5× more likely than "Archived"
var state = Populi.OneOf(("Active", 50), ("Pending", 30), ("Archived", 10), ("Deleted", 10));

// Subset
var winners = Populi.SomeOf(3, allUsers);
```

---

## Person & Identity

| Method | Description |
|---|---|
| `NextName()` | Random full name (first + last). |
| `NextFirstName()` | Random first name (male or female). |
| `NextSurname()` | Random surname. |
| `NextEmail(name)` | Email derived from a full-name string (spaces replaced with dots). |
| `NextEmail(firstName, surname)` | Email in the form `firstName.surname@domain`. |
| `NextCompany()` | Fictitious company name (e.g., "Henderson Corp"). |

```csharp
var first   = Populi.NextFirstName();  // "Laura"
var last    = Populi.NextSurname();    // "Torres"
var email   = Populi.NextEmail(first, last); // "Laura.Torres@gmail.com"
var company = Populi.NextCompany();    // "Arizona Group Holdings"
```

---

## Location

### US Addresses & Places

| Method | Description |
|---|---|
| `NextAddress()` | Random US-style street address (e.g., "4821 Springfield Ave"). |
| `NextPlace()` | Random US place name (city or geographic name). |
| `NextFullState()` | Random US state full name (e.g., "California"). |
| `NextShortState()` | Random US state abbreviation (e.g., "CA"). |

### Coordinates

| Method | Description |
|---|---|
| `NextUsLat()` | Random latitude within the contiguous United States (~24.7° to ~49.3°). |
| `NextUsLong()` | Random longitude within the contiguous United States (~−124.8° to ~−67.0°). |

### Websites

| Method | Description |
|---|---|
| `AsWebsite(name)` | Converts a name to a plausible HTTP URL (e.g., `"Acme Corp"` → `"http://www.AcmeCorp.com"`). |

```csharp
var address = Populi.NextAddress();   // "312 Madison Dr"
var lat     = Populi.NextUsLat();     // 39.7392
var lng     = Populi.NextUsLong();    // -104.9903
var site    = Populi.AsWebsite(Populi.NextCompany()); // "http://www.NevadaInc.biz"
```

---

## Numbers

| Method | Description |
|---|---|
| `NextNumber(digits)` | Random numeric string of exactly `digits` digits (may include leading zeros). |
| `NextNumberNoLeadZero(digits)` | Random numeric string of `digits` digits with no leading zero. |
| `NextNumber(separator, params int[] digitGroups)` | Formatted numeric string with digit groups joined by `separator`. Defaults to a single 9-digit group if no groups are provided. |

```csharp
var zip   = Populi.NextNumberNoLeadZero(5); // "84032"
var phone = Populi.NextNumber("-", 3, 3, 4); // "555-248-9103"
var ssn   = Populi.NextNumber("-", 3, 2, 4); // "421-67-8810"
```

---

## Dates

| Method | Description |
|---|---|
| `NextFutureDate(maxDays = 1826)` | Random future date within `maxDays` days from today (~5 years default). |
| `NextPastDate(maxDays)` | Random past date within `maxDays` days before today. |
| `NextPastDate(minDays = 1, maxDays = 1826)` | Random past date between `minDays` and `maxDays` days before today. |
| `NextPastDateAfter(after)` | Random past date no earlier than `after`. |
| `NextBirthdate(age = 22)` | Random birthdate for a person of approximately `age` years old. |

```csharp
var renewal  = Populi.NextFutureDate(90);          // within the next 90 days
var created  = Populi.NextPastDate(30, 365);       // 1–12 months ago
var dob      = Populi.NextBirthdate(35);           // ~35 years ago
var activity = Populi.NextPastDateAfter(created);  // after the created date
```

---

## Text

| Method | Description |
|---|---|
| `LoremIpsum(words = 69)` | Lorem Ipsum placeholder string of `words` words. |
| `NextWord()` | Single random common English word. |
| `NextWords(count)` | `count` random words joined by spaces. |

```csharp
var blurb   = Populi.LoremIpsum(20);   // "Lorem ipsum dolor sit amet..."
var title   = Populi.NextWords(4);     // "mountain silver narrow bridge"
```

---

## See Also

| Package | Purpose |
|---|---|
| [`RossWright.MetalCore`](../RossWright.MetalCore/README.md) | Core extensions, utilities, options builders, load logging, exceptions, signing |
| [`RossWright.MetalCore.Data`](../RossWright.MetalCore.Data/README.md) | Entity Framework extensions, GeoCoder, database timing interceptor |
| [`RossWright.MetalCore.Blazor`](../RossWright.MetalCore.Blazor/README.md) | Blazor WebAssembly services: local storage, JS script loader |
| [`RossWright.MetalCore.Server`](../RossWright.MetalCore.Server/README.md) | ASP.NET Core messaging contracts, SMTP email, `WebApplicationBuilder` helpers |

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
