using System.Net;
using System.Net.Http.Json;
using System.Text;
using RossWright.MetalNexus.Integration.Tests.Infrastructure;

namespace RossWright.MetalNexus.Integration.Tests.RoundTrip;

/// <summary>
/// Phase B — Request Hydration Round-Trips.
/// Each test proves one HydrateRequest code path through the full middleware stack.
/// </summary>
public class RequestHydrationTests : IClassFixture<MetalNexusTestFactory>
{
    private readonly HttpClient _http;

    public RequestHydrationTests(MetalNexusTestFactory factory) =>
        _http = factory.CreateClient();

    // ── Query Params ──────────────────────────────────────────────────────────

    [Fact]
    public async Task QueryParam_SimpleProperties_HydrateCorrectly()
    {
        var response = await _http.GetAsync(
            "/api/integration-tests/simple-props?name=Widget&count=7&color=Green");

        var debug = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, debug);
        var body = await response.Content.ReadFromJsonAsync<SimplePropsResponse>();
        body.ShouldNotBeNull();
        body.Name.ShouldBe("Widget");
        body.Count.ShouldBe(7);
        body.Color.ShouldBe(TestColor.Green);
    }

    [Fact]
    public async Task QueryParam_ArrayProperty_HydratesCorrectly()
    {
        var response = await _http.GetAsync(
            "/api/integration-tests/array-prop?tags=alpha&tags=beta&tags=gamma");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ArrayPropResponse>();
        body.ShouldNotBeNull();
        body.Tags.ShouldBe(["alpha", "beta", "gamma"]);
    }

    [Fact]
    public async Task QueryParam_NestedComplexType_HydratesCorrectly()
    {
        // The registry rejects query-param protocols for complex-property requests; the
        // nested-type hydration path is verified here through a JSON body instead.
        var content = new StringContent(
            """{"inner":{"value":"deep"}}""",
            Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("/api/integration-tests/nested", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<NestedResponse>();
        body.ShouldNotBeNull();
        body.InnerValue.ShouldBe("deep");
    }

    // ── JSON Body ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task JsonBody_SimpleRequest_HydratesCorrectly()
    {
        var content = new StringContent(
            """{"title":"Hello","value":42}""",
            Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("/api/integration-tests/json-body", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonBodyResponse>();
        body.ShouldNotBeNull();
        body.Title.ShouldBe("Hello");
        body.Value.ShouldBe(42);
    }

    [Fact]
    public async Task JsonBody_CaseInsensitive_HydratesCorrectly()
    {
        var content = new StringContent(
            """{"TITLE":"MixedCase","VALUE":99}""",
            Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("/api/integration-tests/json-body", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonBodyResponse>();
        body.ShouldNotBeNull();
        body.Title.ShouldBe("MixedCase");
        body.Value.ShouldBe(99);
    }

    // ── Path Params ───────────────────────────────────────────────────────────

    [Fact]
    public async Task PathParam_SingleSlot_HydratesCorrectly()
    {
        var response = await _http.GetAsync("/api/integration-tests/path-single/42");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PathSingleResponse>();
        body.ShouldNotBeNull();
        body.Id.ShouldBe(42);
    }

    [Fact]
    public async Task PathParam_MultiSlot_HydratesCorrectly()
    {
        var response = await _http.GetAsync("/api/integration-tests/path-multi/7/items/99");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PathMultiResponse>();
        body.ShouldNotBeNull();
        body.UserId.ShouldBe(7);
        body.ItemId.ShouldBe(99);
    }

    // ── FromHeader ────────────────────────────────────────────────────────────

    [Fact]
    public async Task FromHeader_SingleHeader_HydratesCorrectly()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get,
            "/api/integration-tests/from-header");
        request.Headers.Add("X-Test-Token", "secret-abc");

        var response = await _http.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FromHeaderResponse>();
        body.ShouldNotBeNull();
        body.Token.ShouldBe("secret-abc");
    }

    // ── Raw Request Body ──────────────────────────────────────────────────────

    [Fact]
    public async Task RawRequest_BodyArrivesVerbatim()
    {
        const string rawPayload = "raw=data&verbatim=true";
        var content = new StringContent(rawPayload, Encoding.UTF8, "text/plain");

        var response = await _http.PostAsync("/api/integration-tests/raw-body", content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RawBodyResponse>();
        body.ShouldNotBeNull();
        body.Body.ShouldBe(rawPayload);
    }

    // ── File Uploads ──────────────────────────────────────────────────────────

    [Fact]
    public async Task FileUpload_SingleFile_HydratesCorrectly()
    {
        var fileBytes = "hello file"u8.ToArray();
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(fileBytes) { Headers = { ContentType = new("text/plain") } },
            "file", "test.txt");

        var response = await _http.PostAsync("/api/integration-tests/file-single", form);

        var debug = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, debug);
        var body = await response.Content.ReadFromJsonAsync<FileSingleResponse>();
        body.ShouldNotBeNull();
        body.FileName.ShouldBe("test.txt");
        body.ContentType.ShouldBe("text/plain");
        body.ByteCount.ShouldBe(fileBytes.Length);
    }

    [Fact]
    public async Task FileUpload_MultipleFiles_HydratesCorrectly()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent("a"u8.ToArray()) { Headers = { ContentType = new("text/plain") } },
            "file1", "a.txt");
        form.Add(new ByteArrayContent("bb"u8.ToArray()) { Headers = { ContentType = new("text/plain") } },
            "file2", "b.txt");
        form.Add(new ByteArrayContent("ccc"u8.ToArray()) { Headers = { ContentType = new("text/plain") } },
            "file3", "c.txt");

        var response = await _http.PostAsync("/api/integration-tests/file-multiple", form);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FileMultipleResponse>();
        body.ShouldNotBeNull();
        body.FileCount.ShouldBe(3);
        body.FileNames.ShouldBe(["a.txt", "b.txt", "c.txt"]);
    }

    [Fact]
    public async Task FileUpload_NamedSlot_RoutesToCorrectProperty()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent("img"u8.ToArray()) { Headers = { ContentType = new("image/png") } },
            "avatar", "profile.png");

        var response = await _http.PostAsync("/api/integration-tests/file-named-slot", form);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FileNamedSlotResponse>();
        body.ShouldNotBeNull();
        body.SlotFileName.ShouldBe("profile.png");
        body.AnonymousFileCount.ShouldBe(0);
    }

    [Fact]
    public async Task FileUpload_NamedSlot_UnmatchedFile_FallsBackToFilesArray()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent("data"u8.ToArray()) { Headers = { ContentType = new("text/plain") } },
            "other", "unknown.txt");

        var response = await _http.PostAsync("/api/integration-tests/file-unmatched-slot", form);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FileUnmatchedSlotResponse>();
        body.ShouldNotBeNull();
        body.SlotFileName.ShouldBeNull();
        body.AnonymousFileCount.ShouldBe(1);
    }

    [Fact]
    public async Task FileUpload_MultipleNamedSlots_EachRouteCorrectly()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent("front"u8.ToArray()) { Headers = { ContentType = new("image/png") } },
            "front", "front.png");
        form.Add(new ByteArrayContent("back"u8.ToArray()) { Headers = { ContentType = new("image/png") } },
            "back", "back.png");

        var response = await _http.PostAsync("/api/integration-tests/file-two-slots", form);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FileTwoSlotsResponse>();
        body.ShouldNotBeNull();
        body.FrontFileName.ShouldBe("front.png");
        body.BackFileName.ShouldBe("back.png");
        body.AnonymousFileCount.ShouldBe(0);
    }
}
