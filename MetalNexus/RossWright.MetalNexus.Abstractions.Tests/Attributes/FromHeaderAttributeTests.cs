using RossWright.MetalNexus;
using Shouldly;
using System.Reflection;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Attributes;

public class FromHeaderAttributeTests
{
    [Fact]
    public void HeaderName_WithTypicalValue_ReturnsHeaderName()
    {
        // Arrange
        var expectedHeaderName = "X-Custom-Header";
        var attribute = new FromHeaderAttribute(expectedHeaderName);

        // Act
        var actualHeaderName = attribute.HeaderName;

        // Assert
        actualHeaderName.ShouldBe(expectedHeaderName);
    }

    [Fact]
    public void HeaderName_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var expectedHeaderName = string.Empty;
        var attribute = new FromHeaderAttribute(expectedHeaderName);

        // Act
        var actualHeaderName = attribute.HeaderName;

        // Assert
        actualHeaderName.ShouldBe(expectedHeaderName);
    }

    [Fact]
    public void HeaderName_WithWhitespace_ReturnsWhitespace()
    {
        // Arrange
        var expectedHeaderName = "   ";
        var attribute = new FromHeaderAttribute(expectedHeaderName);

        // Act
        var actualHeaderName = attribute.HeaderName;

        // Assert
        actualHeaderName.ShouldBe(expectedHeaderName);
    }

    [Fact]
    public void HeaderName_WithSpecialCharacters_ReturnsSpecialCharacters()
    {
        // Arrange
        var expectedHeaderName = "X-@#$%-Header!";
        var attribute = new FromHeaderAttribute(expectedHeaderName);

        // Act
        var actualHeaderName = attribute.HeaderName;

        // Assert
        actualHeaderName.ShouldBe(expectedHeaderName);
    }

    [Fact]
    public void HeaderName_WithLongValue_ReturnsLongValue()
    {
        // Arrange
        var expectedHeaderName = new string('A', 1000);
        var attribute = new FromHeaderAttribute(expectedHeaderName);

        // Act
        var actualHeaderName = attribute.HeaderName;

        // Assert
        actualHeaderName.ShouldBe(expectedHeaderName);
    }

    [Fact]
    public void HeaderName_WithMixedCasing_ReturnsMixedCasing()
    {
        // Arrange
        var expectedHeaderName = "X-CuStOm-HeAdEr";
        var attribute = new FromHeaderAttribute(expectedHeaderName);

        // Act
        var actualHeaderName = attribute.HeaderName;

        // Assert
        actualHeaderName.ShouldBe(expectedHeaderName);
    }

    [Fact]
    public void Attribute_CanBeAppliedToProperty_AttributeExists()
    {
        // Arrange & Act
        var property = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));
        var attribute = property?.GetCustomAttribute<FromHeaderAttribute>();

        // Assert
        attribute.ShouldNotBeNull();
        attribute.HeaderName.ShouldBe("X-Test-Header");
    }

    [Fact]
    public void AttributeUsage_AllowMultiple_IsFalse()
    {
        // Arrange & Act
        var attributeUsage = typeof(FromHeaderAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        attributeUsage.ShouldNotBeNull();
        attributeUsage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void AttributeUsage_ValidOn_IsProperty()
    {
        // Arrange & Act
        var attributeUsage = typeof(FromHeaderAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        attributeUsage.ShouldNotBeNull();
        attributeUsage.ValidOn.ShouldBe(AttributeTargets.Property);
    }

    private class TestClass
    {
        [FromHeader("X-Test-Header")]
        public string? TestProperty { get; set; }
    }
}
