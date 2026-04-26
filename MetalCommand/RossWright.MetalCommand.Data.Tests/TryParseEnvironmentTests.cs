using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class TryParseEnvironmentTests
{
    private static DatabaseContextFactory<TestDbContext> SingleUnprotectedFactory() =>
        DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("dev") });

    private static DatabaseContextFactory<TestDbContext> EmptyFactory() =>
        new([], "dev");

    [Fact]
    public void ValidUnprotectedEnvironment_ReturnsEnvironmentString()
    {
        var console = new TestConsole();
        var factory = SingleUnprotectedFactory();

        var result = console.TryParseEnvironment(factory, "dev");

        result.ShouldBe("dev");
        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public void UnknownEnvironment_WritesErrorListingValidEnvs_ReturnsNull()
    {
        var console = new TestConsole();
        var factory = SingleUnprotectedFactory();

        var result = console.TryParseEnvironment(factory, "staging");

        result.ShouldBeNull();
        console.ErrorLines.ShouldHaveSingleItem();
        console.ErrorLines[0].ShouldContain("dev");
    }

    [Fact]
    public void UnknownEnvironment_NoValidEnvs_WritesNoEnvsMessage()
    {
        var console = new TestConsole();
        // a factory with only a protected env and allowProtected=false means no valid envs from the caller's perspective
        var factory = DbContextFixture.BuildFactory("prod",
            new DatabaseEnvironment { Environment = "prod", IsProtected = true, SetOptions = b => b.UseInMemoryDatabase("prod") });

        var result = console.TryParseEnvironment(factory, "staging", allowProtected: false);

        result.ShouldBeNull();
        console.ErrorLines.ShouldHaveSingleItem();
        console.ErrorLines[0].ShouldContain("no valid environments");
    }

    [Fact]
    public void ProtectedEnvironment_AllowProtectedFalse_WritesError_ReturnsNull()
    {
        var console = new TestConsole();
        var factory = DbContextFixture.BuildDefaultFactory();

        var result = console.TryParseEnvironment(factory, "prod", allowProtected: false);

        result.ShouldBeNull();
        console.ErrorLines.ShouldHaveSingleItem();
        console.ErrorLines[0].ShouldContain("cannot be used");
    }

    [Fact]
    public void ProtectedEnvironment_AllowProtectedTrue_UserConfirms_ReturnsEnv()
    {
        var console = new TestConsole("yes");
        var factory = DbContextFixture.BuildDefaultFactory();

        var result = console.TryParseEnvironment(factory, "prod", allowProtected: true);

        result.ShouldBe("prod");
        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public void ProtectedEnvironment_AllowProtectedTrue_UserDeclines_ReturnsNull()
    {
        var console = new TestConsole("no");
        var factory = DbContextFixture.BuildDefaultFactory();

        var result = console.TryParseEnvironment(factory, "prod", allowProtected: true);

        result.ShouldBeNull();
        console.ErrorLines.ShouldHaveSingleItem();
        console.ErrorLines[0].ShouldContain("aborted");
    }

    [Fact]
    public void NullEnvironment_FallsBackToDefault()
    {
        var console = new TestConsole();
        var factory = SingleUnprotectedFactory();

        var result = console.TryParseEnvironment(factory, null);

        result.ShouldBe("dev");
        console.ErrorLines.ShouldBeEmpty();
    }
}
