using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalInjection;
using Shouldly;
using Xunit;

namespace RossWright.MetalInjection.Abstractions.UnitTests.Attributes;

public class ScopedServiceAttributeTests
{
    [Fact]
    public void Constructor_WithType_SetsServiceInterfaceTypeAndLifetime()
    {
        // Arrange
        var serviceType = typeof(ITestService);

        // Act
        var attribute = new ScopedServiceAttribute(serviceType);

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(serviceType);
        attribute.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        attribute.Key.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithTypeAndKey_SetsAllProperties()
    {
        // Arrange
        var serviceType = typeof(ITestService);
        var key = "test-key";

        // Act
        var attribute = new ScopedServiceAttribute(serviceType, key);

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(serviceType);
        attribute.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        attribute.Key.ShouldBe(key);
    }

    [Fact]
    public void Constructor_WithTypeAndNullKey_SetsKeyToNull()
    {
        // Arrange
        var serviceType = typeof(ITestService);

        // Act
        var attribute = new ScopedServiceAttribute(serviceType, null);

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(serviceType);
        attribute.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        attribute.Key.ShouldBeNull();
    }

    [Fact]
    public void GenericConstructor_WithoutKey_SetsServiceInterfaceTypeToGenericParameter()
    {
        // Arrange & Act
        var attribute = new ScopedServiceAttribute<ITestService>();

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(typeof(ITestService));
        attribute.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        attribute.Key.ShouldBeNull();
    }

    [Fact]
    public void GenericConstructor_WithKey_SetsAllProperties()
    {
        // Arrange
        var key = "test-key";

        // Act
        var attribute = new ScopedServiceAttribute<ITestService>(key);

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(typeof(ITestService));
        attribute.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        attribute.Key.ShouldBe(key);
    }

    [Fact]
    public void GenericConstructor_WithNullKey_SetsKeyToNull()
    {
        // Arrange & Act
        var attribute = new ScopedServiceAttribute<ITestService>(null);

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(typeof(ITestService));
        attribute.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        attribute.Key.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithTypeAndIntegerKey_SetsKeyToInteger()
    {
        // Arrange
        var serviceType = typeof(ITestService);
        var key = 42;

        // Act
        var attribute = new ScopedServiceAttribute(serviceType, key);

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(serviceType);
        attribute.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        attribute.Key.ShouldBe(key);
    }

    [Fact]
    public void GenericConstructor_WithObjectKey_SetsKeyToObject()
    {
        // Arrange
        var key = new object();

        // Act
        var attribute = new ScopedServiceAttribute<ITestService>(key);

        // Assert
        attribute.ServiceInterfaceType.ShouldBe(typeof(ITestService));
        attribute.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        attribute.Key.ShouldBe(key);
    }

    [Fact]
    public void Constructor_WithDifferentTypes_SetsCorrectServiceInterfaceType()
    {
        // Arrange
        var serviceType1 = typeof(ITestService);
        var serviceType2 = typeof(string);

        // Act
        var attribute1 = new ScopedServiceAttribute(serviceType1);
        var attribute2 = new ScopedServiceAttribute(serviceType2);

        // Assert
        attribute1.ServiceInterfaceType.ShouldBe(serviceType1);
        attribute2.ServiceInterfaceType.ShouldBe(serviceType2);
    }

    [Fact]
    public void GenericConstructor_WithDifferentGenericParameters_SetsCorrectServiceInterfaceType()
    {
        // Arrange & Act
        var attribute1 = new ScopedServiceAttribute<ITestService>();
        var attribute2 = new ScopedServiceAttribute<string>();

        // Assert
        attribute1.ServiceInterfaceType.ShouldBe(typeof(ITestService));
        attribute2.ServiceInterfaceType.ShouldBe(typeof(string));
    }

    // Test helper interface
    private interface ITestService { }
}
