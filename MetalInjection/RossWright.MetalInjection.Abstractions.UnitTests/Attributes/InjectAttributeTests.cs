using RossWright.MetalInjection;
using Shouldly;

namespace RossWright.MetalInjection.Abstractions.UnitTests.Attributes;

public class InjectAttributeTests
{
    // ── Constructor Tests ────────────────────────────────────────────────────────────────────
    
    [Fact]
    public void Constructor_WithNullKey_SetsKeyToNull()
    {
        // Arrange & Act
        var attribute = new InjectAttribute(null);

        // Assert
        attribute.Key.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNoArgument_SetsKeyToNull()
    {
        // Arrange & Act
        var attribute = new InjectAttribute();

        // Assert
        attribute.Key.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithStringKey_SetsKeyToString()
    {
        // Arrange & Act
        var attribute = new InjectAttribute("myKey");

        // Assert
        attribute.Key.ShouldBe("myKey");
    }

    [Fact]
    public void Constructor_WithIntKey_SetsKeyToInt()
    {
        // Arrange & Act
        var attribute = new InjectAttribute(42);

        // Assert
        attribute.Key.ShouldBe(42);
    }

    [Fact]
    public void Constructor_WithObjectKey_SetsKeyToObject()
    {
        // Arrange & Act
        var obj = new object();
        var attribute = new InjectAttribute(obj);

        // Assert
        attribute.Key.ShouldBe(obj);
    }

    [Fact]
    public void Constructor_WithEnumKey_SetsKeyToEnum()
    {
        // Arrange & Act
        var attribute = new InjectAttribute(TestEnum.Value1);

        // Assert
        attribute.Key.ShouldBe(TestEnum.Value1);
    }

    // ── Optional Property Tests ──────────────────────────────────────────────────────────────
    
    [Fact]
    public void Optional_DefaultValue_ReturnsFalse()
    {
        // Arrange & Act
        var attribute = new InjectAttribute();

        // Assert
        attribute.Optional.ShouldBeFalse();
    }

    [Fact]
    public void Optional_SetToTrue_ReturnsTrue()
    {
        // Arrange
        var attribute = new InjectAttribute();

        // Act
        attribute.Optional = true;

        // Assert
        attribute.Optional.ShouldBeTrue();
    }

    [Fact]
    public void Optional_SetToFalse_ReturnsFalse()
    {
        // Arrange
        var attribute = new InjectAttribute();

        // Act
        attribute.Optional = false;

        // Assert
        attribute.Optional.ShouldBeFalse();
    }

    [Fact]
    public void Optional_SetToTrueThenFalse_ReturnsFalse()
    {
        // Arrange
        var attribute = new InjectAttribute();
        attribute.Optional = true;

        // Act
        attribute.Optional = false;

        // Assert
        attribute.Optional.ShouldBeFalse();
    }

    [Fact]
    public void Optional_SetToFalseThenTrue_ReturnsTrue()
    {
        // Arrange
        var attribute = new InjectAttribute();
        attribute.Optional = false;

        // Act
        attribute.Optional = true;

        // Assert
        attribute.Optional.ShouldBeTrue();
    }

    // ── EffectiveOptional Property Tests ─────────────────────────────────────────────────────
    
    [Fact]
    public void EffectiveOptional_WhenNotSet_ReturnsNull()
    {
        // Arrange & Act
        var attribute = new InjectAttribute();

        // Assert
        attribute.EffectiveOptional.ShouldBeNull();
    }

    [Fact]
    public void EffectiveOptional_WhenOptionalSetToTrue_ReturnsTrue()
    {
        // Arrange
        var attribute = new InjectAttribute();

        // Act
        attribute.Optional = true;

        // Assert
        attribute.EffectiveOptional.ShouldBe(true);
    }

    [Fact]
    public void EffectiveOptional_WhenOptionalSetToFalse_ReturnsFalse()
    {
        // Arrange
        var attribute = new InjectAttribute();

        // Act
        attribute.Optional = false;

        // Assert
        attribute.EffectiveOptional.ShouldBe(false);
    }

    [Fact]
    public void EffectiveOptional_WhenOptionalToggled_ReturnsLatestValue()
    {
        // Arrange
        var attribute = new InjectAttribute();
        attribute.Optional = true;

        // Act
        attribute.Optional = false;

        // Assert
        attribute.EffectiveOptional.ShouldBe(false);
    }

    [Fact]
    public void EffectiveOptional_AfterMultipleChanges_ReflectsCurrentState()
    {
        // Arrange
        var attribute = new InjectAttribute();
        
        // Act & Assert - Initially null
        attribute.EffectiveOptional.ShouldBeNull();
        
        // Act & Assert - Set to false
        attribute.Optional = false;
        attribute.EffectiveOptional.ShouldBe(false);
        
        // Act & Assert - Set to true
        attribute.Optional = true;
        attribute.EffectiveOptional.ShouldBe(true);
        
        // Act & Assert - Set back to false
        attribute.Optional = false;
        attribute.EffectiveOptional.ShouldBe(false);
    }

    // ── Combined Scenarios ───────────────────────────────────────────────────────────────────
    
    [Fact]
    public void Constructor_WithKey_AndOptionalSetToTrue_PreservesKeyAndOptional()
    {
        // Arrange & Act
        var attribute = new InjectAttribute("testKey") { Optional = true };

        // Assert
        attribute.Key.ShouldBe("testKey");
        attribute.Optional.ShouldBeTrue();
        attribute.EffectiveOptional.ShouldBe(true);
    }

    [Fact]
    public void Constructor_WithKey_AndOptionalSetToFalse_PreservesKeyAndOptional()
    {
        // Arrange & Act
        var attribute = new InjectAttribute(123) { Optional = false };

        // Assert
        attribute.Key.ShouldBe(123);
        attribute.Optional.ShouldBeFalse();
        attribute.EffectiveOptional.ShouldBe(false);
    }

    [Fact]
    public void Constructor_WithKey_WithoutOptionalSet_EffectiveOptionalIsNull()
    {
        // Arrange & Act
        var attribute = new InjectAttribute("key");

        // Assert
        attribute.Key.ShouldBe("key");
        attribute.EffectiveOptional.ShouldBeNull();
    }

    // ── Helper Types ─────────────────────────────────────────────────────────────────────────
    
    private enum TestEnum
    {
        Value1,
        Value2
    }
}
