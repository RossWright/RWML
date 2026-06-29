using RossWright.MetalNexus.Schema;

namespace RossWright.MetalNexus;

/// <summary>
/// Configuration builder shared by both the MetalNexus client and server, providing
/// assembly scanning, endpoint schema customization, and error-handling options.
/// </summary>
public interface IMetalNexusOptionsBuilder 
    : IAssemblyScanningOptionsBuilder
{
    /// <summary>
    /// Replaces the default convention-based endpoint schema inference with a custom implementation.
    /// </summary>
    /// <param name="schema">The custom schema that overrides path, tag, protocol, and auth derivation per request type.</param>
    void UseCustomEndpointSchema(ICustomEndpointSchema schema);

    /// <summary>
    /// Configures the endpoint schema options such as path prefix, casing, and auto-protocol thresholds.
    /// </summary>
    /// <param name="config">A delegate that modifies the <see cref="IEndpointSchemaOptions"/>.</param>
    void ConfigureEndpointSchema(Action<IEndpointSchemaOptions> config);

    /// <summary>
    /// Controls whether server stack traces are included in error responses sent to clients.
    /// Defaults to <c>false</c>; enable only in development environments.
    /// </summary>
    /// <param name="include"><c>true</c> to include stack traces in error responses; otherwise <c>false</c>.</param>
    void IncludeServerStackTraceOnExceptions(bool include = true);

    /// <summary>
    /// When <c>true</c> (the default), any handler exception that is not already a known
    /// MetalNexus exception type is wrapped in an <see cref="InternalServerErrorException"/>
    /// before being serialized, ensuring the client always receives a well-formed error envelope.
    /// Set to <c>false</c> to allow unrecognised exceptions to propagate with their original type.
    /// </summary>
    /// <param name="throwIseByDefault"><c>true</c> to wrap unknown exceptions; <c>false</c> to leave them as-is.</param>
    void TreatUnknownExceptionsAsInternalServiceError(bool throwIseByDefault = true);
}