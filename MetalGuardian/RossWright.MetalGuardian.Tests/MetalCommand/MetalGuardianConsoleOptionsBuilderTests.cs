using Microsoft.Extensions.Configuration;
using NSubstitute;
using RossWright.MetalCommand;
using Shouldly;

namespace RossWright.MetalGuardian.Tests.MetalCommand;

public class MetalGuardianConsoleOptionsBuilderTests
{
    [Fact]
    public void Constructor_WithValidBuilder_AssignsConfiguration()
    {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var mockBuilder = Substitute.For<IConsoleApplicationBuilder>();
        mockBuilder.Configuration.Returns(mockConfiguration);

        // Act
        var sut = new RossWright.MetalGuardian.MetalGuardianConsoleOptionsBuilder(mockBuilder);

        // Assert
        sut.Configuration.ShouldBe(mockConfiguration);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_AssignsNullToConfiguration()
    {
        // Arrange
        var mockBuilder = Substitute.For<IConsoleApplicationBuilder>();
        mockBuilder.Configuration.Returns((IConfiguration?)null);

        // Act
        var sut = new RossWright.MetalGuardian.MetalGuardianConsoleOptionsBuilder(mockBuilder);

        // Assert
        sut.Configuration.ShouldBeNull();
    }
}
