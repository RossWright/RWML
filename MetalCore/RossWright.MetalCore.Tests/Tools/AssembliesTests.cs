using System.Reflection;

namespace RossWright.Tests.Tools;

public class AssembliesTests
{
    [Fact]
    public void BuildList_NullBuilder_ReturnsEmptyArray()
    {
        // Act
        var result = Assemblies.BuildList(null);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildList_NoBuilder_ReturnsEmptyArray()
    {
        // Act
        var result = Assemblies.BuildList();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildList_WithBuilder_ReturnsAssembliesAddedByBuilder()
    {
        // Arrange
        var expectedAssembly = Assembly.GetExecutingAssembly();

        // Act
        var result = Assemblies.BuildList(builder =>
        {
            builder.ScanAssembly(expectedAssembly);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(1);
        result.ShouldContain(expectedAssembly);
    }

    [Fact]
    public void BuildList_WithBuilderAddingMultipleAssemblies_ReturnsAllAssemblies()
    {
        // Arrange
        var assembly1 = Assembly.GetExecutingAssembly();
        var assembly2 = typeof(string).Assembly;

        // Act
        var result = Assemblies.BuildList(builder =>
        {
            builder.ScanAssembly(assembly1);
            builder.ScanAssembly(assembly2);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(2);
        result.ShouldContain(assembly1);
        result.ShouldContain(assembly2);
    }

    [Fact]
    public void BuildList_WithBuilderAddingNoAssemblies_ReturnsEmptyArray()
    {
        // Act
        var result = Assemblies.BuildList(builder =>
        {
            // Builder callback that does nothing
        });

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
