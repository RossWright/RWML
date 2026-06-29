using Shouldly;

namespace RossWright.MetalGuardian.Tests.Authentication.Internal;

public class BaseAddressRepositoryTests
{
    [Fact]
    public void Add_FirstConnection_SetsDefaultConnectionName()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();

        // Act
        repository.Add("connection1", "https://example.com", asDefault: false);

        // Assert
        repository.DefaultConnectionName.ShouldBe("connection1");
    }

    [Fact]
    public void Add_AsDefaultTrue_SetsDefaultConnectionName()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();
        repository.Add("connection1", "https://example.com", asDefault: false);

        // Act
        repository.Add("connection2", "https://example2.com", asDefault: true);

        // Assert
        repository.DefaultConnectionName.ShouldBe("connection2");
    }

    [Fact]
    public void Add_AsDefaultFalseWithExistingConnections_DoesNotChangeDefaultConnectionName()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();
        repository.Add("connection1", "https://example.com", asDefault: false);

        // Act
        repository.Add("connection2", "https://example2.com", asDefault: false);

        // Assert
        repository.DefaultConnectionName.ShouldBe("connection1");
    }

    [Fact]
    public void Add_AddsConnectionToDictionary()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();

        // Act
        repository.Add("connection1", "https://example.com", asDefault: false);

        // Assert
        repository.BaseUrlsByConnectionName.ShouldContain(kvp => 
            kvp.Key == "connection1" && kvp.Value == "https://example.com");
    }

    [Fact]
    public void Add_NullBaseAddress_AddsConnectionWithNullValue()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();

        // Act
        repository.Add("connection1", null, asDefault: false);

        // Assert
        repository.BaseUrlsByConnectionName.ShouldContain(kvp => 
            kvp.Key == "connection1" && kvp.Value == null);
    }

    [Fact]
    public void BaseUrlsByConnectionName_ReturnsAllAddedConnections()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();
        repository.Add("connection1", "https://example.com", asDefault: false);
        repository.Add("connection2", "https://example2.com", asDefault: false);

        // Act
        var result = repository.BaseUrlsByConnectionName.ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(kvp => kvp.Key == "connection1" && kvp.Value == "https://example.com");
        result.ShouldContain(kvp => kvp.Key == "connection2" && kvp.Value == "https://example2.com");
    }

    [Fact]
    public void GetBaseAddress_WithNullConnectionName_ReturnsDefaultConnectionBaseAddress()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();
        repository.Add("connection1", "https://example.com", asDefault: true);

        // Act
        var result = repository.GetBaseAddress(null);

        // Assert
        result.ShouldBe("https://example.com");
    }

    [Fact]
    public void GetBaseAddress_WithSpecificConnectionName_ReturnsThatConnectionBaseAddress()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();
        repository.Add("connection1", "https://example.com", asDefault: true);
        repository.Add("connection2", "https://example2.com", asDefault: false);

        // Act
        var result = repository.GetBaseAddress("connection2");

        // Assert
        result.ShouldBe("https://example2.com");
    }

    [Fact]
    public void GetBaseAddress_ConnectionNotFound_ReturnsEmptyString()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();

        // Act
        var result = repository.GetBaseAddress("nonexistent");

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetBaseAddress_NullBaseAddress_ReturnsEmptyString()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();
        repository.Add("connection1", null, asDefault: true);

        // Act
        var result = repository.GetBaseAddress("connection1");

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetBaseAddress_NoConnectionNameAndDefaultNotFound_ReturnsEmptyString()
    {
        // Arrange
        var repository = new RossWright.MetalGuardian.Authentication.BaseAddressRepository();

        // Act
        var result = repository.GetBaseAddress(null);

        // Assert
        result.ShouldBe(string.Empty);
    }
}
