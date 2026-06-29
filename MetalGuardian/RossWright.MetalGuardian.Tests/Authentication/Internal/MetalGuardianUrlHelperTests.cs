using NSubstitute;
using RossWright.MetalGuardian.Authentication;
using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalGuardian.Tests.Authentication.Internal;

public class MetalGuardianUrlHelperTests
{
    private readonly IMetalGuardianAuthenticationClient _client;
    private readonly IMetalNexusUrlHelper _urlHelper;
    private readonly MetalGuardianUrlHelper _sut;

    public MetalGuardianUrlHelperTests()
    {
        _client = Substitute.For<IMetalGuardianAuthenticationClient>();
        _urlHelper = Substitute.For<IMetalNexusUrlHelper>();
        _sut = new MetalGuardianUrlHelper(_client, _urlHelper);
    }

    [Fact]
    public void GetUrlFor_WhenUserNotAuthenticated_ReturnsUrlWithoutAccessToken()
    {
        // Arrange
        var request = new TestRequest();
        var baseUrl = "https://api.example.com/endpoint";
        _client.GetUser(null).Returns((IAuthenticationInformation?)null);
        _urlHelper.GetUrlFor(request).Returns(baseUrl);

        // Act
        var result = _sut.GetUrlFor(request);

        // Assert
        result.ShouldBe(baseUrl);
        _client.Received(1).GetUser(null);
        _urlHelper.Received(1).GetUrlFor(request);
    }

    [Fact]
    public void GetUrlFor_WhenUserAuthenticated_ReturnsUrlWithAccessToken()
    {
        // Arrange
        var request = new TestRequest();
        var baseUrl = "https://api.example.com/endpoint";
        var token = "test-access-token-123";
        var authInfo = Substitute.For<IAuthenticationInformation>();
        authInfo.Token.Returns(token);
        _client.GetUser(null).Returns(authInfo);
        _urlHelper.GetUrlFor(request).Returns(baseUrl);

        // Act
        var result = _sut.GetUrlFor(request);

        // Assert
        result.ShouldBe("https://api.example.com/endpoint?access_token=test-access-token-123");
        _client.Received(1).GetUser(null);
        _urlHelper.Received(1).GetUrlFor(request);
    }

    [Fact]
    public void GetUrlFor_WithConnectionName_PassesConnectionNameToGetUser()
    {
        // Arrange
        var request = new TestRequest();
        var baseUrl = "https://api.example.com/endpoint";
        var connectionName = "MyConnection";
        _client.GetUser(connectionName).Returns((IAuthenticationInformation?)null);
        _urlHelper.GetUrlFor(request).Returns(baseUrl);

        // Act
        var result = _sut.GetUrlFor(request, connectionName);

        // Assert
        result.ShouldBe(baseUrl);
        _client.Received(1).GetUser(connectionName);
        _urlHelper.Received(1).GetUrlFor(request);
    }

    [Fact]
    public void GetUrlFor_WhenUrlHasExistingQueryParameters_AppendsAccessTokenWithAmpersand()
    {
        // Arrange
        var request = new TestRequest();
        var baseUrl = "https://api.example.com/endpoint?param1=value1";
        var token = "my-token";
        var authInfo = Substitute.For<IAuthenticationInformation>();
        authInfo.Token.Returns(token);
        _client.GetUser(null).Returns(authInfo);
        _urlHelper.GetUrlFor(request).Returns(baseUrl);

        // Act
        var result = _sut.GetUrlFor(request);

        // Assert
        result.ShouldBe("https://api.example.com/endpoint?param1=value1&access_token=my-token");
        _client.Received(1).GetUser(null);
        _urlHelper.Received(1).GetUrlFor(request);
    }

    [Fact]
    public void GetUrlFor_WhenTokenContainsSpecialCharacters_UrlEncodesToken()
    {
        // Arrange
        var request = new TestRequest();
        var baseUrl = "https://api.example.com/endpoint";
        var token = "token+with=special&chars";
        var authInfo = Substitute.For<IAuthenticationInformation>();
        authInfo.Token.Returns(token);
        _client.GetUser(null).Returns(authInfo);
        _urlHelper.GetUrlFor(request).Returns(baseUrl);

        // Act
        var result = _sut.GetUrlFor(request);

        // Assert
        result.ShouldBe("https://api.example.com/endpoint?access_token=token%2bwith%3dspecial%26chars");
        _client.Received(1).GetUser(null);
        _urlHelper.Received(1).GetUrlFor(request);
    }

    [Fact]
    public void GetUrlFor_WithConnectionNameAndAuthenticated_UsesConnectionNameAndAppendsToken()
    {
        // Arrange
        var request = new TestRequest();
        var baseUrl = "https://api.example.com/endpoint";
        var connectionName = "TestConnection";
        var token = "connection-token";
        var authInfo = Substitute.For<IAuthenticationInformation>();
        authInfo.Token.Returns(token);
        _client.GetUser(connectionName).Returns(authInfo);
        _urlHelper.GetUrlFor(request).Returns(baseUrl);

        // Act
        var result = _sut.GetUrlFor(request, connectionName);

        // Assert
        result.ShouldBe("https://api.example.com/endpoint?access_token=connection-token");
        _client.Received(1).GetUser(connectionName);
        _urlHelper.Received(1).GetUrlFor(request);
    }

    [Fact]
    public void GetUrlFor_PassesRequestToUrlHelper()
    {
        // Arrange
        var request = new TestRequest();
        var baseUrl = "https://api.example.com/specific-endpoint";
        _client.GetUser(null).Returns((IAuthenticationInformation?)null);
        _urlHelper.GetUrlFor(request).Returns(baseUrl);

        // Act
        var result = _sut.GetUrlFor(request);

        // Assert
        result.ShouldBe(baseUrl);
        _urlHelper.Received(1).GetUrlFor(request);
    }

    private class TestRequest
    {
    }
}
