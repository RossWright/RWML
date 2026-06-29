using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Attributes;

public class AnonymousAttributeTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var attribute = new AnonymousAttribute();

        // Assert
        attribute.ShouldNotBeNull();
        attribute.ShouldBeOfType<AnonymousAttribute>();
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        // Arrange & Act
        var attributes = typeof(TestClassWithAttribute).GetCustomAttributes(typeof(AnonymousAttribute), false);

        // Assert
        attributes.Length.ShouldBe(1);
        attributes[0].ShouldBeOfType<AnonymousAttribute>();
    }

    [Fact]
    public void Attribute_CannotBeAppliedMultipleTimes()
    {
        // Arrange & Act
        var attributeUsage = (AttributeUsageAttribute)typeof(AnonymousAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        // Assert
        attributeUsage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void Attribute_TargetsClassOnly()
    {
        // Arrange & Act
        var attributeUsage = (AttributeUsageAttribute)typeof(AnonymousAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        // Assert
        attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
    }

    // Test helper class
    [Anonymous]
    private class TestClassWithAttribute { }
}
