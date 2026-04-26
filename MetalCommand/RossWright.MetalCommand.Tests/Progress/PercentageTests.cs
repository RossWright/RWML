using Microsoft.VisualStudio.TestPlatform.Utilities;
using NSubstitute.Core;
using System.Security.Claims;
using System.Threading.Channels;

namespace RossWright.MetalCommand.Tests;

public class PercentageTests
{
    [Fact]
    public void Width_IsFour()
    {
        new Percentage().Width.ShouldBe(4);
    }

    [Fact]
    public void Output_WithZeroProgress_ReturnsZeroPercent()
    {
        // Arrange
        var percentage = new Percentage();

        // Act
        var result = percentage.Output(0.0);

        // Assert
        result.ShouldBe("  0%");
    }

    [Fact]
    public void Output_WithFullProgress_ReturnsOneHundredPercent()
    {
        // Arrange
        var percentage = new Percentage();

        // Act
        var result = percentage.Output(1.0);

        // Assert
        result.ShouldBe("100%");
    }

    [Fact]
    public void Output_WithHalfProgress_ReturnsFiftyPercent()
    {
        // Arrange
        var percentage = new Percentage();

        // Act
        var result = percentage.Output(0.5);

        // Assert
        result.ShouldBe(" 50%");
    }

    [Fact]
    public void Output_WithNegativeProgress_ReturnsZeroPercent()
    {
        // Arrange
        var percentage = new Percentage();

        // Act
        var result = percentage.Output(-0.5);

        // Assert
        result.ShouldBe("  0%");
    }

    [Fact]
    public void Output_WithProgressGreaterThanOne_ReturnsOneHundredPercent()
    {
        // Arrange
        var percentage = new Percentage();

        // Act
        var result = percentage.Output(1.5);

        // Assert
        result.ShouldBe("100%");
    }

    [Fact]
    public void Output_WithProgressAtBoundary_ReturnsCorrectFormat()
    {
        // Arrange
        var percentage = new Percentage();

        // Act
        var result = percentage.Output(0.01);

        // Assert
        result.ShouldBe("  1%");
    }

    [Fact]
    public void Output_WithProgressNearFull_ReturnsNinetyNinePercent()
    {
        // Arrange
        var percentage = new Percentage();

        // Act
        var result = percentage.Output(0.99);

        // Assert
        result.ShouldBe(" 99%");
    }

    [Fact]
    public void Output_WithProgressAtQuarter_ReturnsTwentyFivePercent()
    {
        // Arrange
        var percentage = new Percentage();

        // Act
        var result = percentage.Output(0.25);

        // Assert
        result.ShouldBe(" 25%");
    }
}
