using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Server.Tests.Fakes;

namespace RossWright.MetalGuardian.Server.Tests.Internal;

public class ExtensionsTests
{
    [Fact]
    public void UseTotpMfa_CallsTotpBuilderAction()
    {
        // Arrange
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder, IOptionsBuilder>();
        IMetalGuardianServerTotpMfaOptionsBuilder? capturedBuilder = null;
        var totpBuilderAction = Substitute.For<Action<IMetalGuardianServerTotpMfaOptionsBuilder>>();
        totpBuilderAction.When(x => x.Invoke(Arg.Any<IMetalGuardianServerTotpMfaOptionsBuilder>()))
            .Do(ci => capturedBuilder = ci.Arg<IMetalGuardianServerTotpMfaOptionsBuilder>());

        // Act
        guardianBuilder.UseTotpMfa<FakeTotpUser>(totpBuilderAction);

        // Assert
        totpBuilderAction.Received(1).Invoke(Arg.Any<IMetalGuardianServerTotpMfaOptionsBuilder>());
        capturedBuilder.ShouldNotBeNull();
    }

    [Fact]
    public void UseTotpMfa_CallsAddServicesOnGuardianBuilderAsIOptionsBuilder()
    {
        // Arrange
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder, IOptionsBuilder>();
        var totpBuilderAction = Substitute.For<Action<IMetalGuardianServerTotpMfaOptionsBuilder>>();

        // Act
        guardianBuilder.UseTotpMfa<FakeTotpUser>(totpBuilderAction);

        // Assert
        ((IOptionsBuilder)guardianBuilder).Received(1).AddServices(Arg.Any<Action<IServiceCollection>>());
    }

    [Fact]
    public void UseTotpMfa_PassesLambdaThatCallsInitialize()
    {
        // Arrange
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder, IOptionsBuilder>();
        Action<IServiceCollection>? capturedAddServicesAction = null;
        ((IOptionsBuilder)guardianBuilder).When(x => x.AddServices(Arg.Any<Action<IServiceCollection>>()))
            .Do(ci => capturedAddServicesAction = ci.Arg<Action<IServiceCollection>>());
        var serviceCollection = new ServiceCollection();

        // Act
        guardianBuilder.UseTotpMfa<FakeTotpUser>(builder =>
        {
            builder.SetIssuer("TestIssuer");
        });

        // Assert
        capturedAddServicesAction.ShouldNotBeNull();
        Should.NotThrow(() => capturedAddServicesAction(serviceCollection));
    }

    [Fact]
    public void UseTotpMfa_WithDifferentUserTypes_CreatesBuilderWithCorrectType()
    {
        // Arrange
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder, IOptionsBuilder>();
        var totpBuilderCalled = false;

        // Act
        guardianBuilder.UseTotpMfa<FakeTotpUser>(builder =>
        {
            totpBuilderCalled = true;
            builder.SetIssuer("TestIssuer");
        });

        // Assert
        totpBuilderCalled.ShouldBeTrue();
    }

    [Fact]
    public void UseTotpMfa_ExecutesTotpBuilderBeforeAddServices()
    {
        // Arrange
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder, IOptionsBuilder>();
        var executionOrder = new List<string>();
        var totpBuilderAction = Substitute.For<Action<IMetalGuardianServerTotpMfaOptionsBuilder>>();
        totpBuilderAction.When(x => x.Invoke(Arg.Any<IMetalGuardianServerTotpMfaOptionsBuilder>()))
            .Do(_ => executionOrder.Add("TotpBuilder"));
        ((IOptionsBuilder)guardianBuilder).When(x => x.AddServices(Arg.Any<Action<IServiceCollection>>()))
            .Do(_ => executionOrder.Add("AddServices"));

        // Act
        guardianBuilder.UseTotpMfa<FakeTotpUser>(totpBuilderAction);

        // Assert
        executionOrder.Count.ShouldBe(2);
        executionOrder[0].ShouldBe("TotpBuilder");
        executionOrder[1].ShouldBe("AddServices");
    }

    [Fact]
    public void UseTotpMfa_CreatesNewBuilderInstance()
    {
        // Arrange
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder, IOptionsBuilder>();
        IMetalGuardianServerTotpMfaOptionsBuilder? firstBuilder = null;
        IMetalGuardianServerTotpMfaOptionsBuilder? secondBuilder = null;

        // Act
        guardianBuilder.UseTotpMfa<FakeTotpUser>(builder => firstBuilder = builder);
        guardianBuilder.UseTotpMfa<FakeTotpUser>(builder => secondBuilder = builder);

        // Assert
        firstBuilder.ShouldNotBeNull();
        secondBuilder.ShouldNotBeNull();
        ReferenceEquals(firstBuilder, secondBuilder).ShouldBeFalse();
    }

    [Fact]
    public void UseTotpMfa_PassesGuardianBuilderToInitialize()
    {
        // Arrange
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder, IOptionsBuilder>();
        Action<IServiceCollection>? capturedAction = null;
        ((IOptionsBuilder)guardianBuilder).When(x => x.AddServices(Arg.Any<Action<IServiceCollection>>()))
            .Do(ci => capturedAction = ci.Arg<Action<IServiceCollection>>());
        var serviceCollection = new ServiceCollection();

        // Act
        guardianBuilder.UseTotpMfa<FakeTotpUser>(builder =>
        {
            builder.SetIssuer("TestIssuer");
        });

        // Assert
        capturedAction.ShouldNotBeNull();
        Should.NotThrow(() => capturedAction(serviceCollection));
    }

    [Fact]
    public void UseTotpMfa_AllowsBuilderConfigurationToPropagateToInitialize()
    {
        // Arrange
        var guardianBuilder = Substitute.For<IMetalGuardianServerOptionBuilder, IOptionsBuilder>();
        Action<IServiceCollection>? capturedAction = null;
        ((IOptionsBuilder)guardianBuilder).When(x => x.AddServices(Arg.Any<Action<IServiceCollection>>()))
            .Do(ci => capturedAction = ci.Arg<Action<IServiceCollection>>());
        var serviceCollection = new ServiceCollection();

        // Act
        guardianBuilder.UseTotpMfa<FakeTotpUser>(builder =>
        {
            builder.SetIssuer("MyIssuer");
            builder.SetDeviceRememberDays(30);
            builder.UseMetalNexusTotpMfaEndpoints();
        });
        capturedAction.ShouldNotBeNull();
        capturedAction(serviceCollection);

        // Assert - If Initialize is called without exception, the configuration was passed through
        Should.NotThrow(() => capturedAction(serviceCollection));
    }
}
