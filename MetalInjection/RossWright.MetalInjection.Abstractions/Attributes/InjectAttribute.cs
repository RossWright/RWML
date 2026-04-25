namespace RossWright.MetalInjection;

/// <summary>
/// Marks a property or constructor parameter for explicit injection by MetalInjection.
/// On properties: the property must have a setter (public or non-public) and its type must be
/// resolvable from the service provider at activation time.
/// On constructor parameters: an alternative to <c>[FromKeyedServices]</c> that also supports
/// the <see cref="Optional"/> flag and is consistent with the property injection attribute.
/// Required vs optional is determined by nullability unless overridden by <see cref="Optional"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class InjectAttribute : Attribute
{
    /// <summary>Initializes a new instance with an optional service key.</summary>
    /// <param name="key">The keyed-service key used to resolve this member, or <see langword="null"/> for unkeyed resolution.</param>
    public InjectAttribute(object? key = null) { Key = key; }

    /// <summary>
    /// The optional keyed-service key used to resolve this property or parameter.
    /// When <see langword="null"/> the member is resolved as an unkeyed service.
    /// </summary>
    public object? Key { get; }

    private bool _optional;
    private bool _hasOptionalOverride;

    /// <summary>
    /// Named-argument override for required/optional determination.
    /// Set <see langword="true"/> to treat missing service as optional;
    /// set <see langword="false"/> to require the service regardless of nullability.
    /// When not set the engine defers to nullability (default behaviour).
    /// </summary>
    public bool Optional
    {
        get => _optional;
        set { _optional = value; _hasOptionalOverride = true; }
    }

    /// <summary>
    /// <see langword="true"/> or <see langword="false"/> when explicitly set via <see cref="Optional"/>,
    /// <see langword="null"/> when not set (engine defers to nullability).
    /// </summary>
    public bool? EffectiveOptional => _hasOptionalOverride ? _optional : null;
}
