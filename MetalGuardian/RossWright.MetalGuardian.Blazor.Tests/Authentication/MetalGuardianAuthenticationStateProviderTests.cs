using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using RossWright.MetalGuardian;

namespace RossWright.MetalGuardian.Blazor.Tests.Authentication;

public class MetalGuardianAuthenticationStateProviderTests
{
    private static MetalGuardianAuthenticationStateProvider BuildProvider(
        IMetalGuardianAuthenticationClient client,
        string? connectionName = null) =>
        new(client, NullLogger<MetalGuardianAuthenticationStateProvider>.Instance, connectionName);

    // --- GetAuthenticationStateAsync ---

    [Fact]
    public async Task AuthenticatedUser_ReturnsClaimsPrincipalWithIdentity()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var authInfo = new FakeAuthenticationInformation(Guid.NewGuid(), "alice");
        client.Authenticate(default, default, default).ReturnsForAnyArgs(authInfo);
        var provider = BuildProvider(client);

        var state = await provider.GetAuthenticationStateAsync();

        state.User.Identity!.IsAuthenticated.ShouldBeTrue();
        state.User.Identity.Name.ShouldBe("alice");
    }

    [Fact]
    public async Task NullAuthentication_ReturnsEmptyIdentity()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.Authenticate(default, default, default).ReturnsForAnyArgs((IAuthenticationInformation?)null);
        var provider = BuildProvider(client);

        var state = await provider.GetAuthenticationStateAsync();

        state.User.Identity!.IsAuthenticated.ShouldBeFalse();
    }

    [Fact]
    public async Task MultipleRoleClaims_PreservedAsIndividualRoleClaims()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        var authInfo = new FakeAuthenticationInformation(Guid.NewGuid(), "bob",
            new Claim(System.Security.Claims.ClaimTypes.Role, "admin"),
            new Claim(System.Security.Claims.ClaimTypes.Role, "editor"));
        client.Authenticate(default, default, default).ReturnsForAnyArgs(authInfo);
        var provider = BuildProvider(client);

        var state = await provider.GetAuthenticationStateAsync();

        var roles = state.User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
        roles.ShouldContain("admin");
        roles.ShouldContain("editor");
        roles.Count.ShouldBe(2);
    }

    [Fact]
    public async Task AuthenticateThrows_ReturnsUnauthenticated()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.Authenticate(default, default, default)
            .ReturnsForAnyArgs(Task.FromException<IAuthenticationInformation?>(new HttpRequestException("server error")));
        var provider = BuildProvider(client);

        var state = await provider.GetAuthenticationStateAsync();

        state.User.Identity!.IsAuthenticated.ShouldBeFalse();
    }

    // --- AuthenticationChanged event ---

    [Fact]
    public async Task AuthenticationChangedEvent_NotifiesStateChanged()
    {
        var client = Substitute.For<IMetalGuardianAuthenticationClient>();
        client.Authenticate(default, default, default).ReturnsForAnyArgs((IAuthenticationInformation?)null);
        var provider = BuildProvider(client);

        AuthenticationState? notifiedState = null;
        provider.AuthenticationStateChanged += async task =>
        {
            notifiedState = await task;
        };

        var authInfo = new FakeAuthenticationInformation(Guid.NewGuid(), "charlie");
        client.AuthenticationChanged += Raise.Event<AuthenticationChangedEventHandler>("default", authInfo, CancellationToken.None);

        notifiedState.ShouldNotBeNull();
        notifiedState!.User.Identity!.IsAuthenticated.ShouldBeTrue();
    }
}

// ─── Fake IAuthenticationInformation ─────────────────────────────────────────

internal class FakeAuthenticationInformation : IAuthenticationInformation
{
    private readonly List<Claim> _extraClaims;

    public FakeAuthenticationInformation(Guid userId, string userName, params Claim[] extraClaims)
    {
        UserId = userId;
        UserName = userName;
        _extraClaims = [.. extraClaims];
        Token = "fake-token";
        ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
    }

    public string Token { get; }
    public DateTimeOffset ExpiresOn { get; }
    public Guid UserId { get; }
    public string? UserName { get; }
    public bool IsProvisional => false;
    public bool? IsKnownDevice => null;

    public string? GetAdditionalClaim(string claimType) => null;

    public ClaimsIdentity AsClaimsIdentity()
    {
        var claims = new List<Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, UserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Name, UserName ?? string.Empty)
        };
        claims.AddRange(_extraClaims);
        return new ClaimsIdentity(claims, "MetalGuardian");
    }
}
