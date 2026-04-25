using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;
using RossWright.MetalNexus.Server;
using System.Reflection;
using System.Text.Json;

namespace RossWright.MetalNexus.Tests;

public class AddMetalNexusServerExtensionsTest
{
    public AddMetalNexusServerExtensionsTest()
    {
        app = Substitute.For<IApplicationBuilder>();
        serviceCollection = new();
    }
    IApplicationBuilder app;
    ServiceCollection serviceCollection;

    IDisposable SetupForHandleResult()
    {
        mockMediator = Substitute.For<IMediator>();
        serviceCollection.AddScoped(sp => mockMediator);

        httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceCollection.BuildServiceProvider();
        responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        return responseBody;
    }
    IMediator mockMediator = null!;
    HttpContext httpContext = null!;
    MemoryStream responseBody = null!;
    string GetResponseBody()
    {
        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody);
        return reader.ReadToEnd();
    }

    [Fact] public async Task HandleRequest_EmptyCommandHandler()
    {
        using var stream = SetupForHandleResult();

        TestEndpoint endpoint = new()
        {
            RequestType = typeof(EmptyCommand),
            ResponseType = null,
            Tag = null,
            Path = "/",
            HttpMethod = HttpMethod.Post,
            RequestAsQueryParams = true,
            HttpClientName = null
        };

        mockMediator.Send(Arg.Any<EmptyCommand.Request>())
            .Returns(Task.FromResult<object?>(null));

        IMetalNexusOptions options = Substitute.For<IMetalNexusOptions>();
        options.ServerStackTraceOnExceptionsIncluded.Returns(false);
        options.DefaultToBadRequest.Returns(true);
        await MetalNexusMiddleware.HandleRequest(httpContext, endpoint, false, options);
    }

    [Fact] public async Task HandleRequest_Throws()
    {
        using var stream = SetupForHandleResult();

        TestEndpoint endpoint = new()
        {
            RequestType = typeof(EmptyCommand.Request),
            ResponseType = null,
            Tag = null,
            Path = "/",
            HttpMethod = HttpMethod.Post,
            RequestAsQueryParams = true,
            HttpClientName = null
        };

        mockMediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<object?>(
                new TargetInvocationException(new NotImplementedException())));

        IMetalNexusOptions options = Substitute.For<IMetalNexusOptions>();
        options.ServerStackTraceOnExceptionsIncluded.Returns(false);
        options.DefaultToBadRequest.Returns(true);
        await MetalNexusMiddleware.HandleRequest(httpContext, endpoint, false, options);
        httpContext.Response.StatusCode.ShouldBe(501);

        var body = GetResponseBody();
        var exceptionResponse = JsonSerializer.Deserialize<ExceptionResponse>(body);
        exceptionResponse?.TypeName.ShouldBe(typeof(NotImplementedException).FullName);
    }
}

public class TestEndpoint : IEndpoint
{
    public string Path { get; set; } = null!;
    public HttpMethod HttpMethod { get; set; } = null!;
    public bool RequestAsQueryParams { get; set; }
    public bool HasPathParams { get; set; }
    public string? HttpClientName { get; set; }
    public Type RequestType { get; set; } = null!;
    public Type? ResponseType { get; set; }
    public string? Tag { get; set; }
    public bool RequiresAuthentication { get; set; }
    public string[]? AuthorizedRoles { get; set; }
    public bool AllowProvisional { get; set; }
    public TimeSpan? HttpClientTimeout { get; set; }
    public string[] HeaderProperties { get; set; } = [];
}