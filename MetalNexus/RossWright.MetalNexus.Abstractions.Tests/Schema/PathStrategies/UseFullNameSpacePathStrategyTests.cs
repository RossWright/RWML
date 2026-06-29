using RossWright.MetalNexus.Schema.PathStrategies;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies;

public class UseFullNameSpacePathStrategyTests
{
    [Fact]
    public void Trim_WithMultipleNamespaceLevels_ReturnsNamespaceAsPath()
    {
        // Arrange
        var strategy = new UseFullNameSpacePathStrategy();
        var type = typeof(UseFullNameSpacePathStrategyTests);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests/Schema/PathStrategies");
    }

    [Fact]
    public void Trim_WithTwoLevels_ReturnsSingleLevelPath()
    {
        // Arrange
        var strategy = new UseFullNameSpacePathStrategy();
        var type = typeof(System.String);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("System");
    }

    [Fact]
    public void Trim_WithNestedClass_IncludesParentClassInPath()
    {
        // Arrange
        var strategy = new UseFullNameSpacePathStrategy();
        var type = typeof(NestedClassContainer.NestedClass);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests/Schema/PathStrategies/UseFullNameSpacePathStrategyTests/NestedClassContainer");
    }

    [Fact]
    public void Trim_WithDeeplyNestedClass_IncludesFullNestingPath()
    {
        // Arrange
        var strategy = new UseFullNameSpacePathStrategy();
        var type = typeof(NestedClassContainer.NestedClass.DoublyNestedClass);

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBe("RossWright/MetalNexus/Abstractions/UnitTests/Schema/PathStrategies/UseFullNameSpacePathStrategyTests/NestedClassContainer/NestedClass");
    }

    [Fact]
    public void Trim_WithTypeInDefaultNamespace_ReturnsNull()
    {
        // Arrange
        var strategy = new UseFullNameSpacePathStrategy();
        // Create a simple type mock - we'll use a constructed generic which has no namespace
        var simpleTypeName = "SimpleType";
        var type = System.Reflection.Emit.AssemblyBuilder
            .DefineDynamicAssembly(new System.Reflection.AssemblyName("TestAssembly"), System.Reflection.Emit.AssemblyBuilderAccess.Run)
            .DefineDynamicModule("TestModule")
            .DefineType(simpleTypeName, System.Reflection.TypeAttributes.Public)
            .CreateType()!;

        // Act
        var result = strategy.Trim(type);

        // Assert
        result.ShouldBeNull();
    }

    // -- Helper Types --------------------------------------------------------------------------

    private class NestedClassContainer
    {
        internal class NestedClass
        {
            internal class DoublyNestedClass { }
        }
    }
}
