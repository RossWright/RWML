using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class CommandDescriptorShapeTests
{
    private static CommandAttribute GetAttr(Type type) =>
        type.GetCustomAttribute<CommandAttribute>()
        ?? throw new InvalidOperationException($"[Command] not found on {type.Name}");

    [Fact]
    public void ClearDataCommand_Descriptor_InvocationsCorrect()
    {
        var attr = GetAttr(typeof(ClearDataCommand<TestDbContext>));

        attr.Invocations.ShouldBe(["ClearData", "cd"]);
    }

    [Fact]
    public void LoadDataCommand_Descriptor_InvocationsCorrect()
    {
        var attr = GetAttr(typeof(LoadDataCommand<TestDbContext>));

        attr.Invocations.ShouldBe(["LoadData", "ld"]);
    }

    [Fact]
    public void MigrateCommand_Descriptor_InvocationsCorrect()
    {
        var attr = GetAttr(typeof(MigrateCommand<TestDbContext>));

        attr.Invocations.ShouldBe(["Migrate"]);
    }

    [Fact]
    public void ObliterateCommand_Descriptor_InvocationsCorrect()
    {
        var attr = GetAttr(typeof(ObliterateCommand<TestDbContext>));

        attr.Invocations.ShouldBe(["Obliterate", "MegaNuke"]);
    }

    [Fact]
    public void ReloadDatabaseCommand_Descriptor_InvocationsCorrect()
    {
        var attr = GetAttr(typeof(ReloadDatabaseCommand<TestDbContext>));

        attr.Invocations.ShouldBe(["Reload", "Nuke"]);
    }

    [Fact]
    public void ClearDataCommand_HasEnvironmentArgProperty()
    {
        var prop = typeof(ClearDataCommand<TestDbContext>)
            .GetProperty("Environment");

        prop.ShouldNotBeNull();
        prop!.GetCustomAttribute<EnvironmentArgAttribute>().ShouldNotBeNull();
    }

    [Fact]
    public void MigrateCommand_HasEnvironmentArgProperty()
    {
        var prop = typeof(MigrateCommand<TestDbContext>)
            .GetProperty("Environment");

        prop.ShouldNotBeNull();
        prop!.GetCustomAttribute<EnvironmentArgAttribute>().ShouldNotBeNull();
    }
}
