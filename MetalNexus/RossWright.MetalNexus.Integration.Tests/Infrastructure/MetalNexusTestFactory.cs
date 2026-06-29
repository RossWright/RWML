using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Integration.Tests.Infrastructure;

/// <summary>
/// Creates an in-process ASP.NET Core test server wired with the full MetalNexus
/// server pipeline.  Each test class that implements
/// <see cref="IClassFixture{MetalNexusTestFactory}"/> shares one server instance.
/// Call <see cref="CreateClient"/> to get an <see cref="HttpClient"/> backed by
/// the in-process <see cref="TestServer"/> -- no TCP sockets are used.
/// </summary>
public sealed class MetalNexusTestFactory : IAsyncLifetime
{
    private readonly Action<IMetalNexusServerOptionsBuilder>? _configureServer;
    private readonly TestAuthOptions? _authOptions;
    private readonly bool _nullifyBodySizeFeature;
    private IHost? _host;

    /// <summary>
    /// Creates a factory with default server options.
    /// This parameterless constructor is required by xUnit's class-fixture mechanism.
    /// To customise server options, instantiate the factory directly and call
    /// <see cref="InitializeAsync"/> yourself (see stack-trace test for an example).
    /// </summary>
    public MetalNexusTestFactory() { }

    internal MetalNexusTestFactory(Action<IMetalNexusServerOptionsBuilder> configureServer) =>
        _configureServer = configureServer;

    internal MetalNexusTestFactory(TestAuthOptions authOptions) =>
        _authOptions = authOptions;

    internal MetalNexusTestFactory(bool nullifyBodySizeFeature) =>
        _nullifyBodySizeFeature = nullifyBodySizeFeature;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Test",
            ContentRootPath = AppContext.BaseDirectory
        });

        // Wire the test server transport before building so that CreateClient()
        // returns an HttpClient with an in-memory handler; no real port is bound.
        builder.WebHost.UseTestServer();

        var app = TestApp.CreateApplication(builder, _configureServer, _authOptions, _nullifyBodySizeFeature);

        _host = app;
        await app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host != null)
            await _host.StopAsync();
        _host?.Dispose();
    }

    /// <summary>Returns an <see cref="HttpClient"/> that sends requests to the in-process server.</summary>
    public HttpClient CreateClient() =>
        _host!.GetTestServer().CreateClient();
}