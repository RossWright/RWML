using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Shouldly;

namespace RossWright.MetalNexus.Tests.Internal.Handlers;

public class ApiRequestHandlerBaseTests
{
    [Fact]
    public async Task InnerHandle_EndpointNotFound_ThrowsMetalNexusException()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        
        registry.FindEndpoint(typeof(SimpleRequest)).Returns((IEndpoint?)null);

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();
        
        // Act & Assert
        var ex = await Should.ThrowAsync<MetalNexusException>(async () =>
            await handler.InnerHandle(null, request, CancellationToken.None));
        
        ex.Message.ShouldContain("Endpoint for");
        ex.Message.ShouldContain("not defined");
    }

    [Fact]
    public async Task InnerHandle_RequestAsQueryParams_DoesNotIncludeBody()
    {
        // Arrange
        var (handler, endpoint, httpClient, responseMessage) = SetupHandler<SimpleRequest>();
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.Path.Returns("/api/test");
        
        var request = new SimpleRequest { Name = "TestName" };
        
        // Act
        var result = await handler.InnerHandle(null, request, CancellationToken.None);
        
        // Assert
        result.ShouldBe(responseMessage);
        await httpClient.Received(1).SendAsync(
            Arg.Is<HttpRequestMessage>(r => r.Content == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InnerHandle_RequestNotAsQueryParams_IncludesJsonBody()
    {
        // Arrange
        var (handler, endpoint, httpClient, responseMessage) = SetupHandler<SimpleRequest>();
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.Path.Returns("/api/test");
        
        var request = new SimpleRequest { Name = "TestName" };
        
        // Act
        var result = await handler.InnerHandle(null, request, CancellationToken.None);
        
        // Assert
        result.ShouldBe(responseMessage);
        await httpClient.Received(1).SendAsync(
            Arg.Is<HttpRequestMessage>(r => 
                r.Content != null && 
                r.Content is StringContent),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InnerHandle_SuccessfulResponse_ReturnsResponse()
    {
        // Arrange
        var (handler, endpoint, httpClient, responseMessage) = SetupHandler<SimpleRequest>();
        endpoint.Path.Returns("/api/test");
        
        var request = new SimpleRequest();
        
        // Act
        var result = await handler.InnerHandle(null, request, CancellationToken.None);
        
        // Assert
        result.ShouldBe(responseMessage);
        result.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Fact]
    public async Task InnerHandle_NullResponse_ThrowsMetalNexusException()
    {
        // Arrange
        var (handler, endpoint, httpClient, _) = SetupHandler<SimpleRequest>();
        endpoint.Path.Returns("/api/test");
        
        httpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<HttpResponseMessage>(_ => null!);
        
        var request = new SimpleRequest();
        
        // Act & Assert
        var ex = await Should.ThrowAsync<MetalNexusException>(async () =>
            await handler.InnerHandle(null, request, CancellationToken.None));
        
        ex.Message.ShouldContain("failed without response");
    }

    [Fact]
    public async Task InnerHandle_UnsuccessfulResponse_ThrowsExceptionFromResponse()
    {
        // Arrange
        var (handler, endpoint, httpClient, _) = SetupHandler<SimpleRequest>();
        endpoint.Path.Returns("/api/test");
        
        var errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(JsonSerializer.Serialize(new ExceptionResponse
            {
                AssemblyName = "TestAssembly",
                TypeName = "TestException",
                Message = "Test error message"
            }))
        };
        
        httpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(errorResponse);
        
        var request = new SimpleRequest();
        
        // Act & Assert
        var ex = await Should.ThrowAsync<Exception>(async () =>
            await handler.InnerHandle(null, request, CancellationToken.None));
        
        ex.Message.ShouldContain("Test error message");
    }

    [Fact]
    public async Task InnerHandle_WithOverrideHttpClientName_UsesOverrideName()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        var httpClient = CreateMockHttpClient();
        
        registry.FindEndpoint(typeof(SimpleRequest)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        
        httpClientFactory.CreateClient("CustomClient").Returns(httpClient);
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();

        // Act
        await handler.InnerHandle("CustomClient", request, CancellationToken.None);
        
        // Assert
        httpClientFactory.Received(1).CreateClient("CustomClient");
    }

    [Fact]
    public async Task InnerHandle_WithEndpointHttpClientName_UsesEndpointName()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        var httpClient = CreateMockHttpClient();
        
        registry.FindEndpoint(typeof(SimpleRequest)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HttpClientName.Returns("EndpointClient");
        
        httpClientFactory.CreateClient("EndpointClient").Returns(httpClient);
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();

        // Act
        await handler.InnerHandle(null, request, CancellationToken.None);

        // Assert
        httpClientFactory.Received(1).CreateClient("EndpointClient");
    }

    [Fact]
    public async Task InnerHandle_WithDefaultConnectionName_UsesDefaultName()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        var httpClient = CreateMockHttpClient();
        
        registry.FindEndpoint(typeof(SimpleRequest)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HttpClientName.Returns((string?)null);
        
        httpClientFactory.CreateClient("DefaultClient").Returns(httpClient);
        options.DefaultConnectionName.Returns("DefaultClient");
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();

        // Act
        await handler.InnerHandle(null, request, CancellationToken.None);

        // Assert
        httpClientFactory.Received(1).CreateClient("DefaultClient");
    }

    [Fact]
    public async Task InnerHandle_WithNoConnectionName_UsesOptionsDefaultName()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        var httpClient = CreateMockHttpClient();
        
        registry.FindEndpoint(typeof(SimpleRequest)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HttpClientName.Returns((string?)null);
        
        httpClientFactory.CreateClient(Microsoft.Extensions.Options.Options.DefaultName).Returns(httpClient);
        options.DefaultConnectionName.Returns((string?)null);
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();

        // Act
        await handler.InnerHandle(null, request, CancellationToken.None);

        // Assert
        httpClientFactory.Received(1).CreateClient(Microsoft.Extensions.Options.Options.DefaultName);
    }

    [Fact]
    public async Task InnerHandle_WithTimeout_UsesTimeoutCancellationToken()
    {
        // Arrange
        var (handler, endpoint, httpClient, responseMessage) = SetupHandler<SimpleRequest>();
        endpoint.Path.Returns("/api/test");
        endpoint.HttpClientTimeout.Returns(TimeSpan.FromSeconds(30));
        
        var request = new SimpleRequest();
        
        // Act
        var result = await handler.InnerHandle(null, request, CancellationToken.None);
        
        // Assert
        result.ShouldBe(responseMessage);
        await httpClient.Received(1).SendAsync(
            Arg.Any<HttpRequestMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InnerHandle_TimeoutOccurs_ThrowsTimeoutException()
    {
        // Arrange
        var (handler, endpoint, httpClient, _) = SetupHandler<SimpleRequest>();
        endpoint.Path.Returns("/api/test");
        endpoint.HttpClientTimeout.Returns(TimeSpan.FromMilliseconds(1));
        
        var tcs = new TaskCompletionSource<HttpResponseMessage>();
        httpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                var token = callInfo.Arg<CancellationToken>();
                await Task.Delay(100);
                token.ThrowIfCancellationRequested();
                return await tcs.Task;
            }));
        
        var request = new SimpleRequest();
        
        // Act & Assert
        await Should.ThrowAsync<TimeoutException>(async () =>
            await handler.InnerHandle(null, request, CancellationToken.None));
    }

    [Fact]
    public async Task InnerHandle_CancelledByUser_ThrowsOperationCanceledException()
    {
        // Arrange
        var (handler, endpoint, httpClient, _) = SetupHandler<SimpleRequest>();
        endpoint.Path.Returns("/api/test");
        
        httpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(_ => throw new OperationCanceledException());
        
        var request = new SimpleRequest();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await handler.InnerHandle(null, request, cts.Token));
    }

    [Fact]
    public void GetEndpointWithParams_PathWithLeadingSlash_ReturnsUnchanged()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        
        var request = new SimpleRequest();
        
        // Act
        var result = endpoint.GetEndpointWithParams<SimpleRequest>(request);
        
        // Assert
        result.ShouldBe("/api/test");
    }

    [Fact]
    public void GetEndpointWithParams_PathWithoutLeadingSlash_AddsSlash()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        
        var request = new SimpleRequest();
        
        // Act
        var result = endpoint.GetEndpointWithParams<SimpleRequest>(request);
        
        // Assert
        result.ShouldBe("/api/test");
    }

    [Fact]
    public void GetEndpointWithParams_PathStartsWithHttp_DoesNotAddSlash()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("http://example.com/api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        
        var request = new SimpleRequest();
        
        // Act
        var result = endpoint.GetEndpointWithParams<SimpleRequest>(request);
        
        // Assert
        result.ShouldBe("http://example.com/api/test");
    }

    [Fact]
    public void GetEndpointWithParams_PathStartsWithHttps_DoesNotAddSlash()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("https://example.com/api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        
        var request = new SimpleRequest();
        
        // Act
        var result = endpoint.GetEndpointWithParams<SimpleRequest>(request);
        
        // Assert
        result.ShouldBe("https://example.com/api/test");
    }

    [Fact]
    public void GetEndpointWithParams_WithPathParams_ReplacesParameters()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/users/{Id}/posts/{PostId}");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(true);
        endpoint.RequestType.Returns(typeof(RequestWithPathParams));
        
        var request = new RequestWithPathParams { Id = 123, PostId = 456 };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithPathParams>(request);
        
        // Assert
        result.ShouldBe("/api/users/123/posts/456");
    }

    [Fact]
    public void GetEndpointWithParams_RequestAsQueryParams_AddsQueryString()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        
        var request = new SimpleRequest { Name = "TestValue" };
        
        // Act
        var result = endpoint.GetEndpointWithParams<SimpleRequest>(request);
        
        // Assert
        result.ShouldBe("/api/test?Name=TestValue");
    }

    [Fact]
    public void GetEndpointWithParams_RequestAsQueryParams_EncodesSpecialCharacters()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        
        var request = new SimpleRequest { Name = "Test Value & Special" };
        
        // Act
        var result = endpoint.GetEndpointWithParams<SimpleRequest>(request);
        
        // Assert
        result.ShouldContain("Name=Test+Value+%26+Special");
    }

    [Fact]
    public void GetEndpointWithParams_WithNullProperty_ExcludesFromQueryString()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        
        var request = new SimpleRequest { Name = null };
        
        // Act
        var result = endpoint.GetEndpointWithParams<SimpleRequest>(request);
        
        // Assert
        result.ShouldBe("/api/test");
    }

    [Fact]
    public void GetEndpointWithParams_WithArrayProperty_AddsMultipleParams()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithArray));
        
        var request = new RequestWithArray { Values = new[] { "A", "B", "C" } };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithArray>(request);
        
        // Assert
        result.ShouldBe("/api/test?Values=A&Values=B&Values=C");
    }

    [Fact]
    public void GetEndpointWithParams_WithArrayContainingNull_SkipsNullValues()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithArray));
        
        var request = new RequestWithArray { Values = new[] { "A", null, "C" } };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithArray>(request);
        
        // Assert
        result.ShouldBe("/api/test?Values=A&Values=C");
    }

    [Fact]
    public void GetEndpointWithParams_WithComplexProperty_AddsNestedProperties()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithComplexProperty));
        
        var request = new RequestWithComplexProperty 
        { 
            Filter = new FilterObject { Search = "test", Page = 2 } 
        };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithComplexProperty>(request);
        
        // Assert
        result.ShouldContain("Search=test");
        result.ShouldContain("Page=2");
    }

    [Fact]
    public void GetEndpointWithParams_WithComplexPropertyArrayProperty_AddsNestedArrays()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithComplexProperty));
        
        var request = new RequestWithComplexProperty 
        { 
            Filter = new FilterObject { Tags = new[] { "tag1", "tag2" } } 
        };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithComplexProperty>(request);
        
        // Assert
        result.ShouldContain("Tags=tag1");
        result.ShouldContain("Tags=tag2");
    }

    [Fact]
    public void GetEndpointWithParams_WithComplexPropertyNullSubProperty_SkipsNullSubProperty()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithComplexProperty));
        
        var request = new RequestWithComplexProperty 
        { 
            Filter = new FilterObject { Search = "test", Page = null } 
        };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithComplexProperty>(request);
        
        // Assert
        result.ShouldContain("Search=test");
        result.ShouldNotContain("Page=");
    }

    [Fact]
    public void GetEndpointWithParams_WithFromHeaderAttribute_ExcludesFromQueryString()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithHeader));
        
        var request = new RequestWithHeader { Name = "test", AuthToken = "secret" };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithHeader>(request);
        
        // Assert
        result.ShouldContain("Name=test");
        result.ShouldNotContain("AuthToken");
    }

    [Fact]
    public void GetEndpointWithParams_WithMetalNexusFileRequestProperty_ExcludesFromQueryString()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithFileBase));
        
        var request = new RequestWithFileBase { Name = "test" };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithFileBase>(request);
        
        // Assert
        result.ShouldContain("Name=test");
        result.ShouldNotContain("Files");
    }

    [Fact]
    public void GetEndpointWithParams_WithPathParamsAndQueryParams_CombinesBoth()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/users/{Id}");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(true);
        endpoint.RequestType.Returns(typeof(RequestWithPathAndQueryParams));
        
        var request = new RequestWithPathAndQueryParams { Id = 123, Name = "test" };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithPathAndQueryParams>(request);
        
        // Assert
        result.ShouldBe("/api/users/123?Name=test");
    }

    [Fact]
    public void GetEndpointWithParams_WithTrailingSlash_RemovesBeforeQueryString()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test/");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        
        var request = new SimpleRequest { Name = "test" };
        
        // Act
        var result = endpoint.GetEndpointWithParams<SimpleRequest>(request);
        
        // Assert
        result.ShouldBe("/api/test?Name=test");
    }

    [Fact]
    public void GetEndpointWithParams_WithComplexPropertyArrayWithNulls_SkipsNullElements()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithComplexProperty));
        
        var request = new RequestWithComplexProperty 
        { 
            Filter = new FilterObject { Tags = new[] { "tag1", null, "tag3" } } 
        };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithComplexProperty>(request);
        
        // Assert
        result.ShouldContain("Tags=tag1");
        result.ShouldContain("Tags=tag3");
    }

    [Fact]
    public async Task InnerHandle_WithLogger_LogsTraceMessages()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        var logger = Substitute.For<ILogger>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
        var httpClient = CreateMockHttpClient();

        registry.FindEndpoint(typeof(SimpleRequest)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);

        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();

        // Act
        await handler.InnerHandle(null, request, CancellationToken.None);

        // Assert
        logger.Received().Log(
            Arg.Any<Microsoft.Extensions.Logging.LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InnerHandle_WithNullLogger_DoesNotThrow()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var httpClient = CreateMockHttpClient();

        registry.FindEndpoint(typeof(SimpleRequest)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);

        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();

        // Act & Assert
        await Should.NotThrowAsync(async () => 
            await handler.InnerHandle(null, request, CancellationToken.None));
    }

    [Fact]
    public async Task InnerHandle_WithHttpClientBaseAddress_DoesNotThrow()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var httpClient = CreateMockHttpClient();
        httpClient.BaseAddress = new Uri("https://api.example.com");

        registry.FindEndpoint(typeof(SimpleRequest)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);

        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await handler.InnerHandle(null, request, CancellationToken.None));
    }

    [Fact]
    public async Task InnerHandle_WithConnectionName_DoesNotThrow()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var httpClient = CreateMockHttpClient();

        registry.FindEndpoint(typeof(SimpleRequest)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HttpClientName.Returns("MyConnection");

        httpClientFactory.CreateClient("MyConnection").Returns(httpClient);
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());

        var handler = new ApiRequestHandlerBase<SimpleRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new SimpleRequest();

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await handler.InnerHandle(null, request, CancellationToken.None));
    }

    [Fact]
    public void GetEndpointWithParams_WithMultipleProperties_CombinesAllInQueryString()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithMultipleProperties));
        
        var request = new RequestWithMultipleProperties 
        { 
            Name = "test",
            Age = 25,
            Active = true
        };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithMultipleProperties>(request);
        
        // Assert
        result.ShouldContain("Name=test");
        result.ShouldContain("Age=25");
        result.ShouldContain("Active=True");
    }

    [Fact]
    public void GetEndpointWithParams_WithEmptyArray_ExcludesFromQueryString()
    {
        // Arrange
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithArray));
        
        var request = new RequestWithArray { Values = Array.Empty<string>() };
        
        // Act
        var result = endpoint.GetEndpointWithParams<RequestWithArray>(request);
        
        // Assert
        result.ShouldBe("/api/test");
    }

    private static (ApiRequestHandlerBase<T>, IEndpoint, HttpClient, HttpResponseMessage) SetupHandler<T>()
    {
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };
        
        var httpClient = CreateMockHttpClient(responseMessage);
        
        registry.FindEndpoint(typeof(T)).Returns(endpoint);
        endpoint.RequestType.Returns(typeof(T));
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.RequestAsQueryParams.Returns(false);
        
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        options.RequestBodyJsonSerializerOptions.Returns(new JsonSerializerOptions());
        
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new ApiRequestHandlerBase<T>(registry, httpClientFactory, options, loggerFactory);

        return (handler, endpoint, httpClient, responseMessage);
    }

    private static HttpClient CreateMockHttpClient(HttpResponseMessage? response = null)
    {
        response ??= new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        
        var messageHandler = Substitute.For<HttpMessageHandler>();
        var httpClient = Substitute.For<HttpClient>();
        
        httpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(response);
        
        return httpClient;
    }

    private class SimpleRequest : IRequest
    {
        public string? Name { get; set; }
    }

    private class RequestWithPathParams : IRequest
    {
        public int Id { get; set; }
        public int PostId { get; set; }
    }

    private class RequestWithArray : IRequest
    {
        public string?[]? Values { get; set; }
    }

    private class RequestWithComplexProperty : IRequest
    {
        public FilterObject? Filter { get; set; }
    }

    private class FilterObject
    {
        public string? Search { get; set; }
        public int? Page { get; set; }
        public string?[]? Tags { get; set; }
    }

    private class RequestWithHeader : IRequest
    {
        public string? Name { get; set; }
        
        [FromHeader("Authorization")]
        public string? AuthToken { get; set; }
    }

    private class RequestWithFileBase : MetalNexusFileRequest, IRequest
    {
        public string? Name { get; set; }
    }

    private class RequestWithPathAndQueryParams : IRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class RequestWithMultipleProperties : IRequest
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
    }

    // --- File request types used by BuildMultipartContent tests ---

    private class PlainFileRequest : MetalNexusFileRequest, IRequest { }

    private class FileRequestWithScalar : MetalNexusFileRequest, IRequest
    {
        public string? Description { get; set; }
        public int Count { get; set; }
    }

    private class FileRequestWithSlot : MetalNexusFileRequest, IRequest
    {
        [FileSlot("avatar")]
        public MetalNexusFile? Avatar { get; set; }
    }

    private class FileRequestWithNullSlot : MetalNexusFileRequest, IRequest
    {
        [FileSlot("avatar")]
        public MetalNexusFile? Avatar { get; set; }
    }

    private class FileRequestWithSlotAndAnonymous : MetalNexusFileRequest, IRequest
    {
        [FileSlot("thumbnail")]
        public MetalNexusFile? Thumbnail { get; set; }
    }

    // --- Helper: capture HttpRequestMessage from SendAsync ---

    private static (ApiRequestHandlerBase<T>, IEndpoint, HttpClient) SetupFileHandler<T>()
    {
        var (handler, endpoint, httpClient, _) = SetupHandler<T>();
        endpoint.Path.Returns("/api/upload");
        return (handler, endpoint, httpClient);
    }

    private static HttpRequestMessage? CaptureRequest(HttpClient httpClient)
    {
        HttpRequestMessage? captured = null;
        httpClient.SendAsync(
                Arg.Do<HttpRequestMessage>(r => captured = r),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("{}") });
        return null; // captured after the call
    }

    // Returns the captured request; must be called BEFORE InnerHandle
    private static Func<HttpRequestMessage?> CaptureSentRequest(HttpClient httpClient)
    {
        HttpRequestMessage? captured = null;
        httpClient.SendAsync(
                Arg.Do<HttpRequestMessage>(r => captured = r),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("{}") });
        return () => captured;
    }

    // --- BuildMultipartContent tests ---

    [Fact]
    public async Task InnerHandle_FileRequest_UsesMultipartFormDataContent()
    {
        var (handler, _, httpClient) = SetupFileHandler<PlainFileRequest>();
        var getRequest = CaptureSentRequest(httpClient);
        var request = new PlainFileRequest { Files = [] };

        await handler.InnerHandle(null, request, CancellationToken.None);

        getRequest()!.Content.ShouldBeOfType<MultipartFormDataContent>();
    }

    [Fact]
    public async Task InnerHandle_FileRequest_WithAnonymousFiles_SendsUnderFilesFieldName()
    {
        var (handler, _, httpClient) = SetupFileHandler<PlainFileRequest>();
        var getRequest = CaptureSentRequest(httpClient);
        var file = new MetalNexusFile { FileName = "test.txt", ContentType = "text/plain", Data = "hello"u8.ToArray() };
        var request = new PlainFileRequest { Files = [file] };

        await handler.InnerHandle(null, request, CancellationToken.None);

        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        var part = multipart.Single(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "files");
        part.Headers.ContentDisposition!.FileName!.Trim('"').ShouldBe("test.txt");
    }

    [Fact]
    public async Task InnerHandle_FileRequest_WithFileSlotProperty_SendsUnderSlotName()
    {
        var (handler, _, httpClient) = SetupFileHandler<FileRequestWithSlot>();
        var getRequest = CaptureSentRequest(httpClient);
        var file = new MetalNexusFile { FileName = "avatar.png", ContentType = "image/png", Data = [1, 2, 3] };
        var request = new FileRequestWithSlot { Files = [], Avatar = file };

        await handler.InnerHandle(null, request, CancellationToken.None);

        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        var part = multipart.Single(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "avatar");
        part.Headers.ContentDisposition!.FileName!.Trim('"').ShouldBe("avatar.png");
    }

    [Fact]
    public async Task InnerHandle_FileRequest_WithScalarProperties_IncludesAsFormFields()
    {
        var (handler, _, httpClient) = SetupFileHandler<FileRequestWithScalar>();
        var getRequest = CaptureSentRequest(httpClient);
        var request = new FileRequestWithScalar { Files = [], Description = "my file", Count = 42 };

        await handler.InnerHandle(null, request, CancellationToken.None);

        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        var parts = multipart.ToList();

        var descPart = parts.Single(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "Description");
        (await descPart.ReadAsStringAsync()).ShouldBe("my file");

        var countPart = parts.Single(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "Count");
        (await countPart.ReadAsStringAsync()).ShouldBe("42");
    }

    [Fact]
    public async Task InnerHandle_FileRequest_WithDataStream_UsesStreamContent()
    {
        var (handler, _, httpClient) = SetupFileHandler<PlainFileRequest>();
        var getRequest = CaptureSentRequest(httpClient);
        using var stream = new MemoryStream("stream data"u8.ToArray());
        var file = new MetalNexusFile { FileName = "streamed.bin", ContentType = "application/octet-stream", DataStream = stream };
        var request = new PlainFileRequest { Files = [file] };

        await handler.InnerHandle(null, request, CancellationToken.None);

        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        var part = multipart.Single(p => p.Headers.ContentDisposition?.FileName?.Trim('"') == "streamed.bin");
        part.ShouldBeOfType<StreamContent>();
    }

    [Fact]
    public async Task InnerHandle_FileRequest_WithDataBytes_UsesByteArrayContent()
    {
        var (handler, _, httpClient) = SetupFileHandler<PlainFileRequest>();
        var getRequest = CaptureSentRequest(httpClient);
        var file = new MetalNexusFile { FileName = "bytes.bin", ContentType = "application/octet-stream", Data = [0xDE, 0xAD, 0xBE, 0xEF] };
        var request = new PlainFileRequest { Files = [file] };

        await handler.InnerHandle(null, request, CancellationToken.None);

        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        var part = multipart.Single(p => p.Headers.ContentDisposition?.FileName?.Trim('"') == "bytes.bin");
        part.ShouldBeOfType<ByteArrayContent>();
    }

    [Fact]
    public async Task InnerHandle_FileRequest_SetsContentTypeOnFilePart()
    {
        var (handler, _, httpClient) = SetupFileHandler<PlainFileRequest>();
        var getRequest = CaptureSentRequest(httpClient);
        var file = new MetalNexusFile { FileName = "photo.jpg", ContentType = "image/jpeg", Data = [1] };
        var request = new PlainFileRequest { Files = [file] };

        await handler.InnerHandle(null, request, CancellationToken.None);

        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        var part = multipart.Single(p => p.Headers.ContentDisposition?.FileName?.Trim('"') == "photo.jpg");
        part.Headers.ContentType!.MediaType.ShouldBe("image/jpeg");
    }

    [Fact]
    public async Task InnerHandle_FileRequest_WithNullFilesArray_DoesNotThrow()
    {
        var (handler, _, httpClient) = SetupFileHandler<PlainFileRequest>();
        CaptureSentRequest(httpClient);
        var request = new PlainFileRequest { Files = null! };

        await Should.NotThrowAsync(() => handler.InnerHandle(null, request, CancellationToken.None));
    }

    [Fact]
    public async Task InnerHandle_FileRequest_WithNullFileSlotProperty_SkipsSlot()
    {
        var (handler, _, httpClient) = SetupFileHandler<FileRequestWithNullSlot>();
        var getRequest = CaptureSentRequest(httpClient);
        var request = new FileRequestWithNullSlot { Files = [], Avatar = null };

        await Should.NotThrowAsync(() => handler.InnerHandle(null, request, CancellationToken.None));

        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        multipart.Any(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "avatar").ShouldBeFalse();
    }

    [Fact]
    public async Task InnerHandle_FileRequest_FileSlotPropertyIsNotIncludedAsScalarField()
    {
        var (handler, _, httpClient) = SetupFileHandler<FileRequestWithSlot>();
        var getRequest = CaptureSentRequest(httpClient);
        var file = new MetalNexusFile { FileName = "avatar.png", ContentType = "image/png", Data = [1] };
        var request = new FileRequestWithSlot { Files = [], Avatar = file };

        await handler.InnerHandle(null, request, CancellationToken.None);

        // The Avatar slot should appear exactly once under "avatar", not also as a scalar "Avatar" field
        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        multipart.Any(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "Avatar").ShouldBeFalse();
    }

    [Fact]
    public async Task InnerHandle_FileRequest_BaseClassFilesPropertyNotIncludedAsScalarField()
    {
        var (handler, _, httpClient) = SetupFileHandler<FileRequestWithScalar>();
        var getRequest = CaptureSentRequest(httpClient);
        var request = new FileRequestWithScalar { Files = [], Description = "test" };

        await handler.InnerHandle(null, request, CancellationToken.None);

        var multipart = (MultipartFormDataContent)getRequest()!.Content!;
        multipart.Any(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "Files").ShouldBeFalse();
    }
}
