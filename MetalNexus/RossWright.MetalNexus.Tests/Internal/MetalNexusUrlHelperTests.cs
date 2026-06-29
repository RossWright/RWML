using NSubstitute;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Shouldly;

namespace RossWright.MetalNexus.Tests.Internal;

public class MetalNexusUrlHelperTests
{
    [Fact]
    public void GetUrlFor_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => helper.GetUrlFor<TestRequest>(null!));
    }

    [Fact]
    public void GetUrlFor_EndpointNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new TestRequest { Value = "test" };

        registry.FindEndpoint(typeof(TestRequest)).Returns((IEndpoint?)null);

        // Act & Assert
        var ex = Should.Throw<InvalidOperationException>(() => helper.GetUrlFor(request));
        ex.Message.ShouldBe($"Endpoint Schema contains no endpoint for {typeof(TestRequest).FullName}");
    }

    [Fact]
    public void GetUrlFor_ValidRequest_CombinesBaseAddressAndEndpointPath()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var endpoint = Substitute.For<IEndpoint>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new TestRequest { Value = "test" };

        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        endpoint.HttpClientName.Returns((string?)null);
        endpoint.Path.Returns("/test/endpoint");
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.RequestType.Returns(typeof(TestRequest));
        httpClientFactory.CreateClient(string.Empty).Returns(httpClient);

        // Act
        var result = helper.GetUrlFor(request);

        // Assert
        result.ShouldBe("https://api.example.com/test/endpoint");
        httpClientFactory.Received(1).CreateClient(string.Empty);
    }

    [Fact]
    public void GetUrlFor_WithHttpClientName_UsesSpecifiedClientName()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var endpoint = Substitute.For<IEndpoint>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new TestRequest { Value = "test" };

        var httpClient = new HttpClient { BaseAddress = new Uri("https://custom.example.com") };
        
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        endpoint.HttpClientName.Returns("CustomClient");
        endpoint.Path.Returns("/custom/path");
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.RequestType.Returns(typeof(TestRequest));
        httpClientFactory.CreateClient("CustomClient").Returns(httpClient);

        // Act
        var result = helper.GetUrlFor(request);

        // Assert
        result.ShouldBe("https://custom.example.com/custom/path");
        httpClientFactory.Received(1).CreateClient("CustomClient");
    }

    [Fact]
    public void GetUrlFor_NullBaseAddress_UsesEmptyString()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var endpoint = Substitute.For<IEndpoint>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new TestRequest { Value = "test" };

        var httpClient = new HttpClient(); // No BaseAddress set
        
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        endpoint.HttpClientName.Returns((string?)null);
        endpoint.Path.Returns("/path/only");
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.RequestType.Returns(typeof(TestRequest));
        httpClientFactory.CreateClient(string.Empty).Returns(httpClient);

        // Act
        var result = helper.GetUrlFor(request);

        // Assert
        result.ShouldBe("path/only");
    }

    [Fact]
    public void GetUrlFor_EmptyBaseAddressAndEmptyPath_ReturnsEmptyString()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var endpoint = Substitute.For<IEndpoint>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new TestRequest { Value = "test" };

        var httpClient = new HttpClient();
        
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        endpoint.HttpClientName.Returns((string?)null);
        endpoint.Path.Returns(string.Empty);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.RequestType.Returns(typeof(TestRequest));
        httpClientFactory.CreateClient(string.Empty).Returns(httpClient);

        // Act
        var result = helper.GetUrlFor(request);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetUrlFor_BaseAddressWithTrailingSlash_CombinesProperly()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var endpoint = Substitute.For<IEndpoint>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new TestRequest { Value = "test" };

        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
        
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        endpoint.HttpClientName.Returns((string?)null);
        endpoint.Path.Returns("/api/test");
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.RequestType.Returns(typeof(TestRequest));
        httpClientFactory.CreateClient(string.Empty).Returns(httpClient);

        // Act
        var result = helper.GetUrlFor(request);

        // Assert
        result.ShouldBe("https://api.example.com/api/test");
    }

    [Fact]
    public void GetUrlFor_PathWithoutLeadingSlash_CombinesProperly()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var endpoint = Substitute.For<IEndpoint>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new TestRequest { Value = "test" };

        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        endpoint.HttpClientName.Returns((string?)null);
        endpoint.Path.Returns("api/test");
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.RequestType.Returns(typeof(TestRequest));
        httpClientFactory.CreateClient(string.Empty).Returns(httpClient);

        // Act
        var result = helper.GetUrlFor(request);

        // Assert
        result.ShouldBe("https://api.example.com/api/test");
    }

    [Fact]
    public void GetUrlFor_WithPathParams_SubstitutesParams()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var endpoint = Substitute.For<IEndpoint>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new RequestWithPathParams { Id = "123", Value = "test" };

        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        
        registry.FindEndpoint(typeof(RequestWithPathParams)).Returns(endpoint);
        endpoint.HttpClientName.Returns((string?)null);
        endpoint.Path.Returns("/api/{Id}");
        endpoint.HasPathParams.Returns(true);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.RequestType.Returns(typeof(RequestWithPathParams));
        httpClientFactory.CreateClient(string.Empty).Returns(httpClient);

        // Act
        var result = helper.GetUrlFor(request);

        // Assert
        result.ShouldBe("https://api.example.com/api/123");
    }

    [Fact]
    public void GetUrlFor_HttpClientDisposed_DoesNotThrow()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var endpoint = Substitute.For<IEndpoint>();
        var helper = new MetalNexusUrlHelper(registry, httpClientFactory);
        var request = new TestRequest { Value = "test" };

        var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        
        registry.FindEndpoint(typeof(TestRequest)).Returns(endpoint);
        endpoint.HttpClientName.Returns((string?)null);
        endpoint.Path.Returns("/test");
        endpoint.HasPathParams.Returns(false);
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.RequestType.Returns(typeof(TestRequest));
        httpClientFactory.CreateClient(string.Empty).Returns(httpClient);

        // Act
        var result = helper.GetUrlFor(request);

        // Assert - HttpClient should be disposed after use but not throw
        result.ShouldBe("https://api.example.com/test");
        httpClient.BaseAddress.ShouldNotBeNull(); // Still accessible after method completes
    }

    private class TestRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    private class RequestWithPathParams
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
