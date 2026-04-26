using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Http.Internal;

namespace RossWright.MetalCommand.Http.Tests;

public class AddHttpConnectionsTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static IConsoleApplicationBuilder Builder() =>
        ConsoleApplication.CreateBuilder();

    // ── Named client registration ──────────────────────────────────────────

    [Fact]
    public void AddHttpConnections_UnnamedGroup_RegistersNamedClientInFactory()
    {
        var app = Builder()
            .AddHttpConnections(cfg =>
            {
                cfg.AddDefault("local", "http://localhost:5100");
                cfg.Add("prod", "https://api.example.com");
            })
            .Build();

        // Verify the qualified names are resolvable via the raw factory.
        // We use IHttpConnectionResolver to get the keys rather than hardcoding them.
        using var scope = app.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();

        resolver.GetClientName("local").ShouldBe("MetalCommand:local");
        resolver.GetClientName("prod").ShouldBe("MetalCommand:prod");
    }

    [Fact]
    public void AddHttpConnections_NamedGroup_RegistersQualifiedClient()
    {
        var app = Builder()
            .AddHttpConnections("payments", cfg =>
            {
                cfg.AddDefault("dev", "http://payments.dev.example.com");
                cfg.Add("prod", "https://payments.example.com");
            })
            .Build();

        using var scope = app.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();

        resolver.GetClientName("dev", "payments").ShouldBe("MetalCommand:payments:dev");
        resolver.GetClientName("prod", "payments").ShouldBe("MetalCommand:payments:prod");
    }

    // ── Validation ─────────────────────────────────────────────────────────

    [Fact]
    public void AddHttpConnections_NoEnvironments_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            Builder()
                .AddHttpConnections(_ => { })
                .Build());
    }

    // ── Service resolution ─────────────────────────────────────────────────

    [Fact]
    public void AddHttpConnections_IHttpConnectionResolver_ResolvableFromServices()
    {
        var app = Builder()
            .AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost"))
            .Build();

        using var scope = app.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetService<IHttpConnectionResolver>();

        resolver.ShouldNotBeNull();
    }

    [Fact]
    public void AddHttpConnections_IEnvironmentSource_ResolvableFromServices()
    {
        var app = Builder()
            .AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost"))
            .Build();

        using var scope = app.Services.CreateScope();
        var source = scope.ServiceProvider.GetService<IEnvironmentSource>();

        source.ShouldNotBeNull();
    }

    [Fact]
    public void AddHttpConnections_IEnvironmentSource_ExposesRegisteredEnvironments()
    {
        var app = Builder()
            .AddHttpConnections(cfg =>
            {
                cfg.AddDefault("dev", "http://localhost");
                cfg.AddProtected("prod", "https://api.example.com");
            })
            .Build();

        using var scope = app.Services.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<IEnvironmentSource>();

        source.Environments.Length.ShouldBe(2);
        source.Environments.ShouldContain(e => e.Name == "dev" && !e.IsProtected);
        source.Environments.ShouldContain(e => e.Name == "prod" && e.IsProtected);
    }

    [Fact]
    public void AddHttpConnections_IEnvironmentSource_DefaultEnvironmentMatchesMarkedDefault()
    {
        var app = Builder()
            .AddHttpConnections(cfg =>
            {
                cfg.Add("dev", "http://localhost");
                cfg.AddDefault("staging", "http://staging.example.com");
            })
            .Build();

        using var scope = app.Services.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<IEnvironmentSource>();

        source.DefaultEnvironment.ShouldBe("staging");
    }

    [Fact]
    public void AddHttpConnections_IHttpConnectionResolver_RegisteredAsSingleton_SingleInstance()
    {
        var app = Builder()
            .AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost"))
            .Build();

        using var scope1 = app.Services.CreateScope();
        using var scope2 = app.Services.CreateScope();

        var resolver1a = scope1.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();
        var resolver1b = scope1.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();
        var resolver2 = scope2.ServiceProvider.GetRequiredService<IHttpConnectionResolver>();

        // IHttpConnectionResolver is scoped: same instance within a scope, different across scopes.
        resolver1a.ShouldBeSameAs(resolver1b);
        resolver1a.ShouldNotBeSameAs(resolver2);
    }

    // ── Multiple groups ────────────────────────────────────────────────────

    [Fact]
    public void AddHttpConnections_MultipleGroups_BothRegistered()
    {
        var app = Builder()
            .AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost"))
            .AddHttpConnections("payments", cfg => cfg.AddDefault("dev", "http://payments.dev"))
            .Build();

        using var scope = app.Services.CreateScope();
        var registry = scope.ServiceProvider.GetRequiredService<HttpConnectionRegistry>();

        registry.GetEntries(string.Empty).ShouldNotBeEmpty();
        registry.GetEntries("payments").ShouldNotBeEmpty();
    }

    // ── Decorator ──────────────────────────────────────────────────────────

    [Fact]
    public void AddHttpConnections_IHttpClientFactory_IsDecorated()
    {
        var app = Builder()
            .AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost"))
            .Build();

        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        factory.ShouldBeOfType<EnvironmentAwareHttpClientFactory>();
    }

    [Fact]
    public void AddHttpConnections_IHttpClientFactory_IsScoped()
    {
        var app = Builder()
            .AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost"))
            .Build();

        using var scope1 = app.Services.CreateScope();
        using var scope2 = app.Services.CreateScope();

        var f1 = scope1.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var f2 = scope1.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var f3 = scope2.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        // Same scope → same instance; different scope → different instance.
        f1.ShouldBeSameAs(f2);
        f1.ShouldNotBeSameAs(f3);
    }

    // ── HttpConnectionRegistry helpers ─────────────────────────────────────

    [Fact]
    public void AddHttpConnections_Registry_RegisteredBaseNamesContainsDefaultGroup()
    {
        var app = Builder()
            .AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost"))
            .Build();

        var registry = app.Services.GetRequiredService<HttpConnectionRegistry>();

        registry.RegisteredBaseNames.ShouldContain(string.Empty);
    }

    [Fact]
    public void AddHttpConnections_Registry_RegisteredBaseNamesContainsNamedGroup()
    {
        var app = Builder()
            .AddHttpConnections("payments", cfg => cfg.AddDefault("dev", "http://localhost"))
            .Build();

        var registry = app.Services.GetRequiredService<HttpConnectionRegistry>();

        registry.RegisteredBaseNames.ShouldContain("payments");
    }

    // ── BaseAddress configuration ──────────────────────────────────────────

    [Fact]
    public void AddHttpConnections_BaseAddress_AppliedToNamedClient()
    {
        var app = Builder()
            .AddHttpConnections(cfg => cfg.AddDefault("dev", "http://localhost:5100"))
            .Build();

        using var scope = app.Services.CreateScope();
        // GetClient uses the real (snapshot) factory directly, which has the base address.
        var client = scope.ServiceProvider.GetRequiredService<IHttpConnectionResolver>()
                                          .GetClient("dev");

        client.BaseAddress.ShouldBe(new Uri("http://localhost:5100"));
    }
}
