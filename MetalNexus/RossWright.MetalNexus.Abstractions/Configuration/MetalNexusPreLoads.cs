using System.ComponentModel;

namespace RossWright.MetalNexus.Internal;

/// <summary>
/// Stores MetalNexus endpoint types registered before the client or server registry is created.
/// This type is hidden from IntelliSense and is not intended for direct application use.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class MetalNexusPreLoads
{
    internal MetalNexusPreLoads(Type[] types) => Types = types;

    /// <summary>The endpoint request types queued for later registry initialization.</summary>
    public Type[] Types { get; }
}
