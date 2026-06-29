using RossWright.MetalGuardian.Server.Tests.Fakes;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests.MetalGuardianTotpMfa;

public class TotpMfaServiceTests
{
    private static (IMetalGuardianTotpMfaService service, FakeTotpAuthRepo repo, FakeTotpDeviceRepo deviceRepo, FakeTotpAuthService authSvc)
        BuildService(bool withDeviceRepo = false, string issuer = "TestIssuer")
    {
        var repo = new FakeTotpAuthRepo();
        var authSvc = new FakeTotpAuthService();
        var deviceRepo = new FakeTotpDeviceRepo();
        var service = new MetalGuardianTotpMfaService(
            issuer,
            30,
            repo,
            authSvc,
            withDeviceRepo ? deviceRepo : null);
        return (service, repo, deviceRepo, authSvc);
    }

    // --- GetSetupQrCode ---

    [Fact]
    public async Task GetSetupQrCode_NewUser_SavesSecretAndReturnsQrCode()
    {
        var (service, repo, _, _) = BuildService();
        var userId = repo.AddUser();

        var result = await service.GetSetupQrCode(userId, CancellationToken.None);

        result.ShouldStartWith("data:image/png;base64,");
        repo.GetUser(userId).MfaTotpSecret.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetSetupQrCode_SecretAlreadyExists_ReturnsQrCodeIdempotently()
    {
        var (service, repo, _, _) = BuildService();
        var userId = repo.AddUser();

        var first = await service.GetSetupQrCode(userId, CancellationToken.None);
        var secret = repo.GetUser(userId).MfaTotpSecret;

        // Second call: secret already exists so same secret kept, QR re-generated
        var second = await service.GetSetupQrCode(userId, CancellationToken.None);
        repo.GetUser(userId).MfaTotpSecret.ShouldBe(secret);
        second.ShouldStartWith("data:image/png;base64,");
    }

    [Fact]
    public async Task GetSetupQrCode_AlreadyEnabled_ThrowsMetalGuardianException()
    {
        var (service, repo, _, _) = BuildService();
        var userId = repo.AddUser(totpEnabled: true, totpSecret: "JBSWY3DPEHPK3PXP");

        await Should.ThrowAsync<MetalGuardianException>(
            () => service.GetSetupQrCode(userId, CancellationToken.None));
    }

    [Fact]
    public async Task GetSetupQrCode_UnknownUser_ThrowsMetalGuardianException()
    {
        var (service, _, _, _) = BuildService();

        await Should.ThrowAsync<MetalGuardianException>(
            () => service.GetSetupQrCode(Guid.NewGuid(), CancellationToken.None));
    }

    // --- VerifyCode ---

    [Fact]
    public async Task VerifyCode_CorrectTotpFirstSetup_ReturnTokenAndEnablesTotp()
    {
        var (service, repo, _, authSvc) = BuildService();
        var userId = repo.AddUser(totpSecret: GenerateSecret());

        var tokens = await service.VerifyCode(userId, GenerateValidCode(repo.GetUser(userId).MfaTotpSecret!), null, CancellationToken.None);

        tokens.ShouldNotBeNull();
        repo.GetUser(userId).IsMfaTotpEnabled.ShouldBeTrue();
        authSvc.LoginCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyCode_CorrectTotpAlreadyEnabled_ReturnsToken()
    {
        var secret = GenerateSecret();
        var (service, repo, _, authSvc) = BuildService();
        var userId = repo.AddUser(totpEnabled: true, totpSecret: secret);

        var tokens = await service.VerifyCode(userId, GenerateValidCode(secret), null, CancellationToken.None);

        tokens.ShouldNotBeNull();
        authSvc.LoginCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyCode_IncorrectTotp_ReturnsNull()
    {
        var (service, repo, _, _) = BuildService();
        var userId = repo.AddUser(totpSecret: GenerateSecret());

        var tokens = await service.VerifyCode(userId, "000000", null, CancellationToken.None);

        tokens.ShouldBeNull();
    }

    [Fact]
    public async Task VerifyCode_UnknownUser_ThrowsMetalGuardianException()
    {
        var (service, _, _, _) = BuildService();

        await Should.ThrowAsync<MetalGuardianException>(
            () => service.VerifyCode(Guid.NewGuid(), "000000", null, CancellationToken.None));
    }

    [Fact]
    public async Task VerifyCode_NoSecretSet_ReturnsNull()
    {
        var (service, repo, _, _) = BuildService();
        var userId = repo.AddUser(); // no secret

        var tokens = await service.VerifyCode(userId, "000000", null, CancellationToken.None);

        tokens.ShouldBeNull();
    }

    [Fact]
    public async Task VerifyCode_DeviceFingerprint_NoDeviceRepo_TokenStillReturned()
    {
        var secret = GenerateSecret();
        var (service, repo, _, authSvc) = BuildService(withDeviceRepo: false);
        var userId = repo.AddUser(totpEnabled: true, totpSecret: secret);

        var tokens = await service.VerifyCode(userId, GenerateValidCode(secret), "fp-abc", CancellationToken.None);

        tokens.ShouldNotBeNull();
        authSvc.LoginCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyCode_DeviceFingerprint_DeviceRepo_NewDevice_AddsDevice()
    {
        var secret = GenerateSecret();
        var (service, repo, deviceRepo, _) = BuildService(withDeviceRepo: true);
        var userId = repo.AddUser(totpEnabled: true, totpSecret: secret);

        await service.VerifyCode(userId, GenerateValidCode(secret), "fp-new", CancellationToken.None);

        deviceRepo.AddCalled.ShouldBeTrue();
        deviceRepo.UpdateCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyCode_DeviceFingerprint_DeviceRepo_KnownDevice_UpdatesDevice()
    {
        var secret = GenerateSecret();
        var (service, repo, deviceRepo, _) = BuildService(withDeviceRepo: true);
        var userId = repo.AddUser(totpEnabled: true, totpSecret: secret);
        deviceRepo.AddExistingDevice(userId, "fp-known");

        await service.VerifyCode(userId, GenerateValidCode(secret), "fp-known", CancellationToken.None);

        deviceRepo.UpdateCalled.ShouldBeTrue();
        deviceRepo.AddCalled.ShouldBeFalse();
    }

    // --- ResetUser ---

    [Fact]
    public async Task ResetUser_ClearsSecretAndDisablesTotp()
    {
        var (service, repo, _, _) = BuildService();
        var userId = repo.AddUser(totpEnabled: true, totpSecret: "JBSWY3DPEHPK3PXP");

        await service.ResetUser(userId, CancellationToken.None);

        var user = repo.GetUser(userId);
        user.IsMfaTotpEnabled.ShouldBeFalse();
        user.MfaTotpSecret.ShouldBeNull();
    }

    // --- Helpers ---

    private static string GenerateSecret()
    {
        var key = OtpNet.KeyGeneration.GenerateRandomKey(20);
        return OtpNet.Base32Encoding.ToString(key);
    }

    private static string GenerateValidCode(string base32Secret)
    {
        var totp = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(base32Secret));
        return totp.ComputeTotp();
    }
}
