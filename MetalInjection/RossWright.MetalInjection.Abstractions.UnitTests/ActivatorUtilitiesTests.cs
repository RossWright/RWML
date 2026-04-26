using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalInjection;
using Shouldly;

namespace RossWright.MetalInjection.Abstractions.UnitTests;

public class ActivatorUtilitiesTests
{
    // ── IMetalInjectionServiceProvider.InjectProperties Tests ────────────────────────────────

    [Fact]
    public void InjectProperties_WithNull_DoesNotThrow()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();

        // Act & Assert
        Should.NotThrow(() => provider.InjectProperties(null));
    }

    [Fact]
    public void InjectProperties_WithObject_CallsImplementation()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var testObject = new TestClass();

        // Act
        provider.InjectProperties(testObject);

        // Assert
        provider.Received(1).InjectProperties(testObject);
    }

    // ── ActivatorUtilities.CreateInstance(IServiceProvider, Type, params object[]) Tests ─────

    [Fact]
    public void CreateInstance_WithType_CreatesInstance()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        provider.GetService(typeof(ITestService)).Returns(new TestService());

        // Act
        var result = ActivatorUtilities.CreateInstance(provider, typeof(TestClass), Array.Empty<object>());

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestClass>();
        provider.Received(1).InjectProperties(Arg.Any<TestClass>());
    }

    [Fact]
    public void CreateInstance_WithTypeAndParameters_CreatesInstanceWithParameters()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();

        // Act
        var result = ActivatorUtilities.CreateInstance(provider, typeof(TestClassWithParameter), 42);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestClassWithParameter>();
        ((TestClassWithParameter)result).Value.ShouldBe(42);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithParameter>());
    }

    [Fact]
    public void CreateInstance_WithTypeAndServiceDependency_ResolvesFromProvider()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var testService = new TestService();
        provider.GetService(typeof(ITestService)).Returns(testService);

        // Act
        var result = ActivatorUtilities.CreateInstance(provider, typeof(TestClassWithService), Array.Empty<object>());

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestClassWithService>();
        ((TestClassWithService)result).Service.ShouldBe(testService);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithService>());
    }

    // ── ActivatorUtilities.CreateFactory(Type, Type[]) Tests ─────────────────────────────────

    [Fact]
    public void CreateFactory_WithNoArgumentTypes_CreatesFactoryDelegate()
    {
        // Arrange & Act
        var factory = ActivatorUtilities.CreateFactory(typeof(TestClass), Type.EmptyTypes);

        // Assert
        factory.ShouldNotBeNull();
    }

    [Fact]
    public void CreateFactory_WithArgumentTypes_CreatesFactoryDelegate()
    {
        // Arrange & Act
        var factory = ActivatorUtilities.CreateFactory(typeof(TestClassWithParameter), new[] { typeof(int) });

        // Assert
        factory.ShouldNotBeNull();
    }

    [Fact]
    public void CreateFactory_InvokedFactory_CreatesInstanceAndInjectsProperties()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var factory = ActivatorUtilities.CreateFactory(typeof(TestClassWithParameter), new[] { typeof(int) });

        // Act
        var result = factory(provider, new object[] { 42 });

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestClassWithParameter>();
        ((TestClassWithParameter)result).Value.ShouldBe(42);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithParameter>());
    }

    // ── ActivatorUtilities.CreateFactory<T>(Type[]) Tests ────────────────────────────────────

    [Fact]
    public void CreateFactory_Generic_WithNoArgumentTypes_CreatesFactoryDelegate()
    {
        // Arrange & Act
        var factory = ActivatorUtilities.CreateFactory<TestClass>(Type.EmptyTypes);

        // Assert
        factory.ShouldNotBeNull();
    }

    [Fact]
    public void CreateFactory_Generic_WithArgumentTypes_CreatesFactoryDelegate()
    {
        // Arrange & Act
        var factory = ActivatorUtilities.CreateFactory<TestClassWithParameter>(new[] { typeof(int) });

        // Assert
        factory.ShouldNotBeNull();
    }

    [Fact]
    public void CreateFactory_Generic_InvokedFactory_CreatesInstanceAndInjectsProperties()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var factory = ActivatorUtilities.CreateFactory<TestClassWithParameter>(new[] { typeof(int) });

        // Act
        var result = factory(provider, new object[] { 42 });

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(42);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithParameter>());
    }

    // ── ActivatorUtilities.CreateInstance<T>(IServiceProvider, params object[]) Tests ────────

    [Fact]
    public void CreateInstance_Generic_CreatesInstance()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();

        // Act
        var result = ActivatorUtilities.CreateInstance<TestClass>(provider);

        // Assert
        result.ShouldNotBeNull();
        provider.Received(1).InjectProperties(Arg.Any<TestClass>());
    }

    [Fact]
    public void CreateInstance_Generic_WithParameters_CreatesInstanceWithParameters()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();

        // Act
        var result = ActivatorUtilities.CreateInstance<TestClassWithParameter>(provider, 42);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(42);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithParameter>());
    }

    [Fact]
    public void CreateInstance_Generic_WithServiceDependency_ResolvesFromProvider()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var testService = new TestService();
        provider.GetService(typeof(ITestService)).Returns(testService);

        // Act
        var result = ActivatorUtilities.CreateInstance<TestClassWithService>(provider);

        // Assert
        result.ShouldNotBeNull();
        result.Service.ShouldBe(testService);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithService>());
    }

    [Fact]
    public void CreateInstance_Generic_WithMultipleParameters_CreatesInstanceWithAllParameters()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();

        // Act
        var result = ActivatorUtilities.CreateInstance<TestClassWithMultipleParameters>(provider, 42, "test");

        // Assert
        result.ShouldNotBeNull();
        result.IntValue.ShouldBe(42);
        result.StringValue.ShouldBe("test");
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithMultipleParameters>());
    }

    [Fact]
    public void CreateFactory_WithServiceDependencyInType_FactoryResolvesService()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var testService = new TestService();
        provider.GetService(typeof(ITestService)).Returns(testService);
        var factory = ActivatorUtilities.CreateFactory(typeof(TestClassWithService), Type.EmptyTypes);

        // Act
        var result = factory(provider, null);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestClassWithService>();
        ((TestClassWithService)result).Service.ShouldBe(testService);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithService>());
    }

    [Fact]
    public void CreateFactory_Generic_WithServiceDependencyInType_FactoryResolvesService()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var testService = new TestService();
        provider.GetService(typeof(ITestService)).Returns(testService);
        var factory = ActivatorUtilities.CreateFactory<TestClassWithService>(Type.EmptyTypes);

        // Act
        var result = factory(provider, null);

        // Assert
        result.ShouldNotBeNull();
        result.Service.ShouldBe(testService);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithService>());
    }

    // ── ActivatorUtilities.GetServiceOrCreateInstance<T>(IServiceProvider) Tests ─────────────

    [Fact]
    public void GetServiceOrCreateInstance_Generic_WhenServiceExists_ReturnsServiceAndInjectsProperties()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var existingService = new TestClass();
        provider.GetService(typeof(TestClass)).Returns(existingService);

        // Act
        var result = ActivatorUtilities.GetServiceOrCreateInstance<TestClass>(provider);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(existingService);
        provider.Received(1).InjectProperties(existingService);
    }

    [Fact]
    public void GetServiceOrCreateInstance_Generic_WhenServiceDoesNotExist_CreatesInstanceAndInjectsProperties()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        provider.GetService(typeof(TestClass)).Returns((TestClass?)null);

        // Act
        var result = ActivatorUtilities.GetServiceOrCreateInstance<TestClass>(provider);

        // Assert
        result.ShouldNotBeNull();
        provider.Received(1).InjectProperties(Arg.Any<TestClass>());
    }

    [Fact]
    public void GetServiceOrCreateInstance_Generic_WithServiceDependency_ResolvesFromProvider()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var testService = new TestService();
        provider.GetService(typeof(ITestService)).Returns(testService);
        provider.GetService(typeof(TestClassWithService)).Returns((TestClassWithService?)null);

        // Act
        var result = ActivatorUtilities.GetServiceOrCreateInstance<TestClassWithService>(provider);

        // Assert
        result.ShouldNotBeNull();
        result.Service.ShouldBe(testService);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithService>());
    }

    // ── ActivatorUtilities.GetServiceOrCreateInstance(IServiceProvider, Type) Tests ──────────

    [Fact]
    public void GetServiceOrCreateInstance_WhenServiceExists_ReturnsServiceAndInjectsProperties()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var existingService = new TestClass();
        provider.GetService(typeof(TestClass)).Returns(existingService);

        // Act
        var result = ActivatorUtilities.GetServiceOrCreateInstance(provider, typeof(TestClass));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(existingService);
        provider.Received(1).InjectProperties(existingService);
    }

    [Fact]
    public void GetServiceOrCreateInstance_WhenServiceDoesNotExist_CreatesInstanceAndInjectsProperties()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        provider.GetService(typeof(TestClass)).Returns(null);

        // Act
        var result = ActivatorUtilities.GetServiceOrCreateInstance(provider, typeof(TestClass));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestClass>();
        provider.Received(1).InjectProperties(Arg.Any<TestClass>());
    }

    [Fact]
    public void GetServiceOrCreateInstance_WithServiceDependency_ResolvesFromProvider()
    {
        // Arrange
        var provider = Substitute.For<IMetalInjectionServiceProvider>();
        var testService = new TestService();
        provider.GetService(typeof(ITestService)).Returns(testService);
        provider.GetService(typeof(TestClassWithService)).Returns(null);

        // Act
        var result = ActivatorUtilities.GetServiceOrCreateInstance(provider, typeof(TestClassWithService));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestClassWithService>();
        ((TestClassWithService)result).Service.ShouldBe(testService);
        provider.Received(1).InjectProperties(Arg.Any<TestClassWithService>());
    }

    // ── Helper Types ─────────────────────────────────────────────────────────────────────────

    private interface ITestService
    {
    }

    private class TestService : ITestService
    {
    }

    private class TestClass
    {
    }

    private class TestClassWithParameter
    {
        public int Value { get; }

        public TestClassWithParameter(int value)
        {
            Value = value;
        }
    }

    private class TestClassWithService
    {
        public ITestService Service { get; }

        public TestClassWithService(ITestService service)
        {
            Service = service;
        }
    }

    private class TestClassWithMultipleParameters
    {
        public int IntValue { get; }
        public string StringValue { get; }

        public TestClassWithMultipleParameters(int intValue, string stringValue)
        {
            IntValue = intValue;
            StringValue = stringValue;
        }
    }
}
