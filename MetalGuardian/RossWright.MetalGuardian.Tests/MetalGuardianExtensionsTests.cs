using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Security.Claims;

namespace RossWright.MetalGuardian.Tests;

public class MetalGuardianExtensionsTests
{
    [Fact]
    public void AddMetalGuardianClient_ShouldInvokeCallback_WhenCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var callbackInvoked = false;

        // Act
        var result = services.AddMetalGuardianClient(builder =>
        {
            callbackInvoked = true;
        });

        // Assert
        callbackInvoked.ShouldBeTrue();
        result.ShouldBe(services);
    }

    [Fact]
    public void AddMetalGuardianClient_ShouldReturnServiceCollection_WhenCalled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddMetalGuardianClient(builder => { });

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeSameAs(services);
    }

    [Fact]
    public void AddMetalGuardianClient_ShouldPassBuilderToCallback_WhenCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        IMetalGuardianClientOptionsBuilder? capturedBuilder = null;

        // Act
        services.AddMetalGuardianClient(builder =>
        {
            capturedBuilder = builder;
        });

        // Assert
        capturedBuilder.ShouldNotBeNull();
        capturedBuilder.ShouldBeOfType<MetalGuardianClientOptionsBuilder>();
    }

    [Fact]
    public void AddMetalGuardianClient_ShouldRegisterServices_WhenCalled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMetalGuardianClient(builder =>
        {
            builder.UseMetalNexusAuthenticationEndpoints();
        });

        // Assert
        services.ShouldNotBeEmpty();
    }

    [Fact]
    public void DecodeAccessToken_ShouldReturnNull_WhenAccessTokenIsNull()
    {
        // Arrange
        var authTokens = new AuthenticationTokens { AccessToken = null! };

        // Act
        var result = authTokens.DecodeAccessToken();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void DecodeAccessToken_ShouldReturnNull_WhenAccessTokenIsEmpty()
    {
        // Arrange
        var authTokens = new AuthenticationTokens { AccessToken = string.Empty };

        // Act
        var result = authTokens.DecodeAccessToken();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void DecodeAccessToken_ShouldReturnNull_WhenAccessTokenIsWhitespace()
    {
        // Arrange
        var authTokens = new AuthenticationTokens { AccessToken = "   " };

        // Act
        var result = authTokens.DecodeAccessToken();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void DecodeAccessToken_ShouldReturnNull_WhenAccessTokenIsMalformed()
    {
        // Arrange
        var authTokens = new AuthenticationTokens { AccessToken = "invalid.token" };

        // Act
        var result = authTokens.DecodeAccessToken();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void DecodeAccessToken_ShouldReturnAuthenticationInformation_WhenAccessTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        
        var payload = $$"""{"{{ClaimTypes.NameIdentifier}}":"{{userId}}","{{ClaimTypes.Name}}":"{{userName}}","exp":{{exp}}}""";
        var payloadBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
        var token = $"header.{payloadBase64}.signature";
        
        var authTokens = new AuthenticationTokens { AccessToken = token };

        // Act
        var result = authTokens.DecodeAccessToken();

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.UserName.ShouldBe(userName);
    }

    [Fact]
    public void DecodeAccessToken_ShouldReturnAuthenticationInformation_WhenAccessTokenHasNoPaddingRequired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        
        // Create payload with length that's multiple of 4 (no padding needed)
        var payload = $$"""{"{{ClaimTypes.NameIdentifier}}":"{{userId}}","exp":{{exp}},"extra":"data"}""";
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        var payloadBase64 = Convert.ToBase64String(payloadBytes).TrimEnd('=');
        
        // Ensure no padding by adjusting payload if needed
        while (payloadBase64.Length % 4 != 0)
        {
            payload += " ";
            payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
            payloadBase64 = Convert.ToBase64String(payloadBytes).TrimEnd('=');
        }
        
        var token = $"header.{payloadBase64}.signature";
        var authTokens = new AuthenticationTokens { AccessToken = token };

        // Act
        var result = authTokens.DecodeAccessToken();

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
    }
}
