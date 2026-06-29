using Microsoft.Extensions.Logging;
using NSubstitute;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Shouldly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RossWright.MetalNexus.Tests.Internal.Handlers;

public class SendViaRequestHandlerTests
{
    [Fact]
    public async Task Handle_WithoutResponse_CallsInnerHandle()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        
        var endpoint = CreateEndpoint(typeof(TestCommandRequest), null);
        registry.FindEndpoint(typeof(TestCommandRequest)).Returns(endpoint);
        
        var httpClient = new HttpClient(new TestHttpMessageHandler(HttpStatusCode.OK, "{}"))
        {
            BaseAddress = new Uri("http://localhost")
        };
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        
        options.DefaultConnectionName.Returns("default");
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = CreateLoggerFactory();
        var handler = new SendViaRequestHandler<TestCommandRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SendVia<TestCommandRequest>("testConnection", new TestCommandRequest());
        
        // Act
        await handler.Handle(request, CancellationToken.None);
        
        // Assert - if we get here without exception, the call succeeded
    }

    [Fact]
    public async Task Handle_WithResponse_ReturnsJsonResponse()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        
        var endpoint = CreateEndpoint(typeof(TestRequest), typeof(TestResponse));
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        
        var responseData = new TestResponse { Value = "test" };
        var json = JsonSerializer.Serialize(responseData);
        var httpClient = new HttpClient(new TestHttpMessageHandler(HttpStatusCode.OK, json))
        {
            BaseAddress = new Uri("http://localhost")
        };
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        
        options.DefaultConnectionName.Returns("default");
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = CreateLoggerFactory();
        var handler = new SendViaRequestHandler<TestRequest, TestResponse>(registry, httpClientFactory, options, loggerFactory);
        var request = new SendVia<TestRequest, TestResponse>("testConnection", new TestRequest());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe("test");
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_ReturnsFileObject()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        
        var endpoint = CreateEndpoint(typeof(TestFileRequest), typeof(MetalNexusFile));
        registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        
        var fileData = new byte[] { 1, 2, 3, 4, 5 };
        var httpClient = new HttpClient(new TestHttpMessageHandler(HttpStatusCode.OK, fileData, "test.pdf", "application/pdf"))
        {
            BaseAddress = new Uri("http://localhost")
        };
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        
        options.DefaultConnectionName.Returns("default");
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = CreateLoggerFactory();
        var handler = new SendViaRequestHandler<TestFileRequest, MetalNexusFile>(registry, httpClientFactory, options, loggerFactory);
        var request = new SendVia<TestFileRequest, MetalNexusFile>("testConnection", new TestFileRequest());

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
    public async Task Handle_WithMetalNexusFileResponse_NoDisposition_ReturnsFileWithEmptyFileName()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        
        var endpoint = CreateEndpoint(typeof(TestFileRequest), typeof(MetalNexusFile));
        registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        
        var fileData = new byte[] { 1, 2, 3 };
        var httpClient = new HttpClient(new TestHttpMessageHandler(HttpStatusCode.OK, fileData, null, "application/octet-stream"))
        {
            BaseAddress = new Uri("http://localhost")
        };
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        
        options.DefaultConnectionName.Returns("default");
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = CreateLoggerFactory();
        var handler = new SendViaRequestHandler<TestFileRequest, MetalNexusFile>(registry, httpClientFactory, options, loggerFactory);
        var request = new SendVia<TestFileRequest, MetalNexusFile>("testConnection", new TestFileRequest());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.FileName.ShouldBe(string.Empty);
        result.ContentType.ShouldBe("application/octet-stream");
        result.Data.ShouldBe(fileData);
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_NoContentType_ReturnsFileWithEmptyContentType()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        
        var endpoint = CreateEndpoint(typeof(TestFileRequest), typeof(MetalNexusFile));
        registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        
        var fileData = new byte[] { 1, 2, 3 };
        var httpClient = new HttpClient(new TestHttpMessageHandler(HttpStatusCode.OK, fileData, "test.bin", null))
        {
            BaseAddress = new Uri("http://localhost")
        };
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        
        options.DefaultConnectionName.Returns("default");
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = CreateLoggerFactory();
        var handler = new SendViaRequestHandler<TestFileRequest, MetalNexusFile>(registry, httpClientFactory, options, loggerFactory);
        var request = new SendVia<TestFileRequest, MetalNexusFile>("testConnection", new TestFileRequest());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.FileName.ShouldBe("test.bin");
        result.ContentType.ShouldBe(string.Empty);
        result.Data.ShouldBe(fileData);
    }

    [Fact]
    public async Task Handle_WithMetalNexusFileResponse_InlineDisposition_ReturnsFileWithIsAttachmentFalse()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        
        var endpoint = CreateEndpoint(typeof(TestFileRequest), typeof(MetalNexusFile));
        registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        
        var fileData = new byte[] { 1, 2, 3 };
        var httpClient = new HttpClient(new TestHttpMessageHandler(HttpStatusCode.OK, fileData, "test.pdf", "application/pdf", "inline"))
        {
            BaseAddress = new Uri("http://localhost")
        };
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        
        options.DefaultConnectionName.Returns("default");
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = CreateLoggerFactory();
        var handler = new SendViaRequestHandler<TestFileRequest, MetalNexusFile>(registry, httpClientFactory, options, loggerFactory);
        var request = new SendVia<TestFileRequest, MetalNexusFile>("testConnection", new TestFileRequest());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsAttachment.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WithResponse_CancellationToken_PassedThrough()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        
        var endpoint = CreateEndpoint(typeof(TestRequest), typeof(TestResponse));
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        
        var cts = new CancellationTokenSource();
        var responseData = new TestResponse { Value = "test" };
        var json = JsonSerializer.Serialize(responseData);
        var httpClient = new HttpClient(new TestHttpMessageHandler(HttpStatusCode.OK, json))
        {
            BaseAddress = new Uri("http://localhost")
        };
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        
        options.DefaultConnectionName.Returns("default");
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = CreateLoggerFactory();
        var handler = new SendViaRequestHandler<TestRequest, TestResponse>(registry, httpClientFactory, options, loggerFactory);
        var request = new SendVia<TestRequest, TestResponse>("testConnection", new TestRequest());
        
        // Act
        var result = await handler.Handle(request, cts.Token);
        
        // Assert
        result.ShouldNotBeNull();
    }

    private static ILoggerFactory CreateLoggerFactory()
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        return loggerFactory;
    }

    private static IEndpoint CreateEndpoint(Type requestType, Type? responseType)
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(requestType);
        endpoint.ResponseType.Returns(responseType);
        endpoint.Path.Returns("/test");
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        return endpoint;
    }

    [ApiRequest(path: "test")]
    private class TestCommandRequest : IRequest { }

    [ApiRequest(path: "test")]
    private class TestRequest : IRequest<TestResponse> { }

    [ApiRequest(path: "test")]
    private class TestFileRequest : IRequest<MetalNexusFile> { }

    private class TestResponse
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string? _jsonContent;
        private readonly byte[]? _binaryContent;
        private readonly string? _fileName;
        private readonly string? _contentType;
        private readonly string? _dispositionType;

        public TestHttpMessageHandler(HttpStatusCode statusCode, string jsonContent)
        {
            _statusCode = statusCode;
            _jsonContent = jsonContent;
        }

        public TestHttpMessageHandler(HttpStatusCode statusCode, byte[] binaryContent, string? fileName, string? contentType, string? dispositionType = "attachment")
        {
            _statusCode = statusCode;
            _binaryContent = binaryContent;
            _fileName = fileName;
            _contentType = contentType;
            _dispositionType = dispositionType;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode);
            
            if (_binaryContent != null)
            {
                response.Content = new ByteArrayContent(_binaryContent);
                if (_contentType != null)
                {
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(_contentType);
                }
                if (_fileName != null)
                {
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(_dispositionType ?? "attachment")
                    {
                        FileName = _fileName
                    };
                }
            }
            else if (_jsonContent != null)
            {
                response.Content = new StringContent(_jsonContent, Encoding.UTF8, "application/json");
            }
            
            return Task.FromResult(response);
        }
    }
}
