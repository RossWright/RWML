namespace RossWright;

/// <summary>
/// Provides an alternate member name used during <c>CloneAs</c> mapping. When
/// applied to a destination member, the mapper looks for a source member with
/// the given <see cref="Alias"/> name in addition to the member's own name.
/// When applied to a source member, the mapper writes to a destination member
/// whose name matches <see cref="Alias"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class AkaAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="AkaAttribute"/> with the specified alias.
    /// </summary>
    /// <param name="alias">The alternate member name to map from or to.</param>
    public AkaAttribute(string alias) => Alias = alias;

    /// <summary>The alternate member name used for cross-type mapping.</summary>
    public readonly string Alias;
}
