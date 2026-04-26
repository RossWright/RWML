using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalCommand.Http.Internal;

namespace RossWright.MetalCommand.Http;

/// <summary>
/// Extension methods on <see cref="IConsoleApplicationBuilder"/> for registering
/// environment-aware HTTP connection groups.
/// </summary>
public static class AddHttpConnectionsExtensions
{
    /// <summary>
    /// Registers an unnamed (default) HTTP connection group with per-environment entries.
    /// </summary>
    /// <param name="appBuilder">The application builder.</param>
    /// <param name="configure">
    /// Delegate that adds one or more environment entries via
    /// <see cref="IHttpConnectionsBuilder"/>.
    /// </param>
    /// <returns>The same <see cref="IConsoleApplicationBuilder"/> for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="configure"/> adds no environment entries.
    /// </exception>
    public static IConsoleApplicationBuilder AddHttpConnections(
        this IConsoleApplicationBuilder appBuilder,
        Action<IHttpConnectionsBuilder> configure) =>
        appBuilder.AddHttpConnections(string.Empty, configure);

    /// <summary>
    /// Registers a named HTTP connection group with per-environment entries.
    /// </summary>
    /// <param name="appBuilder">The application builder.</param>
    /// <param name="connectionName">
    /// The logical connection group name, used to distinguish multiple independent
    /// HTTP services (e.g. <c>"payments"</c>, <c>"notifications"</c>). Pass an empty
    /// string or use the <see cref="AddHttpConnections(IConsoleApplicationBuilder,Action{IHttpConnectionsBuilder})"/>
    /// overload for the unnamed default group.
    /// </param>
    /// <param name="configure">
    /// Delegate that adds one or more environment entries via
    /// <see cref="IHttpConnectionsBuilder"/>.
    /// </param>
    /// <returns>The same <see cref="IConsoleApplicationBuilder"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="appBuilder"/> or <paramref name="configure"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="configure"/> adds no environment entries.
    /// </exception>
    public static IConsoleApplicationBuilder AddHttpConnections(
        this IConsoleApplicationBuilder appBuilder,
        string connectionName,
        Action<IHttpConnectionsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(appBuilder);
        ArgumentNullException.ThrowIfNull(configure);

        var groupName = connectionName ?? string.Empty;
        var services = appBuilder.Services;

        // ── 1. Build entries ──────────────────────────────────────────────
        var builder = new HttpConnectionsBuilder(appBuilder.Configuration);
        configure(builder);
        if (builder.Entries.Count == 0)
            throw new InvalidOperationException(
                "At least one HTTP connection environment must be added. " +
                "Call AddDefault, Add, or AddProtected inside the configure delegate.");

        // ── 2. Register named HttpClients for each environment entry ──────
        foreach (var entry in builder.Entries)
        {
            var qualifiedName = BuildQualifiedName(groupName, entry.Environment);
            var httpClientBuilder = services.AddHttpClient(qualifiedName, client =>
            {
                client.BaseAddress = new Uri(entry.BaseAddress);
                entry.ConfigureClient?.Invoke(client);
            });

            if (entry.AuthHandlerFactory is { } authFactory)
                httpClientBuilder.AddHttpMessageHandler(sp => authFactory(sp));
        }

        // ── 3. Upsert the singleton registry ─────────────────────────────
        var registry = GetOrCreateRegistry(services);
        registry.Upsert(groupName, [.. builder.Entries]);

        // ── 4. One-time wiring: resolver, environment source, decorator ───
        //      Guard on the IHttpClientFactory replacement so this block only
        //      runs on the first AddHttpConnections call. Subsequent calls only
        //      add named clients and upsert the shared registry.
        var alreadyDecorated = services.Any(d =>
            d.ServiceType == typeof(IHttpClientFactory) &&
            d.Lifetime == ServiceLifetime.Scoped);

        if (!alreadyDecorated)
        {
            // Capture the real IHttpClientFactory singleton BEFORE replacing it.
            // BuildServiceProvider() is acceptable here: called once at startup,
            // IHttpClientFactory is a side-effect-free singleton, and the snapshot
            // must NOT be disposed because DefaultHttpClientFactory holds an internal
            // reference to the service provider for handler-scope creation.
#pragma warning disable CA2000 // Dispose objects before losing scope — intentional: see above
            var snapshot = services.BuildServiceProvider();
#pragma warning restore CA2000
            var realFactory = snapshot.GetRequiredService<IHttpClientFactory>();

            // Register the resolver with the captured real factory to avoid
            // a circular dependency (resolver → IHttpClientFactory → decorator
            // → resolver).
            services.TryAdd(ServiceDescriptor.Scoped<IHttpConnectionResolver>(sp =>
                new HttpConnectionResolver(
                    sp.GetRequiredService<HttpConnectionRegistry>(),
                    realFactory,
                    sp.GetServices<IEnvironmentSource>())));

            // Register an IEnvironmentSource backed directly by the registry so
            // EnvironmentArgMiddleware can enumerate environments for [EnvironmentArg]
            // properties. Using AddScoped (not TryAdd) so both a database and an HTTP
            // source can coexist in IEnumerable<IEnvironmentSource>.
            services.AddScoped<IEnvironmentSource>(sp =>
                new HttpConnectionEnvironmentSource(
                    sp.GetRequiredService<HttpConnectionRegistry>()));

            // Replace the singleton IHttpClientFactory with the scoped decorator.
            services.Replace(ServiceDescriptor.Scoped<IHttpClientFactory>(sp =>
                new EnvironmentAwareHttpClientFactory(
                    realFactory,
                    sp.GetRequiredService<HttpConnectionRegistry>(),
                    sp.GetRequiredService<IHttpConnectionResolver>())));
        }

        // ── 5. Ensure EnvironmentArgMiddleware is in the pipeline ─────────
        appBuilder.AddMiddleware<EnvironmentArgMiddleware>();

        return appBuilder;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    internal static string BuildQualifiedName(string groupName, string environment) =>
        string.IsNullOrEmpty(groupName)
            ? $"MetalCommand:{environment}"
            : $"MetalCommand:{groupName}:{environment}";

    private static HttpConnectionRegistry GetOrCreateRegistry(IServiceCollection services)
    {
        var existing = services
            .FirstOrDefault(d => d.ServiceType == typeof(HttpConnectionRegistry))
            ?.ImplementationInstance as HttpConnectionRegistry;

        if (existing is not null)
            return existing;

        var registry = new HttpConnectionRegistry();
        services.AddSingleton(registry);
        return registry;
    }

    // ── Inner types ────────────────────────────────────────────────────────

    /// <summary>
    /// Provides <see cref="IEnvironmentSource"/> for the default (unnamed) connection
    /// group, backed directly by the <see cref="HttpConnectionRegistry"/>. Registered
    /// separately from <see cref="HttpConnectionResolver"/> to avoid a circular DI
    /// dependency (the resolver takes <see cref="IEnumerable{IEnvironmentSource}"/>).
    /// </summary>
    private sealed class HttpConnectionEnvironmentSource(HttpConnectionRegistry registry)
        : IEnvironmentSource
    {
        public string DefaultEnvironment => registry.DefaultEnvironment(null);

        public EnvironmentEntry[] Environments =>
            registry.GetEntries(null)
                    .Select(e => new EnvironmentEntry
                    {
                        Name = e.Environment,
                        IsProtected = e.IsProtected
                    })
                    .ToArray();
    }

    private sealed class HttpConnectionsBuilder(IConfiguration configuration) : IHttpConnectionsBuilder
    {
        public IConfiguration Configuration { get; } = configuration;
        public List<HttpConnectionEntry> Entries { get; } = [];

        public void Add(
            string environment,
            string baseAddress,
            Action<HttpClient>? configure = null,
            Func<IServiceProvider, DelegatingHandler>? authHandlerFactory = null,
            bool isDefault = false,
            bool isProtected = false)
        {
            Entries.Add(new HttpConnectionEntry
            {
                Environment = environment,
                BaseAddress = baseAddress,
                ConfigureClient = configure,
                AuthHandlerFactory = authHandlerFactory,
                IsDefault = isDefault,
                IsProtected = isProtected
            });
        }
    }
}
