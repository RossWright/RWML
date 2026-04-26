using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class AddDatabaseContextFactoryTests
{
    [Fact]
    public void AddDatabaseContextFactory_RegistersFactory_ResolvableFromServices()
    {
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
                b.Add("dev", o => o.UseInMemoryDatabase("add-factory-test")))
            .Build();

        var factory = app.Services.GetService<IDatabaseContextFactory<TestDbContext>>();

        factory.ShouldNotBeNull();
    }

    [Fact]
    public void AddDatabaseContextFactory_SingleEnvironment_DefaultIsFirstAdded()
    {
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
                b.Add("dev", o => o.UseInMemoryDatabase("default-env-test")))
            .Build();

        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DefaultEnvironment.ShouldBe("dev");
    }

    [Fact]
    public void AddDatabaseContextFactory_ExplicitDefault_SetsDefaultEnvironment()
    {
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
            {
                b.Add("dev", o => o.UseInMemoryDatabase("explicit-default-dev"));
                b.Add("prod", o => o.UseInMemoryDatabase("explicit-default-prod"), isDefault: true);
            })
            .Build();

        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DefaultEnvironment.ShouldBe("prod");
    }

    [Fact]
    public void AddDatabaseContextFactory_MultipleEnvironments_AllRegistered()
    {
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
            {
                b.Add("dev", o => o.UseInMemoryDatabase("multi-dev"));
                b.Add("staging", o => o.UseInMemoryDatabase("multi-staging"));
                b.Add("prod", o => o.UseInMemoryDatabase("multi-prod"), isProtected: true);
            })
            .Build();

        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DatabaseEnvironments.Length.ShouldBe(3);
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "dev");
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "staging");
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "prod" && e.IsProtected);
    }

    [Fact]
    public void AddDatabaseContextFactory_NoEnvironments_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            ConsoleApplication.CreateBuilder()
                .AddDatabaseContextFactory<TestDbContext>(_ => { })
                .Build());
    }

    [Fact]
    public void AddDefault_SetsDefaultFlag()
    {
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
            {
                b.Add("dev", o => o.UseInMemoryDatabase("adddefault-dev"));
                b.AddDefault("staging", o => o.UseInMemoryDatabase("adddefault-staging"));
            })
            .Build();

        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DefaultEnvironment.ShouldBe("staging");
    }

    [Fact]
    public void AddProtected_SetsProtectedFlag()
    {
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
            {
                b.Add("dev", o => o.UseInMemoryDatabase("addprotected-dev"));
                b.AddProtected("prod", o => o.UseInMemoryDatabase("addprotected-prod"));
            })
            .Build();

        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "prod" && e.IsProtected);
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "dev" && !e.IsProtected);
    }

    [Fact]
    public void AddDefaultProtected_SetsDefaultAndProtectedFlags()
    {
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
            {
                b.Add("dev", o => o.UseInMemoryDatabase("adddefaultprotected-dev"));
                b.AddDefaultProtected("prod", o => o.UseInMemoryDatabase("adddefaultprotected-prod"));
            })
            .Build();

        var factory = app.Services.GetRequiredService<IDatabaseContextFactory<TestDbContext>>();

        factory.DefaultEnvironment.ShouldBe("prod");
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "prod" && e.IsProtected);
    }
}
