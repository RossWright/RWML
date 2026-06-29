namespace RossWright.MetalNexus.Schema;

/// <summary>
/// The runtime registry of all MetalNexus endpoints discovered at startup.
/// </summary>
/// <remarks>
/// The registry is populated during application startup by scanning the assemblies specified
/// in the MetalNexus options builder, then sealed before the first request is served.
/// Inject this interface to enumerate or look up endpoint schema at runtime.
/// </remarks>
public interface IMetalNexusRegistry
{
    /// <summary>All registered endpoints.</summary>
    IEnumerable<IEndpoint> Endpoints { get; }

    /// <summary>
    /// Finds the endpoint registered for <paramref name="requestType"/>.
    /// </summary>
    /// <param name="requestType">The request type to look up.</param>
    /// <returns>The matching <see cref="IEndpoint"/>, or <c>null</c> if none is registered.</returns>
    IEndpoint? FindEndpoint(Type requestType);

    /// <summary>
    /// Adds one or more request types to the registry.
    /// </summary>
    /// <param name="types">The request types to register as endpoints.</param>
    /// <exception cref="InvalidOperationException">Thrown when the registry is already sealed.</exception>
    void AddEndpoints(params Type[] types);

    /// <summary>
    /// Seals the registry after the middleware snapshot is taken at startup.
    /// Calling <see cref="AddEndpoints"/> on a sealed registry throws <see cref="InvalidOperationException"/>.
    /// </summary>
    void Seal();

    /// <summary>Whether the registry has been sealed and no further endpoints can be added.</summary>
    bool IsSealed { get; }
}
