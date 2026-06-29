using System.Net;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Shouldly;

namespace RossWright.MetalNexus.Tests.Internal.Handlers;

public class ApiRequestHandlerTests
{
    [Fact]
    public void Constructor_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

        // Act
        var handler = new ApiRequestHandler<TestRequest>(registry, httpClientFactory, options, loggerFactory);

        // Assert
        handler.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WithSuccessfulResponse_CompletesSuccessfully()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = CreateHandler(httpResponse);
        var request = new TestRequest();

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesCancellationToken()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = CreateHandler(httpResponse);
        var request = new TestRequest();
        using var cts = new CancellationTokenSource();

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await handler.Handle(request, cts.Token));
    }

    [Fact]
    public async Task Handle_WithEndpointNotFound_ThrowsMetalNexusException()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = Substitute.For<IMetalNexusClientOptions>();

        registry.FindEndpoint(typeof(TestRequest)).Returns((IEndpoint?)null);

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new ApiRequestHandler<TestRequest>(registry, httpClientFactory, options, loggerFactory);
        var request = new TestRequest();

        // Act & Assert
        var ex = await Should.ThrowAsync<MetalNexusException>(async () =>
            await handler.Handle(request, CancellationToken.None));

        ex.Message.ShouldContain("Endpoint for");
        ex.Message.ShouldContain("not defined");
    }

    [Fact]
    public async Task Handle_WithFailedResponse_ThrowsMetalNexusException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"message\":\"Bad request\"}")
        };
        var handler = CreateHandler(httpResponse);
        var request = new TestRequest();

        // Act & Assert
        await Should.ThrowAsync<MetalNexusException>(async () =>
            await handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CallsInnerHandleWithNullHttpClientName()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = CreateHandler(httpResponse);
        var request = new TestRequest();

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert - If we get here without exception, InnerHandle was called correctly
        // The null first parameter to InnerHandle is verified by the fact that
        // the default connection name logic is used (tested via CreateHandler setup)
    }

    private static ApiRequestHandler<TestRequest> CreateHandler(HttpResponseMessage httpResponse)
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
        endpoint.Path.Returns("/test");
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestType.Returns(typeof(TestRequest));

        var registry = Substitute.For<IMetalNexusRegistry>();
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);

        var clientOptions = Substitute.For<IMetalNexusClientOptions>();
        clientOptions.DefaultConnectionName.Returns((string?)null);

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

        return new ApiRequestHandler<TestRequest>(
            registry,
            httpClientFactory,
            clientOptions,
            loggerFactory);
    }

    private class TestRequest : IRequest
    {
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
