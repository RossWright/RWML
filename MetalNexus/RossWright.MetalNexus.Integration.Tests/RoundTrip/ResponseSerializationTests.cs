using System.Net;
using System.Net.Http.Json;
using RossWright.MetalNexus.Integration.Tests.Infrastructure;

namespace RossWright.MetalNexus.Integration.Tests.RoundTrip;

/// <summary>
/// Phase C — proves that each response serialization path on the server
/// (void, JSON, file-attachment, file-inline) is correctly written to the
/// HTTP response and decoded correctly on the client side.
/// </summary>
public class ResponseSerializationTests : IClassFixture<MetalNexusTestFactory>
{
    private readonly HttpClient _http;

    public ResponseSerializationTests(MetalNexusTestFactory factory) =>
        _http = factory.CreateClient();

    [Fact]
    public async Task VoidResponse_Returns200NoBody()
    {
        // Arrange
        var payload = new { Value = "ignored" };

        // Act
        var response = await _http.PostAsJsonAsync("/api/integration-tests/void-response", payload);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsByteArrayAsync();
        body.Length.ShouldBe(0);
    }

    [Fact]
    public async Task JsonResponse_DeserializesOnClient()
    {
        // Act
        var response = await _http.GetAsync("/api/integration-tests/json-response?name=Widget&count=5");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonResponseResponse>();
        body.ShouldNotBeNull();
        body.Name.ShouldBe("Widget");
        body.Count.ShouldBe(5);
        body.Computed.ShouldBe("Widget:5");
    }

    [Fact]
    public async Task FileResponse_DownloadedCorrectly()
    {
        // Act
        var response = await _http.GetAsync("/api/integration-tests/file-download?fileName=report.txt");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/plain");

        var disposition = response.Content.Headers.ContentDisposition;
        disposition.ShouldNotBeNull();
        disposition!.DispositionType.ShouldBe("attachment");
        disposition.FileName?.Trim('"').ShouldBe("report.txt");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.ShouldBe("Hello from report.txt");
    }

    [Fact]
    public async Task FileResponse_Inline_ContentDispositionCorrect()
    {
        // Act
        var response = await _http.GetAsync("/api/integration-tests/file-download-inline");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var disposition = response.Content.Headers.ContentDisposition;
        disposition.ShouldNotBeNull();
        disposition!.DispositionType.ShouldBe("inline");
        disposition.FileName?.Trim('"').ShouldBe("inline.txt");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        System.Text.Encoding.UTF8.GetString(bytes).ShouldBe("inline content");
    }
}
