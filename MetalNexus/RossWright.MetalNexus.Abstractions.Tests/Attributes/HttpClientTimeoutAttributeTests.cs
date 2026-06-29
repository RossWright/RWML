using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests;

public class HttpClientTimeoutAttributeTests
{
    [Fact]
    public void Constructor_WithPositiveTimeout_SetsTimeoutSeconds()
    {
        // Arrange
        var expectedTimeout = 30;

        // Act
        var attribute = new HttpClientTimeoutAttribute(expectedTimeout);

        // Assert
        attribute.TimeoutSeconds.ShouldBe(expectedTimeout);
    }

    [Fact]
    public void Constructor_WithZeroTimeout_SetsTimeoutSeconds()
    {
        // Arrange
        var expectedTimeout = 0;

        // Act
        var attribute = new HttpClientTimeoutAttribute(expectedTimeout);

        // Assert
        attribute.TimeoutSeconds.ShouldBe(expectedTimeout);
    }

    [Fact]
    public void Constructor_WithNegativeTimeout_SetsTimeoutSeconds()
    {
        // Arrange
        var expectedTimeout = -1;

        // Act
        var attribute = new HttpClientTimeoutAttribute(expectedTimeout);

        // Assert
        attribute.TimeoutSeconds.ShouldBe(expectedTimeout);
    }

    [Fact]
    public void Constructor_WithLargeTimeout_SetsTimeoutSeconds()
    {
        // Arrange
        var expectedTimeout = 86400; // 24 hours in seconds

        // Act
        var attribute = new HttpClientTimeoutAttribute(expectedTimeout);

        // Assert
        attribute.TimeoutSeconds.ShouldBe(expectedTimeout);
    }

    [Fact]
    public void Constructor_WithMaxIntValue_SetsTimeoutSeconds()
    {
        // Arrange
        var expectedTimeout = int.MaxValue;

        // Act
        var attribute = new HttpClientTimeoutAttribute(expectedTimeout);

        // Assert
        attribute.TimeoutSeconds.ShouldBe(expectedTimeout);
    }

    [Fact]
    public void Constructor_WithMinIntValue_SetsTimeoutSeconds()
    {
        // Arrange
        var expectedTimeout = int.MinValue;

        // Act
        var attribute = new HttpClientTimeoutAttribute(expectedTimeout);

        // Assert
        attribute.TimeoutSeconds.ShouldBe(expectedTimeout);
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        // Arrange & Act
        var attributes = typeof(TestClassWithAttribute).GetCustomAttributes(typeof(HttpClientTimeoutAttribute), false);

        // Assert
        attributes.Length.ShouldBe(1);
        var attribute = (HttpClientTimeoutAttribute)attributes[0];
        attribute.TimeoutSeconds.ShouldBe(60);
    }

    [Fact]
    public void Attribute_CannotBeAppliedMultipleTimes()
    {
        // Arrange & Act
        var attributeUsage = (AttributeUsageAttribute)typeof(HttpClientTimeoutAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        // Assert
        attributeUsage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void Attribute_TargetsClassOnly()
    {
        // Arrange & Act
        var attributeUsage = (AttributeUsageAttribute)typeof(HttpClientTimeoutAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        // Assert
        attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
    }

    // Test helper class
    [HttpClientTimeout(60)]
    private class TestClassWithAttribute { }
}
