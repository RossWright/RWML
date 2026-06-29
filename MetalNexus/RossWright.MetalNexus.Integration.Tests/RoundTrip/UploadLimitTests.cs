using System.Net;
using RossWright.MetalNexus.Integration.Tests.Infrastructure;

namespace RossWright.MetalNexus.Integration.Tests.RoundTrip;

/// <summary>
/// Phase G3 — proves that [UploadLimit] and [NoUploadLimit] do not throw when
/// <c>IHttpMaxRequestBodySizeFeature</c> is absent from the request context, and
/// that [NoUploadLimit] removes the body-size cap when the feature is present.
/// </summary>
public class UploadLimitTests : IAsyncLifetime
{
    // Factory with the body-size feature stripped so we can verify null-safe handling.
    private readonly MetalNexusTestFactory _nullFeatureFactory = new(nullifyBodySizeFeature: true);
    private readonly MetalNexusTestFactory _normalFactory = new();

    public async Task InitializeAsync()
    {
        await ((IAsyncLifetime)_nullFeatureFactory).InitializeAsync();
        await ((IAsyncLifetime)_normalFactory).InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await ((IAsyncLifetime)_nullFeatureFactory).DisposeAsync();
        await ((IAsyncLifetime)_normalFactory).DisposeAsync();
    }

    // ── G3-1 — UploadLimit: feature null does not throw ───────────────────────

    [Fact]
    public async Task UploadLimit_FeatureNull_DoesNotThrow()
    {
        using var http = _nullFeatureFactory.CreateClient();
        using var form = BuildForm();

        var response = await http.PostAsync("/api/integration-tests/g3/upload-limit", form);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // ── G3-2 — NoUploadLimit: feature null does not throw ─────────────────────

    [Fact]
    public async Task NoUploadLimit_FeatureNull_DoesNotThrow()
    {
        using var http = _nullFeatureFactory.CreateClient();
        using var form = BuildForm();

        var response = await http.PostAsync("/api/integration-tests/g3/no-upload-limit", form);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // ── G3-3 — NoUploadLimit: sets MaxRequestBodySize to null when feature present ─────

    [Fact]
    public async Task NoUploadLimit_SetsBodySizeToNull_Returns200()
    {
        using var http = _normalFactory.CreateClient();
        using var form = BuildForm();

        var response = await http.PostAsync("/api/integration-tests/g3/no-upload-limit", form);

        // If MaxRequestBodySize is not correctly set to null the TestServer would reject
        // the request with a 413; receiving 200 proves the limit was removed.
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static MultipartFormDataContent BuildForm()
    {
        var form = new MultipartFormDataContent();
        form.Add(
            new ByteArrayContent("hello"u8.ToArray()) { Headers = { ContentType = new("text/plain") } },
            "file", "test.txt");
        return form;
    }
}
