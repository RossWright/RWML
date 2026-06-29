using System.Net.Http;

namespace RossWright.MetalCommand;

/// <summary>
/// Resolves the <c>IHttpClientFactory</c> key for a given environment
/// and optional connection group, and provides a convenience method to obtain an
/// <see cref="HttpClient"/> directly.
/// </summary>
/// <remarks>
/// <para>
/// Register connection groups with <c>AddHttpConnections</c> in
/// <c>RossWright.MetalCommand.Http</c>. This interface is placed in
/// <c>RossWright.MetalCommand.Abstractions</c> so it can be consumed by any library that
/// already references the abstractions assembly without introducing a new NuGet dependency —
/// <see cref="HttpClient"/> ships in the BCL (<c>System.Net.Http</c>).
/// </para>
/// <para>
/// Commands that dispatch requests via MetalNexus (<c>IMediator.Send</c>) do not need this
/// interface directly; the <c>EnvironmentAwareHttpClientFactory</c> decorator handles
/// environment routing transparently. Use <see cref="GetClientName"/> when you need to
/// pass an explicit client key to <c>SendVia</c> for multi-environment scenarios (Scenario D),
/// or when consuming <see cref="HttpClient"/> directly without MetalNexus (Scenario A).
/// </para>
/// </remarks>
public interface IHttpConnectionResolver
{
    /// <summary>
    /// Returns the <c>IHttpClientFactory</c> key for the given
    /// environment and optional base connection name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The returned key follows the internal naming convention:
    /// <list type="bullet">
    ///   <item><description>
    ///     Default (unnamed) group: <c>"MetalCommand:{env}"</c>
    ///   </description></item>
    ///   <item><description>
    ///     Named group: <c>"MetalCommand:{baseConnectionName}:{env}"</c>
    ///   </description></item>
    /// </list>
    /// Fully-qualified keys (e.g. <c>"MetalCommand:payments:prod"</c>) are never registered
    /// as base connection names, so the <c>EnvironmentAwareHttpClientFactory</c> decorator
    /// passes them through unchanged — this is the intentional bypass used in Scenario D.
    /// </para>
    /// </remarks>
    /// <param name="environment">
    /// The environment name selected by the user (e.g. <c>"local"</c>, <c>"prod"</c>).
    /// Pass <see langword="null"/> to fall back to the registered default environment for
    /// the connection group.
    /// </param>
    /// <param name="baseConnectionName">
    /// The logical connection group registered via <c>AddHttpConnections(name, cfg)</c>.
    /// Pass <see langword="null"/> or omit to use the default (unnamed) group.
    /// </param>
    /// <returns>
    /// The fully-qualified <c>IHttpClientFactory</c> key, e.g.
    /// <c>"MetalCommand:prod"</c> or <c>"MetalCommand:payments:prod"</c>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="environment"/> is not registered for the specified
    /// connection group.
    /// </exception>
    string GetClientName(string? environment = null, string? baseConnectionName = null);

    /// <summary>
    /// Convenience method: resolves the <see cref="HttpClient"/> for the given environment
    /// and optional base connection name directly.
    /// </summary>
    /// <remarks>
    /// Equivalent to calling
    /// <c>httpClientFactory.CreateClient(resolver.GetClientName(environment, baseConnectionName))</c>.
    /// </remarks>
    /// <param name="environment">
    /// The environment name selected by the user. Pass <see langword="null"/> to use the
    /// registered default environment.
    /// </param>
    /// <param name="baseConnectionName">
    /// The logical connection group name. Pass <see langword="null"/> to use the default
    /// (unnamed) group.
    /// </param>
    /// <returns>An <see cref="HttpClient"/> configured for the resolved environment.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="environment"/> is not registered for the specified
    /// connection group.
    /// </exception>
    HttpClient GetClient(string? environment = null, string? baseConnectionName = null);
}
