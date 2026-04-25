using Microsoft.Extensions.DependencyInjection;

namespace RossWright;

/// <summary>
/// Base contract for Metal library options builders. Provides a mechanism to
/// queue DI service registrations for deferred batch application.
/// </summary>
public interface IOptionsBuilder
{
    /// <summary>Queues a service registration delegate to be applied when services are initialized.</summary>
    /// <param name="services">A delegate that registers one or more services on the provided collection.</param>
    void AddServices(Action<IServiceCollection> services);
}

/// <summary>
/// Base implementation of <see cref="IOptionsBuilder"/> that accumulates service
/// registration delegates and applies them in a single batch.
/// </summary>
public class OptionsBuilder : IOptionsBuilder
{
    /// <inheritdoc/>
    public void AddServices(Action<IServiceCollection> addService) => _registrations.Add(addService);
    private readonly List<Action<IServiceCollection>> _registrations = new();
    /// <summary>Applies all queued registration delegates to <paramref name="services"/>.</summary>
    /// <param name="services">The service collection to register services into.</param>
    protected void AddServices(IServiceCollection services) => services.AddServices(_registrations);
}