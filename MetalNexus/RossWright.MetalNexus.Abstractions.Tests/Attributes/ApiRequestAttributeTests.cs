using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Attributes;

public class ApiRequestAttributeTests
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var protocol = HttpProtocol.Get;
        var path = "/api/test";
        var tag = "TestTag";
        var connectionName = "TestConnection";

        // Act
        var attribute = new ApiRequestAttribute(protocol, path, tag, connectionName);

        // Assert
        attribute.HttpProtocol.ShouldBe(protocol);
        attribute.Path.ShouldBe(path);
        attribute.Tag.ShouldBe(tag);
        attribute.ConnectionName.ShouldBe(connectionName);
    }

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldSetDefaults()
    {
        // Arrange & Act
        var attribute = new ApiRequestAttribute();

        // Assert
        attribute.HttpProtocol.ShouldBe(HttpProtocol.Auto);
        attribute.Path.ShouldBeNull();
        attribute.Tag.ShouldBeNull();
        attribute.ConnectionName.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullPath_ShouldSetPathToNull()
    {
        // Arrange & Act
        var attribute = new ApiRequestAttribute(HttpProtocol.PostViaBody, null);

        // Assert
        attribute.Path.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullTag_ShouldSetTagToNull()
    {
        // Arrange & Act
        var attribute = new ApiRequestAttribute(HttpProtocol.PostViaBody, "/api/test", null);

        // Assert
        attribute.Tag.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullConnectionName_ShouldSetConnectionNameToNull()
    {
        // Arrange & Act
        var attribute = new ApiRequestAttribute(HttpProtocol.PostViaBody, "/api/test", "tag", null);

        // Assert
        attribute.ConnectionName.ShouldBeNull();
    }

    [Fact]
    public void ToHttpMethod_WithGet_ReturnsHttpGet()
    {
        // Arrange
        var protocol = HttpProtocol.Get;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Get);
    }

    [Fact]
    public void ToHttpMethod_WithPostViaBody_ReturnsHttpPost()
    {
        // Arrange
        var protocol = HttpProtocol.PostViaBody;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Post);
    }

    [Fact]
    public void ToHttpMethod_WithPostViaQuery_ReturnsHttpPost()
    {
        // Arrange
        var protocol = HttpProtocol.PostViaQuery;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Post);
    }

    [Fact]
    public void ToHttpMethod_WithPutViaBody_ReturnsHttpPut()
    {
        // Arrange
        var protocol = HttpProtocol.PutViaBody;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Put);
    }

    [Fact]
    public void ToHttpMethod_WithPutViaQuery_ReturnsHttpPut()
    {
        // Arrange
        var protocol = HttpProtocol.PutViaQuery;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Put);
    }

    [Fact]
    public void ToHttpMethod_WithPatchViaBody_ReturnsHttpPatch()
    {
        // Arrange
        var protocol = HttpProtocol.PatchViaBody;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Patch);
    }

    [Fact]
    public void ToHttpMethod_WithPatchViaQuery_ReturnsHttpPatch()
    {
        // Arrange
        var protocol = HttpProtocol.PatchViaQuery;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Patch);
    }

    [Fact]
    public void ToHttpMethod_WithDelete_ReturnsHttpDelete()
    {
        // Arrange
        var protocol = HttpProtocol.Delete;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Delete);
    }

    [Fact]
    public void ToHttpMethod_WithDeleteViaBody_ReturnsHttpDelete()
    {
        // Arrange
        var protocol = HttpProtocol.DeleteViaBody;

        // Act
        var result = protocol.ToHttpMethod();

        // Assert
        result.ShouldBe(HttpMethod.Delete);
    }

    [Fact]
    public void ToHttpMethod_WithAuto_ThrowsInvalidOperationException()
    {
        // Arrange
        var protocol = HttpProtocol.Auto;

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => protocol.ToHttpMethod())
            .Message.ShouldBe("Cannot determine HTTP method for Auto");
    }

    [Fact]
    public void UsesQueryParams_WithGet_ReturnsTrue()
    {
        // Arrange
        var protocol = HttpProtocol.Get;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void UsesQueryParams_WithPostViaQuery_ReturnsTrue()
    {
        // Arrange
        var protocol = HttpProtocol.PostViaQuery;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void UsesQueryParams_WithPutViaQuery_ReturnsTrue()
    {
        // Arrange
        var protocol = HttpProtocol.PutViaQuery;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void UsesQueryParams_WithPatchViaQuery_ReturnsTrue()
    {
        // Arrange
        var protocol = HttpProtocol.PatchViaQuery;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void UsesQueryParams_WithPostViaBody_ReturnsFalse()
    {
        // Arrange
        var protocol = HttpProtocol.PostViaBody;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void UsesQueryParams_WithPutViaBody_ReturnsFalse()
    {
        // Arrange
        var protocol = HttpProtocol.PutViaBody;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void UsesQueryParams_WithPatchViaBody_ReturnsFalse()
    {
        // Arrange
        var protocol = HttpProtocol.PatchViaBody;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void UsesQueryParams_WithDelete_ReturnsFalse()
    {
        // Arrange
        var protocol = HttpProtocol.Delete;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void UsesQueryParams_WithDeleteViaBody_ReturnsFalse()
    {
        // Arrange
        var protocol = HttpProtocol.DeleteViaBody;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void UsesQueryParams_WithAuto_ReturnsFalse()
    {
        // Arrange
        var protocol = HttpProtocol.Auto;

        // Act
        var result = protocol.UsesQueryParams();

        // Assert
        result.ShouldBeFalse();
    }
}
