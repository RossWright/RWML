namespace RossWright.MetalCommand.Http;

/// <summary>
/// Holds the per-environment configuration for one logical HTTP connection group.
/// One instance is created for each environment registered via
/// <see cref="IHttpConnectionsBuilder.Add"/>.
/// </summary>
public class HttpConnectionEntry
{
    /// <summary>
    /// The environment name for this entry (e.g. <c>"local"</c>, <c>"test"</c>,
    /// <c>"prod"</c>).
    /// </summary>
    public string Environment { get; set; } = null!;

    /// <summary>
    /// When <see langword="true"/>, this entry is used when no explicit environment is
    /// supplied by the caller. The first entry added to a group is the default unless
    /// another entry is explicitly marked with <c>isDefault: true</c>.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// When <see langword="true"/>, this environment is considered protected.
    /// Protected environments may be excluded from commands that should only operate
    /// on non-production targets (e.g. destructive operations).
    /// </summary>
    public bool IsProtected { get; set; }

    /// <summary>The base URL of the HTTP service for this environment.</summary>
    public string BaseAddress { get; set; } = null!;

    /// <summary>
    /// Optional delegate invoked after <see cref="BaseAddress"/> is set to apply
    /// additional configuration to the <see cref="HttpClient"/>
    /// (e.g. adding default request headers).
    /// </summary>
    public Action<HttpClient>? ConfigureClient { get; set; }

    /// <summary>
    /// Optional factory that produces a <see cref="DelegatingHandler"/> to add to the
    /// HTTP pipeline for this environment (e.g. a Bearer-token or API-key handler).
    /// The delegate receives the <see cref="IServiceProvider"/> for the current DI scope
    /// so it can resolve scoped dependencies such as an authentication client.
    /// </summary>
    /// <remarks>
    /// When using MetalGuardian, supply
    /// <c>sp => new AuthenticationDelegatingHandler(sp.GetRequiredService&lt;IMetalGuardianAuthenticationClient&gt;(), connectionName)</c>.
    /// Any <see cref="DelegatingHandler"/> implementation is accepted here.
    /// </remarks>
    public Func<IServiceProvider, DelegatingHandler>? AuthHandlerFactory { get; set; }
}
