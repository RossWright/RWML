namespace RossWright.MetalInjection;

/// <summary>
/// Applied to an interface: allows multiple implementations to be registered without a
/// duplicate-registration error. Equivalent to calling
/// <c>options.AllowMultipleServicesOf(typeof(TInterface))</c> but self-documenting and
/// co-located with the type declaration.
/// </summary>
/// <remarks>
/// Suppresses the duplicate-registration check at startup; it does not suppress the
/// resolution-time ambiguity error when a single instance is requested. If multiple
/// implementations are registered, resolve via <c>IEnumerable&lt;TInterface&gt;</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
public sealed class AllowMultipleRegistrationsAttribute : Attribute { }
