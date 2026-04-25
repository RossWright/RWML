namespace RossWright;

/// <summary>
/// Marks a property or field so that it is skipped during
/// <c>Clone</c>, <c>CloneAs</c>, and <c>CopyTo</c> mapping.
/// Apply to members that hold internal state, computed values, or references
/// that should not be shared between the source and the clone.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class IgnoreAttribute : Attribute { }
