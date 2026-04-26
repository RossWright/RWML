using Microsoft.Extensions.Configuration;

namespace RossWright.MetalCommand.Http;

/// <summary>
/// Builds the per-environment configuration for one logical HTTP connection group.
/// Obtain an instance via <c>AddHttpConnections</c> on <see cref="IConsoleApplicationBuilder"/>.
/// </summary>
public interface IHttpConnectionsBuilder
{
    /// <summary>
    /// The application configuration, used by <c>ByConfigurationName</c> extension methods
    /// to resolve base addresses from <c>appsettings.json</c>.
    /// </summary>
    IConfiguration Configuration { get; }
    /// <summary>
    /// Registers an environment entry for this connection group.
    /// </summary>
    /// <param name="environment">
    /// The environment name (e.g. <c>"local"</c>, <c>"prod"</c>). Case-insensitive at
    /// resolution time.
    /// </param>
    /// <param name="baseAddress">The base URL of the HTTP service for this environment.</param>
    /// <param name="configure">
    /// Optional delegate applied to the <see cref="HttpClient"/> after
    /// <see cref="HttpClient.BaseAddress"/> is set (e.g. to add default headers).
    /// </param>
    /// <param name="authHandlerFactory">
    /// Optional factory that produces a <see cref="DelegatingHandler"/> for this
    /// environment's HTTP pipeline (e.g. a Bearer-token or API-key handler). The delegate
    /// receives the scoped <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="isDefault">
    /// When <see langword="true"/>, this environment is used when no explicit environment
    /// is supplied. If no entry is marked as default, the first registered entry is used.
    /// </param>
    /// <param name="isProtected">
    /// When <see langword="true"/>, this environment is considered protected and may be
    /// excluded from commands that restrict destructive operations.
    /// </param>
    void Add(
        string environment,
        string baseAddress,
        Action<HttpClient>? configure = null,
        Func<IServiceProvider, DelegatingHandler>? authHandlerFactory = null,
        bool isDefault = false,
        bool isProtected = false);
}

/// <summary>
/// Convenience extension methods for <see cref="IHttpConnectionsBuilder"/>.
/// </summary>
public static class HttpConnectionsBuilderExtensions
{
    /// <summary>
    /// Registers an environment entry and marks it as the default.
    /// When the user does not supply an explicit <c>--env</c> value, this environment
    /// is used.
    /// </summary>
    /// <param name="builder">The connection group builder.</param>
    /// <param name="environment">The environment name (e.g. <c>"local"</c>).</param>
    /// <param name="baseAddress">The base URL of the HTTP service.</param>
    /// <param name="configure">
    /// Optional delegate for additional <see cref="HttpClient"/> configuration.
    /// </param>
    /// <param name="authHandlerFactory">
    /// Optional factory producing a <see cref="DelegatingHandler"/> for this environment.
    /// </param>
    public static void AddDefault(
        this IHttpConnectionsBuilder builder,
        string environment,
        string baseAddress,
        Action<HttpClient>? configure = null,
        Func<IServiceProvider, DelegatingHandler>? authHandlerFactory = null) =>
        builder.Add(environment, baseAddress, configure, authHandlerFactory,
            isDefault: true, isProtected: false);

    /// <summary>
    /// Registers a standard (non-default, non-protected) environment entry.
    /// </summary>
    /// <param name="builder">The connection group builder.</param>
    /// <param name="environment">The environment name (e.g. <c>"test"</c>).</param>
    /// <param name="baseAddress">The base URL of the HTTP service.</param>
    /// <param name="configure">
    /// Optional delegate for additional <see cref="HttpClient"/> configuration.
    /// </param>
    /// <param name="authHandlerFactory">
    /// Optional factory producing a <see cref="DelegatingHandler"/> for this environment.
    /// </param>
    public static void Add(
        this IHttpConnectionsBuilder builder,
        string environment,
        string baseAddress,
        Action<HttpClient>? configure = null,
        Func<IServiceProvider, DelegatingHandler>? authHandlerFactory = null) =>
        builder.Add(environment, baseAddress, configure, authHandlerFactory,
            isDefault: false, isProtected: false);

    /// <summary>
    /// Registers an environment entry and marks it as protected.
    /// Protected environments may be excluded from commands that restrict destructive
    /// operations to non-production targets.
    /// </summary>
    /// <param name="builder">The connection group builder.</param>
    /// <param name="environment">The environment name (e.g. <c>"prod"</c>).</param>
    /// <param name="baseAddress">The base URL of the HTTP service.</param>
    /// <param name="configure">
    /// Optional delegate for additional <see cref="HttpClient"/> configuration.
    /// </param>
    /// <param name="authHandlerFactory">
    /// Optional factory producing a <see cref="DelegatingHandler"/> for this environment.
    /// </param>
    public static void AddProtected(
        this IHttpConnectionsBuilder builder,
        string environment,
        string baseAddress,
        Action<HttpClient>? configure = null,
        Func<IServiceProvider, DelegatingHandler>? authHandlerFactory = null) =>
        builder.Add(environment, baseAddress, configure, authHandlerFactory,
            isDefault: false, isProtected: true);

    // ── Configuration-name overloads ───────────────────────────────────────

    private static string ResolveBaseAddress(IHttpConnectionsBuilder builder, string configurationName)
    {
        var value = builder.Configuration[configurationName];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"HTTP base address configuration key '{configurationName}' was not found or is empty.");
        return value;
    }

    /// <summary>
    /// Registers an environment entry whose base address is read from
    /// <see cref="IHttpConnectionsBuilder.Configuration"/> under <paramref name="configurationName"/>,
    /// and marks the entry as the default.
    /// </summary>
    /// <param name="builder">The connection group builder.</param>
    /// <param name="environment">The environment name (e.g. <c>"local"</c>).</param>
    /// <param name="configurationName">
    /// The configuration key whose value is the base URL (e.g. <c>"HttpUrls:Local"</c>).
    /// </param>
    /// <param name="configure">
    /// Optional delegate for additional <see cref="HttpClient"/> configuration.
    /// </param>
    /// <param name="authHandlerFactory">
    /// Optional factory producing a <see cref="DelegatingHandler"/> for this environment.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="configurationName"/> is not found or is empty in configuration.
    /// </exception>
    public static void AddDefaultByConfigurationName(
        this IHttpConnectionsBuilder builder,
        string environment,
        string configurationName,
        Action<HttpClient>? configure = null,
        Func<IServiceProvider, DelegatingHandler>? authHandlerFactory = null) =>
        builder.Add(environment, ResolveBaseAddress(builder, configurationName),
            configure, authHandlerFactory, isDefault: true, isProtected: false);

    /// <summary>
    /// Registers a standard (non-default, non-protected) environment entry whose base
    /// address is read from <see cref="IHttpConnectionsBuilder.Configuration"/> under
    /// <paramref name="configurationName"/>.
    /// </summary>
    /// <param name="builder">The connection group builder.</param>
    /// <param name="environment">The environment name (e.g. <c>"test"</c>).</param>
    /// <param name="configurationName">
    /// The configuration key whose value is the base URL (e.g. <c>"HttpUrls:Test"</c>).
    /// </param>
    /// <param name="configure">
    /// Optional delegate for additional <see cref="HttpClient"/> configuration.
    /// </param>
    /// <param name="authHandlerFactory">
    /// Optional factory producing a <see cref="DelegatingHandler"/> for this environment.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="configurationName"/> is not found or is empty in configuration.
    /// </exception>
    public static void AddByConfigurationName(
        this IHttpConnectionsBuilder builder,
        string environment,
        string configurationName,
        Action<HttpClient>? configure = null,
        Func<IServiceProvider, DelegatingHandler>? authHandlerFactory = null) =>
        builder.Add(environment, ResolveBaseAddress(builder, configurationName),
            configure, authHandlerFactory, isDefault: false, isProtected: false);

    /// <summary>
    /// Registers a protected environment entry whose base address is read from
    /// <see cref="IHttpConnectionsBuilder.Configuration"/> under <paramref name="configurationName"/>.
    /// </summary>
    /// <param name="builder">The connection group builder.</param>
    /// <param name="environment">The environment name (e.g. <c>"prod"</c>).</param>
    /// <param name="configurationName">
    /// The configuration key whose value is the base URL (e.g. <c>"HttpUrls:Prod"</c>).
    /// </param>
    /// <param name="configure">
    /// Optional delegate for additional <see cref="HttpClient"/> configuration.
    /// </param>
    /// <param name="authHandlerFactory">
    /// Optional factory producing a <see cref="DelegatingHandler"/> for this environment.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="configurationName"/> is not found or is empty in configuration.
    /// </exception>
    public static void AddProtectedByConfigurationName(
        this IHttpConnectionsBuilder builder,
        string environment,
        string configurationName,
        Action<HttpClient>? configure = null,
        Func<IServiceProvider, DelegatingHandler>? authHandlerFactory = null) =>
        builder.Add(environment, ResolveBaseAddress(builder, configurationName),
            configure, authHandlerFactory, isDefault: false, isProtected: true);
}
