namespace RossWright.MetalGuardian.Blazor.Tests.Authentication;

public class IMetalGuardianAuthenticationClientTests
{
    // --- Login(string, string, string?, CancellationToken) ---

    [Fact]
    public async Task Login_WithCredentials_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login("testuser", "password123", null, default).Returns(expectedAuth);

        var result = await client.Login("testuser", "password123", null, default);

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public async Task Login_WithCredentialsAndConnectionName_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login("testuser", "password123", "myConnection", default).Returns(expectedAuth);

        var result = await client.Login("testuser", "password123", "myConnection", default);

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsNull()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.Login("baduser", "badpassword", null, default).Returns((IAuthenticationInformation?)null);

        var result = await client.Login("baduser", "badpassword", null, default);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Login_WithCancellationToken_PassesCancellationToken()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var cts = new CancellationTokenSource();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login("testuser", "password123", null, cts.Token).Returns(expectedAuth);

        var result = await client.Login("testuser", "password123", null, cts.Token);

        result.ShouldBe(expectedAuth);
    }

    // --- Login(AuthenticationTokens, string?, CancellationToken) ---

    [Fact]
    public async Task Login_WithTokens_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login(tokens, null, default).Returns(expectedAuth);

        var result = await client.Login(tokens, null, default);

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public async Task Login_WithTokensAndConnectionName_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login(tokens, "myConnection", default).Returns(expectedAuth);

        var result = await client.Login(tokens, "myConnection", default);

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public async Task Login_WithInvalidTokens_ReturnsNull()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var tokens = new AuthenticationTokens { AccessToken = "invalid", RefreshToken = "invalid" };
        client.Login(tokens, null, default).Returns((IAuthenticationInformation?)null);

        var result = await client.Login(tokens, null, default);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Login_WithTokensAndCancellationToken_PassesCancellationToken()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        var cts = new CancellationTokenSource();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login(tokens, null, cts.Token).Returns(expectedAuth);

        var result = await client.Login(tokens, null, cts.Token);

        result.ShouldBe(expectedAuth);
    }

    // --- Authenticate(string?, bool, CancellationToken) ---

    [Fact]
    public async Task Authenticate_WithoutForceRefresh_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Authenticate(null, false, default).Returns(expectedAuth);

        var result = await client.Authenticate(null, false, default);

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public async Task Authenticate_WithForceRefresh_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Authenticate(null, true, default).Returns(expectedAuth);

        var result = await client.Authenticate(null, true, default);

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public async Task Authenticate_WithConnectionName_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Authenticate("myConnection", false, default).Returns(expectedAuth);

        var result = await client.Authenticate("myConnection", false, default);

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public async Task Authenticate_WhenExpired_ReturnsNull()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.Authenticate(null, false, default).Returns((IAuthenticationInformation?)null);

        var result = await client.Authenticate(null, false, default);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Authenticate_WithCancellationToken_PassesCancellationToken()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var cts = new CancellationTokenSource();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Authenticate(null, false, cts.Token).Returns(expectedAuth);

        var result = await client.Authenticate(null, false, cts.Token);

        result.ShouldBe(expectedAuth);
    }

    // --- IsAuthenticated(string?) ---

    [Fact]
    public void IsAuthenticated_WhenAuthenticated_ReturnsTrue()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.IsAuthenticated(null).Returns(true);

        var result = client.IsAuthenticated(null);

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAuthenticated_WhenNotAuthenticated_ReturnsFalse()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.IsAuthenticated(null).Returns(false);

        var result = client.IsAuthenticated(null);

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsAuthenticated_WithConnectionName_ReturnsTrue()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.IsAuthenticated("myConnection").Returns(true);

        var result = client.IsAuthenticated("myConnection");

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAuthenticated_WithDifferentConnectionName_ReturnsFalse()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.IsAuthenticated("connection1").Returns(true);
        client.IsAuthenticated("connection2").Returns(false);

        var result1 = client.IsAuthenticated("connection1");
        var result2 = client.IsAuthenticated("connection2");

        result1.ShouldBeTrue();
        result2.ShouldBeFalse();
    }

    // --- GetUser(string?) ---

    [Fact]
    public void GetUser_WhenAuthenticated_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.GetUser(null).Returns(expectedAuth);

        var result = client.GetUser(null);

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public void GetUser_WhenNotAuthenticated_ReturnsNull()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.GetUser(null).Returns((IAuthenticationInformation?)null);

        var result = client.GetUser(null);

        result.ShouldBeNull();
    }

    [Fact]
    public void GetUser_WithConnectionName_ReturnsAuthenticationInformation()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.GetUser("myConnection").Returns(expectedAuth);

        var result = client.GetUser("myConnection");

        result.ShouldBe(expectedAuth);
    }

    [Fact]
    public void GetUser_WithDifferentConnectionNames_ReturnsDifferentUsers()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var user1 = new FakeAuthenticationInformation(Guid.NewGuid(), "user1");
        var user2 = new FakeAuthenticationInformation(Guid.NewGuid(), "user2");
        client.GetUser("connection1").Returns(user1);
        client.GetUser("connection2").Returns(user2);

        var result1 = client.GetUser("connection1");
        var result2 = client.GetUser("connection2");

        result1.ShouldBe(user1);
        result2.ShouldBe(user2);
    }

    // --- Logout(string?, CancellationToken) ---

    [Fact]
    public async Task Logout_WithoutConnectionName_CompletesSuccessfully()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();

        await client.Logout(null, default);

        await client.Received(1).Logout(null, default);
    }

    [Fact]
    public async Task Logout_WithConnectionName_CompletesSuccessfully()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();

        await client.Logout("myConnection", default);

        await client.Received(1).Logout("myConnection", default);
    }

    [Fact]
    public async Task Logout_WithCancellationToken_PassesCancellationToken()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var cts = new CancellationTokenSource();

        await client.Logout(null, cts.Token);

        await client.Received(1).Logout(null, cts.Token);
    }

    // --- Extension Methods ---

    [Fact]
    public async Task LoginExtension_WithCredentials_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login("testuser", "password123", null, default).Returns(expectedAuth);

        var result = await client.Login("testuser", "password123", default);

        result.ShouldBe(expectedAuth);
        await client.Received(1).Login("testuser", "password123", null, default);
    }

    [Fact]
    public async Task LoginExtension_WithCredentialsAndCancellationToken_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var cts = new CancellationTokenSource();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login("testuser", "password123", null, cts.Token).Returns(expectedAuth);

        var result = await client.Login("testuser", "password123", cts.Token);

        result.ShouldBe(expectedAuth);
        await client.Received(1).Login("testuser", "password123", null, cts.Token);
    }

    [Fact]
    public async Task LoginExtension_WithTokens_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login(tokens, null, default).Returns(expectedAuth);

        var result = await client.Login(tokens, default);

        result.ShouldBe(expectedAuth);
        await client.Received(1).Login(tokens, null, default);
    }

    [Fact]
    public async Task LoginExtension_WithTokensAndCancellationToken_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var tokens = new AuthenticationTokens { AccessToken = "access", RefreshToken = "refresh" };
        var cts = new CancellationTokenSource();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Login(tokens, null, cts.Token).Returns(expectedAuth);

        var result = await client.Login(tokens, cts.Token);

        result.ShouldBe(expectedAuth);
        await client.Received(1).Login(tokens, null, cts.Token);
    }

    [Fact]
    public async Task AuthenticateExtension_WithDefaults_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Authenticate(null, false, default).Returns(expectedAuth);

        var result = await client.Authenticate();

        result.ShouldBe(expectedAuth);
        await client.Received(1).Authenticate(null, false, default);
    }

    [Fact]
    public async Task AuthenticateExtension_WithForceRefresh_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Authenticate(null, true, default).Returns(expectedAuth);

        var result = await client.Authenticate(true);

        result.ShouldBe(expectedAuth);
        await client.Received(1).Authenticate(null, true, default);
    }

    [Fact]
    public async Task AuthenticateExtension_WithCancellationToken_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var cts = new CancellationTokenSource();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Authenticate(null, false, cts.Token).Returns(expectedAuth);

        var result = await client.Authenticate(false, cts.Token);

        result.ShouldBe(expectedAuth);
        await client.Received(1).Authenticate(null, false, cts.Token);
    }

    [Fact]
    public async Task AuthenticateExtension_WithForceRefreshAndCancellationToken_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var cts = new CancellationTokenSource();
        var expectedAuth = new FakeAuthenticationInformation(Guid.NewGuid(), "testuser");
        client.Authenticate(null, true, cts.Token).Returns(expectedAuth);

        var result = await client.Authenticate(true, cts.Token);

        result.ShouldBe(expectedAuth);
        await client.Received(1).Authenticate(null, true, cts.Token);
    }

    [Fact]
    public async Task LogoutExtension_WithDefaults_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();

        await client.Logout();

        await client.Received(1).Logout(null, default);
    }

    [Fact]
    public async Task LogoutExtension_WithCancellationToken_CallsMainMethodWithNullConnectionName()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var cts = new CancellationTokenSource();

        await client.Logout(cts.Token);

        await client.Received(1).Logout(null, cts.Token);
    }
}
