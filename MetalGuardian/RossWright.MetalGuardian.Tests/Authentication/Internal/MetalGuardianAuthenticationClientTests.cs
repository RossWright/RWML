using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalGuardian.Authentication;
using Shouldly;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace RossWright.MetalGuardian.Tests.Authentication.Internal;

public class MetalGuardianAuthenticationClientTests
{
    private const string DefaultConnectionName = "default";

    [Fact]
    public void Constructor_WithAllParameters_AssignsAllFields()
    {
        // Arrange
        var accessTokenRepository = Substitute.For<IAccessTokenRepository>();
        var authenticationApiService = Substitute.For<IAuthenticationApiService>();
        var baseAddressRepository = Substitute.For<IBaseAddressRepository>();
        var authenticationTokenStorage = Substitute.For<IAuthenticationTokenStorage>();

        // Act
        var client = new MetalGuardianAuthenticationClient(
            accessTokenRepository,
            authenticationApiService,
            baseAddressRepository,
            authenticationTokenStorage);

        // Assert
        client.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithOnlyRequiredParameters_AssignsFields()
    {
        // Arrange
        var accessTokenRepository = Substitute.For<IAccessTokenRepository>();
        var authenticationApiService = Substitute.For<IAuthenticationApiService>();
        var baseAddressRepository = Substitute.For<IBaseAddressRepository>();

        // Act
        var client = new MetalGuardianAuthenticationClient(
            accessTokenRepository,
            authenticationApiService,
            baseAddressRepository);

        // Assert
        client.ShouldNotBeNull();
    }

    [Fact]
    public async Task Login_WithCredentials_CallsApiServiceAndSavesTokens()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AuthenticationApiService.Login(
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<string>())
            .Returns(tokens);

        // Act
        var result = await client.Login("user@example.com", "password", DefaultConnectionName);

        // Assert
        await mocks.AuthenticationApiService.Received(1).Login(
            "user@example.com", 
            "password", 
            DefaultConnectionName);
        mocks.AccessTokenRepository.Received(1).Set(DefaultConnectionName, tokens);
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Login_WithCredentialsAndNullConnectionName_UsesDefaultConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AuthenticationApiService.Login(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(tokens);

        // Act
        var result = await client.Login("user@example.com", "password");

        // Assert
        await mocks.AuthenticationApiService.Received(1).Login(
            "user@example.com",
            "password",
            Microsoft.Extensions.Options.Options.DefaultName);
        mocks.AccessTokenRepository.Received(1).Set(DefaultConnectionName, tokens);
    }

    [Fact]
    public async Task Login_WithCredentialsReturningNull_ReturnsNull()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AuthenticationApiService.Login(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns((AuthenticationTokens?)null);

        // Act
        var result = await client.Login("user@example.com", "password");

        // Assert
        result.ShouldBeNull();
        mocks.AccessTokenRepository.DidNotReceive().Set(Arg.Any<string>(), Arg.Any<AuthenticationTokens>());
    }

    [Fact]
    public async Task Login_WithTokens_SavesTokensAndReturnsAccessToken()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);

        // Act
        var result = await client.Login(tokens, DefaultConnectionName);

        // Assert
        mocks.AccessTokenRepository.Received(1).Set(DefaultConnectionName, tokens);
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Login_WithTokensAndNullConnectionName_UsesDefaultConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);

        // Act
        var result = await client.Login(tokens);

        // Assert
        mocks.AccessTokenRepository.Received(1).Set(DefaultConnectionName, tokens);
    }

    [Fact]
    public async Task Authenticate_WithNoTokensInRepository_ReturnsNull()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        // Act
        var result = await client.Authenticate();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Authenticate_WithNullConnectionName_UsesDefaultConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        // Act
        await client.Authenticate();

        // Assert
        _ = mocks.BaseAddressRepository.Received(1).DefaultConnectionName;
    }

    [Fact]
    public async Task Authenticate_WithTokensInStorageButNotRepository_LoadsFromStorage()
    {
        // Arrange
        var (client, mocks) = CreateClient(includeTokenStorage: true);
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });
        mocks.AuthenticationTokenStorage!.LoadTokens(DefaultConnectionName, Arg.Any<CancellationToken>())
            .Returns(tokens);

        // Act
        var result = await client.Authenticate();

        // Assert
        await mocks.AuthenticationTokenStorage.Received(1).LoadTokens(DefaultConnectionName, Arg.Any<CancellationToken>());
        mocks.AccessTokenRepository.Received(1).Set(DefaultConnectionName, tokens);
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Authenticate_WithValidNonExpiredToken_ReturnsTokenWithoutRefresh()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens(expiresInMinutes: 30);
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });

        // Act
        var result = await client.Authenticate();

        // Assert
        result.ShouldNotBeNull();
        await mocks.AuthenticationApiService.DidNotReceive().Refresh(Arg.Any<AuthenticationTokens>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Authenticate_WithExpiredToken_RefreshesToken()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var oldTokens = CreateAuthenticationTokens(expiresInMinutes: -30);
        var newTokens = CreateAuthenticationTokens(expiresInMinutes: 30);
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = oldTokens;
                return true;
            });
        mocks.AuthenticationApiService.Refresh(oldTokens, DefaultConnectionName)
            .Returns(newTokens);

        // Act
        var result = await client.Authenticate();

        // Assert
        await mocks.AuthenticationApiService.Received(1).Refresh(oldTokens, DefaultConnectionName);
        mocks.AccessTokenRepository.Received(1).Set(DefaultConnectionName, newTokens);
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Authenticate_WithForceRefreshTrue_RefreshesEvenIfNotExpired()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var oldTokens = CreateAuthenticationTokens(expiresInMinutes: 30);
        var newTokens = CreateAuthenticationTokens(expiresInMinutes: 60);
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = oldTokens;
                return true;
            });
        mocks.AuthenticationApiService.Refresh(oldTokens, DefaultConnectionName)
            .Returns(newTokens);

        // Act
        var result = await client.Authenticate(forceRefesh: true);

        // Assert
        await mocks.AuthenticationApiService.Received(1).Refresh(oldTokens, DefaultConnectionName);
        mocks.AccessTokenRepository.Received(1).Set(DefaultConnectionName, newTokens);
    }

    [Fact]
    public async Task Authenticate_WhenRefreshThrowsNotAuthenticatedException_RemovesTokenAndReturnsNull()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens(expiresInMinutes: -30);
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });
        mocks.AuthenticationApiService.Refresh(tokens, DefaultConnectionName)
            .Returns<AuthenticationTokens?>(x => throw new RossWright.NotAuthenticatedException());

        // Act
        var result = await client.Authenticate();

        // Assert
        mocks.AccessTokenRepository.Received(1).Remove(DefaultConnectionName);
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Logout_WithNullConnectionName_UsesDefaultConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        // Act
        await client.Logout();

        // Assert
        _ = mocks.BaseAddressRepository.Received(1).DefaultConnectionName;
    }

    [Fact]
    public async Task Logout_WithTokenStorage_ClearsTokensFromStorage()
    {
        // Arrange
        var (client, mocks) = CreateClient(includeTokenStorage: true);
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        // Act
        await client.Logout();

        // Assert
        await mocks.AuthenticationTokenStorage!.Received(1).ClearTokens(DefaultConnectionName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Logout_WithTokensInRepository_CallsApiLogoutAndRemovesTokens()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });

        // Act
        await client.Logout();

        // Assert
        await mocks.AuthenticationApiService.Received(1).Logout(tokens, DefaultConnectionName, Arg.Any<CancellationToken>());
        mocks.AccessTokenRepository.Received(1).Remove(DefaultConnectionName);
    }

    [Fact]
    public async Task Logout_WithTokensAndAuthenticationChangedHandler_FiresEvent()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        var eventFired = false;
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });
        client.AuthenticationChanged += (connectionName, authInfo, cancellationToken) =>
        {
            eventFired = true;
            connectionName.ShouldBe(DefaultConnectionName);
            authInfo.ShouldBeNull();
            return Task.CompletedTask;
        };

        // Act
        await client.Logout();

        // Assert
        eventFired.ShouldBeTrue();
    }

    [Fact]
    public async Task Logout_WithNoTokensInRepository_DoesNotCallApiLogout()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        // Act
        await client.Logout();

        // Assert
        await mocks.AuthenticationApiService.DidNotReceive().Logout(Arg.Any<AuthenticationTokens>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_WithTokensAndAuthenticationChangedHandler_FiresEvent()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        var eventFired = false;
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        client.AuthenticationChanged += (connectionName, authInfo, cancellationToken) =>
        {
            eventFired = true;
            connectionName.ShouldBe(DefaultConnectionName);
            authInfo.ShouldNotBeNull();
            return Task.CompletedTask;
        };

        // Act
        await client.Login(tokens, DefaultConnectionName);

        // Assert
        eventFired.ShouldBeTrue();
    }

    [Fact]
    public async Task Login_WithTokenStorageAndTokens_SavesTokensToStorage()
    {
        // Arrange
        var (client, mocks) = CreateClient(includeTokenStorage: true);
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);

        // Act
        await client.Login(tokens, DefaultConnectionName);

        // Assert
        await mocks.AuthenticationTokenStorage!.Received(1).SaveTokens(DefaultConnectionName, tokens, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Authenticate_WithTokenStorageAndNullTokensInStorage_ReturnsNull()
    {
        // Arrange
        var (client, mocks) = CreateClient(includeTokenStorage: true);
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });
        mocks.AuthenticationTokenStorage!.LoadTokens(DefaultConnectionName, Arg.Any<CancellationToken>())
            .Returns((AuthenticationTokens?)null);

        // Act
        var result = await client.Authenticate();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Authenticate_WhenRefreshFailsAndTokenStorageExists_ClearsTokens()
    {
        // Arrange
        var (client, mocks) = CreateClient(includeTokenStorage: true);
        var tokens = CreateAuthenticationTokens(expiresInMinutes: -30);
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });
        mocks.AuthenticationApiService.Refresh(tokens, DefaultConnectionName)
            .Returns<AuthenticationTokens?>(x => throw new RossWright.NotAuthenticatedException());

        // Act
        await client.Authenticate();

        // Assert
        await mocks.AuthenticationTokenStorage!.Received(1).ClearTokens(DefaultConnectionName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Authenticate_WhenRefreshFailsAndAuthenticationChangedHandlerExists_FiresEvent()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens(expiresInMinutes: -30);
        var eventFired = false;
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });
        mocks.AuthenticationApiService.Refresh(tokens, DefaultConnectionName)
            .Returns<AuthenticationTokens?>(x => throw new RossWright.NotAuthenticatedException());
        client.AuthenticationChanged += (connectionName, authInfo, cancellationToken) =>
        {
            eventFired = true;
            connectionName.ShouldBe(DefaultConnectionName);
            authInfo.ShouldBeNull();
            return Task.CompletedTask;
        };

        // Act
        await client.Authenticate();

        // Assert
        eventFired.ShouldBeTrue();
    }

    [Fact]
    public async Task Authenticate_WhenRefreshSucceedsAndAuthenticationChangedHandlerExists_FiresEvent()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var oldTokens = CreateAuthenticationTokens(expiresInMinutes: -30);
        var newTokens = CreateAuthenticationTokens(expiresInMinutes: 30);
        var eventFired = false;
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = oldTokens;
                return true;
            });
        mocks.AuthenticationApiService.Refresh(oldTokens, DefaultConnectionName)
            .Returns(newTokens);
        client.AuthenticationChanged += (connectionName, authInfo, cancellationToken) =>
        {
            eventFired = true;
            connectionName.ShouldBe(DefaultConnectionName);
            authInfo.ShouldNotBeNull();
            return Task.CompletedTask;
        };

        // Act
        await client.Authenticate();

        // Assert
        eventFired.ShouldBeTrue();
    }

    [Fact]
    public async Task Logout_WithoutAuthenticationChangedHandler_DoesNotThrow()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });

        // Act & Assert
        await Should.NotThrowAsync(async () => await client.Logout());
    }

    [Fact]
    public async Task Authenticate_WithSpecificConnectionName_UsesThatConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var connectionName = "specific-connection";
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        // Act
        await client.Authenticate(connectionName);

        // Assert
        mocks.AccessTokenRepository.Received(1).TryGet(connectionName, out Arg.Any<AuthenticationTokens?>());
    }

    [Fact]
    public async Task Logout_WithSpecificConnectionName_UsesThatConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient(includeTokenStorage: true);
        var connectionName = "specific-connection";
        mocks.AccessTokenRepository.TryGet(Arg.Any<string>(), out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        // Act
        await client.Logout(connectionName);

        // Assert
        await mocks.AuthenticationTokenStorage!.Received(1).ClearTokens(connectionName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void IsAuthenticated_WithConnectionNameProvided_UsesProvidedConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var connectionName = "specific-connection";
        mocks.AccessTokenRepository.Contains(connectionName).Returns(true);

        // Act
        var result = client.IsAuthenticated(connectionName);

        // Assert
        mocks.AccessTokenRepository.Received(1).Contains(connectionName);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAuthenticated_WithNullConnectionName_UsesDefaultConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.Contains(DefaultConnectionName).Returns(true);

        // Act
        var result = client.IsAuthenticated(null);

        // Assert
        mocks.AccessTokenRepository.Received(1).Contains(DefaultConnectionName);
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAuthenticated_WhenTokenRepositoryContainsToken_ReturnsTrue()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.Contains(DefaultConnectionName).Returns(true);

        // Act
        var result = client.IsAuthenticated();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAuthenticated_WhenTokenRepositoryDoesNotContainToken_ReturnsFalse()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.Contains(DefaultConnectionName).Returns(false);

        // Act
        var result = client.IsAuthenticated();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GetUser_WithConnectionNameProvided_UsesProvidedConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var connectionName = "specific-connection";
        var tokens = CreateAuthenticationTokens();
        mocks.AccessTokenRepository.TryGet(connectionName, out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });

        // Act
        var result = client.GetUser(connectionName);

        // Assert
        mocks.AccessTokenRepository.Received(1).TryGet(connectionName, out Arg.Any<AuthenticationTokens?>());
        result.ShouldNotBeNull();
    }

    [Fact]
    public void GetUser_WithNullConnectionName_UsesDefaultConnectionName()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(DefaultConnectionName, out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });

        // Act
        var result = client.GetUser(null);

        // Assert
        mocks.AccessTokenRepository.Received(1).TryGet(DefaultConnectionName, out Arg.Any<AuthenticationTokens?>());
        result.ShouldNotBeNull();
    }

    [Fact]
    public void GetUser_WhenTokenRepositoryContainsValidToken_ReturnsAuthenticationInformation()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        var tokens = CreateAuthenticationTokens();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(DefaultConnectionName, out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = tokens;
                return true;
            });

        // Act
        var result = client.GetUser();

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public void GetUser_WhenTokenRepositoryDoesNotContainToken_ReturnsNull()
    {
        // Arrange
        var (client, mocks) = CreateClient();
        mocks.BaseAddressRepository.DefaultConnectionName.Returns(DefaultConnectionName);
        mocks.AccessTokenRepository.TryGet(DefaultConnectionName, out Arg.Any<AuthenticationTokens?>())
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        // Act
        var result = client.GetUser();

        // Assert
        result.ShouldBeNull();
    }

    private static (MetalGuardianAuthenticationClient Client, Mocks Mocks) CreateClient(
        bool includeTokenStorage = false)
    {
        var accessTokenRepository = Substitute.For<IAccessTokenRepository>();
        var authenticationApiService = Substitute.For<IAuthenticationApiService>();
        var baseAddressRepository = Substitute.For<IBaseAddressRepository>();
        var authenticationTokenStorage = includeTokenStorage ? Substitute.For<IAuthenticationTokenStorage>() : null;

        var client = new MetalGuardianAuthenticationClient(
            accessTokenRepository,
            authenticationApiService,
            baseAddressRepository,
            authenticationTokenStorage);

        return (client, new Mocks
        {
            AccessTokenRepository = accessTokenRepository,
            AuthenticationApiService = authenticationApiService,
            BaseAddressRepository = baseAddressRepository,
            AuthenticationTokenStorage = authenticationTokenStorage
        });
    }

    private static AuthenticationTokens CreateAuthenticationTokens(int expiresInMinutes = 60)
    {
        var userId = Guid.NewGuid();
        var expiresOn = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes);
        var exp = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds() 
            + (long)expiresOn.Subtract(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds;

        var payload = new Dictionary<string, object>
        {
            ["exp"] = exp,
            [ClaimTypes.NameIdentifier] = userId.ToString(),
            [ClaimTypes.Name] = "Test User"
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
        var payloadBase64 = Convert.ToBase64String(payloadBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var accessToken = $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{payloadBase64}.signature";

        return new AuthenticationTokens
        {
            AccessToken = accessToken,
            RefreshToken = "refresh_token"
        };
    }

    private class Mocks
    {
        public IAccessTokenRepository AccessTokenRepository { get; init; } = null!;
        public IAuthenticationApiService AuthenticationApiService { get; init; } = null!;
        public IBaseAddressRepository BaseAddressRepository { get; init; } = null!;
        public IAuthenticationTokenStorage? AuthenticationTokenStorage { get; init; }
    }
}
