using NSubstitute;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests.Internal;

public class MetalGuardianTotpMfaServiceTests
{
    [Fact]
    public async Task GetSetupQrCode_UserNotFound_ThrowsMetalGuardianException()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        
        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAuthenticationUser?>(null));
        
        var service = new MetalGuardianTotpMfaService("TestIssuer", null, authRepo, authSvc, null);
        
        // Act & Assert
        var ex = await Should.ThrowAsync<MetalGuardianException>(async () =>
            await service.GetSetupQrCode(userId, CancellationToken.None));
        
        ex.Message.ShouldBe("Unknown User ID");
    }

    [Fact]
    public async Task GetSetupQrCode_TotpAlreadyEnabled_ThrowsMetalGuardianException()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        var user = new FakeTotpUser
        {
            UserId = userId,
            Name = "TestUser",
            MfaTotpSecret = "JBSWY3DPEHPK3PXP",
            IsMfaTotpEnabled = true
        };
        
        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAuthenticationUser?>(user));
        
        var service = new MetalGuardianTotpMfaService("TestIssuer", null, authRepo, authSvc, null);
        
        // Act & Assert
        var ex = await Should.ThrowAsync<MetalGuardianException>(async () =>
            await service.GetSetupQrCode(userId, CancellationToken.None));
        
        ex.Message.ShouldBe("TOTP MFA is already enabled for this user. Call ResetUser before re-enrolling.");
    }

    [Fact]
    public async Task GetSetupQrCode_ValidUser_ReturnsBase64QrCodeDataUri()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        var user = new FakeTotpUser
        {
            UserId = userId,
            Name = "TestUser",
            MfaTotpSecret = "JBSWY3DPEHPK3PXP",
            IsMfaTotpEnabled = false
        };
        
        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAuthenticationUser?>(user));
        
        var service = new MetalGuardianTotpMfaService("TestIssuer", null, authRepo, authSvc, null);
        
        // Act
        var result = await service.GetSetupQrCode(userId, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        result.ShouldStartWith("data:image/png;base64,");
        result.Length.ShouldBeGreaterThan(22);
    }

    [Fact]
    public async Task GetSetupQrCode_WithSpecialCharactersInName_ReturnsValidQrCode()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        var user = new FakeTotpUser
        {
            UserId = userId,
            Name = "Test@User:With&Special=Characters",
            MfaTotpSecret = "JBSWY3DPEHPK3PXP",
            IsMfaTotpEnabled = false
        };
        
        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAuthenticationUser?>(user));
        
        var service = new MetalGuardianTotpMfaService("TestIssuer", null, authRepo, authSvc, null);
        
        // Act
        var result = await service.GetSetupQrCode(userId, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        result.ShouldStartWith("data:image/png;base64,");
        result.Length.ShouldBeGreaterThan(22);
    }

    [Fact]
    public async Task GetSetupQrCode_WithDifferentIssuer_IncludesIssuerInQrCode()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        var user = new FakeTotpUser
        {
            UserId = userId,
            Name = "TestUser",
            MfaTotpSecret = "JBSWY3DPEHPK3PXP",
            IsMfaTotpEnabled = false
        };
        
        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAuthenticationUser?>(user));
        
        var service = new MetalGuardianTotpMfaService("MyCustomIssuer", null, authRepo, authSvc, null);
        
        // Act
        var result = await service.GetSetupQrCode(userId, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        result.ShouldStartWith("data:image/png;base64,");
        result.Length.ShouldBeGreaterThan(22);
    }

    [Fact]
    public async Task GetSetupQrCode_WithExtremelyLongData_ThrowsMetalGuardianException()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        
        // Create an extremely long secret that exceeds QR code capacity
        var veryLongSecret = new string('A', 10000);
        var user = new FakeTotpUser
        {
            UserId = userId,
            Name = "TestUser",
            MfaTotpSecret = veryLongSecret,
            IsMfaTotpEnabled = false
        };
        
        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAuthenticationUser?>(user));
        
        var service = new MetalGuardianTotpMfaService("TestIssuer", null, authRepo, authSvc, null);
        
        // Act & Assert
        var ex = await Should.ThrowAsync<MetalGuardianException>(async () =>
            await service.GetSetupQrCode(userId, CancellationToken.None));
        
        ex.Message.ShouldBe("Failed To Generate QR Code");
        ex.InnerException.ShouldNotBeNull();
    }

    [Fact]
    public async Task ResetUser_WhenAlreadyReset_DoesNotSaveChanges()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        var user = new FakeTotpUser
        {
            UserId = userId,
            IsMfaTotpEnabled = false,
            MfaTotpSecret = null
        };

        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var func = callInfo.Arg<Func<IAuthenticationUser, bool>>();
                var saved = func(user);
                saved.ShouldBeFalse();
                return Task.FromResult<IAuthenticationUser?>(user);
            });

        var service = new MetalGuardianTotpMfaService("TestIssuer", null, authRepo, authSvc, null);

        // Act
        await service.ResetUser(userId, CancellationToken.None);

        // Assert: user state is unchanged
        user.IsMfaTotpEnabled.ShouldBeFalse();
        user.MfaTotpSecret.ShouldBeNull();
    }

    [Fact]
    public async Task ResetUser_WhenTotpEnabled_ClearsSecretAndDisablesTotp()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        var user = new FakeTotpUser
        {
            UserId = userId,
            IsMfaTotpEnabled = true,
            MfaTotpSecret = "JBSWY3DPEHPK3PXP"
        };

        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var func = callInfo.Arg<Func<IAuthenticationUser, bool>>();
                var saved = func(user);
                saved.ShouldBeTrue();
                return Task.FromResult<IAuthenticationUser?>(user);
            });

        var service = new MetalGuardianTotpMfaService("TestIssuer", null, authRepo, authSvc, null);

        // Act
        await service.ResetUser(userId, CancellationToken.None);

        // Assert
        user.IsMfaTotpEnabled.ShouldBeFalse();
        user.MfaTotpSecret.ShouldBeNull();
    }

    [Fact]
    public async Task ResetUser_WhenSecretExistsButTotpNotEnabled_ClearsSecret()
    {
        // Arrange
        var authRepo = Substitute.For<IAuthenticationRepository>();
        var authSvc = Substitute.For<IMetalGuardianAuthenticationService>();
        var userId = Guid.NewGuid();
        var user = new FakeTotpUser
        {
            UserId = userId,
            IsMfaTotpEnabled = false,
            MfaTotpSecret = "JBSWY3DPEHPK3PXP"
        };

        authRepo.UpdateUser(userId, Arg.Any<Func<IAuthenticationUser, bool>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var func = callInfo.Arg<Func<IAuthenticationUser, bool>>();
                var saved = func(user);
                saved.ShouldBeTrue();
                return Task.FromResult<IAuthenticationUser?>(user);
            });

        var service = new MetalGuardianTotpMfaService("TestIssuer", null, authRepo, authSvc, null);

        // Act
        await service.ResetUser(userId, CancellationToken.None);

        // Assert
        user.IsMfaTotpEnabled.ShouldBeFalse();
        user.MfaTotpSecret.ShouldBeNull();
    }

    private class FakeTotpUser : ITotpMfaAuthenticationUser
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "TestUser";
        public bool IsDisabled { get; set; }
        public string PasswordSalt { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? MfaTotpSecret { get; set; }
        public bool IsMfaTotpEnabled { get; set; }
        public bool IsMfaTotpRequired { get; set; }
    }
}
