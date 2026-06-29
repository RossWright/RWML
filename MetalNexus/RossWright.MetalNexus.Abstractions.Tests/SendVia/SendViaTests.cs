using RossWright.MetalChain;
using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.SendVia;

public class SendViaTests
{
    private class TestRequest : IRequest
    {
    }

    private class TestRequestWithResponse : IRequest<TestResponse>
    {
    }

    private class TestResponse
    {
    }

    [Fact]
    public void ConnectionName_WithValidConnectionName_ShouldReturnConnectionName()
    {
        // Arrange
        var connectionName = "TestConnection";
        var request = new TestRequest();

        // Act
        var sendVia = new SendVia<TestRequest>(connectionName, request);

        // Assert
        sendVia.ConnectionName.ShouldBe(connectionName);
    }

    [Fact]
    public void Request_WithValidRequest_ShouldReturnRequest()
    {
        // Arrange
        var connectionName = "TestConnection";
        var request = new TestRequest();

        // Act
        var sendVia = new SendVia<TestRequest>(connectionName, request);

        // Assert
        sendVia.Request.ShouldBe(request);
    }

    [Fact]
    public void ConnectionName_WithEmptyConnectionName_ShouldReturnEmptyString()
    {
        // Arrange
        var connectionName = string.Empty;
        var request = new TestRequest();

        // Act
        var sendVia = new SendVia<TestRequest>(connectionName, request);

        // Assert
        sendVia.ConnectionName.ShouldBe(string.Empty);
    }

    [Fact]
    public void ConnectionName_WithNullConnectionName_ShouldReturnNull()
    {
        // Arrange
        string? connectionName = null;
        var request = new TestRequest();

        // Act
        var sendVia = new SendVia<TestRequest>(connectionName!, request);

        // Assert
        sendVia.ConnectionName.ShouldBeNull();
    }

    [Fact]
    public void Request_WithNullRequest_ShouldReturnNull()
    {
        // Arrange
        var connectionName = "TestConnection";
        TestRequest? request = null;

        // Act
        var sendVia = new SendVia<TestRequest>(connectionName, request!);

        // Assert
        sendVia.Request.ShouldBeNull();
    }

    [Fact]
    public void ConnectionName_WithResponse_WithValidConnectionName_ShouldReturnConnectionName()
    {
        // Arrange
        var connectionName = "TestConnection";
        var request = new TestRequestWithResponse();

        // Act
        var sendVia = new SendVia<TestRequestWithResponse, TestResponse>(connectionName, request);

        // Assert
        sendVia.ConnectionName.ShouldBe(connectionName);
    }

    [Fact]
    public void Request_WithResponse_WithValidRequest_ShouldReturnRequest()
    {
        // Arrange
        var connectionName = "TestConnection";
        var request = new TestRequestWithResponse();

        // Act
        var sendVia = new SendVia<TestRequestWithResponse, TestResponse>(connectionName, request);

        // Assert
        sendVia.Request.ShouldBe(request);
    }

    [Fact]
    public void ConnectionName_WithResponse_WithEmptyConnectionName_ShouldReturnEmptyString()
    {
        // Arrange
        var connectionName = string.Empty;
        var request = new TestRequestWithResponse();

        // Act
        var sendVia = new SendVia<TestRequestWithResponse, TestResponse>(connectionName, request);

        // Assert
        sendVia.ConnectionName.ShouldBe(string.Empty);
    }

    [Fact]
    public void ConnectionName_WithResponse_WithNullConnectionName_ShouldReturnNull()
    {
        // Arrange
        string? connectionName = null;
        var request = new TestRequestWithResponse();

        // Act
        var sendVia = new SendVia<TestRequestWithResponse, TestResponse>(connectionName!, request);

        // Assert
        sendVia.ConnectionName.ShouldBeNull();
    }

    [Fact]
    public void Request_WithResponse_WithNullRequest_ShouldReturnNull()
    {
        // Arrange
        var connectionName = "TestConnection";
        TestRequestWithResponse? request = null;

        // Act
        var sendVia = new SendVia<TestRequestWithResponse, TestResponse>(connectionName, request!);

        // Assert
        sendVia.Request.ShouldBeNull();
    }
}
