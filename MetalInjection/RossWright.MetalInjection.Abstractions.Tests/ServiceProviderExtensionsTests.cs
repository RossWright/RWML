using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalInjection;
using Shouldly;

namespace RossWright.MetalInjection.Abstractions.UnitTests;

public class ServiceProviderExtensionsTests
{
    [Fact]
    public void InjectProperties_WithMetalInjectionServiceProvider_CallsInjectPropertiesOnProvider()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();
        var testObject = new TestClass();

        // Act
        var result = ServiceProviderExtensions.InjectProperties(metalProvider, testObject);

        // Assert
        result.ShouldBe(testObject);
        metalProvider.Received(1).InjectProperties(testObject);
    }

    [Fact]
    public void InjectProperties_WithRegularServiceProvider_ReturnsObjectUnchanged()
    {
        // Arrange
        var provider = Substitute.For<IServiceProvider>();
        var testObject = new TestClass();

        // Act
        var result = ServiceProviderExtensions.InjectProperties(provider, testObject);

        // Assert
        result.ShouldBe(testObject);
    }

    [Fact]
    public void CreateInstance_NonGeneric_CreatesInstanceAndInjectsProperties()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();
        var dependency = new TestDependency();
        metalProvider.GetService(typeof(IDependency)).Returns(dependency);

        // Act
        var result = ServiceProviderExtensions.CreateInstance(metalProvider, typeof(ClassWithDependency));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ClassWithDependency>();
        metalProvider.Received(1).InjectProperties(Arg.Any<object>());
    }

    [Fact]
    public void CreateInstance_NonGeneric_WithParameters_CreatesInstanceAndInjectsProperties()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();
        var testParam = "test parameter";

        // Act
        var result = ServiceProviderExtensions.CreateInstance(metalProvider, typeof(ClassWithParameter), testParam);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ClassWithParameter>();
        ((ClassWithParameter)result).Parameter.ShouldBe(testParam);
        metalProvider.Received(1).InjectProperties(Arg.Any<object>());
    }

    [Fact]
    public void CreateInstance_NonGeneric_WithRegularServiceProvider_CreatesInstanceWithoutInjection()
    {
        // Arrange
        var provider = Substitute.For<IServiceProvider>();
        var testParam = "test parameter";

        // Act
        var result = ServiceProviderExtensions.CreateInstance(provider, typeof(ClassWithParameter), testParam);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ClassWithParameter>();
        ((ClassWithParameter)result).Parameter.ShouldBe(testParam);
    }

    [Fact]
    public void CreateInstance_Generic_CreatesInstanceAndInjectsProperties()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();
        var dependency = new TestDependency();
        metalProvider.GetService(typeof(IDependency)).Returns(dependency);

        // Act
        var result = ServiceProviderExtensions.CreateInstance<ClassWithDependency>(metalProvider);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ClassWithDependency>();
        metalProvider.Received(1).InjectProperties(Arg.Any<object>());
    }

    [Fact]
    public void CreateInstance_Generic_WithParameters_CreatesInstanceAndInjectsProperties()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();
        var testParam = "test parameter";

        // Act
        var result = ServiceProviderExtensions.CreateInstance<ClassWithParameter>(metalProvider, testParam);

        // Assert
        result.ShouldNotBeNull();
        result.Parameter.ShouldBe(testParam);
        metalProvider.Received(1).InjectProperties(Arg.Any<object>());
    }

    [Fact]
    public void CreateInstance_Generic_WithRegularServiceProvider_CreatesInstanceWithoutInjection()
    {
        // Arrange
        var provider = Substitute.For<IServiceProvider>();
        var testParam = "test parameter";

        // Act
        var result = ServiceProviderExtensions.CreateInstance<ClassWithParameter>(provider, testParam);

        // Assert
        result.ShouldNotBeNull();
        result.Parameter.ShouldBe(testParam);
    }

    [Fact]
    public void InjectProperties_ReturnsSameObjectReference()
    {
        // Arrange
        var provider = Substitute.For<IServiceProvider>();
        var testObject = new TestClass();

        // Act
        var result = ServiceProviderExtensions.InjectProperties(provider, testObject);

        // Assert
        ReferenceEquals(result, testObject).ShouldBeTrue();
    }

    [Fact]
    public void CreateInstance_NonGeneric_WithNoParameters_CreatesInstance()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();

        // Act
        var result = ServiceProviderExtensions.CreateInstance(metalProvider, typeof(ClassWithNoParameters));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ClassWithNoParameters>();
        metalProvider.Received(1).InjectProperties(Arg.Any<object>());
    }

    [Fact]
    public void CreateInstance_Generic_WithNoParameters_CreatesInstance()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();

        // Act
        var result = ServiceProviderExtensions.CreateInstance<ClassWithNoParameters>(metalProvider);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ClassWithNoParameters>();
        metalProvider.Received(1).InjectProperties(Arg.Any<object>());
    }

    [Fact]
    public void CreateInstance_NonGeneric_WithMultipleParameters_CreatesInstance()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();
        var param1 = "test";
        var param2 = 42;

        // Act
        var result = ServiceProviderExtensions.CreateInstance(metalProvider, typeof(ClassWithMultipleParameters), param1, param2);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ClassWithMultipleParameters>();
        var typed = (ClassWithMultipleParameters)result;
        typed.Param1.ShouldBe(param1);
        typed.Param2.ShouldBe(param2);
        metalProvider.Received(1).InjectProperties(Arg.Any<object>());
    }

    [Fact]
    public void CreateInstance_Generic_WithMultipleParameters_CreatesInstance()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();
        var param1 = "test";
        var param2 = 42;

        // Act
        var result = ServiceProviderExtensions.CreateInstance<ClassWithMultipleParameters>(metalProvider, param1, param2);

        // Assert
        result.ShouldNotBeNull();
        result.Param1.ShouldBe(param1);
        result.Param2.ShouldBe(param2);
        metalProvider.Received(1).InjectProperties(Arg.Any<object>());
    }

    [Fact]
    public void InjectProperties_WithMetalInjectionServiceProvider_ReturnsSameObjectReference()
    {
        // Arrange
        var metalProvider = Substitute.For<IMetalInjectionServiceProvider>();
        var testObject = new TestClass();

        // Act
        var result = ServiceProviderExtensions.InjectProperties(metalProvider, testObject);

        // Assert
        ReferenceEquals(result, testObject).ShouldBeTrue();
        metalProvider.Received(1).InjectProperties(testObject);
    }

    private class TestClass
    {
    }

    private class ClassWithNoParameters
    {
    }

    private interface IDependency
    {
    }

    private class TestDependency : IDependency
    {
    }

    private class ClassWithDependency
    {
        public ClassWithDependency(IDependency dependency)
        {
            Dependency = dependency;
        }

        public IDependency Dependency { get; }
    }

    private class ClassWithParameter
    {
        public ClassWithParameter(string parameter)
        {
            Parameter = parameter;
        }

        public string Parameter { get; }
    }

    private class ClassWithMultipleParameters
    {
        public ClassWithMultipleParameters(string param1, int param2)
        {
            Param1 = param1;
            Param2 = param2;
        }

        public string Param1 { get; }
        public int Param2 { get; }
    }
}
