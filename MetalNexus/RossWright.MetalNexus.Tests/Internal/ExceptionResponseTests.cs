using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using RossWright.MetalNexus.Internal;

namespace RossWright.MetalNexus.Tests.Internal;

public class ExceptionResponseTests
{
    [Fact]
    public void Constructor_Default_CreatesInstance()
    {
        // Arrange & Act
        var response = new ExceptionResponse();

        // Assert
        response.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithValidationException_SetsUnprocessableEntityStatusCode()
    {
        // Arrange
        var exception = new ValidationException("Validation failed");

        // Act
        var response = new ExceptionResponse(exception);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.Message.ShouldBe("Validation failed");
        response.TypeName.ShouldBe(typeof(ValidationException).FullName);
    }

    [Fact]
    public void Constructor_WithInternalServerErrorException_SetsInternalServerErrorStatusCode()
    {
        // Arrange
        var exception = new InternalServerErrorException("Internal error");

        // Act
        var response = new ExceptionResponse(exception);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Message.ShouldBe("Internal error");
        response.TypeName.ShouldBe(typeof(InternalServerErrorException).FullName);
    }

    [Fact]
    public void Deserialize_WithNullJson_ReturnsExceptionBasedOnStatusCode_Unauthorized()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.Unauthorized;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, null, false);

        // Assert
        exception.ShouldBeOfType<NotAuthenticatedException>();
        exception.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void Deserialize_WithNullJson_ReturnsExceptionBasedOnStatusCode_Forbidden()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.Forbidden;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, null, false);

        // Assert
        exception.ShouldBeOfType<NotAuthorizedException>();
        exception.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void Deserialize_WithNullJson_ReturnsExceptionBasedOnStatusCode_NotFound()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.NotFound;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, null, false);

        // Assert
        exception.ShouldBeOfType<NotFoundException>();
        exception.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void Deserialize_WithNullJson_ReturnsExceptionBasedOnStatusCode_NotImplemented()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.NotImplemented;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, null, false);

        // Assert
        exception.ShouldBeOfType<NotImplementedException>();
    }

    [Fact]
    public void Deserialize_WithNullJson_ReturnsExceptionBasedOnStatusCode_UnprocessableEntity()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.UnprocessableEntity;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, null, false);

        // Assert
        exception.ShouldBeOfType<ValidationException>();
    }

    [Fact]
    public void Deserialize_WithNullJson_ReturnsExceptionBasedOnStatusCode_InternalServerError()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.InternalServerError;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, null, false);

        // Assert
        exception.ShouldBeOfType<InternalServerErrorException>();
    }

    [Fact]
    public void Deserialize_WithNullJson_AndNullStatusCode_ReturnsMetalNexusException()
    {
        // Arrange
        HttpStatusCode? statusCode = null;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, null, false);

        // Assert
        exception.ShouldBeOfType<MetalNexusException>();
        exception.Message.ShouldBe("HTTP status code: ");
    }

    [Fact]
    public void Deserialize_WithNullJson_AndUnknownStatusCode_ReturnsMetalNexusException()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.BadRequest;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, null, false);

        // Assert
        exception.ShouldBeOfType<MetalNexusException>();
        exception.Message.ShouldBe("HTTP status code: BadRequest");
    }

    [Fact]
    public void Deserialize_WithWhitespaceJson_AndStatusCode_ReturnsMetalNexusExceptionWithStatusCode()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.BadRequest;
        var json = "   ";

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, json, false);

        // Assert
        exception.ShouldBeOfType<MetalNexusException>();
        exception.Message.ShouldBe("HTTP status code: BadRequest");
    }

    [Fact]
    public void Deserialize_WithNonWhitespaceJson_AndUnknownStatusCode_ReturnsMetalNexusExceptionWithResponse()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.BadRequest;
        var json = "Some error message";

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, json, false);

        // Assert
        exception.ShouldBeOfType<MetalNexusException>();
        exception.Message.ShouldBe("HTTP status code: BadRequest with response Some error message");
    }

    [Fact]
    public void Deserialize_WithJsonForUnauthorized_ReturnsNotAuthenticatedException()
    {
        // Arrange
        HttpStatusCode? statusCode = HttpStatusCode.Unauthorized;
        var json = "Unauthorized error";

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, json, false);

        // Assert
        exception.ShouldBeOfType<NotAuthenticatedException>();
        exception.Message.ShouldBe("Unauthorized error");
    }

    [Fact]
    public void Deserialize_WithValidJson_ReturnsDeserializedException()
    {
        // Arrange
        var originalException = new InvalidOperationException("Test error");
        var exceptionResponse = new ExceptionResponse(originalException);
        var json = JsonSerializer.Serialize(exceptionResponse);

        // Act
        var exception = ExceptionResponse.Deserialize(HttpStatusCode.BadRequest, json, false);

        // Assert
        exception.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Test error");
    }

    [Fact]
    public void Deserialize_WithInvalidJson_FallsBackToStatusCodeLogic()
    {
        // Arrange
        var json = "{ invalid json";
        HttpStatusCode? statusCode = HttpStatusCode.Unauthorized;

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, json, false);

        // Assert
        exception.ShouldBeOfType<NotAuthenticatedException>();
    }

    [Fact]
    public void ToException_WithUnknownType_ReturnsMetalNexusException()
    {
        // Arrange
        var response = new ExceptionResponse
        {
            AssemblyName = "NonExistent.Assembly",
            TypeName = "NonExistent.ExceptionType",
            Message = "Test message"
        };

        // Act
        var exception = response.ToException();

        // Assert
        exception.ShouldBeOfType<MetalNexusException>();
        exception.Message.ShouldBe("Server Exception of type NonExistent.ExceptionType: Test message");
    }

    [Fact]
    public void ToException_WithUnknownTypeAndInnerException_ReturnsMetalNexusExceptionWithInner()
    {
        // Arrange
        var innerException = new ArgumentException("Inner error");
        var innerResponse = new ExceptionResponse(innerException);
        var response = new ExceptionResponse
        {
            AssemblyName = "NonExistent.Assembly",
            TypeName = "NonExistent.ExceptionType",
            Message = "Test message",
            Inner = innerResponse
        };

        // Act
        var exception = response.ToException();

        // Assert
        exception.ShouldBeOfType<MetalNexusException>();
        exception.Message.ShouldBe("Server Exception of type NonExistent.ExceptionType: Test message");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<ArgumentException>();
        exception.InnerException!.Message.ShouldBe("Inner error");
    }

    [Fact]
    public void Constructor_WithExceptionAndStackTrace_IncludesStackTrace()
    {
        // Arrange
        Exception thrownException;
        try
        {
            throw new InvalidOperationException("Test");
        }
        catch (Exception ex)
        {
            thrownException = ex;
        }

        // Act
        var response = new ExceptionResponse(thrownException, includeStackTrace: true);

        // Assert
        response.StackTrace.ShouldNotBeNull();
        response.StackTrace.ShouldContain("ExceptionResponseTests");
    }

    [Fact]
    public void Constructor_WithExceptionWithoutStackTrace_ExcludesStackTrace()
    {
        // Arrange
        Exception thrownException;
        try
        {
            throw new InvalidOperationException("Test");
        }
        catch (Exception ex)
        {
            thrownException = ex;
        }

        // Act
        var response = new ExceptionResponse(thrownException, includeStackTrace: false);

        // Assert
        response.StackTrace.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithDefaultToBadRequestFalse_SetsInternalServerError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");

        // Act
        var response = new ExceptionResponse(exception, defaultToBadRequest: false);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    // -----------------------------------------------------------------------
    // GetStatusCode — all recognized mappings
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(typeof(NotAuthenticatedException), HttpStatusCode.Unauthorized)]
    [InlineData(typeof(NotAuthorizedException),    HttpStatusCode.Forbidden)]
    [InlineData(typeof(NotFoundException),          HttpStatusCode.NotFound)]
    [InlineData(typeof(NotImplementedException),    HttpStatusCode.NotImplemented)]
    [InlineData(typeof(ValidationException),        HttpStatusCode.UnprocessableEntity)]
    [InlineData(typeof(InternalServerErrorException), HttpStatusCode.InternalServerError)]
    public void GetStatusCode_WithKnownExceptionType_ReturnsExpectedStatusCode(
        Type exceptionType, HttpStatusCode expected)
    {
        ExceptionResponse.GetStatusCode(exceptionType).ShouldBe(expected);
    }

    [Fact]
    public void GetStatusCode_WithUnknownExceptionType_ReturnsBadRequest()
    {
        ExceptionResponse.GetStatusCode(typeof(InvalidOperationException))
            .ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void GetStatusCode_WithSubclassOfNotFoundException_ReturnsNotFound()
    {
        ExceptionResponse.GetStatusCode(typeof(DerivedNotFoundException))
            .ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public void GetStatusCode_WithSubclassOfNotAuthenticatedException_ReturnsUnauthorized()
    {
        ExceptionResponse.GetStatusCode(typeof(DerivedNotAuthenticatedException))
            .ShouldBe(HttpStatusCode.Unauthorized);
    }

    private class DerivedNotFoundException : NotFoundException
    {
        public DerivedNotFoundException() : base("derived") { }
    }

    private class DerivedNotAuthenticatedException : NotAuthenticatedException
    {
        public DerivedNotAuthenticatedException() : base("derived") { }
    }

    // -----------------------------------------------------------------------
    // Deserialize tiebreaker — JSON for unknown assembly/type
    // When ToException falls back to MetalNexusException because the server
    // assembly is not loaded on the client, Deserialize should use the HTTP
    // status code to reconstruct the correct typed exception.
    // -----------------------------------------------------------------------

    private static string BuildUnknownTypeJson(string message) =>
        JsonSerializer.Serialize(new ExceptionResponse
        {
            AssemblyName = "NonExistent.Assembly, Version=1.0.0.0",
            TypeName = "NonExistent.SomeException",
            Message = message
        });

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.NotImplemented)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public void Deserialize_WithUnknownAssemblyJson_UsesStatusCodeTiebreaker(HttpStatusCode statusCode)
    {
        // Arrange — valid JSON but from an assembly not present on the client
        var json = BuildUnknownTypeJson("original message");

        // Act
        var exception = ExceptionResponse.Deserialize(statusCode, json, false);

        // Assert — must NOT be MetalNexusException; must be the typed exception for the status
        exception.ShouldNotBeOfType<MetalNexusException>();
        var expectedType = statusCode switch
        {
            HttpStatusCode.Unauthorized      => typeof(NotAuthenticatedException),
            HttpStatusCode.Forbidden         => typeof(NotAuthorizedException),
            HttpStatusCode.NotFound          => typeof(NotFoundException),
            HttpStatusCode.NotImplemented    => typeof(NotImplementedException),
            HttpStatusCode.UnprocessableEntity => typeof(ValidationException),
            HttpStatusCode.InternalServerError => typeof(InternalServerErrorException),
            _ => throw new InvalidOperationException("unexpected")
        };
        exception.GetType().ShouldBe(expectedType);
        exception.Message.ShouldBe("original message");
    }

    [Fact]
    public void Deserialize_WithUnknownAssemblyJsonAndUnrecognizedStatusCode_ReturnsMetalNexusException()
    {
        // Arrange — valid JSON but unresolvable type; status code not in the known mapping
        var json = BuildUnknownTypeJson("original message");

        // Act
        var exception = ExceptionResponse.Deserialize(HttpStatusCode.BadRequest, json, false);

        // Assert — falls through to MetalNexusException with status+message
        exception.ShouldBeOfType<MetalNexusException>();
        exception.Message.ShouldBe("HTTP status code: BadRequest with response original message");
    }

    [Fact]
    public void Deserialize_WithUnknownAssemblyJsonAndNullStatusCode_ReturnsMetalNexusException()
    {
        // Arrange — valid JSON but unresolvable type; no status code available
        var json = BuildUnknownTypeJson("original message");

        // Act
        var exception = ExceptionResponse.Deserialize(null, json, false);

        // Assert — cannot recover typed exception; MetalNexusException with original text
        exception.ShouldBeOfType<MetalNexusException>();
        exception.Message.ShouldBe("Server Exception of type NonExistent.SomeException: original message");
    }

    [Fact]
    public void Deserialize_WithKnownTypeJson_NeverInvokesTiebreaker()
    {
        // Arrange — well-formed JSON for a type the client assembly CAN resolve
        var original = new NotFoundException("item not found");
        var json = JsonSerializer.Serialize(new ExceptionResponse(original));

        // Act — pass a *different* status code to prove the JSON path wins, not the tiebreaker
        var exception = ExceptionResponse.Deserialize(HttpStatusCode.Unauthorized, json, false);

        // Assert — must be NotFoundException from JSON, not NotAuthenticatedException from status
        exception.ShouldBeOfType<NotFoundException>();
        exception.Message.ShouldBe("item not found");
    }
}
