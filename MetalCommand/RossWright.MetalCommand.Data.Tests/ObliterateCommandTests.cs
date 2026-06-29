using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;
using NSubstitute;

namespace RossWright.MetalCommand.Data.Tests;

public class ObliterateCommandTests
{
    [Fact]
    public void Constructor_StoresFactory()
    {
        var factory = Substitute.For<IDatabaseContextFactory<TestDbContext>>();

        var command = new ObliterateCommand<TestDbContext>(factory);

        command.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_DatabaseDoesNotExist_WritesError()
    {
        var console = new TestConsole();
        // Use a file path that doesn't exist so DatabaseExists() returns false
        var nonExistentDb = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseSqlite($"DataSource={nonExistentDb}") });
        var command = new ObliterateCommand<TestDbContext>(factory) { Environment = "dev" };

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeFalse();
        console.ErrorLines.ShouldHaveSingleItem();
        console.ErrorLines[0].ShouldContain("does not exist");
    }

    [Fact]
    public void Descriptor_HasForbiddenEnvironmentPolicy()
    {
        var prop = typeof(ObliterateCommand<TestDbContext>).GetProperty(nameof(ObliterateCommand<TestDbContext>.Environment));
        var attr = prop?.GetCustomAttribute<EnvironmentArgAttribute>();

        attr.ShouldNotBeNull();
        attr!.Policy.ShouldBe(EnvironmentPolicy.Forbidden);
    }

    [Fact]
    public void Descriptor_HasExpectedInvocations()
    {
        var attr = typeof(ObliterateCommand<TestDbContext>).GetCustomAttribute<CommandAttribute>();

        attr.ShouldNotBeNull();
        attr.Invocations.ShouldContain("Obliterate");
        attr.Invocations.ShouldContain("MegaNuke");
    }

    [Fact]
    public async Task ExecuteAsync_NonRelationalProvider_ThrowsDatabaseExistsError()
    {
        var console = new TestConsole();
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("obliterate-unsupported") });
        var command = new ObliterateCommand<TestDbContext>(factory) { Environment = "dev" };

        var exception = await Should.ThrowAsync<InvalidCastException>(() => command.ExecuteAsync(console, CancellationToken.None));

        exception.Message.ShouldContain("RelationalDatabaseCreator");
    }
}
