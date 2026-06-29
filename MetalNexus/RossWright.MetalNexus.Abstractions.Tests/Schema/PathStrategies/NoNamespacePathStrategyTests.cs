using RossWright.MetalNexus.Schema.PathStrategies;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests;

public class NoNamespacePathStrategyTests
{
    [Fact]
    public void Trim_WithRegularClass_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(string);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithInterface_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(IDisposable);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithGenericType_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(List<int>);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithNestedType_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(Environment.SpecialFolder);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithAbstractClass_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(Stream);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithEnumType_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(DayOfWeek);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithValueType_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(int);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithArrayType_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(string[]);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithNullableValueType_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(int?);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Trim_WithDelegate_ReturnsNull()
    {
        // Arrange
        var strategy = new NoNamespacePathStrategy();
        var type = typeof(Action);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }
}
