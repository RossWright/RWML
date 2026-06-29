using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Attributes;

public class UploadLimitAttributeTests
{
    [Fact]
    public void Constructor_WithPositiveByteLimit_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = 1024L;

        // Act
        var attribute = new UploadLimitAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void Constructor_WithZeroByteLimit_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = 0L;

        // Act
        var attribute = new UploadLimitAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void Constructor_WithNegativeByteLimit_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = -1L;

        // Act
        var attribute = new UploadLimitAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void Constructor_WithMaxLongValue_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = long.MaxValue;

        // Act
        var attribute = new UploadLimitAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void Constructor_WithMinLongValue_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = long.MinValue;

        // Act
        var attribute = new UploadLimitAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void ProtectedConstructor_WithNullByteLimit_SetsByteLimitProperty()
    {
        // Arrange & Act
        var attribute = new TestDerivedAttribute(null);

        // Assert
        attribute.ByteLimit.ShouldBeNull();
    }

    [Fact]
    public void ProtectedConstructor_WithPositiveByteLimit_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = 2048L;

        // Act
        var attribute = new TestDerivedAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void ProtectedConstructor_WithZeroByteLimit_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = 0L;

        // Act
        var attribute = new TestDerivedAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void ProtectedConstructor_WithNegativeByteLimit_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = -100L;

        // Act
        var attribute = new TestDerivedAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void ProtectedConstructor_WithMaxLongValue_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = long.MaxValue;

        // Act
        var attribute = new TestDerivedAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void ProtectedConstructor_WithMinLongValue_SetsByteLimitProperty()
    {
        // Arrange
        var expectedByteLimit = long.MinValue;

        // Act
        var attribute = new TestDerivedAttribute(expectedByteLimit);

        // Assert
        attribute.ByteLimit.ShouldBe(expectedByteLimit);
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        // Arrange & Act
        var attributes = typeof(TestClassWithAttribute).GetCustomAttributes(typeof(UploadLimitAttribute), false);

        // Assert
        attributes.Length.ShouldBe(1);
        var attribute = (UploadLimitAttribute)attributes[0];
        attribute.ByteLimit.ShouldBe(5242880L);
    }

    [Fact]
    public void Attribute_CannotBeAppliedMultipleTimes()
    {
        // Arrange & Act
        var attributeUsage = (AttributeUsageAttribute)typeof(UploadLimitAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        // Assert
        attributeUsage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void Attribute_TargetsClassOnly()
    {
        // Arrange & Act
        var attributeUsage = (AttributeUsageAttribute)typeof(UploadLimitAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        // Assert
        attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
    }

    // Test helper classes
    private class TestDerivedAttribute : UploadLimitAttribute
    {
        public TestDerivedAttribute(long? byteLimit) : base(byteLimit)
        {
        }
    }

    [UploadLimit(5242880L)] // 5 MB
    private class TestClassWithAttribute { }
}
