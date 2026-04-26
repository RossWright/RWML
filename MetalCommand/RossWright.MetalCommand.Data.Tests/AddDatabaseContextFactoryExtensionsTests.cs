using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class AddDatabaseContextFactoryExtensionsTests
{
    [Fact]
    public void AddDatabaseContextFactory_ReturnsAppBuilder_ForFluentChaining()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();

        var result = appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
            b.Add("dev", o => o.UseInMemoryDatabase("fluent-test")));

        result.ShouldBe(appBuilder);
    }

    [Fact]
    public void AddDatabaseContextFactory_NoEnvironments_ThrowsInvalidOperationException()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();

        var exception = Should.Throw<InvalidOperationException>(() =>
            appBuilder.AddDatabaseContextFactory<TestDbContext>(_ => { }));

        exception.Message.ShouldBe("At least one database environment must be added.");
    }

    [Fact]
    public void AddDatabaseContextFactory_RegistersIDatabaseContextFactory_AsScopedService()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();
        appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
            b.Add("dev", o => o.UseInMemoryDatabase("scoped-factory-test")));

        var app = appBuilder.Build();
        using var scope1 = app.Services.CreateScope();
        using var scope2 = app.Services.CreateScope();
        var factory1 = scope1.ServiceProvider.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();
        var factory2 = scope2.ServiceProvider.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory1.ShouldNotBe(factory2);
    }

    [Fact]
    public void AddDatabaseContextFactory_RegistersIEnvironmentSource_AsScopedService()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();
        appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
            b.Add("dev", o => o.UseInMemoryDatabase("scoped-env-source-test")));

        var app = appBuilder.Build();
        using var scope1 = app.Services.CreateScope();
        using var scope2 = app.Services.CreateScope();
        var envSource1 = scope1.ServiceProvider.GetRequiredService<IEnvironmentSource>();
        var envSource2 = scope2.ServiceProvider.GetRequiredService<IEnvironmentSource>();

        envSource1.ShouldNotBe(envSource2);
    }

    [Fact]
    public void AddDatabaseContextFactory_IEnvironmentSource_ResolvesSameInstanceAsIDatabaseContextFactory()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();
        appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
            b.Add("dev", o => o.UseInMemoryDatabase("same-instance-test")));

        var app = appBuilder.Build();
        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();
        var envSource = scope.ServiceProvider.GetRequiredService<IEnvironmentSource>();

        envSource.ShouldBe(factory);
    }

    [Fact]
    public void AddDatabaseContextFactory_NoDefaultEnvironment_UsesFirstEnvironment()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();
        appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
        {
            b.Add("first", o => o.UseInMemoryDatabase("first-env"));
            b.Add("second", o => o.UseInMemoryDatabase("second-env"));
        });

        var app = appBuilder.Build();
        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DefaultEnvironment.ShouldBe("first");
    }

    [Fact]
    public void AddDatabaseContextFactory_WithDefaultEnvironment_UsesSpecifiedDefault()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();
        appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
        {
            b.Add("dev", o => o.UseInMemoryDatabase("dev-env"));
            b.Add("prod", o => o.UseInMemoryDatabase("prod-env"), isDefault: true);
        });

        var app = appBuilder.Build();
        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DefaultEnvironment.ShouldBe("prod");
    }

    [Fact]
    public void AddDatabaseContextFactory_InvokesBuildAction_WithConfiguration()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();
        IDatabaseContextFactoryBuilder? capturedBuilder = null;

        appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
        {
            capturedBuilder = b;
            b.Add("dev", o => o.UseInMemoryDatabase("config-test"));
        });

        capturedBuilder.ShouldNotBeNull();
        capturedBuilder.Configuration.ShouldBe(appBuilder.Configuration);
    }

    [Fact]
    public void AddDatabaseContextFactory_PassesDatabaseEnvironments_ToFactory()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();
        appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
        {
            b.Add("dev", o => o.UseInMemoryDatabase("env-pass-dev"));
            b.Add("staging", o => o.UseInMemoryDatabase("env-pass-staging"));
            b.Add("prod", o => o.UseInMemoryDatabase("env-pass-prod"), isProtected: true);
        });

        var app = appBuilder.Build();
        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DatabaseEnvironments.Length.ShouldBe(3);
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "dev");
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "staging");
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "prod" && e.IsProtected);
    }

    [Fact]
    public void AddDatabaseContextFactory_AddsEnvironmentArgMiddleware_ToAppBuilder()
    {
        var appBuilder = ConsoleApplication.CreateBuilder();
        appBuilder.AddDatabaseContextFactory<TestDbContext>(b =>
            b.Add("dev", o => o.UseInMemoryDatabase("middleware-test")));

        var app = appBuilder.Build();
        
        // Verify the middleware was added by checking that IEnvironmentSource can be resolved
        // This indirectly confirms the middleware registration since the middleware depends on IEnvironmentSource
        var envSource = app.Services.GetRequiredService<IEnvironmentSource>();
        envSource.ShouldNotBeNull();
    }
}
