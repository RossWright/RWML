using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;

namespace RossWright.MetalGuardian.Tests.Authentication.Internal;

public class AuthenticationDelegatingHandlerTests
{
    private const string ConnectionName = "testConnection";
    private const string TestToken = "test-token-123";

    [Fact]
    public void Constructor_WithValidParameters_AssignsFields()
    {
        // Arrange
        var authClient = Substitute.For<IMetalGuardianAuthenticationClient>();

        // Act
        var handler = new AuthenticationDelegatingHandler(authClient, ConnectionName);

        // Assert
        handler.ShouldNotBeNull();
    }

    [Fact]
    public async Task SendAsync_WithExistingAuthorizationHeader_DoesNotAddToken()
    {
        // Arrange
        var (handler, authClient, innerHandler) = CreateHandler();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "existing-token");
        innerHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);

        var client = new HttpClient(handler);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await authClient.DidNotReceive().Authenticate(Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>());
        request.Headers.Authorization.Parameter.ShouldBe("existing-token");
    }

    [Fact]
    public async Task SendAsync_WithNoAuthorizationHeaderAndValidToken_AddsBearerToken()
    {
        // Arrange
        var (handler, authClient, innerHandler) = CreateHandler();
        var authInfo = CreateAuthenticationInfo(TestToken);
        authClient.Authenticate(ConnectionName, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(authInfo);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        innerHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);

        var client = new HttpClient(handler);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await authClient.Received(1).Authenticate(ConnectionName, cancellationToken: Arg.Any<CancellationToken>());
        request.Headers.Authorization.ShouldNotBeNull();
        request.Headers.Authorization.Scheme.ShouldBe("Bearer");
        request.Headers.Authorization.Parameter.ShouldBe(TestToken);
    }

    [Fact]
    public async Task SendAsync_WithNoAuthorizationHeaderAndNullToken_DoesNotAddToken()
    {
        // Arrange
        var (handler, authClient, innerHandler) = CreateHandler();
        authClient.Authenticate(ConnectionName, cancellationToken: Arg.Any<CancellationToken>())
            .Returns((IAuthenticationInformation?)null);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        innerHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);

        var client = new HttpClient(handler);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await authClient.Received(1).Authenticate(ConnectionName, cancellationToken: Arg.Any<CancellationToken>());
        request.Headers.Authorization.ShouldBeNull();
    }

    [Fact]
    public async Task SendAsync_WithReentrantCall_DoesNotCallAuthenticateAgain()
    {
        // Arrange
        var authClient = Substitute.For<IMetalGuardianAuthenticationClient>();
        var innerHandler = new TestInnerHandler();
        var handler = new AuthenticationDelegatingHandler(authClient, ConnectionName)
        {
            InnerHandler = innerHandler
        };

        var reentrantRequest = new HttpRequestMessage(HttpMethod.Get, "https://example.com/nested");
        innerHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);

        // Configure the auth client to make a reentrant call during authentication
        var authInfo = CreateAuthenticationInfo(TestToken);
        var callCount = 0;
        authClient.Authenticate(ConnectionName, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // Simulate a reentrant call during authentication
                    var nestedClient = new HttpClient(handler);
                    var task = nestedClient.SendAsync(reentrantRequest, callInfo.Arg<CancellationToken>());
                    task.Wait();
                }
                return Task.FromResult<IAuthenticationInformation?>(authInfo);
            });

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        var client = new HttpClient(handler);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        callCount.ShouldBe(1);
        request.Headers.Authorization.ShouldNotBeNull();
        request.Headers.Authorization.Parameter.ShouldBe(TestToken);
        reentrantRequest.Headers.Authorization.ShouldBeNull();
    }

    [Fact]
    public async Task SendAsync_WithCancellationToken_PassesTokenToAuthenticate()
    {
        // Arrange
        var (handler, authClient, innerHandler) = CreateHandler();
        var authInfo = CreateAuthenticationInfo(TestToken);
        var cts = new CancellationTokenSource();
        authClient.Authenticate(ConnectionName, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(authInfo);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        innerHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);

        var client = new HttpClient(handler);

        // Act
        var response = await client.SendAsync(request, cts.Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await authClient.Received(1).Authenticate(ConnectionName, Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    private static (AuthenticationDelegatingHandler handler, IMetalGuardianAuthenticationClient authClient, TestInnerHandler innerHandler) CreateHandler()
    {
        var authClient = Substitute.For<IMetalGuardianAuthenticationClient>();
        var innerHandler = new TestInnerHandler();
        var handler = new AuthenticationDelegatingHandler(authClient, ConnectionName)
        {
            InnerHandler = innerHandler
        };
        return (handler, authClient, innerHandler);
    }

    private static IAuthenticationInformation CreateAuthenticationInfo(string token)
    {
        var authInfo = Substitute.For<IAuthenticationInformation>();
        authInfo.Token.Returns(token);
        return authInfo;
    }

    private sealed class TestInnerHandler : HttpMessageHandler
    {
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Response);
        }
    }
}
