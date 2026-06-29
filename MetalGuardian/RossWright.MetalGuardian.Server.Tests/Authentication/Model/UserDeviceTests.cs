using RossWright.MetalGuardian;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests;

public class UserDeviceTests
{
    [Fact]
    public void User_WhenAccessedViaInterface_ReturnsGenericUserProperty()
    {
        // Arrange
        var testUser = new TestAuthenticationUser
        {
            UserId = Guid.NewGuid(),
            Name = "TestUser"
        };
        var userDevice = new UserDevice<TestAuthenticationUser>
        {
            User = testUser
        };

        // Act
        IUserDevice interfaceDevice = userDevice;
        var result = interfaceDevice.User;

        // Assert
        result.ShouldBe(testUser);
    }

    [Fact]
    public void User_WhenGenericPropertyUpdated_InterfacePropertyReflectsChange()
    {
        // Arrange
        var firstUser = new TestAuthenticationUser
        {
            UserId = Guid.NewGuid(),
            Name = "FirstUser"
        };
        var secondUser = new TestAuthenticationUser
        {
            UserId = Guid.NewGuid(),
            Name = "SecondUser"
        };
        var userDevice = new UserDevice<TestAuthenticationUser>
        {
            User = firstUser
        };
        IUserDevice interfaceDevice = userDevice;

        // Act
        userDevice.User = secondUser;
        var result = interfaceDevice.User;

        // Assert
        result.ShouldBe(secondUser);
    }

    [Fact]
    public void User_InterfaceAndGenericProperty_ReturnSameReference()
    {
        // Arrange
        var testUser = new TestAuthenticationUser
        {
            UserId = Guid.NewGuid(),
            Name = "TestUser"
        };
        var userDevice = new UserDevice<TestAuthenticationUser>
        {
            User = testUser
        };

        // Act
        IUserDevice interfaceDevice = userDevice;
        var genericUser = userDevice.User;
        var interfaceUser = interfaceDevice.User;

        // Assert
        ReferenceEquals(genericUser, interfaceUser).ShouldBeTrue();
    }

    private class TestAuthenticationUser : IAuthenticationUser
    {
        public Guid UserId { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool IsDisabled { get; init; }
        public string PasswordSalt { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
