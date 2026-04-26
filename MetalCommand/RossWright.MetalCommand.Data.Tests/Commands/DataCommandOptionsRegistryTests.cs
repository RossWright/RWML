namespace RossWright.MetalCommand.Data.Tests.Commands;

public class DataCommandOptionsRegistryTests
{
    [Fact]
    public void GetOrCreate_WhenKeyDoesNotExist_CreatesNewOptions()
    {
        var registry = new DataCommandOptionsRegistry();
        var commandType = typeof(string);

        var result = registry.GetOrCreate<TestCommandOptions>(commandType);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestCommandOptions>();
    }

    [Fact]
    public void GetOrCreate_WhenKeyDoesNotExist_StoresOptionsInRegistry()
    {
        var registry = new DataCommandOptionsRegistry();
        var commandType = typeof(string);

        var result = registry.GetOrCreate<TestCommandOptions>(commandType);

        var retrieved = registry.Get(commandType);
        retrieved.ShouldBe(result);
    }

    [Fact]
    public void GetOrCreate_WhenKeyExistsWithSameType_ReturnsExistingInstance()
    {
        var registry = new DataCommandOptionsRegistry();
        var commandType = typeof(string);

        var first = registry.GetOrCreate<TestCommandOptions>(commandType);
        var second = registry.GetOrCreate<TestCommandOptions>(commandType);

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public void GetOrCreate_WhenKeyExistsWithDifferentType_CreatesNewOptionsAndOverwrites()
    {
        var registry = new DataCommandOptionsRegistry();
        var commandType = typeof(string);

        var first = registry.GetOrCreate<TestCommandOptions>(commandType);
        var second = registry.GetOrCreate<AnotherTestCommandOptions>(commandType);

        second.ShouldNotBeSameAs(first);
        second.ShouldBeOfType<AnotherTestCommandOptions>();
        registry.Get(commandType).ShouldBe(second);
    }

    [Fact]
    public void GetOrCreate_WithDifferentCommandTypes_CreatesSeparateInstances()
    {
        var registry = new DataCommandOptionsRegistry();
        var commandType1 = typeof(string);
        var commandType2 = typeof(int);

        var options1 = registry.GetOrCreate<TestCommandOptions>(commandType1);
        var options2 = registry.GetOrCreate<TestCommandOptions>(commandType2);

        options1.ShouldNotBeSameAs(options2);
    }

    // Test helper classes
    private class TestCommandOptions : CommandOptions
    {
    }

    private class AnotherTestCommandOptions : CommandOptions
    {
    }
}
