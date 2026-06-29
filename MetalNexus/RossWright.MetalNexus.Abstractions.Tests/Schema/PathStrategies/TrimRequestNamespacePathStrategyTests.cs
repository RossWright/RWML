using System.Reflection;
using RossWright.MetalNexus;
using RossWright.MetalNexus.Schema.PathStrategies;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Schema.PathStrategies;

public class TrimRequestNamespacePathStrategyTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithThresholdOfOne()
    {
        // Arrange & Act
        var strategy = new TrimRequestNamespacePathStrategy();

        // Assert
        strategy.ShouldNotBeNull();
        strategy.ShouldBeOfType<TrimRequestNamespacePathStrategy>();
    }

    [Fact]
    public void GetConsideredTypes_WithAssemblyContainingNoApiRequestTypes_ReturnsEmptyArray()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(string).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetConsideredTypes_WithAssemblyContainingApiRequestTypes_IncludesTypeWithAttribute()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(SingleApiRequestType).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(typeof(SingleApiRequestType));
    }

    [Fact]
    public void GetConsideredTypes_WithAssemblyContainingMultipleApiRequestTypes_ReturnsAllApiRequestTypes()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(FirstApiRequestType).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThanOrEqualTo(3);
        result.ShouldContain(typeof(FirstApiRequestType));
        result.ShouldContain(typeof(SecondApiRequestType));
        result.ShouldContain(typeof(ThirdApiRequestType));
    }

    [Fact]
    public void GetConsideredTypes_WithAssemblyContainingMixedTypes_ReturnsOnlyApiRequestTypes()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(TypeWithoutAttribute).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotContain(typeof(TypeWithoutAttribute));
        result.ShouldNotContain(typeof(AnotherTypeWithoutAttribute));
    }

    [Fact]
    public void GetConsideredTypes_WithNestedTypeWithAttribute_IncludesNestedType()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(OuterClass.NestedApiRequestType).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(typeof(OuterClass.NestedApiRequestType));
    }

    [Fact]
    public void GetConsideredTypes_WithInheritedType_IncludesIfAttributePresent()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(DerivedApiRequestType).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(typeof(DerivedApiRequestType));
    }

    [Fact]
    public void GetConsideredTypes_WithGenericTypeWithAttribute_IncludesGenericType()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(GenericApiRequestType<>).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(typeof(GenericApiRequestType<>));
    }

    [Fact]
    public void Constructor_InheritsFromTrimDefaultNamespacePathStrategy()
    {
        // Arrange & Act
        var strategy = new TrimRequestNamespacePathStrategy();

        // Assert
        strategy.ShouldBeAssignableTo<TrimDefaultNamespacePathStrategy>();
    }

    [Fact]
    public void GetConsideredTypes_CalledMultipleTimes_ReturnsConsistentResults()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(FirstApiRequestType).Assembly;

        // Act
        var firstCall = strategy.GetConsideredTypesPublic(assembly);
        var secondCall = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        firstCall.Length.ShouldBe(secondCall.Length);
        firstCall.ShouldBe(secondCall, ignoreOrder: true);
    }

    [Fact]
    public void GetConsideredTypes_WithAbstractTypeWithAttribute_IncludesAbstractType()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(AbstractApiRequestType).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(typeof(AbstractApiRequestType));
    }

    [Fact]
    public void GetConsideredTypes_WithSealedTypeWithAttribute_IncludesSealedType()
    {
        // Arrange
        var strategy = new TestableStrategy();
        var assembly = typeof(SealedApiRequestType).Assembly;

        // Act
        var result = strategy.GetConsideredTypesPublic(assembly);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(typeof(SealedApiRequestType));
    }

    // -- Helper Classes ------------------------------------------------------------------------

    private class TestableStrategy : TrimRequestNamespacePathStrategy
    {
        public Type[] GetConsideredTypesPublic(Assembly assembly) => GetConsideredTypes(assembly);
    }

    [ApiRequest]
    private class SingleApiRequestType { }

    [ApiRequest(HttpProtocol.Get)]
    private class FirstApiRequestType { }

    [ApiRequest(HttpProtocol.PostViaBody)]
    private class SecondApiRequestType { }

    [ApiRequest(HttpProtocol.Delete, "/api/test")]
    private class ThirdApiRequestType { }

    private class TypeWithoutAttribute { }

    private class AnotherTypeWithoutAttribute { }

    private class OuterClass
    {
        [ApiRequest]
        internal class NestedApiRequestType { }
    }

    [ApiRequest]
    private class DerivedApiRequestType : FirstApiRequestType { }

    [ApiRequest]
    private class GenericApiRequestType<T> { }

    [ApiRequest]
    private abstract class AbstractApiRequestType { }

    [ApiRequest]
    private sealed class SealedApiRequestType { }
}
