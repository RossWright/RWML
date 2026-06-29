using RossWright.MetalNexus.Internal;
using Shouldly;
using NSubstitute;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Tests;

public class MetalNexusRegistryTests
{
    [Fact] public void AddEndpoints_ToClient()
    {
        List<Type> addedToChain = new();
        MetalNexusRegistry registry = new(addedToChain.Add, new EndpointSchemaOptions(), null, _isServer: false, null);

        registry.AddEndpoints(
            typeof(EmptyQuery.Request),
            typeof(EmptyQuery.Response),
            typeof(EmptyQuery.Handler),
            typeof(EmptyCommand.Request),
            typeof(EmptyCommand.Handler));

        registry.AddEndpoints(typeof(EmptyCommand.Request)); // send this one again

        Assert.Collection(addedToChain,
            _ => _.ShouldBe(typeof(ApiRequestHandlerWithResponse<EmptyQuery.Request, EmptyQuery.Response>)),
            _ => _.ShouldBe(typeof(ApiRequestHandler<EmptyCommand.Request>)),
            _ => _.ShouldBe(typeof(ApiRequestHandler<EmptyCommand.Request>)));

        Assert.Collection(registry.Endpoints.OrderBy(_ => _.Path),
            _ =>
            {
                _.RequestType.ShouldBe(typeof(EmptyCommand.Request));
                _.ResponseType.ShouldBeNull();
            },
            _ =>
            {
                _.RequestType.ShouldBe(typeof(EmptyQuery.Request));
                _.ResponseType.ShouldBe(typeof(EmptyQuery.Response));
            });

        registry.FindEndpoint(typeof(EmptyCommand.Request)).ShouldNotBeNull();
        registry.FindEndpoint(typeof(EmptyCommand.Handler)).ShouldBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Request)).ShouldNotBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Response)).ShouldBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Handler)).ShouldBeNull();
    }

    [Fact] public void AddEndpoints_ToServer()
    {
        List<Type> addedToChain = new();
        MetalNexusRegistry registry = new(addedToChain.Add, new EndpointSchemaOptions(), null, _isServer: true, null);

        registry.AddEndpoints(
            typeof(EmptyQuery.Request),
            typeof(EmptyQuery.Response),
            typeof(EmptyQuery.Handler),
            typeof(EmptyCommand.Request),
            typeof(EmptyCommand.Handler));

        registry.AddEndpoints(typeof(EmptyCommand.Handler)); // send this one again

        Assert.Collection(addedToChain,
            _ => _.ShouldBe(typeof(EmptyQuery.Handler)),
            _ => _.ShouldBe(typeof(EmptyCommand.Handler)),
            _ => _.ShouldBe(typeof(EmptyCommand.Handler)));

        Assert.Collection(registry.Endpoints.OrderBy(_ => _.Path),
            _ =>
            {
                _.RequestType.ShouldBe(typeof(EmptyCommand.Request));
                _.ResponseType.ShouldBeNull();
            },
            _ =>
            {
                _.RequestType.ShouldBe(typeof(EmptyQuery.Request));
                _.ResponseType.ShouldBe(typeof(EmptyQuery.Response));
            });

        registry.FindEndpoint(typeof(EmptyCommand.Request)).ShouldNotBeNull();
        registry.FindEndpoint(typeof(EmptyCommand.Handler)).ShouldBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Request)).ShouldNotBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Response)).ShouldBeNull();
        registry.FindEndpoint(typeof(EmptyQuery.Handler)).ShouldBeNull();
    }

    [Fact] public void DefineEndpoint_Defaults()
    {
        MetalNexusRegistry registry = new(_ => { }, new EndpointSchemaOptions(), null, _isServer: false, null);
        var ep = registry.DefineEndpoint(typeof(EmptyCommand.Request));
        ep.ShouldNotBeNull();
        ep.RequestType.ShouldBe(typeof(EmptyCommand.Request));
        ep.ResponseType.ShouldBeNull();
        ep.HttpClientName.ShouldBeNull();
        ep.HttpMethod.ShouldBe(HttpMethod.Get);
        ep.Path.ShouldBe("/command");
        ep.RequestAsQueryParams.ShouldBeTrue();
        ep.HasPathParams.ShouldBeFalse();
        ep.Tag.ShouldBe("Command");
        ep.RequiresAuthentication.ShouldBeTrue();
        ep.AuthorizedRoles.ShouldBeNull();
        ep.AllowProvisional.ShouldBeFalse();
        ep.HttpClientTimeout.ShouldBeNull();
        ep.HeaderProperties.ShouldBeEmpty();


        Should.Throw<MetalNexusException>(() => registry.DefineEndpoint(typeof(BadRequestMissingIRequest)));
    }


    [Fact] public void IsValidBracketUrlParsing()
    {
        MetalNexusRegistry.IsValidBracketUrlParsing("{First}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/second", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/second/third", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/{Second}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/second", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/{Second}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/second/third", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/{Second}/third", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/second/{Third}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/{Second}/third", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/{Second}/{Third}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/second/{Third}", typeof(Test)).ShouldBeTrue();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First}/{Second}/{Third}", typeof(Test)).ShouldBeTrue();

        MetalNexusRegistry.IsValidBracketUrlParsing("first{Second}", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("{First}second", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("}{", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/first/second}", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{First/second", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{Obj}", typeof(Test)).ShouldBeFalse();
        MetalNexusRegistry.IsValidBracketUrlParsing("/{{Obj}", typeof(Test)).ShouldBeFalse();
    }

    public class Test
    {
        public string? First { get; set; }
        public string? Second { get; set; }
        public string? Third { get; set; }
        public object? Obj { get; set; }
    }

    [Fact]
    public void DeterminePath_WithPathInAttribute_UsesProvidedPath()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(path: "/custom/path");

        var (path, hasPathParams) = registry.DeterminePath(typeof(EmptyCommand.Request), attribute);

        path.ShouldBe("/custom/path");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DeterminePath_WithNullPath_GeneratesFromRequestType()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();

        var (path, hasPathParams) = registry.DeterminePath(typeof(TestRequestForPath), attribute);

        path.ShouldBe("/api/testrequestforpath");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DeterminePath_WithNullPath_TrimsRequestSuffix()
    {
        var options = new EndpointSchemaOptions();
        var registry = new MetalNexusRegistry(_ => { }, options, null, false, null);
        var attribute = new ApiRequestAttribute();

        var (path, hasPathParams) = registry.DeterminePath(typeof(GetUserRequest), attribute);

        path.ShouldBe("/api/getuser");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DeterminePath_WithNullPath_TrimsCommandSuffix()
    {
        var options = new EndpointSchemaOptions();
        var registry = new MetalNexusRegistry(_ => { }, options, null, false, null);
        var attribute = new ApiRequestAttribute();

        var (path, hasPathParams) = registry.DeterminePath(typeof(CreateUserCommand), attribute);

        path.ShouldBe("/api/createuser");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DeterminePath_WithNullPath_TrimsQuerySuffix()
    {
        var options = new EndpointSchemaOptions();
        var registry = new MetalNexusRegistry(_ => { }, options, null, false, null);
        var attribute = new ApiRequestAttribute();

        var (path, hasPathParams) = registry.DeterminePath(typeof(ListUsersQuery), attribute);

        path.ShouldBe("/api/listusers");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DeterminePath_WithNullPathAndPathStrategy_UsesPathStrategy()
    {
        var pathStrategy = Substitute.For<RossWright.MetalNexus.Schema.IPathStrategy>();
        pathStrategy.Trim(typeof(TestRequestForPath)).Returns("strategypath");
        var options = new EndpointSchemaOptions { PathStrategy = pathStrategy };
        var registry = new MetalNexusRegistry(_ => { }, options, null, false, null);
        var attribute = new ApiRequestAttribute();

        var (path, hasPathParams) = registry.DeterminePath(typeof(TestRequestForPath), attribute);

        path.ShouldBe("/api/strategypath/testrequestforpath");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DeterminePath_WithNullPathAndApiPathToLowerFalse_DoesNotLowerCase()
    {
        var options = new EndpointSchemaOptions { ApiPathToLower = false };
        var registry = new MetalNexusRegistry(_ => { }, options, null, false, null);
        var attribute = new ApiRequestAttribute();

        var (path, hasPathParams) = registry.DeterminePath(typeof(TestRequestForPath), attribute);

        path.ShouldBe("/api/TestRequestForPath");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DeterminePath_WithCustomEndpointSchema_CallsCustomSchema()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DeterminePath(typeof(TestRequestForPath), Arg.Any<string>()).Returns("/custom/modified");
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute(path: "/original");

        var (path, hasPathParams) = registry.DeterminePath(typeof(TestRequestForPath), attribute);

        path.ShouldBe("/custom/modified");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DeterminePath_WithCustomEndpointSchemaThrowsException_ThrowsMetalNexusException()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DeterminePath(typeof(TestRequestForPath), Arg.Any<string>()).Returns(_ => throw new Exception("Custom error"));
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute(path: "/original");

        Should.Throw<MetalNexusException>(() => registry.DeterminePath(typeof(TestRequestForPath), attribute))
            .Message.ShouldContain("Failed to determine path");
    }

    [Fact]
    public void DeterminePath_WithPathParams_ReturnsHasPathParamsTrue()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(path: "/users/{Id}");

        var (path, hasPathParams) = registry.DeterminePath(typeof(RequestForPathParamTest), attribute);

        path.ShouldBe("/users/{Id}");
        hasPathParams.ShouldBeTrue();
    }

    [Fact]
    public void DeterminePath_WithInvalidBrackets_ThrowsMetalNexusException()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(path: "/users/{invalid");

        Should.Throw<MetalNexusException>(() => registry.DeterminePath(typeof(RequestForPathParamTest), attribute))
            .Message.ShouldContain("Path has invalid brackets");
    }

    [Fact]
    public void DeterminePath_WithBackslash_ReplacesWithForwardSlash()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(path: "users\\profile");

        var (path, hasPathParams) = registry.DeterminePath(typeof(RequestForPathParamTest), attribute);

        path.ShouldBe("/users/profile");
        hasPathParams.ShouldBeFalse();
    }

    [Fact]
    public void DetermineTag_WithTagInAttribute_UsesProvidedTag()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(tag: "CustomTag");

        var tag = registry.DetermineTag(typeof(TestRequestForPath), attribute, "/api/test");

        tag.ShouldBe("CustomTag");
    }

    [Fact]
    public void DetermineTag_WithNullTag_DerivesFromPath()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();

        var tag = registry.DetermineTag(typeof(TestRequestForPath), attribute, "/api/users/list");

        tag.ShouldBe("Users");
    }

    [Fact]
    public void DetermineTag_WithNullTagAndSimplePath_UsesSingleSegment()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();

        var tag = registry.DetermineTag(typeof(TestRequestForPath), attribute, "/users");

        tag.ShouldBe("Users");
    }

    [Fact]
    public void DetermineTag_WithNullTagAndPathStartingWithPrefix_TrimsPrefix()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();

        var tag = registry.DetermineTag(typeof(TestRequestForPath), attribute, "/api/products/create");

        tag.ShouldBe("Products");
    }

    [Fact]
    public void DetermineTag_WithCustomEndpointSchema_CallsCustomSchema()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DetermineTag(typeof(TestRequestForPath), Arg.Any<string>()).Returns("ModifiedTag");
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute(tag: "OriginalTag");

        var tag = registry.DetermineTag(typeof(TestRequestForPath), attribute, "/api/test");

        tag.ShouldBe("ModifiedTag");
    }

    [Fact]
    public void DetermineTag_WithCustomEndpointSchemaThrowsException_ThrowsMetalNexusException()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DetermineTag(typeof(TestRequestForPath), Arg.Any<string>()).Returns(_ => throw new Exception("Custom error"));
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute(tag: "OriginalTag");

        Should.Throw<MetalNexusException>(() => registry.DetermineTag(typeof(TestRequestForPath), attribute, "/api/test"))
            .Message.ShouldContain("Failed to determine tag");
    }

    [Fact]
    public void DetermineHttpProtocol_WithAutoAndSimpleRequest_ReturnsGet()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.Auto);

        var (method, usesQueryParams) = registry.DetermineHttpProtocol(typeof(SimpleRequest), attribute);

        method.ShouldBe(HttpMethod.Get);
        usesQueryParams.ShouldBeTrue();
    }

    [Fact]
    public void DetermineHttpProtocol_WithAutoAndComplexRequest_ReturnsPostViaBody()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.Auto);

        var (method, usesQueryParams) = registry.DetermineHttpProtocol(typeof(HasComplexPropertyRequest), attribute);

        method.ShouldBe(HttpMethod.Post);
        usesQueryParams.ShouldBeFalse();
    }

    [Fact]
    public void DetermineHttpProtocol_WithAutoAndFileRequest_ReturnsPostViaBody()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.Auto);

        var (method, usesQueryParams) = registry.DetermineHttpProtocol(typeof(FileRequest), attribute);

        method.ShouldBe(HttpMethod.Post);
        usesQueryParams.ShouldBeFalse();
    }

    [Fact]
    public void DetermineHttpProtocol_WithAutoAndTooManyParameters_ReturnsPostViaBody()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions { MaximumRequestParameters = 4 }, null, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.Auto);

        var (method, usesQueryParams) = registry.DetermineHttpProtocol(typeof(RequestWith5Properties), attribute);

        method.ShouldBe(HttpMethod.Post);
        usesQueryParams.ShouldBeFalse();
    }

    [Fact]
    public void DetermineHttpProtocol_WithAutoAndDefaultHttpProtocol_UsesDefault()
    {
        var options = new EndpointSchemaOptions { DefaultHttpProtocol = HttpProtocol.PostViaBody };
        var registry = new MetalNexusRegistry(_ => { }, options, null, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.Auto);

        var (method, usesQueryParams) = registry.DetermineHttpProtocol(typeof(RequestWith4Properties), attribute);

        method.ShouldBe(HttpMethod.Post);
        usesQueryParams.ShouldBeFalse();
    }

    [Fact]
    public void DetermineHttpProtocol_WithCustomEndpointSchema_CallsCustomSchema()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DetermineHttpProtocol(typeof(SimpleRequest), Arg.Any<HttpProtocol>()).Returns(HttpProtocol.PostViaBody);
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.Get);

        var (method, usesQueryParams) = registry.DetermineHttpProtocol(typeof(SimpleRequest), attribute);

        method.ShouldBe(HttpMethod.Post);
        usesQueryParams.ShouldBeFalse();
    }

    [Fact]
    public void DetermineHttpProtocol_WithCustomEndpointSchemaThrowsException_ThrowsMetalNexusException()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DetermineHttpProtocol(typeof(SimpleRequest), Arg.Any<HttpProtocol>()).Returns(_ => throw new Exception("Custom error"));
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.Get);

        Should.Throw<MetalNexusException>(() => registry.DetermineHttpProtocol(typeof(SimpleRequest), attribute))
            .Message.ShouldContain("Failed to determine HttpMethod");
    }

    [Fact]
    public void DetermineHttpProtocol_WithComplexRequestAndQueryParams_ThrowsException()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.Get);

        Should.Throw<MetalNexusException>(() => registry.DetermineHttpProtocol(typeof(HasComplexPropertyRequest), attribute))
            .Message.ShouldContain("cannot use query params");
    }

    [Fact]
    public void DetermineHttpProtocol_WithExplicitProtocol_UsesSpecified()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute(HttpProtocol.PutViaBody);

        var (method, usesQueryParams) = registry.DetermineHttpProtocol(typeof(SimpleRequest), attribute);

        method.ShouldBe(HttpMethod.Put);
        usesQueryParams.ShouldBeFalse();
    }

    [Fact]
    public void DetermineAuthentication_WithDefaultSettings_ReturnsRequiresAuth()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();

        var (requiresAuth, roles) = registry.DetermineAuthentication(typeof(TestRequestForPath), attribute, null);

        requiresAuth.ShouldBeTrue();
        roles.ShouldBeNull();
    }

    [Fact]
    public void DetermineAuthentication_WithAuthenticatedAttribute_ReturnsRequiresAuth()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();
        var authAttribute = new AuthenticatedAttribute();

        var (requiresAuth, roles) = registry.DetermineAuthentication(typeof(TestRequestForPath), attribute, authAttribute);

        requiresAuth.ShouldBeTrue();
        roles.ShouldBeNull();
    }

    [Fact]
    public void DetermineAuthentication_WithAuthenticatedAttributeAndRoles_ReturnsRoles()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();
        var authAttribute = new AuthenticatedAttribute("Admin", "User");

        var (requiresAuth, roles) = registry.DetermineAuthentication(typeof(TestRequestForPath), attribute, authAttribute);

        requiresAuth.ShouldBeTrue();
        roles.ShouldNotBeNull();
        roles.ShouldBe(new[] { "Admin", "User" });
    }

    [Fact]
    public void DetermineAuthentication_WithAnonymousAttribute_ReturnsNoAuth()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();

        var (requiresAuth, roles) = registry.DetermineAuthentication(typeof(AnonymousTestRequest), attribute, null);

        requiresAuth.ShouldBeFalse();
        roles.ShouldBeNull();
    }

    [Fact]
    public void DetermineAuthentication_WithBothAuthenticatedAndAnonymous_ThrowsException()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        var attribute = new ApiRequestAttribute();
        var authAttribute = new AuthenticatedAttribute();

        Should.Throw<MetalNexusException>(() => registry.DetermineAuthentication(typeof(BothAuthAndAnonRequest), attribute, authAttribute))
            .Message.ShouldContain("cannot have both Authenticated and Anonymous");
    }

    [Fact]
    public void DetermineAuthentication_WithCustomEndpointSchema_CallsCustomSchema()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DetermineRequiresAuthentication(typeof(TestRequestForPath), Arg.Any<bool>()).Returns(false);
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute();

        var (requiresAuth, roles) = registry.DetermineAuthentication(typeof(TestRequestForPath), attribute, null);

        requiresAuth.ShouldBeFalse();
        roles.ShouldBeNull();
    }

    [Fact]
    public void DetermineAuthentication_WithCustomEndpointSchemaAndAuth_DeterminesRoles()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DetermineRequiresAuthentication(typeof(TestRequestForPath), Arg.Any<bool>()).Returns(true);
        customSchema.DetermineAuthorizedRoles(typeof(TestRequestForPath), Arg.Any<string[]?>()).Returns(new[] { "CustomRole" });
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute();

        var (requiresAuth, roles) = registry.DetermineAuthentication(typeof(TestRequestForPath), attribute, null);

        requiresAuth.ShouldBeTrue();
        roles.ShouldBe(new[] { "CustomRole" });
    }

    [Fact]
    public void DetermineAuthentication_WithCustomEndpointSchemaThrowsException_ThrowsMetalNexusException()
    {
        var customSchema = Substitute.For<RossWright.MetalNexus.Schema.ICustomEndpointSchema>();
        customSchema.DetermineRequiresAuthentication(typeof(TestRequestForPath), Arg.Any<bool>()).Returns(_ => throw new Exception("Custom error"));
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), customSchema, false, null);
        var attribute = new ApiRequestAttribute();

        Should.Throw<MetalNexusException>(() => registry.DetermineAuthentication(typeof(TestRequestForPath), attribute, null))
            .Message.ShouldContain("Failed to determine authentication requirement");
    }

    [Fact]
    public void AddEndpoints_WhenNotSealed_Succeeds()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        registry.IsSealed.ShouldBeFalse();
        registry.AddEndpoints(typeof(EmptyQuery.Request));
        registry.Endpoints.ShouldContain(_ => _.RequestType == typeof(EmptyQuery.Request));
    }

    [Fact]
    public void Seal_SetsIsSealed()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        registry.Seal();
        registry.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void AddEndpoints_WhenSealed_ThrowsInvalidOperationException()
    {
        var registry = new MetalNexusRegistry(_ => { }, new EndpointSchemaOptions(), null, false, null);
        registry.Seal();
        Should.Throw<InvalidOperationException>(() => registry.AddEndpoints(typeof(EmptyQuery.Request)))
            .Message.ShouldContain("UseMetalNexusServer");
    }
}