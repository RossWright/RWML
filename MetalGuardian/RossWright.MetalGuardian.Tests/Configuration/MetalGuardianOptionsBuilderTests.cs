using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalGuardian.Tests.Configuration;

public class MetalGuardianOptionsBuilderTests
{
    private class TestableMetalGuardianOptionsBuilder : MetalGuardianOptionsBuilder
    {
        public override void UseMetalNexusAuthenticationEndpoints()
        {
            // Not needed for testing UsePasswordValidator
        }

        public IServiceCollection ApplyServices()
        {
            var services = new ServiceCollection();
            AddServices(services);
            return services;
        }
    }

    [Fact]
    public void UsePasswordValidator_WithNullConfigure_RegistersValidatorWithDefaultRequirements()
    {
        // Arrange
        var builder = new TestableMetalGuardianOptionsBuilder();

        // Act
        builder.UsePasswordValidator(null);
        var services = builder.ApplyServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var validator = provider.GetService<IPasswordValidator>();
        validator.ShouldNotBeNull();
    }

    [Fact]
    public void UsePasswordValidator_WithoutParameter_RegistersValidatorWithDefaultRequirements()
    {
        // Arrange
        var builder = new TestableMetalGuardianOptionsBuilder();

        // Act
        builder.UsePasswordValidator();
        var services = builder.ApplyServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var validator = provider.GetService<IPasswordValidator>();
        validator.ShouldNotBeNull();
        validator.ValidatePassword("TestPass1!").ShouldBeTrue();
    }

    [Fact]
    public void UsePasswordValidator_WithConfigureAction_RegistersValidatorWithCustomRequirements()
    {
        // Arrange
        var builder = new TestableMetalGuardianOptionsBuilder();
        var configureInvoked = false;

        // Act
        builder.UsePasswordValidator(req =>
        {
            configureInvoked = true;
            req.MinimumLength = 12;
            req.RequireUpperCase = true;
            req.RequireLowerCase = true;
            req.RequireDigit = true;
            req.RequireSymbol = true;
        });
        var services = builder.ApplyServices();
        var provider = services.BuildServiceProvider();

        // Assert
        configureInvoked.ShouldBeTrue();
        var validator = provider.GetService<IPasswordValidator>();
        validator.ShouldNotBeNull();
        
        // Short password should fail (min length 12)
        validator.ValidatePassword("Short1!").ShouldBeFalse();
        
        // Valid password meeting all requirements
        validator.ValidatePassword("ValidPass123!").ShouldBeTrue();
    }

    [Fact]
    public void UsePasswordValidator_WithConfigureAction_AppliesConfigurationToRequirements()
    {
        // Arrange
        var builder = new TestableMetalGuardianOptionsBuilder();

        // Act
        builder.UsePasswordValidator(req =>
        {
            req.MinimumLength = 6;
            req.MaximumLength = 10;
            req.RequireUpperCase = false;
            req.RequireLowerCase = false;
            req.RequireDigit = false;
            req.RequireSymbol = false;
        });
        var services = builder.ApplyServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var validator = provider.GetService<IPasswordValidator>();
        validator.ShouldNotBeNull();
        
        // Should accept simple password within length range
        validator.ValidatePassword("simple").ShouldBeTrue();
        
        // Should reject password exceeding max length
        validator.ValidatePassword("toolongpassword").ShouldBeFalse();
    }

    [Fact]
    public void UsePasswordValidator_RegistersAsSingleton_ReturnsSameInstance()
    {
        // Arrange
        var builder = new TestableMetalGuardianOptionsBuilder();

        // Act
        builder.UsePasswordValidator();
        var services = builder.ApplyServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var validator1 = provider.GetService<IPasswordValidator>();
        var validator2 = provider.GetService<IPasswordValidator>();
        
        validator1.ShouldNotBeNull();
        validator2.ShouldNotBeNull();
        validator1.ShouldBeSameAs(validator2);
    }

    [Fact]
    public void UsePasswordValidator_CalledMultipleTimes_LastRegistrationWins()
    {
        // Arrange
        var builder = new TestableMetalGuardianOptionsBuilder();

        // Act
        builder.UsePasswordValidator(req => req.MinimumLength = 5);
        builder.UsePasswordValidator(req => req.MinimumLength = 15);
        var services = builder.ApplyServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var validator = provider.GetService<IPasswordValidator>();
        validator.ShouldNotBeNull();
        
        // Should use last configuration (min length 15)
        validator.ValidatePassword("Short1!").ShouldBeFalse();
        validator.ValidatePassword("ThisIsLongPass1!").ShouldBeTrue();
    }

    [Fact]
    public void UsePasswordValidator_WithComplexConfiguration_RegistersValidatorCorrectly()
    {
        // Arrange
        var builder = new TestableMetalGuardianOptionsBuilder();

        // Act
        builder.UsePasswordValidator(req =>
        {
            req.MinimumLength = 8;
            req.RequireUpperCase = true;
            req.RequireLowerCase = true;
            req.RequireDigit = true;
            req.RequireSymbol = true;
            req.AllowedSymbols = ['!', '@', '#'];
        });
        var services = builder.ApplyServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var validator = provider.GetService<IPasswordValidator>();
        validator.ShouldNotBeNull();
        
        // Valid password with allowed symbol
        validator.ValidatePassword("Password1!").ShouldBeTrue();
        
        // Invalid - symbol not in allowed list
        validator.ValidatePassword("Password1$").ShouldBeFalse();
    }
}
