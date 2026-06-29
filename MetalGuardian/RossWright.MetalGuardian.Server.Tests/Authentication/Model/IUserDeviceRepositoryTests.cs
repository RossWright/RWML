using NSubstitute;
using RossWright.MetalGuardian;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests.Authentication.Model;

public class IUserDeviceRepositoryTests
{
    [Fact]
    public async Task Add_WithValidAction_CallsRepositoryMethod()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var cancellationToken = CancellationToken.None;
        Action<IUserDevice> setProperties = device =>
        {
            device.UserId = userId;
            device.Fingerprint = fingerprint;
        };

        // Act
        await repository.Add(setProperties, cancellationToken);

        // Assert
        await repository.Received(1).Add(setProperties, cancellationToken);
    }

    [Fact]
    public async Task Add_WithCancellationToken_PassesTokenToRepository()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var cancellationToken = new CancellationToken(true);
        Action<IUserDevice> setProperties = device => { };

        // Act
        await repository.Add(setProperties, cancellationToken);

        // Assert
        await repository.Received(1).Add(setProperties, cancellationToken);
    }

    [Fact]
    public async Task Get_WithValidParameters_CallsRepositoryMethod()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var cancellationToken = CancellationToken.None;

        // Act
        await repository.Get(userId, fingerprint, cancellationToken);

        // Assert
        await repository.Received(1).Get(userId, fingerprint, cancellationToken);
    }

    [Fact]
    public async Task Get_WhenDeviceExists_ReturnsDevice()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var cancellationToken = CancellationToken.None;
        var expectedDevice = Substitute.For<IUserDevice>();
        expectedDevice.UserId.Returns(userId);
        expectedDevice.Fingerprint.Returns(fingerprint);
        repository.Get(userId, fingerprint, cancellationToken).Returns(expectedDevice);

        // Act
        var result = await repository.Get(userId, fingerprint, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.Fingerprint.ShouldBe(fingerprint);
    }

    [Fact]
    public async Task Get_WhenDeviceDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var cancellationToken = CancellationToken.None;
        repository.Get(userId, fingerprint, cancellationToken).Returns((IUserDevice?)null);

        // Act
        var result = await repository.Get(userId, fingerprint, cancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Get_WithCancellationToken_PassesTokenToRepository()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var cancellationToken = new CancellationToken(true);

        // Act
        await repository.Get(userId, fingerprint, cancellationToken);

        // Assert
        await repository.Received(1).Get(userId, fingerprint, cancellationToken);
    }

    [Fact]
    public async Task Update_WithValidParameters_CallsRepositoryMethod()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var cancellationToken = CancellationToken.None;
        Action<IUserDevice> setProperties = device => { device.LastSeen = DateTime.UtcNow; };

        // Act
        await repository.Update(userId, fingerprint, setProperties, cancellationToken);

        // Assert
        await repository.Received(1).Update(userId, fingerprint, setProperties, cancellationToken);
    }

    [Fact]
    public async Task Update_WithCancellationToken_PassesTokenToRepository()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.NewGuid();
        var fingerprint = "test-fingerprint";
        var cancellationToken = new CancellationToken(true);
        Action<IUserDevice> setProperties = device => { };

        // Act
        await repository.Update(userId, fingerprint, setProperties, cancellationToken);

        // Assert
        await repository.Received(1).Update(userId, fingerprint, setProperties, cancellationToken);
    }

    [Fact]
    public async Task Update_WithEmptyGuid_CallsRepositoryMethod()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.Empty;
        var fingerprint = "test-fingerprint";
        var cancellationToken = CancellationToken.None;
        Action<IUserDevice> setProperties = device => { };

        // Act
        await repository.Update(userId, fingerprint, setProperties, cancellationToken);

        // Assert
        await repository.Received(1).Update(userId, fingerprint, setProperties, cancellationToken);
    }

    [Fact]
    public async Task Get_WithEmptyGuid_CallsRepositoryMethod()
    {
        // Arrange
        var repository = Substitute.For<IUserDeviceRepository>();
        var userId = Guid.Empty;
        var fingerprint = "test-fingerprint";
        var cancellationToken = CancellationToken.None;

        // Act
        await repository.Get(userId, fingerprint, cancellationToken);

        // Assert
        await repository.Received(1).Get(userId, fingerprint, cancellationToken);
    }
}
