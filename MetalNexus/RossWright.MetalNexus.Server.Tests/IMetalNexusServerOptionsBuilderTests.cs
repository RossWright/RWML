using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Server.Tests;

public class IMetalNexusServerOptionsBuilderTests
{
    [Fact]
    public void SetMultipartBodyLengthLimit_WithNullValue_CallsMethod()
    {
        // Arrange
        var builder = Substitute.For<IMetalNexusServerOptionsBuilder>();

        // Act
        builder.SetMultipartBodyLengthLimit(null);

        // Assert
        builder.Received(1).SetMultipartBodyLengthLimit(null);
    }

    [Fact]
    public void SetMultipartBodyLengthLimit_WithPositiveValue_CallsMethod()
    {
        // Arrange
        var builder = Substitute.For<IMetalNexusServerOptionsBuilder>();
        long limitInBytes = 1024 * 1024 * 10; // 10 MB

        // Act
        builder.SetMultipartBodyLengthLimit(limitInBytes);

        // Assert
        builder.Received(1).SetMultipartBodyLengthLimit(limitInBytes);
    }

    [Fact]
    public void SetMultipartBodyLengthLimit_WithZeroValue_CallsMethod()
    {
        // Arrange
        var builder = Substitute.For<IMetalNexusServerOptionsBuilder>();

        // Act
        builder.SetMultipartBodyLengthLimit(0);

        // Assert
        builder.Received(1).SetMultipartBodyLengthLimit(0);
    }

    [Fact]
    public void MakeEndpointsAnonymousByDefault_CallsConfigureEndpointSchema()
    {
        // Arrange
        var builder = Substitute.For<IMetalNexusServerOptionsBuilder>();

        // Act
        builder.MakeEndpointsAnonymousByDefault();

        // Assert
        builder.Received(1).ConfigureEndpointSchema(Arg.Any<Action<IEndpointSchemaOptions>>());
    }

    [Fact]
    public void MakeEndpointsAnonymousByDefault_SetsRequiresAuthenticationByDefaultToFalse()
    {
        // Arrange
        var builder = Substitute.For<IMetalNexusServerOptionsBuilder>();
        Action<IEndpointSchemaOptions>? capturedAction = null;

        builder.When(b => b.ConfigureEndpointSchema(Arg.Any<Action<IEndpointSchemaOptions>>()))
            .Do(callInfo => capturedAction = callInfo.Arg<Action<IEndpointSchemaOptions>>());

        // Act
        builder.MakeEndpointsAnonymousByDefault();

        // Assert
        capturedAction.ShouldNotBeNull();

        var options = Substitute.For<IEndpointSchemaOptions>();
        options.RequiresAuthenticationByDefault = true; // Set to true initially

        capturedAction(options);

        options.RequiresAuthenticationByDefault.ShouldBeFalse();
    }
}
