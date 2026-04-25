namespace RossWright.MetalInjection;

/// <summary>
/// Specifies that this configuration section object should be validated after binding during
/// <c>InitializeServices</c>. After <c>configuration.Bind</c> completes, MetalInjection calls
/// <see cref="ValidateOrDie"/> on every bound section that implements this interface, allowing
/// invalid configuration to abort application startup before any services are resolved.
/// </summary>
public interface IValidatingConfigSection
{
    /// <summary>
    /// Validates the configuration data bound to this object.
    /// Implementors are expected to throw (e.g. <see cref="InvalidOperationException"/>) when
    /// validation fails, which aborts startup and prevents the invalid section from being injected.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown (or a derived exception) by implementors when the bound configuration is invalid.
    /// </exception>
    void ValidateOrDie();
}
