using RossWright.MetalNexus.Internal;
using System.Text.Json;

namespace RossWright.MetalNexus.Tests.Internal;

public class MetalNexusClientOptionsBuilderTests
{
    [Fact]
    public void SetDefaultConnection_SetsDefaultConnectionNameProperty()
    {
        // Arrange
        var builder = new MetalNexusClientOptionsBuilder();
        var expectedName = "TestConnection";

        // Act
        builder.SetDefaultConnection(expectedName);

        // Assert
        builder.DefaultConnectionName.ShouldBe(expectedName);
    }

    [Fact]
    public void SetDefaultConnection_WithEmptyString_SetsToEmptyString()
    {
        // Arrange
        var builder = new MetalNexusClientOptionsBuilder();

        // Act
        builder.SetDefaultConnection(string.Empty);

        // Assert
        builder.DefaultConnectionName.ShouldBe(string.Empty);
    }

    [Fact]
    public void SetDefaultConnection_CalledMultipleTimes_OverridesPreviousValue()
    {
        // Arrange
        var builder = new MetalNexusClientOptionsBuilder();

        // Act
        builder.SetDefaultConnection("FirstConnection");
        builder.SetDefaultConnection("SecondConnection");

        // Assert
        builder.DefaultConnectionName.ShouldBe("SecondConnection");
    }

    [Fact]
    public void SetRequestBodyJsonSerializerOptions_SetsRequestBodyJsonSerializerOptionsProperty()
    {
        // Arrange
        var builder = new MetalNexusClientOptionsBuilder();
        var expectedOptions = new JsonSerializerOptions { WriteIndented = true };

        // Act
        builder.SetRequestBodyJsonSerializerOptions(expectedOptions);

        // Assert
        builder.RequestBodyJsonSerializerOptions.ShouldBe(expectedOptions);
    }

    [Fact]
    public void SetRequestBodyJsonSerializerOptions_CalledMultipleTimes_OverridesPreviousValue()
    {
        // Arrange
        var builder = new MetalNexusClientOptionsBuilder();
        var firstOptions = new JsonSerializerOptions { WriteIndented = true };
        var secondOptions = new JsonSerializerOptions { WriteIndented = false };

        // Act
        builder.SetRequestBodyJsonSerializerOptions(firstOptions);
        builder.SetRequestBodyJsonSerializerOptions(secondOptions);

        // Assert
        builder.RequestBodyJsonSerializerOptions.ShouldBe(secondOptions);
    }

    [Fact]
    public void DefaultConnectionName_HasDefaultValue()
    {
        // Arrange & Act
        var builder = new MetalNexusClientOptionsBuilder();

        // Assert
        builder.DefaultConnectionName.ShouldBe(Microsoft.Extensions.Options.Options.DefaultName);
    }

    [Fact]
    public void RequestBodyJsonSerializerOptions_HasDefaultValue()
    {
        // Arrange & Act
        var builder = new MetalNexusClientOptionsBuilder();

        // Assert
        builder.RequestBodyJsonSerializerOptions.ShouldNotBeNull();
    }
}
