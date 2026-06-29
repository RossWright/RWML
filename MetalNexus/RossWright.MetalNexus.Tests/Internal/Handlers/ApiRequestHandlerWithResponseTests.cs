using Microsoft.Extensions.Logging;
using NSubstitute;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace RossWright.MetalNexus.Tests.Internal.Handlers;

public class ApiRequestHandlerWithResponseTests
{
    [Fact]
    public async Task Handle_WithJsonResponse_DeserializesCorrectly()
    {
        // Arrange
        var expectedResponse = new TestResponse { Value = "test" };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        };

        var handler = CreateHandler<TestRequest, TestResponse>(httpResponse);
        var request = new TestRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe("test");
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_WithAllHeaders_CreatesFileCorrectly()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("test file content");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileData)
        };
        httpResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        httpResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = "test.pdf"
        };

        var handler = CreateHandler<FileRequest, MetalNexusFile>(httpResponse);
        var request = new FileRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.FileName.ShouldBe("test.pdf");
        result.ContentType.ShouldBe("application/pdf");
        result.Data.ShouldBe(fileData);
        result.IsAttachment.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_WithoutContentDisposition_UsesDefaults()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("test content");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileData)
        };
        httpResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

        var handler = CreateHandler<FileRequest, MetalNexusFile>(httpResponse);
        var request = new FileRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.FileName.ShouldBe(string.Empty);
        result.ContentType.ShouldBe("text/plain");
        result.Data.ShouldBe(fileData);
        result.IsAttachment.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_WithoutContentType_UsesEmptyString()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("test content");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileData)
        };

        var handler = CreateHandler<FileRequest, MetalNexusFile>(httpResponse);
        var request = new FileRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ContentType.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_WithInlineDisposition_IsNotAttachment()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("inline content");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileData)
        };
        httpResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
        {
            FileName = "inline.txt"
        };

        var handler = CreateHandler<FileRequest, MetalNexusFile>(httpResponse);
        var request = new FileRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsAttachment.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_WithMixedCaseAttachment_IsAttachment()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("attachment content");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileData)
        };
        httpResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("AtTaChMeNt");

        var handler = CreateHandler<FileRequest, MetalNexusFile>(httpResponse);
        var request = new FileRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsAttachment.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WithJsonResponse_WithCancellationToken_PassesCancellationToken()
    {
        // Arrange
        var expectedResponse = new TestResponse { Value = "cancelled" };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        };

        var handler = CreateHandler<TestRequest, TestResponse>(httpResponse);
        var request = new TestRequest();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await handler.Handle(request, cts.Token);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe("cancelled");
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_WithCancellationToken_PassesCancellationToken()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("cancellable content");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileData)
        };

        var handler = CreateHandler<FileRequest, MetalNexusFile>(httpResponse);
        var request = new FileRequest();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await handler.Handle(request, cts.Token);

        // Assert
        result.ShouldNotBeNull();
        result.Data.ShouldBe(fileData);
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_WhenServerReturnsJson_ThrowsException()
    {
        // Server returned 200 OK with application/json instead of binary —
        // e.g. a route collision or mis-configured endpoint. Without this guard
        // the handler would silently return a MetalNexusFile whose Data contains
        // raw JSON bytes, which is impossible to distinguish from a real file.
        var jsonBody = """{"assemblyName":"x","typeName":"x","message":"something went wrong"}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };

        var handler = CreateHandler<FileRequest, MetalNexusFile>(httpResponse);
        var request = new FileRequest();

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_WhenServerReturnsErrorStatus_ThrowsException()
    {
        // Non-success status codes are handled by InnerHandle before the file
        // branch is reached — this test documents and guards that behaviour.
        var jsonBody = """{"assemblyName":"x","typeName":"RossWright.MetalNexus.NotFoundException","message":"not found"}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };

        var handler = CreateHandler<FileRequest, MetalNexusFile>(httpResponse);
        var request = new FileRequest();

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => handler.Handle(request, CancellationToken.None));
    }

    private static ApiRequestHandlerWithResponse<TRequest, TResponse> CreateHandler<TRequest, TResponse>(
        HttpResponseMessage httpResponse)
        where TRequest : IRequest<TResponse>
    {
        var messageHandler = new FakeHttpMessageHandler(httpResponse);
        var httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri("http://test.com")
        };

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HttpClientName.Returns((string?)null);
        endpoint.HttpClientTimeout.Returns((TimeSpan?)null);

        var registry = Substitute.For<IMetalNexusRegistry>();
        registry.FindEndpoint(typeof(TRequest)).Returns(endpoint);

        var clientOptions = Substitute.For<IMetalNexusClientOptions>();
        clientOptions.DefaultConnectionName.Returns((string?)null);

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

        return new ApiRequestHandlerWithResponse<TRequest, TResponse>(
            registry,
            httpClientFactory,
            clientOptions,
            loggerFactory);
    }

    [Fact]
    public async Task Handle_With204NoContent_ReturnsDefaultWithoutThrowingOnEmptyBody()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        var handler = CreateHandler<TestRequest, TestResponse>(httpResponse);

        var result = await handler.Handle(new TestRequest(), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_With204NoContent_WhenBodyPresent_DeserializesBody()
    {
        // A non-standard but valid case: 204 with a body should still deserialize.
        // Only the absence of a body (no Content-Length) should be treated as default.
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent)
        {
            Content = new StringContent("""{"value":"present"}""", Encoding.UTF8, "application/json")
        };

        var handler = CreateHandler<TestRequest, TestResponse>(httpResponse);

        var result = await handler.Handle(new TestRequest(), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Value.ShouldBe("present");
    }

    // --- IMetalNexusRawResponse tests ---

    [Fact]
    public async Task Handle_WithInterfaceRawResponse_ReturnsRawResponseImpl()
    {
        var bytes = "raw bytes"u8.ToArray();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        };
        httpResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

        var handler = CreateHandler<RawInterfaceRequest, IMetalNexusRawResponse>(httpResponse);

        var result = await handler.Handle(new RawInterfaceRequest(), CancellationToken.None);

        result.ShouldNotBeNull();
        result.ContentType.ShouldBe("text/csv");
        result.Data.ShouldBe(bytes);
        result.DataStream.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WithInterfaceRawResponse_WhenNoContentType_DefaultsToOctetStream()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([1, 2, 3])
        };

        var handler = CreateHandler<RawInterfaceRequest, IMetalNexusRawResponse>(httpResponse);

        var result = await handler.Handle(new RawInterfaceRequest(), CancellationToken.None);

        result.ContentType.ShouldBe("application/octet-stream");
    }

    [Fact]
    public async Task Handle_WithConcreteRawResponse_SetsDataProperty()
    {
        var bytes = "concrete raw"u8.ToArray();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        };
        httpResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

        var handler = CreateHandler<RawConcreteRequest, ConcreteRawResponse>(httpResponse);

        var result = await handler.Handle(new RawConcreteRequest(), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Data.ShouldBe(bytes);
    }

    [Fact]
    public async Task Handle_WithConcreteRawResponse_WhenNoContentType_DefaultsToOctetStream()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([0xFF])
        };

        var handler = CreateHandler<RawConcreteRequest, ConcreteRawResponse>(httpResponse);

        var result = await handler.Handle(new RawConcreteRequest(), CancellationToken.None);

        result.ContentType.ShouldBe("application/octet-stream");
    }

    [Fact]
    public async Task Handle_WithInterfaceRawResponse_EmptyBody_ReturnsEmptyData()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([])
        };
        httpResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var handler = CreateHandler<RawInterfaceRequest, IMetalNexusRawResponse>(httpResponse);

        var result = await handler.Handle(new RawInterfaceRequest(), CancellationToken.None);

        result.Data.ShouldBe(Array.Empty<byte>());
    }

    private class TestRequest : IRequest<TestResponse>
    {
    }

    private class TestResponse
    {
        public string Value { get; set; } = string.Empty;
    }

    private class FileRequest : IRequest<MetalNexusFile>
    {
    }

    private class RawInterfaceRequest : IRequest<IMetalNexusRawResponse>
    {
    }

    private class RawConcreteRequest : IRequest<ConcreteRawResponse>
    {
    }

    private class ConcreteRawResponse : IMetalNexusRawResponse
    {
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[]? Data { get; set; }
        public Stream? DataStream => null;
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
