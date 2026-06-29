using System.Net;
using System.Net.Http.Json;
using RossWright.MetalNexus.Integration.Tests.Infrastructure;

namespace RossWright.MetalNexus.Integration.Tests.RoundTrip;

/// <summary>
/// Phase F — proves routing edge cases: unregistered paths fall through, static
/// routes beat bracket patterns, wrong HTTP method falls through, and multiple
/// registered endpoints are each dispatched correctly.
/// </summary>
public class RoutingTests : IClassFixture<MetalNexusTestFactory>
{
    private readonly HttpClient _http;

    public RoutingTests(MetalNexusTestFactory factory) =>
        _http = factory.CreateClient();

    // ── F1 — Unregistered path falls through ─────────────────────────────────

    [Fact]
    public async Task UnregisteredPath_FallsThrough_Returns404()
    {
        var response = await _http.GetAsync("/api/integration-tests/this-path-does-not-exist");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ── F2 — Multiple endpoints dispatched correctly ──────────────────────────

    [Fact]
    public async Task MultipleEndpoints_EachRoutedCorrectly()
    {
        // Hit the static route
        var staticResponse = await _http.GetAsync("/api/integration-tests/routing/static");
        staticResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var staticBody = await staticResponse.Content.ReadFromJsonAsync<RoutingResponse>();
        staticBody!.Handler.ShouldBe("static");

        // Hit the path-param route with a real token
        var paramResponse = await _http.GetAsync("/api/integration-tests/routing/abc123");
        paramResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var paramBody = await paramResponse.Content.ReadFromJsonAsync<RoutingResponse>();
        paramBody!.Handler.ShouldBe("param");
        paramBody.Token.ShouldBe("abc123");
    }

    // ── F3 — Static path beats bracket pattern ───────────────────────────────

    [Fact]
    public async Task PathParam_vs_Static_PrefersStatic()
    {
        // "/api/integration-tests/routing/static" is an exact registration.
        // "/api/integration-tests/routing/{Token}" is the competing bracket route.
        // The middleware must return the static handler, not the param handler.
        var response = await _http.GetAsync("/api/integration-tests/routing/static");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RoutingResponse>();
        body!.Handler.ShouldBe("static");
    }

    // ── F4 — Wrong HTTP method falls through ─────────────────────────────────

    [Fact]
    public async Task HttpMethod_WrongMethod_FallsThrough()
    {
        // PostOnlyRequest is registered as POST.  A GET to the same path must
        // fall through (no handler matched) so ASP.NET returns 404.
        var response = await _http.GetAsync("/api/integration-tests/routing-post-only");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
