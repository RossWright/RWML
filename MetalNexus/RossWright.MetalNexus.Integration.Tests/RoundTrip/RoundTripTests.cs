using System.Net;
using System.Net.Http.Json;
using RossWright.MetalNexus.Integration.Tests.Infrastructure;

namespace RossWright.MetalNexus.Integration.Tests.RoundTrip;

/// <summary>
/// Phase A smoke test — proves the in-process server harness works end-to-end.
/// </summary>
public class RoundTripTests : IClassFixture<MetalNexusTestFactory>
{
    private readonly HttpClient _http;

    public RoundTripTests(MetalNexusTestFactory factory) =>
        _http = factory.CreateClient();

    [Fact]
    public async Task QueryParamRequest_ReachesHandlerAndReturnsEchoedMessage()
    {
        var response = await _http.GetAsync("/api/integration-tests/echo?message=hello");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<EchoResponse>();
        body.ShouldNotBeNull();
        body.Echo.ShouldBe("hello");
    }
}
