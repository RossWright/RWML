using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Attributes;

public class NoUploadLimitAttributeTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var attribute = new NoUploadLimitAttribute();

        // Assert
        attribute.ShouldNotBeNull();
        attribute.ShouldBeOfType<NoUploadLimitAttribute>();
    }

    [Fact]
    public void Constructor_ShouldSetByteLimitToNull()
    {
        // Arrange & Act
        var attribute = new NoUploadLimitAttribute();

        // Assert
        attribute.ByteLimit.ShouldBeNull();
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        // Arrange & Act
        var attributes = typeof(TestClassWithAttribute).GetCustomAttributes(typeof(NoUploadLimitAttribute), false);

        // Assert
        attributes.Length.ShouldBe(1);
        var attribute = (NoUploadLimitAttribute)attributes[0];
        attribute.ByteLimit.ShouldBeNull();
    }

    [Fact]
    public void Attribute_CannotBeAppliedMultipleTimes()
    {
        // Arrange & Act
        var attributeUsage = (AttributeUsageAttribute)typeof(NoUploadLimitAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        // Assert
        attributeUsage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void Attribute_TargetsClassOnly()
    {
        // Arrange & Act
        var attributeUsage = (AttributeUsageAttribute)typeof(NoUploadLimitAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        // Assert
        attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
    }

    // Test helper class
    [NoUploadLimit]
    private class TestClassWithAttribute { }
}
