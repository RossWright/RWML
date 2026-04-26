using Microsoft.Extensions.Options;

namespace RossWright.MetalCommand.Http.Internal;

/// <summary>
/// Scoped <see cref="IHttpClientFactory"/> decorator that transparently routes
/// environment-agnostic and base-connection-name requests through
/// <see cref="IHttpConnectionResolver"/>, while passing through any already-qualified
/// or unrecognised client names to the real factory unchanged.
/// </summary>
/// <remarks>
/// <para>
/// This decorator is registered by <c>AddHttpConnections</c> and replaces the default
/// <see cref="IHttpClientFactory"/> in the DI container.  Callers — including MetalNexus
/// handlers — that call <c>IHttpClientFactory.CreateClient(name)</c> with a bare connection
/// name or an empty/null name will automatically receive a client configured for the
/// active environment.
/// </para>
/// <para>
/// Name routing rules:
/// <list type="bullet">
///   <item><description>
///     <see langword="null"/> or empty (<see cref="Options.DefaultName"/>) → resolved via
///     <see cref="IHttpConnectionResolver.GetClientName"/> for the unnamed default group.
///   </description></item>
///   <item><description>
///     A name in <see cref="HttpConnectionRegistry.RegisteredBaseNames"/> → resolved via
///     <see cref="IHttpConnectionResolver.GetClientName"/> for that named group.
///   </description></item>
///   <item><description>
///     Any other name → passed through to the real factory unchanged (e.g. fully-qualified
///     <c>"MetalCommand:prod"</c> keys or third-party clients).
///   </description></item>
/// </list>
/// </para>
/// </remarks>
internal sealed class EnvironmentAwareHttpClientFactory(
    IHttpClientFactory realFactory,
    HttpConnectionRegistry registry,
    IHttpConnectionResolver resolver)
    : IHttpClientFactory
{
    /// <inheritdoc />
    public HttpClient CreateClient(string name)
    {
        // Null or empty → unnamed default group.
        if (string.IsNullOrEmpty(name) || name == Options.DefaultName)
        {
            var qualifiedName = resolver.GetClientName(null, null);
            return realFactory.CreateClient(qualifiedName);
        }

        // Known base connection name → named group.
        if (registry.RegisteredBaseNames.Contains(name))
        {
            var qualifiedName = resolver.GetClientName(null, name);
            return realFactory.CreateClient(qualifiedName);
        }

        // Passthrough — already qualified or belongs to another factory registration.
        return realFactory.CreateClient(name);
    }
}
