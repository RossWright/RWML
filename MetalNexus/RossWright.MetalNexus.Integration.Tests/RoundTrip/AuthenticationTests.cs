using System.Net;
using RossWright.MetalNexus.Integration.Tests.Infrastructure;

namespace RossWright.MetalNexus.Integration.Tests.RoundTrip;

/// <summary>
/// Phase E — proves that <c>[Anonymous]</c>, <c>[Authenticated]</c>, provisional,
/// and role-based authorization attributes are enforced correctly by the middleware.
///
/// Each test creates its own in-process server fixture with the appropriate
/// <see cref="TestAuthOptions"/> so the fake auth handler returns the right principal.
/// </summary>
public class AuthenticationTests
{
    // ── Anonymous ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Anonymous_Endpoint_NoAuth_Returns200()
    {
        // No auth service wired at all — anonymous endpoint must still succeed.
        var factory = new MetalNexusTestFactory();
        await factory.InitializeAsync();
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/integration-tests/auth/anonymous");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await factory.DisposeAsync();
    }

    // ── Authenticated — unauthenticated caller ────────────────────────────────

    [Fact]
    public async Task Authenticated_NoToken_Returns401()
    {
        var factory = new MetalNexusTestFactory(new TestAuthOptions { IsAuthenticated = false });
        await factory.InitializeAsync();
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/integration-tests/auth/require-auth");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        await factory.DisposeAsync();
    }

    // ── Authenticated — valid authenticated caller ────────────────────────────

    [Fact]
    public async Task Authenticated_ValidToken_Returns200()
    {
        var factory = new MetalNexusTestFactory(new TestAuthOptions { IsAuthenticated = true });
        await factory.InitializeAsync();
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/integration-tests/auth/require-auth");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await factory.DisposeAsync();
    }

    // ── Provisional ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Provisional_AllowProvisional_Returns200()
    {
        var factory = new MetalNexusTestFactory(new TestAuthOptions
        {
            IsAuthenticated = true,
            IsProvisional = true
        });
        await factory.InitializeAsync();
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/integration-tests/auth/allow-provisional");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await factory.DisposeAsync();
    }

    [Fact]
    public async Task Provisional_NoAllowProvisional_Returns401()
    {
        var factory = new MetalNexusTestFactory(new TestAuthOptions
        {
            IsAuthenticated = true,
            IsProvisional = true
        });
        await factory.InitializeAsync();
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/integration-tests/auth/no-provisional");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        await factory.DisposeAsync();
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AuthorizedRoles_WrongRole_Returns403()
    {
        var factory = new MetalNexusTestFactory(new TestAuthOptions
        {
            IsAuthenticated = true,
            Roles = ["User"]   // Admin required
        });
        await factory.InitializeAsync();
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/integration-tests/auth/require-admin");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await factory.DisposeAsync();
    }

    [Fact]
    public async Task AuthorizedRoles_CorrectRole_Returns200()
    {
        var factory = new MetalNexusTestFactory(new TestAuthOptions
        {
            IsAuthenticated = true,
            Roles = ["Admin"]
        });
        await factory.InitializeAsync();
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/integration-tests/auth/require-admin");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await factory.DisposeAsync();
    }
}
