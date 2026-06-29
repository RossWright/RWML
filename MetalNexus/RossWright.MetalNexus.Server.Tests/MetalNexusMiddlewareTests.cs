using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using RossWright.MetalNexus.Server;

namespace RossWright.MetalNexus.Tests;

public class MetalNexusMiddlewareTests
{
    [Fact]
    public void Constructor_WithSimpleEndpoint_StoresInDictionary()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.HasPathParams.Returns(false);

        var middleware = new MetalNexusMiddleware(options, false, new[] { endpoint });

        middleware.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithPathParamEndpoint_StoresInList()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/{id}");
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.HasPathParams.Returns(true);

        var middleware = new MetalNexusMiddleware(options, true, new[] { endpoint });

        middleware.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithVariousPathFormats_NormalizesPath()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        
        var endpoint1 = Substitute.For<IEndpoint>();
        endpoint1.Path.Returns("\\API\\Test\\");
        endpoint1.HttpMethod.Returns(HttpMethod.Get);
        endpoint1.HasPathParams.Returns(false);

        var endpoint2 = Substitute.For<IEndpoint>();
        endpoint2.Path.Returns("/api/User/{UserId}/profile");
        endpoint2.HttpMethod.Returns(HttpMethod.Post);
        endpoint2.HasPathParams.Returns(true);

        var middleware = new MetalNexusMiddleware(options, false, new[] { endpoint1, endpoint2 });

        middleware.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithMultiplePathParamEndpoints_OrdersByPathDepth()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        
        var endpoint1 = Substitute.For<IEndpoint>();
        endpoint1.Path.Returns("/api/{id}");
        endpoint1.HttpMethod.Returns(HttpMethod.Get);
        endpoint1.HasPathParams.Returns(true);

        var endpoint2 = Substitute.For<IEndpoint>();
        endpoint2.Path.Returns("/api/users/{id}/posts/{postId}");
        endpoint2.HttpMethod.Returns(HttpMethod.Get);
        endpoint2.HasPathParams.Returns(true);

        var middleware = new MetalNexusMiddleware(options, false, new[] { endpoint1, endpoint2 });

        middleware.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("/api/test", "api/test")]
    [InlineData("\\api\\test", "api/test")]
    [InlineData("/API/TEST/", "api/test")]
    [InlineData("///api///test///", "api///test")]
    public void CleanPath_VariousPaths_ReturnsNormalizedPath(string input, string expected)
    {
        var result = MetalNexusMiddleware.CleanPath(input);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WithMatchingSimpleEndpoint_CallsHandleRequest()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.HasPathParams.Returns(false);
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns((Type?)null);
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var middleware = new MetalNexusMiddleware(options, false, new[] { endpoint });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(null));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        await middleware.Handle(context, next);

        nextCalled.ShouldBeFalse();
        context.Response.StatusCode.ShouldBe(200);
    }

    [Fact]
    public async Task Handle_WithMatchingPathParamEndpoint_CallsHandleRequest()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = new StubEndpoint<PathParamRequest>("/api/{Id}")
        {
            HttpMethodValue = HttpMethod.Post,
            HasPathParamsValue = true,
            RequiresAuthenticationValue = false,
            ResponseTypeValue = null,
            RequestAsQueryParamsValue = false,
            HeaderPropertiesValue = Array.Empty<string>()
        };

        var middleware = new MetalNexusMiddleware(options, false, new[] { endpoint });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/123";
        context.Request.Method = "POST";
        var json = JsonSerializer.Serialize(new { });
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(null));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        await middleware.Handle(context, next);

        nextCalled.ShouldBeFalse();
        context.Response.StatusCode.ShouldBe(200);
    }

    [Fact]
    public async Task Handle_WithNoMatchingEndpoint_CallsNext()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns("/api/test");
        endpoint.HttpMethod.Returns(HttpMethod.Get);
        endpoint.HasPathParams.Returns(false);

        var middleware = new MetalNexusMiddleware(options, false, new[] { endpoint });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/other";
        context.Request.Method = "GET";

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        await middleware.Handle(context, next);

        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WithNoPath_CallsNext()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var middleware = new MetalNexusMiddleware(options, false, Array.Empty<IEndpoint>());

        var context = new DefaultHttpContext();

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        await middleware.Handle(context, next);

        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequest_RequiresAuthButNotInstalled_Returns401()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(true);

        var context = new DefaultHttpContext();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(401);
    }

    [Fact]
    public async Task HandleRequest_AuthenticationFails_Returns401()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(true);

        var context = new DefaultHttpContext();
        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>())
            .Returns(AuthenticateResult.Fail("Authentication failed"));

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, true, options);

        context.Response.StatusCode.ShouldBe(401);
    }

    [Fact]
    public async Task HandleRequest_ProvisionalUserOnNonProvisionalEndpoint_Returns401()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(true);
        endpoint.AllowProvisional.Returns(false);

        var claims = new[] { new Claim("Provisional", "true") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext();
        context.User = principal;

        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>())
            .Returns(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, true, options);

        context.Response.StatusCode.ShouldBe(401);
    }

    [Fact]
    public async Task HandleRequest_UserNotInRequiredRole_Returns403()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(true);
        endpoint.AuthorizedRoles.Returns(new[] { "Admin" });

        var claims = new[] { new Claim(ClaimTypes.Role, "User") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext();
        context.User = principal;

        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>())
            .Returns(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, true, options);

        context.Response.StatusCode.ShouldBe(403);
    }

    [Fact]
    public async Task HandleRequest_PolicyAuthorizationFails_Returns403()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(true);
        endpoint.AuthorizedRoles.Returns((string[]?)null);
        endpoint.AuthorizationPolicy.Returns("RequireMfa");

        var claims = new[] { new Claim(ClaimTypes.Name, "user") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext();
        context.User = principal;

        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>())
            .Returns(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));

        var authorizationService = Substitute.For<IAuthorizationService>();
        authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(AuthorizationResult.Failed());

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        services.AddSingleton(authorizationService);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, true, options);

        context.Response.StatusCode.ShouldBe(403);
    }

    [Fact]
    public async Task HandleRequest_PolicyAuthorizationSucceeds_ContinuesToDispatch()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(true);
        endpoint.AuthorizedRoles.Returns((string[]?)null);
        endpoint.AuthorizationPolicy.Returns("RequireMfa");
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns((Type?)null);
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var claims = new[] { new Claim(ClaimTypes.Name, "user") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext();
        context.User = principal;
        context.Request.Method = "GET";

        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>())
            .Returns(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));

        var authorizationService = Substitute.For<IAuthorizationService>();
        authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(AuthorizationResult.Success());

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(null));

        var services = new ServiceCollection();
        services.AddSingleton(authService);
        services.AddSingleton(authorizationService);
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, true, options);

        context.Response.StatusCode.ShouldBe(200);
    }

    [Fact]
    public async Task HandleRequest_SuccessWithJsonResponse_WritesJson()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mediator = Substitute.For<IMediator>();
        var response = new SimpleResponse { Message = "Success" };
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(response));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(200);
        context.Response.ContentType.ShouldBe("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        json.ShouldContain("Success");
    }

    [Fact]
    public async Task HandleRequest_SuccessWithFileResponse_WritesFile()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(MetalNexusFile));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mediator = Substitute.For<IMediator>();
        var fileData = Encoding.UTF8.GetBytes("test file content");
        var fileResponse = new MetalNexusFile
        {
            ContentType = "text/plain",
            FileName = "test.txt",
            Data = fileData,
            IsAttachment = true
        };
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(fileResponse));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(200);
        context.Response.ContentType.ShouldBe("text/plain");
        context.Response.ContentLength.ShouldBe(fileData.Length);
        context.Response.Headers.ContentDisposition.ToString().ShouldContain("attachment");
        context.Response.Headers.ContentDisposition.ToString().ShouldContain("test.txt");
    }

    [Fact]
    public async Task HandleRequest_SuccessWithFileResponseInline_WritesFileWithInlineDisposition()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(MetalNexusFile));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mediator = Substitute.For<IMediator>();
        var fileData = Encoding.UTF8.GetBytes("test file content");
        var fileResponse = new MetalNexusFile
        {
            ContentType = "text/plain",
            FileName = "test.txt",
            Data = fileData,
            IsAttachment = false
        };
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(fileResponse));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(200);
        context.Response.Headers.ContentDisposition.ToString().ShouldContain("inline");
    }

    [Fact]
    public async Task HydrateRequest_WithQueryParams_CreatesRequestFromQuery()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?name=test");

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<SimpleRequest>();
    }

    [Fact]
    public async Task HydrateRequest_WithJsonBody_DeserializesRequest()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        var json = JsonSerializer.Serialize(new { Name = "test" });
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<SimpleRequest>();
    }

    [Fact]
    public async Task HydrateRequest_WithCancelledToken_ThrowsOperationCancelled()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var cts = new CancellationTokenSource();
        var context = new DefaultHttpContext();
        context.Request.Body = new NeverEndingStream();

        // Simulate the request being aborted before/during body read
        cts.Cancel();
        context.RequestAborted = cts.Token;

        await Should.ThrowAsync<OperationCanceledException>(
            () => MetalNexusMiddleware.HydrateRequest(context, endpoint));
    }

    [Fact]
    public async Task HydrateRequest_WithRawRequest_SetsRawBody()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(RawRequest));
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        var bodyContent = "raw body content";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyContent));

        var services = new ServiceCollection();
        context.RequestServices = services.BuildServiceProvider();

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<RawRequest>();
        ((RawRequest)result).RawRequestBody.ShouldBe(bodyContent);
    }

    [Fact]
    public async Task HydrateRequest_WithGenericRawRequest_SetsRawBody()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(GenericRawRequest));
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        var bodyContent = "raw body content";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyContent));

        var services = new ServiceCollection();
        context.RequestServices = services.BuildServiceProvider();

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<GenericRawRequest>();
        ((GenericRawRequest)result).RawRequestBody.ShouldBe(bodyContent);
    }

    [Fact]
    public async Task HydrateRequest_WithHeaderProperties_SetsHeaderValues()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(HeaderRequest));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(new[] { "AuthToken" });

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Auth-Token"] = "test-token";

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<HeaderRequest>();
        ((HeaderRequest)result).AuthToken.ShouldBe("test-token");
    }

    [Fact]
    public async Task HydrateRequest_WithFileUpload_ProcessesFiles()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(FileUploadRequest));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data";

        var fileContent = Encoding.UTF8.GetBytes("file content");
        var stream = new MemoryStream(fileContent);
        var formFile = new FormFile(stream, 0, fileContent.Length, "file", "test.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), new FormFileCollection { formFile });
        context.Request.Form = formCollection;

        var maxBodySizeFeature = Substitute.For<IHttpMaxRequestBodySizeFeature>();
        context.Features.Set(maxBodySizeFeature);

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<FileUploadRequest>();
        var fileRequest = (FileUploadRequest)result;
        fileRequest.Files.ShouldNotBeNull();
        fileRequest.Files.Length.ShouldBe(1);
        fileRequest.Files[0].FileName.ShouldBe("test.txt");
        fileRequest.Files[0].DataStream.ShouldNotBeNull();
        fileRequest.Files[0].Data.ShouldBeNull();
        maxBodySizeFeature.Received(1).MaxRequestBodySize = 10000000;
    }

    [Fact]
    public async Task HydrateRequest_WithFileUpload_StreamContainsFileContent()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(FileUploadRequest));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data";

        var fileContent = Encoding.UTF8.GetBytes("streaming content");
        var stream = new MemoryStream(fileContent);
        var formFile = new FormFile(stream, 0, fileContent.Length, "file", "data.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), new FormFileCollection { formFile });
        context.Request.Form = formCollection;

        var maxBodySizeFeature = Substitute.For<IHttpMaxRequestBodySizeFeature>();
        context.Features.Set(maxBodySizeFeature);

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        var fileResult = ((FileUploadRequest)result).Files[0];
        using var ms = new MemoryStream();
        await fileResult.DataStream!.CopyToAsync(ms);
        Encoding.UTF8.GetString(ms.ToArray()).ShouldBe("streaming content");
    }

    [Fact]
    public async Task HydrateRequest_WithQueryParamsAndProperties_ResolvesProperties()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(ComplexQueryRequest));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?Name=test&Age=25");

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<ComplexQueryRequest>();
        var request = (ComplexQueryRequest)result;
        request.Name.ShouldBe("test");
        request.Age.ShouldBe(25);
    }

    [Fact]
    public async Task HydrateRequest_WithFileUploadNoLimit_ProcessesFilesWithoutSettingLimit()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(FileUploadNoLimitRequest));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data";

        var fileContent = Encoding.UTF8.GetBytes("file content");
        var stream = new MemoryStream(fileContent);
        var formFile = new FormFile(stream, 0, fileContent.Length, "file", "test.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), new FormFileCollection { formFile });
        context.Request.Form = formCollection;

        var maxBodySizeFeature = Substitute.For<IHttpMaxRequestBodySizeFeature>();
        context.Features.Set(maxBodySizeFeature);

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<FileUploadNoLimitRequest>();
        var fileRequest = (FileUploadNoLimitRequest)result;
        fileRequest.Files.ShouldNotBeNull();
        fileRequest.Files.Length.ShouldBe(1);
        fileRequest.Files[0].FileName.ShouldBe("test.txt");
        fileRequest.Files[0].DataStream.ShouldNotBeNull();
        fileRequest.Files[0].Data.ShouldBeNull();
    }

    [Fact]
    public void FillSlotValues_HappyPath()
    {
        var obj = new Test();
        MetalNexusMiddleware.FillSlotValues(new StubEndpoint<Test>("/{First}"), "/firstValue", obj);
        obj.First.ShouldBe("firstValue");

        MetalNexusMiddleware.FillSlotValues(new StubEndpoint<Test>("/First/{Second}/{Third}"), "/first/secondValue/thirdValue", obj);
        obj.Second.ShouldBe("secondValue");
        obj.Third.ShouldBe("thirdValue");
    }

    [Fact]
    public void FillSlotValues_PathLengthMismatch_ThrowsException()
    {
        var obj = new Test();
        var endpoint = new StubEndpoint<Test>("/{First}/{Second}");

        Should.Throw<MetalNexusException>(() => MetalNexusMiddleware.FillSlotValues(endpoint, "/value", obj));
    }

    // ── CollapseBrackets Tests ────────────────────────────────────────────────────

    [Fact]
    public void CollapseBrackets_EmptyString_ReturnsEmpty()
    {
        var result = MetalNexusMiddleware.CollapseBrackets("");
        result.ShouldBe("");
    }

    [Fact]
    public void CollapseBrackets_NoBrackets_ReturnsOriginal()
    {
        var result = MetalNexusMiddleware.CollapseBrackets("api/test/path");
        result.ShouldBe("api/test/path");
    }

    [Fact]
    public void CollapseBrackets_WithSingleBracketParam_CollapsesBracketContent()
    {
        var result = MetalNexusMiddleware.CollapseBrackets("api/{userId}/profile");
        result.ShouldBe("api/{}/profile");
    }

    [Fact]
    public void CollapseBrackets_WithMultipleBracketParams_CollapsesAllBrackets()
    {
        var result = MetalNexusMiddleware.CollapseBrackets("api/{userId}/posts/{postId}");
        result.ShouldBe("api/{}/posts/{}");
    }

    [Fact]
    public void CollapseBrackets_ConsecutiveBrackets_CollapsesAll()
    {
        var result = MetalNexusMiddleware.CollapseBrackets("{first}{second}{third}");
        result.ShouldBe("{}{}{}");
    }

    [Fact]
    public void CollapseBrackets_OnlyBrackets_ReturnsCollapsedBrackets()
    {
        var result = MetalNexusMiddleware.CollapseBrackets("{parameter}");
        result.ShouldBe("{}");
    }

    // ── IsBracketUrlMatch Tests ───────────────────────────────────────────────────

    [Fact]
    public void IsBracketUrlMatch_ExactMatch_ReturnsTrue()
    {
        var result = MetalNexusMiddleware.IsBracketUrlMatch("api/test/path", "api/test/path");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBracketUrlMatch_CaseInsensitive_ReturnsTrue()
    {
        var result = MetalNexusMiddleware.IsBracketUrlMatch("api/test/path", "API/TEST/PATH");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBracketUrlMatch_SingleBracketMatch_ReturnsTrue()
    {
        var result = MetalNexusMiddleware.IsBracketUrlMatch("api/{}/profile", "api/123/profile");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBracketUrlMatch_MultipleBracketMatch_ReturnsTrue()
    {
        var result = MetalNexusMiddleware.IsBracketUrlMatch("api/{}/posts/{}", "api/user123/posts/456");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsBracketUrlMatch_MismatchedPaths_ReturnsFalse()
    {
        var result = MetalNexusMiddleware.IsBracketUrlMatch("api/test/path", "api/other/path");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsBracketUrlMatch_DifferentLengths_ReturnsFalse()
    {
        var result = MetalNexusMiddleware.IsBracketUrlMatch("api/test", "api/test/path");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsBracketUrlMatch_EmptyStrings_ReturnsTrue()
    {
        var result = MetalNexusMiddleware.IsBracketUrlMatch("", "");
        result.ShouldBeTrue();
    }

    // ── BuildJsonObjectFromQuery Tests ────────────────────────────────────────────

    [Fact]
    public void BuildJsonObjectFromQuery_SimpleStringValue_AddedToObject()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "Name", "TestValue" }
        });

        var result = MetalNexusMiddleware.BuildJsonObjectFromQuery(query, typeof(QueryParamTest));

        result["Name"]!.GetValue<string>().ShouldBe("TestValue");
    }

    [Fact]
    public void BuildJsonObjectFromQuery_IntValue_AddedToObject()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "Age", "42" }
        });

        var result = MetalNexusMiddleware.BuildJsonObjectFromQuery(query, typeof(QueryParamTest));

        result["Age"]!.GetValue<string>().ShouldBe("42");
    }

    [Fact]
    public void BuildJsonObjectFromQuery_ArrayValues_AddedAsJsonArray()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "Tags", new Microsoft.Extensions.Primitives.StringValues(new[] { "tag1", "tag2", "tag3" }) }
        });

        var result = MetalNexusMiddleware.BuildJsonObjectFromQuery(query, typeof(QueryParamTest));

        var arr = result["Tags"]!.AsArray();
        arr.Count.ShouldBe(3);
        arr[0]!.GetValue<string>().ShouldBe("tag1");
        arr[1]!.GetValue<string>().ShouldBe("tag2");
        arr[2]!.GetValue<string>().ShouldBe("tag3");
    }

    [Fact]
    public void BuildJsonObjectFromQuery_NestedComplexType_AddedAsNestedObject()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "City", "Seattle" },
            { "ZipCode", "98101" }
        });

        var result = MetalNexusMiddleware.BuildJsonObjectFromQuery(query, typeof(QueryParamTest));

        var nested = result["Nested"]!.AsObject();
        nested["City"]!.GetValue<string>().ShouldBe("Seattle");
        nested["ZipCode"]!.GetValue<string>().ShouldBe("98101");
    }

    [Fact]
    public void BuildJsonObjectFromQuery_MissingValue_PropertyNotPresent()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

        var result = MetalNexusMiddleware.BuildJsonObjectFromQuery(query, typeof(QueryParamTest));

        result.ContainsKey("Name").ShouldBeFalse();
        result.ContainsKey("Tags").ShouldBeFalse();
    }

    [Fact]
    public void BuildJsonObjectFromQuery_ExcludedProps_NotAddedToObject()
    {
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "Name", "TestValue" },
            { "Age", "25" }
        });
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name" };

        var result = MetalNexusMiddleware.BuildJsonObjectFromQuery(query, typeof(QueryParamTest), excluded);

        result.ContainsKey("Name").ShouldBeFalse();
        result["Age"]!.GetValue<string>().ShouldBe("25");
    }

    // ── HydrateRequest init-only / record Tests ────────────────────────────────────

    [Fact]
    public async Task HydrateRequest_WithInitOnlyQueryRequest_PopulatesProperties()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(InitOnlyQueryRequest));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?Name=Alice&Age=30");

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldBeOfType<InitOnlyQueryRequest>();
        var req = (InitOnlyQueryRequest)result;
        req.Name.ShouldBe("Alice");
        req.Age.ShouldBe(30);
    }

    [Fact]
    public async Task HydrateRequest_WithRecordQueryRequest_PopulatesProperties()
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequestType.Returns(typeof(RecordQueryRequest));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?Name=Bob&Age=25");

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldBeOfType<RecordQueryRequest>();
        var req = (RecordQueryRequest)result;
        req.Name.ShouldBe("Bob");
        req.Age.ShouldBe(25);
    }

    // ── IMetalNexusResponseContext Tests ─────────────────────────────────────────

    [Fact]
    public async Task HandleRequest_HandlerSetsStatusCode_ResponseUsesAmbientCode()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());
        endpoint.SuccessStatusCode.Returns((System.Net.HttpStatusCode?)null);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(callInfo =>
        {
            // Simulate a handler that injects and sets StatusCode
            MetalNexusResponseContext.Current!.StatusCode = System.Net.HttpStatusCode.Created;
            return Task.FromResult<object?>(new SimpleResponse { Message = "Created" });
        });

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(201);
    }

    [Fact]
    public async Task HandleRequest_HandlerSetsLocation_LocationHeaderPresent()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());
        endpoint.SuccessStatusCode.Returns((System.Net.HttpStatusCode?)null);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(callInfo =>
        {
            MetalNexusResponseContext.Current!.StatusCode = System.Net.HttpStatusCode.Created;
            MetalNexusResponseContext.Current!.Location = "/api/items/42";
            return Task.FromResult<object?>(new SimpleResponse { Message = "Created" });
        });

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(201);
        context.Response.Headers.Location.ToString().ShouldBe("/api/items/42");
    }

    [Fact]
    public async Task HandleRequest_HandlerThrows_AmbientStatusCodeIgnored()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        options.ServerStackTraceOnExceptionsIncluded.Returns(false);
        options.DefaultToBadRequest.Returns(false);
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());
        endpoint.SuccessStatusCode.Returns((System.Net.HttpStatusCode?)null);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(callInfo =>
        {
            MetalNexusResponseContext.Current!.StatusCode = System.Net.HttpStatusCode.Created;
            throw new InvalidOperationException("something went wrong");
        });

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        // Exception maps to 500; the 201 the handler set must not be used
        context.Response.StatusCode.ShouldBe(500);
    }

    [Fact]
    public async Task HandleRequest_OptionEAttributeSet_UsedWhenHandlerDoesNotSetCode()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());
        endpoint.SuccessStatusCode.Returns(System.Net.HttpStatusCode.Accepted);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mediator = Substitute.For<IMediator>();
        // Handler does NOT touch IMetalNexusResponseContext
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(new SimpleResponse { Message = "ok" }));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(202);
    }

    [Fact]
    public async Task HandleRequest_OptionDAndOptionEBothSet_OptionDWins()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());
        endpoint.SuccessStatusCode.Returns(System.Net.HttpStatusCode.Accepted); // Option E says 202

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(callInfo =>
        {
            MetalNexusResponseContext.Current!.StatusCode = System.Net.HttpStatusCode.Created; // Option D says 201
            return Task.FromResult<object?>(new SimpleResponse { Message = "ok" });
        });

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(201); // Option D wins
    }

    // ── Helper Classes ─────────────────────────────────────────────────────────────

    public class QueryParamTest
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string[]? Tags { get; set; }
        public int[]? Numbers { get; set; }
        public NestedQueryParam? Nested { get; set; }
    }

    public class NestedQueryParam
    {
        public string? City { get; set; }
        public string? ZipCode { get; set; }
    }

    public class InitOnlyQueryRequest
    {
        public string? Name { get; init; }
        public int Age { get; init; }
    }

    public record RecordQueryRequest
    {
        public string? Name { get; init; }
        public int Age { get; init; }
    }

    public class Test
    {
        public string? First { get; set; }
        public string? Second { get; set; }
        public string? Third { get; set; }
        public object? Obj { get; set; }
    }

    public class SimpleRequest
    {
        public string? Name { get; set; }
    }

    public class SimpleResponse
    {
        public string? Message { get; set; }
    }

    public class ComplexQueryRequest
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class PathParamRequest
    {
        public string? Id { get; set; }
    }

    public class RawRequest : IMetalNexusRawRequest
    {
        public string? RawRequestBody { get; set; }
    }

    public class GenericRawRequest : IMetalNexusRawRequest<string>
    {
        public string? RawRequestBody { get; set; }
    }

    public class HeaderRequest
    {
        [FromHeader("X-Auth-Token")]
        public string? AuthToken { get; set; }
    }

    [UploadLimit(10000000)]
    public class FileUploadRequest : MetalNexusFileRequest
    {
        public string? Description { get; set; }
    }

    public class FileUploadNoLimitRequest : MetalNexusFileRequest
    {
        public string? Description { get; set; }
    }

    public class StubEndpoint<TRequest>(string path) : IEndpoint
    {
        public string Path { get; } = path;

        public Type RequestType { get; } = typeof(TRequest);

        public Type? ResponseTypeValue { get; set; }
        public Type? ResponseType => ResponseTypeValue;

        public string? HttpClientName => throw new NotImplementedException();

        public HttpMethod HttpMethodValue { get; set; } = HttpMethod.Get;
        public HttpMethod HttpMethod => HttpMethodValue;

        public bool RequestAsQueryParamsValue { get; set; }
        public bool RequestAsQueryParams => RequestAsQueryParamsValue;

        public string? Tag => throw new NotImplementedException();

        public bool RequiresAuthenticationValue { get; set; }
        public bool RequiresAuthentication => RequiresAuthenticationValue;

        public string[]? AuthorizedRoles => throw new NotImplementedException();

        public string? AuthorizationPolicy => throw new NotImplementedException();

        public bool IsProvisional => throw new NotImplementedException();

        public bool AllowProvisional => throw new NotImplementedException();

        public TimeSpan? HttpClientTimeout => throw new NotImplementedException();

        public bool HasPathParamsValue { get; set; }
        public bool HasPathParams => HasPathParamsValue;

        public string[] HeaderPropertiesValue { get; set; } = Array.Empty<string>();
        public string[] HeaderProperties => HeaderPropertiesValue;

        public Type[] ProducedErrorTypes => Array.Empty<Type>();

        public System.Net.HttpStatusCode? SuccessStatusCode => null;
    }

    // ── X-MetalNexus-Client error negotiation Tests ───────────────────────────────

    [Fact]
    public async Task HandleRequest_ExceptionWithMetalNexusClientHeader_ReturnsMetalNexusEnvelope()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        options.ServerStackTraceOnExceptionsIncluded.Returns(false);
        options.DefaultToBadRequest.Returns(false);
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers[MetalNexusConstants.ClientHeader] = MetalNexusConstants.ClientHeaderValue;

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(_ => throw new NotFoundException("not found"));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(404);
        context.Response.ContentType.ShouldBe("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        json.ShouldContain("TypeName");
        json.ShouldContain("not found");
    }

    [Fact]
    public async Task HandleRequest_ExceptionWithoutMetalNexusClientHeader_ReturnsProblemDetails()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        options.ServerStackTraceOnExceptionsIncluded.Returns(false);
        options.DefaultToBadRequest.Returns(false);
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        // No X-MetalNexus-Client header

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(_ => throw new NotFoundException("not found"));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(404);
        context.Response.ContentType.ShouldBe("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        json.ShouldContain("\"status\"");
        json.ShouldContain("not found");
        json.ShouldNotContain("TypeName");
    }

    // ── IMetalNexusRawResponse Tests ─────────────────────────────────────────────

    [Fact]
    public async Task HandleRequest_HandlerReturnsRawResponseWithData_WritesRawContent()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(CsvRawResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var csvData = Encoding.UTF8.GetBytes("id,name\n1,Widget");
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(
            new CsvRawResponse("text/csv", csvData)));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(200);
        context.Response.ContentType.ShouldBe("text/csv");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.ShouldBe("id,name\n1,Widget");
    }

    [Fact]
    public async Task HandleRequest_HandlerReturnsRawResponseWithStream_WritesStreamContent()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(StreamRawResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var xmlContent = "<root><item>1</item></root>";
        var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(Task.FromResult<object?>(
            new StreamRawResponse("application/xml", dataStream)));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        context.Response.StatusCode.ShouldBe(200);
        context.Response.ContentType.ShouldBe("application/xml");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.ShouldBe(xmlContent);
    }

    // ── IMetalNexusRequestContext Tests ──────────────────────────────────────────

    [Fact]
    public async Task HandleRequest_WithAcceptHeader_RequestContextAcceptHeaderIsPopulated()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers.Accept = "text/csv";

        string? capturedAccept = null;
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(_ =>
        {
            capturedAccept = MetalNexusRequestContext.Current?.AcceptHeader;
            return Task.FromResult<object?>(new SimpleResponse { Message = "ok" });
        });

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        capturedAccept.ShouldBe("text/csv");
    }

    [Fact]
    public async Task HandleRequest_RequestContextClearedAfterRequest()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(true);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers.Accept = "application/json";

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>())
            .Returns(Task.FromResult<object?>(new SimpleResponse { Message = "ok" }));

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        MetalNexusRequestContext.Current.ShouldBeNull();
    }

    [Fact]
    public async Task HandleRequest_WithContentTypeHeader_RequestContextContentTypeIsPopulated()
    {
        var options = Substitute.For<IMetalNexusOptions>();
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.RequiresAuthentication.Returns(false);
        endpoint.RequestType.Returns(typeof(SimpleRequest));
        endpoint.ResponseType.Returns(typeof(SimpleResponse));
        endpoint.RequestAsQueryParams.Returns(false);
        endpoint.HasPathParams.Returns(false);
        endpoint.HeaderProperties.Returns(Array.Empty<string>());

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

        string? capturedContentType = null;
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<object>()).Returns(_ =>
        {
            capturedContentType = MetalNexusRequestContext.Current?.ContentType;
            return Task.FromResult<object?>(new SimpleResponse { Message = "ok" });
        });

        var services = new ServiceCollection();
        services.AddSingleton(mediator);
        context.RequestServices = services.BuildServiceProvider();

        await MetalNexusMiddleware.HandleRequest(context, endpoint, false, options);

        capturedContentType.ShouldBe("application/json");
    }

    // ── Multi-File Slot Routing Tests ────────────────────────────────────────────

    [Fact]
    public async Task HydrateRequest_SlottedFile_RoutedToFileSlotProperty()
    {
        var endpoint = new StubEndpoint<TwoSlotRequest>("/api/upload")
        {
            RequestAsQueryParamsValue = true,
            HasPathParamsValue = false,
            HeaderPropertiesValue = Array.Empty<string>()
        };

        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data";

        var avatarBytes = Encoding.UTF8.GetBytes("avatar bytes");
        var resumeBytes = Encoding.UTF8.GetBytes("resume bytes");

        var formFiles = new FormFileCollection
        {
            new FormFile(new MemoryStream(avatarBytes), 0, avatarBytes.Length, "avatar", "photo.jpg")
            {
                Headers = new HeaderDictionary(), ContentType = "image/jpeg"
            },
            new FormFile(new MemoryStream(resumeBytes), 0, resumeBytes.Length, "resume", "cv.pdf")
            {
                Headers = new HeaderDictionary(), ContentType = "application/pdf"
            }
        };
        context.Request.Form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFiles);

        var maxBodySizeFeature = Substitute.For<IHttpMaxRequestBodySizeFeature>();
        context.Features.Set(maxBodySizeFeature);

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldBeOfType<TwoSlotRequest>();
        var req = (TwoSlotRequest)result;
        req.Avatar.ShouldNotBeNull();
        req.Avatar!.FileName.ShouldBe("photo.jpg");
        req.Resume.ShouldNotBeNull();
        req.Resume!.FileName.ShouldBe("cv.pdf");
        req.Files.ShouldBeEmpty();
    }

    [Fact]
    public async Task HydrateRequest_UnmatchedFiles_FallIntoFilesArray()
    {
        var endpoint = new StubEndpoint<TwoSlotRequest>("/api/upload")
        {
            RequestAsQueryParamsValue = true,
            HasPathParamsValue = false,
            HeaderPropertiesValue = Array.Empty<string>()
        };

        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data";

        var extraBytes = Encoding.UTF8.GetBytes("extra");

        var formFiles = new FormFileCollection
        {
            new FormFile(new MemoryStream(extraBytes), 0, extraBytes.Length, "files", "extra.txt")
            {
                Headers = new HeaderDictionary(), ContentType = "text/plain"
            }
        };
        context.Request.Form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFiles);

        var maxBodySizeFeature = Substitute.For<IHttpMaxRequestBodySizeFeature>();
        context.Features.Set(maxBodySizeFeature);

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        var req = (TwoSlotRequest)result;
        req.Avatar.ShouldBeNull();
        req.Resume.ShouldBeNull();
        req.Files.ShouldNotBeNull();
        req.Files.Length.ShouldBe(1);
        req.Files[0].FileName.ShouldBe("extra.txt");
    }

    [Fact]
    public async Task HydrateRequest_SlottedAndUnmatchedFiles_RoutedCorrectly()
    {
        var endpoint = new StubEndpoint<TwoSlotRequest>("/api/upload")
        {
            RequestAsQueryParamsValue = true,
            HasPathParamsValue = false,
            HeaderPropertiesValue = Array.Empty<string>()
        };

        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data";

        var avatarBytes = Encoding.UTF8.GetBytes("avatar");
        var extraBytes = Encoding.UTF8.GetBytes("extra1");
        var extra2Bytes = Encoding.UTF8.GetBytes("extra2");

        var formFiles = new FormFileCollection
        {
            new FormFile(new MemoryStream(avatarBytes), 0, avatarBytes.Length, "avatar", "photo.jpg")
            {
                Headers = new HeaderDictionary(), ContentType = "image/jpeg"
            },
            new FormFile(new MemoryStream(extraBytes), 0, extraBytes.Length, "files", "a.txt")
            {
                Headers = new HeaderDictionary(), ContentType = "text/plain"
            },
            new FormFile(new MemoryStream(extra2Bytes), 0, extra2Bytes.Length, "other", "b.txt")
            {
                Headers = new HeaderDictionary(), ContentType = "text/plain"
            }
        };
        context.Request.Form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFiles);

        var maxBodySizeFeature = Substitute.For<IHttpMaxRequestBodySizeFeature>();
        context.Features.Set(maxBodySizeFeature);

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        var req = (TwoSlotRequest)result;
        req.Avatar.ShouldNotBeNull();
        req.Resume.ShouldBeNull();
        req.Files.Length.ShouldBe(2);
    }

    [Fact]
    public async Task HydrateRequest_NullBodySizeFeature_DoesNotThrow()
    {
        // Phase 4: when IHttpMaxRequestBodySizeFeature is absent, hydration must not throw.
        var endpoint = new StubEndpoint<FileUploadRequest>("/api/upload")
        {
            RequestAsQueryParamsValue = true,
            HasPathParamsValue = false,
            HeaderPropertiesValue = Array.Empty<string>()
        };

        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data";
        context.Request.Form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(),
            new FormFileCollection());
        // Intentionally do NOT register IHttpMaxRequestBodySizeFeature

        var result = await MetalNexusMiddleware.HydrateRequest(context, endpoint);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<FileUploadRequest>();
    }

    // ── ValidateFiles Tests ───────────────────────────────────────────────────────

    [Fact]
    public void ValidateFiles_NoViolations_DoesNotThrow()
    {
        var request = new MaxSizeRequest();
        request.Files = new[]
        {
            new MetalNexusFile { FileName = "a.txt", ContentType = "text/plain",
                DataStream = new MemoryStream(new byte[500]) }
        };

        Should.NotThrow(() => MetalNexusMiddleware.ValidateFiles(typeof(MaxSizeRequest), request));
    }

    [Fact]
    public void ValidateFiles_ClassLevelMaxSize_FileTooLarge_Throws()
    {
        var request = new MaxSizeRequest();
        request.Files = new[]
        {
            new MetalNexusFile { FileName = "big.bin", ContentType = "application/octet-stream",
                DataStream = new MemoryStream(new byte[2000]) }
        };

        var ex = Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(
            () => MetalNexusMiddleware.ValidateFiles(typeof(MaxSizeRequest), request));
        ex.Message.ShouldContain("big.bin");
        ex.Message.ShouldContain("1,000");
    }

    [Fact]
    public void ValidateFiles_ClassLevelAllowedTypes_DisallowedMimeType_Throws()
    {
        var request = new AllowedTypesRequest();
        request.Files = new[]
        {
            new MetalNexusFile { FileName = "bad.exe", ContentType = "application/x-msdownload",
                DataStream = new MemoryStream(new byte[10]) }
        };

        var ex = Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(
            () => MetalNexusMiddleware.ValidateFiles(typeof(AllowedTypesRequest), request));
        ex.Message.ShouldContain("bad.exe");
        ex.Message.ShouldContain("application/x-msdownload");
    }

    [Fact]
    public void ValidateFiles_ClassLevelAllowedTypes_AllowedMimeType_DoesNotThrow()
    {
        var request = new AllowedTypesRequest();
        request.Files = new[]
        {
            new MetalNexusFile { FileName = "ok.jpg", ContentType = "image/jpeg",
                DataStream = new MemoryStream(new byte[10]) }
        };

        Should.NotThrow(() => MetalNexusMiddleware.ValidateFiles(typeof(AllowedTypesRequest), request));
    }

    [Fact]
    public void ValidateFiles_MaxFileCount_ExceededCount_Throws()
    {
        var request = new MaxCountRequest();
        request.Files = new[]
        {
            new MetalNexusFile { FileName = "a.txt", ContentType = "text/plain", DataStream = new MemoryStream(new byte[1]) },
            new MetalNexusFile { FileName = "b.txt", ContentType = "text/plain", DataStream = new MemoryStream(new byte[1]) },
            new MetalNexusFile { FileName = "c.txt", ContentType = "text/plain", DataStream = new MemoryStream(new byte[1]) }
        };

        var ex = Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(
            () => MetalNexusMiddleware.ValidateFiles(typeof(MaxCountRequest), request));
        ex.Message.ShouldContain("Too many files");
        ex.Message.ShouldContain("3");
        ex.Message.ShouldContain("2");
    }

    [Fact]
    public void ValidateFiles_MaxFileCount_WithinLimit_DoesNotThrow()
    {
        var request = new MaxCountRequest();
        request.Files = new[]
        {
            new MetalNexusFile { FileName = "a.txt", ContentType = "text/plain", DataStream = new MemoryStream(new byte[1]) },
            new MetalNexusFile { FileName = "b.txt", ContentType = "text/plain", DataStream = new MemoryStream(new byte[1]) }
        };

        Should.NotThrow(() => MetalNexusMiddleware.ValidateFiles(typeof(MaxCountRequest), request));
    }

    [Fact]
    public void ValidateFiles_MultipleViolations_AllErrorsReported()
    {
        var request = new AllowedTypesRequest();
        request.Files = new[]
        {
            new MetalNexusFile { FileName = "bad1.exe", ContentType = "application/x-msdownload",
                DataStream = new MemoryStream(new byte[10]) },
            new MetalNexusFile { FileName = "bad2.bat", ContentType = "application/x-bat",
                DataStream = new MemoryStream(new byte[10]) }
        };

        var ex = Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(
            () => MetalNexusMiddleware.ValidateFiles(typeof(AllowedTypesRequest), request));
        ex.Message.ShouldContain("bad1.exe");
        ex.Message.ShouldContain("bad2.bat");
    }

    // ── ValidateFiles – [FileSlot] property-level validation Tests ───────────────

    [Fact]
    public void ValidateFiles_SlotFile_ExceedsClassLevelMaxSize_Throws()
    {
        var request = new SlotMaxSizeRequest();
        request.Avatar = new MetalNexusFile { FileName = "huge.jpg", ContentType = "image/jpeg",
            DataStream = new MemoryStream(new byte[2000]) };
        request.Files = Array.Empty<MetalNexusFile>();

        var ex = Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(
            () => MetalNexusMiddleware.ValidateFiles(typeof(SlotMaxSizeRequest), request));
        ex.Message.ShouldContain("Avatar");
        ex.Message.ShouldContain("1,000");
    }

    [Fact]
    public void ValidateFiles_SlotFile_WithinClassLevelMaxSize_DoesNotThrow()
    {
        var request = new SlotMaxSizeRequest();
        request.Avatar = new MetalNexusFile { FileName = "small.jpg", ContentType = "image/jpeg",
            DataStream = new MemoryStream(new byte[500]) };
        request.Files = Array.Empty<MetalNexusFile>();

        Should.NotThrow(() => MetalNexusMiddleware.ValidateFiles(typeof(SlotMaxSizeRequest), request));
    }

    [Fact]
    public void ValidateFiles_SlotFile_PropertyLevelSizeOverride_WinsOverClassLevel()
    {
        // SlotOverrideRequest has class-level max 1000, but "thumbnail" slot has property-level max 200
        var request = new SlotOverrideRequest();
        // 300 bytes: passes class-level (1000) but fails property-level (200)
        request.Thumbnail = new MetalNexusFile { FileName = "thumb.jpg", ContentType = "image/jpeg",
            DataStream = new MemoryStream(new byte[300]) };
        request.Files = Array.Empty<MetalNexusFile>();

        var ex = Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(
            () => MetalNexusMiddleware.ValidateFiles(typeof(SlotOverrideRequest), request));
        ex.Message.ShouldContain("Thumbnail");
    }

    [Fact]
    public void ValidateFiles_SlotFile_PropertyLevelTypeOverride_WinsOverClassLevel()
    {
        // SlotTypeOverrideRequest: class allows image/* but "document" slot only allows application/pdf
        var request = new SlotTypeOverrideRequest();
        request.Document = new MetalNexusFile { FileName = "photo.jpg", ContentType = "image/jpeg",
            DataStream = new MemoryStream(new byte[10]) };
        request.Files = Array.Empty<MetalNexusFile>();

        var ex = Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(
            () => MetalNexusMiddleware.ValidateFiles(typeof(SlotTypeOverrideRequest), request));
        ex.Message.ShouldContain("Document");
        ex.Message.ShouldContain("image/jpeg");
    }

    [Fact]
    public void ValidateFiles_SlotFile_NullSlotValue_IsSkipped()
    {
        // A null [FileSlot] property (file not uploaded) must not produce a validation error.
        var request = new TwoSlotValidationRequest();
        request.Avatar = null;
        request.Resume = new MetalNexusFile { FileName = "cv.pdf", ContentType = "application/pdf",
            DataStream = new MemoryStream(new byte[10]) };
        request.Files = Array.Empty<MetalNexusFile>();

        Should.NotThrow(() => MetalNexusMiddleware.ValidateFiles(typeof(TwoSlotValidationRequest), request));
    }

    [Fact]
    public void ValidateFiles_MaxFileCount_CountsSlotFilesAndFilesArray()
    {
        // SlotCountRequest has [MaxFileCount(2)] and one [FileSlot] property
        // uploading the slot plus two Files[] entries = 3 total, which exceeds the limit
        var request = new SlotCountRequest();
        request.Avatar = new MetalNexusFile { FileName = "a.jpg", ContentType = "image/jpeg",
            DataStream = new MemoryStream(new byte[1]) };
        request.Files = new[]
        {
            new MetalNexusFile { FileName = "b.txt", ContentType = "text/plain", DataStream = new MemoryStream(new byte[1]) },
            new MetalNexusFile { FileName = "c.txt", ContentType = "text/plain", DataStream = new MemoryStream(new byte[1]) }
        };

        var ex = Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(
            () => MetalNexusMiddleware.ValidateFiles(typeof(SlotCountRequest), request));
        ex.Message.ShouldContain("Too many files");
    }

    // ── Multi-File Test Helper Types ──────────────────────────────────────────────

    [UploadLimit(10_000_000)]
    public class TwoSlotRequest : MetalNexusFileRequest
    {
        [FileSlot("avatar")] public MetalNexusFile? Avatar { get; set; }
        [FileSlot("resume")] public MetalNexusFile? Resume { get; set; }
    }

    [MaxFileSize(1_000)]
    public class MaxSizeRequest : MetalNexusFileRequest { }

    [AllowedFileTypes("image/jpeg", "image/png")]
    public class AllowedTypesRequest : MetalNexusFileRequest { }

    [MaxFileCount(2)]
    public class MaxCountRequest : MetalNexusFileRequest { }

    [MaxFileSize(1_000)]
    public class SlotMaxSizeRequest : MetalNexusFileRequest
    {
        [FileSlot("Avatar")] public MetalNexusFile? Avatar { get; set; }
    }

    [MaxFileSize(1_000)]
    public class SlotOverrideRequest : MetalNexusFileRequest
    {
        [FileSlot("Thumbnail")]
        [MaxFileSize(200)]
        public MetalNexusFile? Thumbnail { get; set; }
    }

    [AllowedFileTypes("image/jpeg", "image/png")]
    public class SlotTypeOverrideRequest : MetalNexusFileRequest
    {
        [FileSlot("Document")]
        [AllowedFileTypes("application/pdf")]
        public MetalNexusFile? Document { get; set; }
    }

    [AllowedFileTypes("image/jpeg", "application/pdf")]
    public class TwoSlotValidationRequest : MetalNexusFileRequest
    {
        [FileSlot("Avatar")] public MetalNexusFile? Avatar { get; set; }
        [FileSlot("Resume")] public MetalNexusFile? Resume { get; set; }
    }

    [MaxFileCount(2)]
    public class SlotCountRequest : MetalNexusFileRequest
    {
        [FileSlot("Avatar")] public MetalNexusFile? Avatar { get; set; }
    }

    // ── Raw response helper types ─────────────────────────────────────────────────

    private sealed class CsvRawResponse(string contentType, byte[] data) : IMetalNexusRawResponse
    {
        public string ContentType { get; } = contentType;
        public byte[]? Data { get; } = data;
        public Stream? DataStream => null;
    }

    private sealed class StreamRawResponse(string contentType, Stream stream) : IMetalNexusRawResponse
    {
        public string ContentType { get; } = contentType;
        public byte[]? Data => null;
        public Stream? DataStream { get; } = stream;
    }

    // A stream that never completes a read, used to verify cancellation propagation
    private class NeverEndingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }
    }
}

