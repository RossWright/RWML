# MetalCore API Index

Primary namespace: `RossWright`.
Messaging namespace: `RossWright.Messaging`.

## RossWright.StringExtensions.CalcLevenshteinDistanceTo

Package: `RossWright.MetalCore`  
Namespace: `RossWright`  
Signature: `public static double CalcLevenshteinDistanceTo(this string? s1, string? s2)`  
Summary: Calculates a normalized Levenshtein similarity score after trimming, lowercasing, and removing spaces.  
Example: `"kitten".CalcLevenshteinDistanceTo("sitting")`

## RossWright.StringExtensions.SpaceOut

Package: `RossWright.MetalCore`  
Namespace: `RossWright`  
Signature: `public static string SpaceOut(this string text)`  
Summary: Converts PascalCase, camelCase, and Snake_Case text to space-separated words.  
Example: `"PascalCase".SpaceOut()`

## RossWright.QueryableExtensions.WhereIf

Package: `RossWright.MetalCore`  
Namespace: `RossWright`  
Signature: `public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate, Expression<Func<T, bool>>? elsePredicate = null)`  
Summary: Applies a LINQ filter only when a condition is true, with an optional else filter.

## RossWright.CloneExtensions.CloneAs

Package: `RossWright.MetalCore`  
Namespace: `RossWright`  
Signature: `public static TOut CloneAs<TOut>(this object input)`  
Summary: Creates a new object and copies compatible properties from the source object.

## RossWright.CloneExtensions.CopyTo

Package: `RossWright.MetalCore`  
Namespace: `RossWright`  
Signature: `public static void CopyTo(this object input, object output)`  
Summary: Copies compatible values from one object to another.

## RossWright.IValidatable

Package: `RossWright.MetalCore`  
Namespace: `RossWright`  
Summary: Contract for models that can validate themselves with MetalCore validation helpers.

## RossWright.ValidationExtensions.AssertValid

Package: `RossWright.MetalCore`  
Namespace: `RossWright`  
Summary: Throws when an `IValidatable` model is invalid.

## RossWright.Messaging.IEmailService

Package: `RossWright.MetalCore.Server`  
Namespace: `RossWright.Messaging`  
Summary: Email service abstraction used by server-side messaging helpers.

## RossWright.IBrowserLocalStorage

Package: `RossWright.MetalCore.Blazor`  
Namespace: `RossWright`  
Summary: Blazor WebAssembly browser local storage abstraction.
