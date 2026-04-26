using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace RossWright.MetalCommand.Http.Tests;

public class AddHttpConnectionsExtensionsTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static IConsoleApplicationBuilder Builder() =>
        ConsoleApplication.CreateBuilder();

    private sealed class TestAuthHandler : DelegatingHandler
    {
        public bool WasCreated { get; set; }

        public TestAuthHandler()
        {
            WasCreated = true;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }

    // ── Auth handler factory tests ─────────────────────────────────────────

    [Fact]
    public void AddHttpConnections_WithAuthHandlerFactory_RegistersHandler()
    {
        var handlerCreated = false;

        var app = Builder()
            .AddHttpConnections(cfg =>
            {
                cfg.Add(
                    "dev",
                    "http://localhost:5100",
                    configure: null,
                    authHandlerFactory: sp =>
                    {
                        handlerCreated = true;
                        return new TestAuthHandler();
                    });
            })
            .Build();

        using var scope = app.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();
        var client = resolver.GetClient("dev");

        client.ShouldNotBeNull();
        handlerCreated.ShouldBeTrue();
    }

    [Fact]
    public void AddHttpConnections_MultipleEnvironmentsWithAuthHandlers_RegistersBoth()
    {
        var devHandlerCreated = false;
        var prodHandlerCreated = false;

        var app = Builder()
            .AddHttpConnections(cfg =>
            {
                cfg.Add(
                    "dev",
                    "http://localhost:5100",
                    authHandlerFactory: sp =>
                    {
                        devHandlerCreated = true;
                        return new TestAuthHandler();
                    });
                cfg.Add(
                    "prod",
                    "https://api.example.com",
                    authHandlerFactory: sp =>
                    {
                        prodHandlerCreated = true;
                        return new TestAuthHandler();
                    });
            })
            .Build();

        using var scope = app.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();
        
        var devClient = resolver.GetClient("dev");
        var prodClient = resolver.GetClient("prod");

        devClient.ShouldNotBeNull();
        prodClient.ShouldNotBeNull();
        devHandlerCreated.ShouldBeTrue();
        prodHandlerCreated.ShouldBeTrue();
    }

    [Fact]
    public void AddHttpConnections_NamedConnectionWithAuthHandler_RegistersHandler()
    {
        var handlerCreated = false;

        var app = Builder()
            .AddHttpConnections("payments", cfg =>
            {
                cfg.AddDefault(
                    "dev",
                    "http://localhost:5200",
                    authHandlerFactory: sp =>
                    {
                        handlerCreated = true;
                        return new TestAuthHandler();
                    });
            })
            .Build();

        using var scope = app.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();
        var client = resolver.GetClient("dev", "payments");

        client.ShouldNotBeNull();
        handlerCreated.ShouldBeTrue();
    }

    [Fact]
    public void AddHttpConnections_AuthHandlerWithServiceProvider_ReceivesServiceProvider()
    {
        IServiceProvider? capturedServiceProvider = null;

        var app = Builder()
            .AddHttpConnections(cfg =>
            {
                cfg.Add(
                    "dev",
                    "http://localhost:5100",
                    authHandlerFactory: sp =>
                    {
                        capturedServiceProvider = sp;
                        return new TestAuthHandler();
                    });
            })
            .Build();

        using var scope = app.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();
        _ = resolver.GetClient("dev");

        capturedServiceProvider.ShouldNotBeNull();
    }

    // ── Argument validation ────────────────────────────────────────────────

    [Fact]
    public void AddHttpConnections_NullAppBuilder_ThrowsArgumentNullException()
    {
        IConsoleApplicationBuilder? nullBuilder = null;

        Should.Throw<ArgumentNullException>(() =>
            nullBuilder!.AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost")));
    }

    [Fact]
    public void AddHttpConnections_NullConfigure_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            Builder().AddHttpConnections((Action<IHttpConnectionsBuilder>)null!));
    }

    [Fact]
    public void AddHttpConnections_WithConnectionName_NullAppBuilder_ThrowsArgumentNullException()
    {
        IConsoleApplicationBuilder? nullBuilder = null;

        Should.Throw<ArgumentNullException>(() =>
            nullBuilder!.AddHttpConnections("test", cfg => cfg.AddDefault("dev", "http://localhost")));
    }

    [Fact]
    public void AddHttpConnections_WithConnectionName_NullConfigure_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            Builder().AddHttpConnections("test", null!));
    }
}
