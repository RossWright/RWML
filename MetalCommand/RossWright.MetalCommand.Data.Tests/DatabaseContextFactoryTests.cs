using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class DatabaseContextFactoryTests
{
    [Fact]
    public void GetContext_DefaultEnvironment_ReturnsContext()
    {
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("gcdefault") });

        using var ctx = factory.GetContext();

        ctx.ShouldNotBeNull();
    }

    [Fact]
    public void GetContext_NullEnvironment_FallsBackToDefault()
    {
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("gcnull") });

        using var ctx = factory.GetContext(null);

        ctx.ShouldNotBeNull();
    }

    [Fact]
    public void GetContext_NamedEnvironment_ReturnsContext()
    {
        var factory = DbContextFixture.BuildDefaultFactory();

        using var ctx = factory.GetContext("prod");

        ctx.ShouldNotBeNull();
    }

    [Fact]
    public void GetContext_UnknownEnvironment_Throws()
    {
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("gcunknown") });

        Should.Throw<InvalidOperationException>(() => factory.GetContext("staging"));
    }

    [Fact]
    public void DefaultEnvironment_MatchesSpecified()
    {
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("defaultenv") });

        factory.DefaultEnvironment.ShouldBe("dev");
    }

    [Fact]
    public void DatabaseEnvironments_ContainsAllRegistered()
    {
        var factory = DbContextFixture.BuildDefaultFactory();

        factory.DatabaseEnvironments.Length.ShouldBe(2);
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "dev");
        factory.DatabaseEnvironments.ShouldContain(e => e.Environment == "prod");
    }

    [Fact]
    public void Dispose_DisposesAllIssuedContexts()
    {
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("dispose_all") });

        var ctx1 = factory.GetContext("dev");
        var ctx2 = factory.GetContext("dev");

        factory.Dispose();

        // A disposed DbContext throws ObjectDisposedException on use
        Should.Throw<ObjectDisposedException>(() => ctx1.Items.ToList());
        Should.Throw<ObjectDisposedException>(() => ctx2.Items.ToList());
    }

    [Fact]
    public void Dispose_WhenNoContextsIssued_DoesNotThrow()
    {
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("dispose_empty") });

        Should.NotThrow(() => factory.Dispose());
    }

    [Fact]
    public void GetContext_AfterCommandDisposesContext_FactoryDisposalIsStillSafe()
    {
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("dispose_already") });

        var ctx = factory.GetContext("dev");
        ctx.Dispose(); // command author disposes manually — double-dispose must be safe

        Should.NotThrow(() => factory.Dispose());
    }

    [Fact]
    public void GetContext_CaseInsensitiveEnvironmentName_ReturnsContext()
    {
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("case_insensitive") });

        using var ctx = factory.GetContext("DEV");

        ctx.ShouldNotBeNull();
    }

    [Fact]
    public void Environments_ReturnsEntriesMatchingRegisteredDatabaseEnvironments()
    {
        var factory = DbContextFixture.BuildDefaultFactory(); // dev (unprotected) + prod (protected)

        var entries = factory.Environments;

        entries.Length.ShouldBe(2);
        entries.ShouldContain(e => e.Name == "dev"  && !e.IsProtected);
        entries.ShouldContain(e => e.Name == "prod" &&  e.IsProtected);
    }
}
