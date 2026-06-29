using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace RossWright.MetalInjection.Abstractions.UnitTests;

public class SingletonAttributeTests
{
    [Fact]
    public void Constructor_WithType_SetsLifetimeToSingleton()
    {
        // Arrange & Act
        var attribute = new SingletonAttribute(typeof(ITestService));

        // Assert
        attribute.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void Constructor_WithType_SetsServiceInterfaceType()
    {
        // Arrange
        var expectedType = typeof(ITestService);

        // Act
        var attribute = new SingletonAttribute(expectedType);

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(expectedType);
    }

    [Fact]
    public void Constructor_WithTypeAndNullKey_SetsKeyToNull()
    {
        // Arrange & Act
        var attribute = new SingletonAttribute(typeof(ITestService), null);

        // Assert
        attribute.Key.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithTypeAndStringKey_SetsKey()
    {
        // Arrange
        var expectedKey = "test-key";

        // Act
        var attribute = new SingletonAttribute(typeof(ITestService), expectedKey);

        // Assert
        attribute.Key.ShouldBe(expectedKey);
    }

    [Fact]
    public void Constructor_WithTypeAndIntKey_SetsKey()
    {
        // Arrange
        var expectedKey = 42;

        // Act
        var attribute = new SingletonAttribute(typeof(ITestService), expectedKey);

        // Assert
        attribute.Key.ShouldBe(expectedKey);
    }

    [Fact]
    public void Constructor_WithTypeAndObjectKey_SetsKey()
    {
        // Arrange
        var expectedKey = new CustomKey { Id = 123, Name = "test" };

        // Act
        var attribute = new SingletonAttribute(typeof(ITestService), expectedKey);

        // Assert
        attribute.Key.ShouldBe(expectedKey);
    }

    [Fact]
    public void GenericConstructor_WithNullKey_SetsLifetimeToSingleton()
    {
        // Arrange & Act
        var attribute = new SingletonAttribute<ITestService>();

        // Assert
        attribute.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void GenericConstructor_WithNullKey_SetsServiceInterfaceType()
    {
        // Arrange & Act
        var attribute = new SingletonAttribute<ITestService>();

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(typeof(ITestService));
    }

    [Fact]
    public void GenericConstructor_WithNullKey_SetsKeyToNull()
    {
        // Arrange & Act
        var attribute = new SingletonAttribute<ITestService>(null);

        // Assert
        attribute.Key.ShouldBeNull();
    }

    [Fact]
    public void GenericConstructor_WithStringKey_SetsKey()
    {
        // Arrange
        var expectedKey = "generic-key";

        // Act
        var attribute = new SingletonAttribute<ITestService>(expectedKey);

        // Assert
        attribute.Key.ShouldBe(expectedKey);
    }

    [Fact]
    public void GenericConstructor_WithIntKey_SetsKey()
    {
        // Arrange
        var expectedKey = 99;

        // Act
        var attribute = new SingletonAttribute<ITestService>(expectedKey);

        // Assert
        attribute.Key.ShouldBe(expectedKey);
    }

    [Fact]
    public void GenericConstructor_WithObjectKey_SetsKey()
    {
        // Arrange
        var expectedKey = new CustomKey { Id = 456, Name = "generic" };

        // Act
        var attribute = new SingletonAttribute<ITestService>(expectedKey);

        // Assert
        attribute.Key.ShouldBe(expectedKey);
    }

    [Fact]
    public void GenericConstructor_WithConcreteType_SetsServiceInterfaceType()
    {
        // Arrange & Act
        var attribute = new SingletonAttribute<TestService>();

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(typeof(TestService));
    }

    [Fact]
    public void GenericConstructor_WithAbstractClass_SetsServiceInterfaceType()
    {
        // Arrange & Act
        var attribute = new SingletonAttribute<AbstractTestService>();

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(typeof(AbstractTestService));
    }

    // Test helper types
    private interface ITestService { }
    
    private class TestService : ITestService { }
    
    private abstract class AbstractTestService { }

    private class CustomKey
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
