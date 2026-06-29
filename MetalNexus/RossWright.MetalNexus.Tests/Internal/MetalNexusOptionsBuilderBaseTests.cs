using NSubstitute;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using System.Reflection;

namespace RossWright.MetalNexus.Tests.Internal;

public class MetalNexusOptionsBuilderBaseTests
{
    [Fact]
    public void IncludeServerStackTraceOnExceptions_DefaultTrue_SetsFieldToTrue()
    {
        // Arrange
        var sut = new TestOptionsBuilder();

        // Act
        sut.IncludeServerStackTraceOnExceptions();

        // Assert
        sut.ServerStackTraceOnExceptionsIncluded.ShouldBeTrue();
    }

    [Fact]
    public void IncludeServerStackTraceOnExceptions_ExplicitTrue_SetsFieldToTrue()
    {
        // Arrange
        var sut = new TestOptionsBuilder();

        // Act
        sut.IncludeServerStackTraceOnExceptions(true);

        // Assert
        sut.ServerStackTraceOnExceptionsIncluded.ShouldBeTrue();
    }

    [Fact]
    public void IncludeServerStackTraceOnExceptions_False_SetsFieldToFalse()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        sut.IncludeServerStackTraceOnExceptions(true); // Set to true first

        // Act
        sut.IncludeServerStackTraceOnExceptions(false);

        // Assert
        sut.ServerStackTraceOnExceptionsIncluded.ShouldBeFalse();
    }

    [Fact]
    public void ServerStackTraceOnExceptionsIncluded_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var sut = new TestOptionsBuilder();

        // Assert
        sut.ServerStackTraceOnExceptionsIncluded.ShouldBeFalse();
    }

    [Fact]
    public void ServerStackTraceOnExceptionsIncluded_AfterSettingTrue_ReturnsTrue()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        sut.IncludeServerStackTraceOnExceptions(true);

        // Act
        var result = sut.ServerStackTraceOnExceptionsIncluded;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void TreatUnknownExceptionsAsInternalServiceError_DefaultTrue_SetsFieldToTrue()
    {
        // Arrange
        var sut = new TestOptionsBuilder();

        // Act
        sut.TreatUnknownExceptionsAsInternalServiceError();

        // Assert
        sut.DefaultToBadRequest.ShouldBeFalse();
    }

    [Fact]
    public void TreatUnknownExceptionsAsInternalServiceError_ExplicitTrue_SetsFieldToTrue()
    {
        // Arrange
        var sut = new TestOptionsBuilder();

        // Act
        sut.TreatUnknownExceptionsAsInternalServiceError(true);

        // Assert
        sut.DefaultToBadRequest.ShouldBeFalse();
    }

    [Fact]
    public void TreatUnknownExceptionsAsInternalServiceError_False_SetsFieldToFalse()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        sut.TreatUnknownExceptionsAsInternalServiceError(true); // Set to true first

        // Act
        sut.TreatUnknownExceptionsAsInternalServiceError(false);

        // Assert
        sut.DefaultToBadRequest.ShouldBeTrue();
    }

    [Fact]
    public void DefaultToBadRequest_DefaultValue_IsTrue()
    {
        // Arrange & Act
        var sut = new TestOptionsBuilder();

        // Assert
        sut.DefaultToBadRequest.ShouldBeTrue();
    }

    [Fact]
    public void DefaultToBadRequest_AfterTreatAsInternalServiceError_ReturnsFalse()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        sut.TreatUnknownExceptionsAsInternalServiceError(true);

        // Act
        var result = sut.DefaultToBadRequest;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void DefaultToBadRequest_AfterNotTreatAsInternalServiceError_ReturnsTrue()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        sut.TreatUnknownExceptionsAsInternalServiceError(false);

        // Act
        var result = sut.DefaultToBadRequest;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ConfigureEndpointSchema_InvokesActionWithEndpointSchema()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        var wasCalled = false;
        IEndpointSchemaOptions? capturedOptions = null;

        // Act
        sut.ConfigureEndpointSchema(options =>
        {
            wasCalled = true;
            capturedOptions = options;
        });

        // Assert
        wasCalled.ShouldBeTrue();
        capturedOptions.ShouldNotBeNull();
        capturedOptions.ShouldBeSameAs(sut._endpointSchema);
    }

    [Fact]
    public void ConfigureEndpointSchema_AllowsModifyingEndpointSchema()
    {
        // Arrange
        var sut = new TestOptionsBuilder();

        // Act
        sut.ConfigureEndpointSchema(options =>
        {
            options.ApiPathPrefix = "/custom-api";
            options.RequiresAuthenticationByDefault = false;
        });

        // Assert
        sut._endpointSchema.ApiPathPrefix.ShouldBe("/custom-api");
        sut._endpointSchema.RequiresAuthenticationByDefault.ShouldBeFalse();
    }

    [Fact]
    public void ConfigureEndpointSchema_DefaultHttpProtocol_DefaultIsGet()
    {
        // Arrange & Act
        var sut = new TestOptionsBuilder();

        // Assert
        sut._endpointSchema.DefaultHttpProtocol.ShouldBe(HttpProtocol.Get);
    }

    [Fact]
    public void ConfigureEndpointSchema_DefaultHttpProtocol_CanBeSetViaInterface()
    {
        // Arrange
        var sut = new TestOptionsBuilder();

        // Act
        sut.ConfigureEndpointSchema(options => options.DefaultHttpProtocol = HttpProtocol.PostViaBody);

        // Assert
        sut._endpointSchema.DefaultHttpProtocol.ShouldBe(HttpProtocol.PostViaBody);
    }

    [Fact]
    public void ConfigureEndpointSchema_MultipleInvocations_AppliesAllChanges()
    {
        // Arrange
        var sut = new TestOptionsBuilder();

        // Act
        sut.ConfigureEndpointSchema(options => options.ApiPathPrefix = "/v1");
        sut.ConfigureEndpointSchema(options => options.ApiPathToLower = false);

        // Assert
        sut._endpointSchema.ApiPathPrefix.ShouldBe("/v1");
        sut._endpointSchema.ApiPathToLower.ShouldBeFalse();
    }

    [Fact]
    public void UseCustomEndpointSchema_WithValidSchema_SetsCustomEndpointSchema()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        var schema = Substitute.For<ICustomEndpointSchema>();

        // Act
        sut.UseCustomEndpointSchema(schema);

        // Assert
        sut.GetCustomEndpointSchema().ShouldBeSameAs(schema);
    }

    [Fact]
    public void UseCustomEndpointSchema_WithNull_SetsCustomEndpointSchemaToNull()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        var schema = Substitute.For<ICustomEndpointSchema>();
        sut.UseCustomEndpointSchema(schema); // Set to non-null first

        // Act
        sut.UseCustomEndpointSchema(null!);

        // Assert
        sut.GetCustomEndpointSchema().ShouldBeNull();
    }

    [Fact]
    public void UseCustomEndpointSchema_MultipleInvocations_OverwritesPreviousValue()
    {
        // Arrange
        var sut = new TestOptionsBuilder();
        var firstSchema = Substitute.For<ICustomEndpointSchema>();
        var secondSchema = Substitute.For<ICustomEndpointSchema>();
        sut.UseCustomEndpointSchema(firstSchema);

        // Act
        sut.UseCustomEndpointSchema(secondSchema);

        // Assert
        sut.GetCustomEndpointSchema().ShouldBeSameAs(secondSchema);
    }

    // Test helper class to instantiate abstract base class
    private class TestOptionsBuilder : MetalNexusOptionsBuilderBase
    {
        // Abstract base class requires a concrete implementation to test
        // Expose private field for testing using reflection
        public ICustomEndpointSchema? GetCustomEndpointSchema()
        {
            var field = typeof(MetalNexusOptionsBuilderBase).GetField("_customEndpointSchema", BindingFlags.NonPublic | BindingFlags.Instance);
            return (ICustomEndpointSchema?)field?.GetValue(this);
        }
    }
}
