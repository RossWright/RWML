using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class ReloadDatabaseCommandTests
{
    private static ICommandExecutor BuildMockExecutor() => Substitute.For<ICommandExecutor>();

    [Fact]
    public async Task ExecuteAsync_ExecutesThreeSubCommandsByInvocation()
    {
        var console = new TestConsole();
        var executor = BuildMockExecutor();
        var command = new ReloadDatabaseCommand<TestDbContext>(executor) { Environment = "dev" };

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeTrue();
        await executor.Received(1).Execute("Migrate", "dev");
        await executor.Received(1).Execute("ClearData", "dev");
        await executor.Received(1).Execute("LoadData", "dev");
    }

    [Fact]
    public async Task ExecuteAsync_SubCommandsExecutedInOrder_MigrateThenClearThenLoad()
    {
        var order = new List<string>();
        var console = new TestConsole();
        var executor = BuildMockExecutor();
        executor.When(x => x.Execute(Arg.Any<string>(), Arg.Any<string[]>()))
                .Do(ci => order.Add(ci.ArgAt<string>(0)));
        var command = new ReloadDatabaseCommand<TestDbContext>(executor) { Environment = "dev" };

        await command.ExecuteAsync(console, CancellationToken.None);

        order.ShouldBe(["Migrate", "ClearData", "LoadData"]);
    }

    [Fact]
    public void Descriptor_HasForbiddenEnvironmentPolicy()
    {
        var prop = typeof(ReloadDatabaseCommand<TestDbContext>).GetProperty(nameof(ReloadDatabaseCommand<TestDbContext>.Environment));
        var attr = prop?.GetCustomAttribute<EnvironmentArgAttribute>();

        attr.ShouldNotBeNull();
        attr!.Policy.ShouldBe(EnvironmentPolicy.Forbidden);
    }

    [Fact]
    public async Task EnvironmentValue_PassedToAllSubCommands()
    {
        var console = new TestConsole();
        var executor = BuildMockExecutor();
        var command = new ReloadDatabaseCommand<TestDbContext>(executor) { Environment = "qa" };

        await command.ExecuteAsync(console, CancellationToken.None);

        await executor.Received(1).Execute("Migrate", "qa");
        await executor.Received(1).Execute("ClearData", "qa");
        await executor.Received(1).Execute("LoadData", "qa");
    }

    [Fact]
    public void Descriptor_HasExpectedInvocations()
    {
        var attr = typeof(ReloadDatabaseCommand<TestDbContext>).GetCustomAttribute<CommandAttribute>();

        attr.ShouldNotBeNull();
        attr.Invocations.ShouldContain("Reload");
        attr.Invocations.ShouldContain("Nuke");
    }

    [Fact]
    public void Constructor_ValidExecutor_CreatesInstance()
    {
        // Arrange
        var executor = BuildMockExecutor();

        // Act
        var command = new ReloadDatabaseCommand<TestDbContext>(executor);

        // Assert
        command.ShouldNotBeNull();
    }

    [Fact]
    public async Task Constructor_ValidExecutor_StoresExecutorCorrectly()
    {
        // Arrange
        var console = new TestConsole();
        var executor = BuildMockExecutor();

        // Act
        var command = new ReloadDatabaseCommand<TestDbContext>(executor) { Environment = "test" };
        await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        await executor.Received(1).Execute("Migrate", "test");
        await executor.Received(1).Execute("ClearData", "test");
        await executor.Received(1).Execute("LoadData", "test");
    }
}

