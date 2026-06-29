using Microsoft.OpenApi;
using RossWright.MetalNexus.Schema;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RossWright.MetalNexus.Server.Tests.Internal;

public class MetalNexusApiDocumentFilterTests
{
    [Fact]
    public void Apply_CreatesTagsFromEndpoints_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint1 = CreateEndpoint("/api/test1", "Tag1");
        var endpoint2 = CreateEndpoint("/api/test2", "Tag2");
        var endpoint3 = CreateEndpoint("/api/test3", "Tag1"); // Duplicate tag
        registry.Endpoints.Returns(new[] { endpoint1, endpoint2, endpoint3 });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        document.Tags.ShouldNotBeNull();
        document.Tags.Count.ShouldBe(2);
        document.Tags.Any(t => t.Name == "Tag1").ShouldBeTrue();
        document.Tags.Any(t => t.Name == "Tag2").ShouldBeTrue();
    }

    [Fact]
    public void Apply_SkipsExistingPaths_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/existing");
        registry.Endpoints.Returns(new[] { endpoint });

        var existingPathItem = new OpenApiPathItem();
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/api/existing"] = existingPathItem
            }
        };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        document.Paths["/api/existing"].ShouldBe(existingPathItem);
    }

    [Fact]
    public void Apply_CreatesOperationWithBasicProperties_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", "TestTag", requestType: typeof(TestRequest));
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var pathItem = document.Paths["/api/test"];
        pathItem.ShouldNotBeNull();
        var operation = pathItem.Operations![HttpMethod.Get];
        operation.ShouldNotBeNull();
        operation.Summary.ShouldBe("TestRequest");
    }

    [Fact]
    public void Apply_SetsDeprecatedWhenObsolete_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
#pragma warning disable CS0612 // Type or member is obsolete
        var endpoint = CreateEndpoint("/api/obsolete", requestType: typeof(ObsoleteRequest));
#pragma warning restore CS0612 // Type or member is obsolete
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/obsolete"].Operations![HttpMethod.Get];
        operation.Deprecated.ShouldBeTrue();
    }

    [Fact]
    public void Apply_SetsTagsWhenProvided_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", "MyTag");
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Tags.ShouldNotBeNull();
        operation.Tags!.Count.ShouldBe(1);
        operation.Tags.First().Name.ShouldBe("MyTag");
    }

    [Fact]
    public void Apply_TagsIsNullWhenNoTag_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", tag: null);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Tags.ShouldBeNull();
    }

    [Fact]
    public void Apply_SetsSecurityWhenAuthenticationRequired_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/secure", requiresAuth: true);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/secure"].Operations![HttpMethod.Get];
        operation.Security.ShouldNotBeNull();
        operation.Security!.Count.ShouldBe(1);
    }

    [Fact]
    public void Apply_SecurityIsNullWhenNoAuthentication_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/public", requiresAuth: false);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/public"].Operations![HttpMethod.Get];
        operation.Security.ShouldBeNull();
    }

    [Fact]
    public void Apply_CreatesSuccessAndErrorResponses_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", responseType: typeof(TestResponse));
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Responses!["200"].ShouldNotBeNull();
        operation.Responses["200"]!.Description.ShouldBe("Success");
        operation.Responses["500"].ShouldNotBeNull();
        operation.Responses["500"]!.Description.ShouldBe("Internal Server Error");
        operation.Responses.ContainsKey("400").ShouldBeFalse();
    }

    [Fact]
    public void Apply_CreatesRequestBodyWhenNotQueryParams_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", requestAsQueryParams: false, requestType: typeof(TestRequest));
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.RequestBody.ShouldNotBeNull();
        operation.RequestBody!.Content.ShouldNotBeNull();
        operation.RequestBody.Content!.ContainsKey("application/json").ShouldBeTrue();
    }

    [Fact]
    public void Apply_CreatesParametersWhenQueryParams_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", requestAsQueryParams: true, requestType: typeof(TestRequest));
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Parameters.ShouldNotBeNull();
        operation.Parameters!.Count.ShouldBeGreaterThan(0);
        operation.Parameters.Any(p => p.Name == "Name").ShouldBeTrue();
        operation.Parameters.Any(p => p.Name == "Value").ShouldBeTrue();
    }

    [Fact]
    public void Apply_Adds401ResponseWhenAuthenticationRequired_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/secure", requiresAuth: true);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/secure"].Operations![HttpMethod.Get];
        operation.Responses!["401"].ShouldNotBeNull();
        operation.Responses["401"]!.Description.ShouldBe("Unauthorized");
    }

    [Fact]
    public void Apply_DoesNotAdd401ResponseWhenNoAuthentication_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/public", requiresAuth: false);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/public"].Operations![HttpMethod.Get];
        operation.Responses!.ContainsKey("401").ShouldBeFalse();
    }

    [Fact]
    public void Apply_Adds403ResponseWhenRolesSpecified_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/admin", authorizedRoles: new[] { "Admin" });
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/admin"].Operations![HttpMethod.Get];
        operation.Responses!["403"].ShouldNotBeNull();
        operation.Responses["403"]!.Description.ShouldBe("Forbidden");
    }

    [Fact]
    public void Apply_DoesNotAdd403ResponseWhenNoRoles_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/public", authorizedRoles: null);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/public"].Operations![HttpMethod.Get];
        operation.Responses!.ContainsKey("403").ShouldBeFalse();
    }

    [Fact]
    public void Apply_ProcessesMultipleEndpoints_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint1 = CreateEndpoint("/api/endpoint1");
        var endpoint2 = CreateEndpoint("/api/endpoint2");
        var endpoint3 = CreateEndpoint("/api/endpoint3");
        registry.Endpoints.Returns(new[] { endpoint1, endpoint2, endpoint3 });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        document.Paths.Count.ShouldBe(3);
        document.Paths.ContainsKey("/api/endpoint1").ShouldBeTrue();
        document.Paths.ContainsKey("/api/endpoint2").ShouldBeTrue();
        document.Paths.ContainsKey("/api/endpoint3").ShouldBeTrue();
    }

    [Fact]
    public void Apply_UsesCorrectHttpMethod_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", httpMethod: HttpMethod.Post);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var pathItem = document.Paths["/api/test"];
        pathItem.Operations!.ContainsKey(HttpMethod.Post).ShouldBeTrue();
        pathItem.Operations.ContainsKey(HttpMethod.Get).ShouldBeFalse();
    }

    [Fact]
    public void Apply_HandlesNullResponseType_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", responseType: null);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Responses!["200"].ShouldNotBeNull();
    }

    [Fact]
    public void Apply_FiltersMetalNexusFileRequestProperties_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/upload", requestAsQueryParams: true, requestType: typeof(FileUploadRequest));
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/upload"].Operations![HttpMethod.Get];
        operation.Parameters.ShouldNotBeNull();
        // Should only have FileName parameter, not Files from MetalNexusFileRequest base class
        operation.Parameters!.Any(p => p.Name == "FileName").ShouldBeTrue();
        operation.Parameters.Any(p => p.Name == "Files").ShouldBeFalse();
    }

    [Fact]
    public void Apply_SetsParameterLocationToQuery_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", requestAsQueryParams: true, requestType: typeof(TestRequest));
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Parameters.ShouldNotBeNull();
        operation.Parameters!.All(p => p.In == ParameterLocation.Query).ShouldBeTrue();
    }

    [Fact]
    public void Apply_CreatesDistinctTags_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint1 = CreateEndpoint("/api/1", "Users");
        var endpoint2 = CreateEndpoint("/api/2", "Users");
        var endpoint3 = CreateEndpoint("/api/3", "Users");
        registry.Endpoints.Returns(new[] { endpoint1, endpoint2, endpoint3 });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        document.Tags.ShouldNotBeNull();
        document.Tags.Count.ShouldBe(1);
        document.Tags.First().Name.ShouldBe("Users");
    }

    [Fact]
    public void Apply_AddsPathItemToDocument_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/newpath");
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var pathItem = document.Paths["/api/newpath"];
        pathItem.ShouldNotBeNull();
        pathItem.Operations!.ShouldNotBeNull();
    }

    [Fact]
    public void Apply_BothAuthenticationAndRoles_AddsBothResponses()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/restricted", requiresAuth: true, authorizedRoles: new[] { "Admin", "User" });
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/restricted"].Operations![HttpMethod.Get];
        operation.Responses!.ContainsKey("401").ShouldBeTrue();
        operation.Responses.ContainsKey("403").ShouldBeTrue();
    }

    // Helper methods
    private static IEndpoint CreateEndpoint(
        string path,
        string? tag = null,
        Type? requestType = null,
        Type? responseType = null,
        bool requestAsQueryParams = false,
        bool requiresAuth = false,
        string[]? authorizedRoles = null,
        string? authorizationPolicy = null,
        HttpMethod? httpMethod = null,
        Type[]? producedErrorTypes = null)
    {
        var endpoint = Substitute.For<IEndpoint>();
        endpoint.Path.Returns(path);
        endpoint.Tag.Returns(tag);
        endpoint.RequestType.Returns(requestType ?? typeof(TestRequest));
        endpoint.ResponseType.Returns(responseType ?? typeof(TestResponse));
        endpoint.RequestAsQueryParams.Returns(requestAsQueryParams);
        endpoint.RequiresAuthentication.Returns(requiresAuth);
        endpoint.AuthorizedRoles.Returns(authorizedRoles);
        endpoint.AuthorizationPolicy.Returns(authorizationPolicy);
        endpoint.HttpMethod.Returns(httpMethod ?? HttpMethod.Get);
        endpoint.ProducedErrorTypes.Returns(producedErrorTypes ?? []);
        return endpoint;
    }

    private static DocumentFilterContext CreateContext()
    {
        var schemaGenerator = Substitute.For<ISchemaGenerator>();
        var schemaRepository = new SchemaRepository();
        
        // Setup schema generator to return a simple schema
        schemaGenerator.GenerateSchema(Arg.Any<Type>(), Arg.Any<SchemaRepository>())
            .Returns(callInfo => new OpenApiSchema());

        var apiDescriptions = new List<Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription>();
        return new DocumentFilterContext(
            apiDescriptions,
            schemaGenerator,
            schemaRepository);
    }

    [Fact]
    public void Apply_AlwaysAdds500Response_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test");
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Responses!["500"].ShouldNotBeNull();
        operation.Responses["500"]!.Description.ShouldBe("Internal Server Error");
    }

    [Fact]
    public void Apply_DoesNotAdd400Response_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test");
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Responses!.ContainsKey("400").ShouldBeFalse();
    }

    [Fact]
    public void Apply_DoesNotAdd403WhenAuthenticatedButNoRolesOrPolicy_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/secure", requiresAuth: true, authorizedRoles: null, authorizationPolicy: null);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/secure"].Operations![HttpMethod.Get];
        operation.Responses!.ContainsKey("401").ShouldBeTrue();
        operation.Responses.ContainsKey("403").ShouldBeFalse();
    }

    [Fact]
    public void Apply_Adds403WhenAuthorizationPolicySet_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/policy", requiresAuth: true, authorizationPolicy: "RequireMfa");
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/policy"].Operations![HttpMethod.Get];
        operation.Responses!.ContainsKey("403").ShouldBeTrue();
        operation.Responses["403"]!.Description.ShouldBe("Forbidden");
    }

    [Fact]
    public void Apply_AddsResponseFromProducedErrorTypes_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", producedErrorTypes: [typeof(NotFoundException)]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Responses!.ContainsKey("404").ShouldBeTrue();
        operation.Responses["404"]!.Description.ShouldBe("NotFound");
    }

    [Fact]
    public void Apply_DeduplicatesProducedErrorTypesWithSameStatusCode_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        // Both map to 404
        var endpoint = CreateEndpoint("/api/test",
            producedErrorTypes: [typeof(NotFoundException), typeof(NotFoundException)]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert — only one 404
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Responses!.Keys.Count(_ => _ == "404").ShouldBe(1);
    }

    [Fact]
    public void Apply_ProducedErrorTypeDoesNotDuplicateAutomatic401_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        // Explicit [ProducesError<NotAuthenticatedException>] on an auth endpoint — should not double-emit 401
        var endpoint = CreateEndpoint("/api/secure", requiresAuth: true,
            producedErrorTypes: [typeof(NotAuthenticatedException)]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/secure"].Operations![HttpMethod.Get];
        operation.Responses!.Keys.Count(_ => _ == "401").ShouldBe(1);
    }

    [Fact]
    public void Apply_MultipleProducedErrorTypes_AddsAllStatusCodes_Success()
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test",
            producedErrorTypes: [typeof(NotFoundException), typeof(ValidationException)]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateContext();
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, context);

        // Assert
        var operation = document.Paths["/api/test"].Operations![HttpMethod.Get];
        operation.Responses!.ContainsKey("404").ShouldBeTrue();
        operation.Responses.ContainsKey("422").ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // [ProducesError] per recognized exception → expected status code
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(typeof(NotAuthenticatedException), "401")]
    [InlineData(typeof(NotAuthorizedException),    "403")]
    [InlineData(typeof(NotFoundException),          "404")]
    [InlineData(typeof(NotImplementedException),   "501")]
    [InlineData(typeof(ValidationException),        "422")]
    [InlineData(typeof(InternalServerErrorException), "500")]
    public void Apply_ProducesError_KnownExceptionType_EmitsCorrectStatusCode(
        Type exceptionType, string expectedCode)
    {
        // Arrange
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test", producedErrorTypes: [exceptionType]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var filter = new MetalNexusApiDocumentFilter(registry);

        // Act
        filter.Apply(document, CreateContext());

        // Assert
        var responses = document.Paths["/api/test"].Operations![HttpMethod.Get].Responses!;
        responses.ContainsKey(expectedCode).ShouldBeTrue(
            $"Expected response key '{expectedCode}' for exception type '{exceptionType.Name}'");
    }

    [Fact]
    public void Apply_ProducesError_CustomUnknownException_EmitsBadRequest()
    {
        // Custom exceptions that don't inherit any recognized MetalNexus type
        // map to 400 Bad Request — this is what [ProducesError<MyCustomException>]
        // will show in Swagger docs when the exception type is unrecognized.
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test",
            producedErrorTypes: [typeof(CustomBusinessException)]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var filter = new MetalNexusApiDocumentFilter(registry);

        filter.Apply(document, CreateContext());

        var responses = document.Paths["/api/test"].Operations![HttpMethod.Get].Responses!;
        responses.ContainsKey("400").ShouldBeTrue(
            "Custom exceptions that don't inherit a recognized type should map to 400 Bad Request");
    }

    [Fact]
    public void Apply_ProducesError_SubclassOfNotFoundException_EmitsNotFound()
    {
        // A subclass of a recognized exception type should inherit its status code
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test",
            producedErrorTypes: [typeof(DerivedNotFoundException)]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var filter = new MetalNexusApiDocumentFilter(registry);

        filter.Apply(document, CreateContext());

        var responses = document.Paths["/api/test"].Operations![HttpMethod.Get].Responses!;
        responses.ContainsKey("404").ShouldBeTrue(
            "Subclass of NotFoundException should emit 404");
    }

    [Fact]
    public void Apply_ProducesError_InternalServerError500NotDuplicated_Success()
    {
        // [ProducesError<InternalServerErrorException>] should not add a second 500
        // because 500 is already emitted automatically
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/test",
            producedErrorTypes: [typeof(InternalServerErrorException)]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var filter = new MetalNexusApiDocumentFilter(registry);

        filter.Apply(document, CreateContext());

        var responses = document.Paths["/api/test"].Operations![HttpMethod.Get].Responses!;
        responses.Keys.Count(k => k == "500").ShouldBe(1,
            "500 should appear exactly once even when [ProducesError<InternalServerErrorException>] is set");
    }

    // -----------------------------------------------------------------------
    // Authentication / authorization response rules
    // -----------------------------------------------------------------------

    [Fact]
    public void Apply_AuthenticatedEndpoint_Has401But_Not403WithoutRolesOrPolicy()
    {
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/auth", requiresAuth: true,
            authorizedRoles: null, authorizationPolicy: null);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var filter = new MetalNexusApiDocumentFilter(registry);
        filter.Apply(document, CreateContext());

        var responses = document.Paths["/api/auth"].Operations![HttpMethod.Get].Responses!;
        responses.ContainsKey("401").ShouldBeTrue("401 is automatic for authenticated endpoints");
        responses.ContainsKey("403").ShouldBeFalse("403 requires roles or policy");
    }

    [Fact]
    public void Apply_AnonymousEndpoint_HasNeither401Nor403()
    {
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/public", requiresAuth: false,
            authorizedRoles: null, authorizationPolicy: null);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var filter = new MetalNexusApiDocumentFilter(registry);
        filter.Apply(document, CreateContext());

        var responses = document.Paths["/api/public"].Operations![HttpMethod.Get].Responses!;
        responses.ContainsKey("401").ShouldBeFalse("anonymous endpoint should not have 401");
        responses.ContainsKey("403").ShouldBeFalse("anonymous endpoint should not have 403");
    }

    [Fact]
    public void Apply_EndpointWithRoles_Has401And403()
    {
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/admin", requiresAuth: true,
            authorizedRoles: ["Admin", "SuperUser"]);
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var filter = new MetalNexusApiDocumentFilter(registry);
        filter.Apply(document, CreateContext());

        var responses = document.Paths["/api/admin"].Operations![HttpMethod.Get].Responses!;
        responses.ContainsKey("401").ShouldBeTrue();
        responses.ContainsKey("403").ShouldBeTrue();
    }

    [Fact]
    public void Apply_EndpointWithPolicy_Has401And403()
    {
        var registry = Substitute.For<IMetalNexusRegistry>();
        var endpoint = CreateEndpoint("/api/mfa", requiresAuth: true,
            authorizationPolicy: "RequireMfa");
        registry.Endpoints.Returns(new[] { endpoint });

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
        var filter = new MetalNexusApiDocumentFilter(registry);
        filter.Apply(document, CreateContext());

        var responses = document.Paths["/api/mfa"].Operations![HttpMethod.Get].Responses!;
        responses.ContainsKey("401").ShouldBeTrue();
        responses.ContainsKey("403").ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Test types
    // -----------------------------------------------------------------------

    // Custom exception that doesn't inherit any recognized MetalNexus type → maps to 400
    public class CustomBusinessException : Exception
    {
        public CustomBusinessException() : base("custom") { }
    }

    // Subclass of a recognized type — inherits its status code mapping
    public class DerivedNotFoundException : NotFoundException
    {
        public DerivedNotFoundException() : base("derived") { }
    }

    // Test types
    public class TestRequest
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    public class TestResponse
    {
        public bool Success { get; set; }
    }

    [Obsolete]
    public class ObsoleteRequest
    {
        public string? Data { get; set; }
    }

    public class FileUploadRequest : MetalNexusFileRequest
    {
        public string? FileName { get; set; }
    }
}
