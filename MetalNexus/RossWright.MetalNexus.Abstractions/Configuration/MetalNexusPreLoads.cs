using System.ComponentModel;

namespace RossWright.MetalNexus.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class MetalNexusPreLoads
{
    internal MetalNexusPreLoads(Type[] types) => Types = types;
    public Type[] Types { get; }
}
