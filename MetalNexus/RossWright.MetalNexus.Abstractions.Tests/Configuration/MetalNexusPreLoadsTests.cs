using RossWright.MetalNexus.Internal;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Configuration;

public class MetalNexusPreLoadsTests
{
    [Fact]
    public void Constructor_WithTypesArray_AssignsTypesToProperty()
    {
        // Arrange
        var types = new[] { typeof(string), typeof(int) };

        // Act
        var preLoads = new MetalNexusPreLoads(types);

        // Assert
        preLoads.Types.ShouldBe(types);
    }

    [Fact]
    public void Constructor_WithEmptyArray_AssignsEmptyArrayToProperty()
    {
        // Arrange
        var types = Array.Empty<Type>();

        // Act
        var preLoads = new MetalNexusPreLoads(types);

        // Assert
        preLoads.Types.ShouldBe(types);
        preLoads.Types.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullArray_AssignsNullToProperty()
    {
        // Arrange
        Type[] types = null!;

        // Act
        var preLoads = new MetalNexusPreLoads(types);

        // Assert
        preLoads.Types.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithSingleType_AssignsSingleTypeToProperty()
    {
        // Arrange
        var types = new[] { typeof(string) };

        // Act
        var preLoads = new MetalNexusPreLoads(types);

        // Assert
        preLoads.Types.ShouldBe(types);
        preLoads.Types.Length.ShouldBe(1);
    }

    [Fact]
    public void Constructor_WithMultipleTypes_AssignsAllTypesToProperty()
    {
        // Arrange
        var types = new[] { typeof(string), typeof(int), typeof(bool), typeof(double) };

        // Act
        var preLoads = new MetalNexusPreLoads(types);

        // Assert
        preLoads.Types.ShouldBe(types);
        preLoads.Types.Length.ShouldBe(4);
    }

    [Fact]
    public void Constructor_WithSameTypeMultipleTimes_AssignsAllOccurrences()
    {
        // Arrange
        var types = new[] { typeof(string), typeof(string), typeof(string) };

        // Act
        var preLoads = new MetalNexusPreLoads(types);

        // Assert
        preLoads.Types.ShouldBe(types);
        preLoads.Types.Length.ShouldBe(3);
    }

    [Fact]
    public void Constructor_WithTypesArray_StoresSameReferenceAsProvided()
    {
        // Arrange
        var types = new[] { typeof(string), typeof(int) };

        // Act
        var preLoads = new MetalNexusPreLoads(types);

        // Assert
        ReferenceEquals(preLoads.Types, types).ShouldBeTrue();
    }
}
