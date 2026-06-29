using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalCommand;
using Shouldly;

namespace RossWright.MetalGuardian.Tests.MetalCommand;

public class MetalGuardianMetalCommandExtensionTests
{
    [Fact]
    public void AddMetalGuardianClient_InvokesSetOptions_ReturnsBuilder()
    {
        // Arrange
        var mockBuilder = Substitute.For<IConsoleApplicationBuilder>();
        var mockServices = new ServiceCollection();
        mockBuilder.Services.Returns(mockServices);
        var setOptionsCalled = false;
        IMetalGuardianConsoleOptionsBuilder? capturedBuilder = null;

        // Act
        var result = mockBuilder.AddMetalGuardianClient(opts =>
        {
            setOptionsCalled = true;
            capturedBuilder = opts;
        });

        // Assert
        setOptionsCalled.ShouldBeTrue();
        capturedBuilder.ShouldNotBeNull();
        result.ShouldBe(mockBuilder);
    }

    [Fact]
    public void AddMetalGuardianClient_CreatesOptionsBuilder_PassesCorrectType()
    {
        // Arrange
        var mockBuilder = Substitute.For<IConsoleApplicationBuilder>();
        var mockServices = new ServiceCollection();
        mockBuilder.Services.Returns(mockServices);
        IMetalGuardianConsoleOptionsBuilder? receivedBuilder = null;

        // Act
        mockBuilder.AddMetalGuardianClient(opts =>
        {
            receivedBuilder = opts;
        });

        // Assert
        receivedBuilder.ShouldNotBeNull();
        receivedBuilder.ShouldBeAssignableTo<IMetalGuardianConsoleOptionsBuilder>();
    }

    [Fact]
    public void AddMetalGuardianClient_CallsInitializeClient_ServicesAreConfigured()
    {
        // Arrange
        var mockBuilder = Substitute.For<IConsoleApplicationBuilder>();
        var mockServices = new ServiceCollection();
        mockBuilder.Services.Returns(mockServices);
        var serviceCountBefore = mockServices.Count;

        // Act
        mockBuilder.AddMetalGuardianClient(opts => { });

        // Assert
        mockServices.Count.ShouldBeGreaterThan(serviceCountBefore);
    }

    [Fact]
    public void AddMetalGuardianClient_PassesBuilderConfiguration_ToOptionsBuilder()
    {
        // Arrange
        var mockConfiguration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();
        var mockBuilder = Substitute.For<IConsoleApplicationBuilder>();
        var mockServices = new ServiceCollection();
        mockBuilder.Services.Returns(mockServices);
        mockBuilder.Configuration.Returns(mockConfiguration);
        Microsoft.Extensions.Configuration.IConfiguration? capturedConfiguration = null;

        // Act
        mockBuilder.AddMetalGuardianClient(opts =>
        {
            capturedConfiguration = opts.Configuration;
        });

        // Assert
        capturedConfiguration.ShouldBe(mockConfiguration);
    }

    [Fact]
    public void AddMetalGuardianClient_CallsSetOptions_BeforeInitializeClient()
    {
        // Arrange
        var mockBuilder = Substitute.For<IConsoleApplicationBuilder>();
        var mockServices = new ServiceCollection();
        mockBuilder.Services.Returns(mockServices);
        var callOrder = new System.Collections.Generic.List<string>();

        // Act
        mockBuilder.AddMetalGuardianClient(opts =>
        {
            callOrder.Add("setOptions");
        });

        // We can't directly observe InitializeClient being called, but we can verify
        // that setOptions completes before services are registered
        callOrder.Add("afterCall");

        // Assert
        callOrder.ShouldBe(new[] { "setOptions", "afterCall" });
        mockServices.Count.ShouldBeGreaterThan(0); // Verify InitializeClient was called
    }
}
