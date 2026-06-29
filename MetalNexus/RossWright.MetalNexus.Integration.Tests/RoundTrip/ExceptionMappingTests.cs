using System.Net;
using System.Net.Http.Json;
using RossWright.MetalNexus.Integration.Tests.Infrastructure;

namespace RossWright.MetalNexus.Integration.Tests.RoundTrip;

/// <summary>
/// Phase D — proves that exceptions thrown by handlers are mapped to the correct
/// HTTP status codes, that the error body is serialized correctly, and that the
/// <c>IncludeServerStackTraceOnExceptions</c> option controls stack-trace inclusion.
/// </summary>
public class ExceptionMappingTests : IClassFixture<MetalNexusTestFactory>
{
    private readonly HttpClient _http;

    public ExceptionMappingTests(MetalNexusTestFactory factory)
    {
        _http = factory.CreateClient();
        // Opt into the MetalNexus JSON error envelope so we can inspect typed fields.
        _http.DefaultRequestHeaders.Add(MetalNexusConstants.ClientHeader, MetalNexusConstants.ClientHeaderValue);
    }

    [Fact]
    public async Task MetalNexusException_Default_Returns400OnClient()
    {
        var response = await _http.GetAsync("/api/integration-tests/throw-metalNexus");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ExceptionBody>();
        body.ShouldNotBeNull();
        body!.Message.ShouldBe("test bad-request error");
    }

    [Fact]
    public async Task MetalNexusException_ServerError_Returns500OnClient()
    {
        var response = await _http.GetAsync("/api/integration-tests/throw-internal-server-error");

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadFromJsonAsync<ExceptionBody>();
        body.ShouldNotBeNull();
        body!.Message.ShouldBe("test internal server error");
    }

    [Fact]
    public async Task ValidationException_Returns422OnClient()
    {
        var response = await _http.GetAsync("/api/integration-tests/throw-validation");

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var body = await response.Content.ReadFromJsonAsync<ExceptionBody>();
        body.ShouldNotBeNull();
        body!.Message.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UnauthorizedException_Returns401OnClient()
    {
        var response = await _http.GetAsync("/api/integration-tests/throw-unauthorized");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<ExceptionBody>();
        body.ShouldNotBeNull();
        body!.Message.ShouldBe("test unauthorized error");
    }

    [Fact]
    public async Task InternalServerError_StackTrace_ControlledByOption()
    {
        // Without stack trace (default factory)
        var responseWithout = await _http.GetAsync("/api/integration-tests/throw-for-stack-trace");
        var bodyWithout = await responseWithout.Content.ReadFromJsonAsync<ExceptionBody>();
        bodyWithout.ShouldNotBeNull();
        bodyWithout!.StackTrace.ShouldBeNull();

        // With stack trace — spin up a second factory with the option enabled
        await using var factoryWithTrace = new MetalNexusTestFactory(
            server => server.IncludeServerStackTraceOnExceptions());
        await ((IAsyncLifetime)factoryWithTrace).InitializeAsync();
        using var httpWithTrace = factoryWithTrace.CreateClient();
        httpWithTrace.DefaultRequestHeaders.Add(
            MetalNexusConstants.ClientHeader, MetalNexusConstants.ClientHeaderValue);

        var responseWith = await httpWithTrace.GetAsync("/api/integration-tests/throw-for-stack-trace");
        var bodyWith = await responseWith.Content.ReadFromJsonAsync<ExceptionBody>();
        bodyWith.ShouldNotBeNull();
        bodyWith!.StackTrace.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>Minimal projection of the MetalNexus JSON error envelope for assertion.</summary>
    private sealed class ExceptionBody
    {
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
    }
}
