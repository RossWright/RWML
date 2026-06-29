using NSubstitute;
using RossWright.Messaging;
using RossWright.MetalGuardian.OneTimePassword;
using RossWright.MetalGuardian.Server.OneTimePassword;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests.OneTimePassword;

public class OtpServiceTests
{
    private static (IOtpService service, FakeOtpRepository repo) BuildService()
    {
        var repo = new FakeOtpRepository();
        var service = new OtpService(repo);
        return (service, repo);
    }

    [Fact]
    public async Task VerifyOtp_ValidCode_ReturnsValid()
    {
        var (service, repo) = BuildService();
        var otp = await repo.CreateOtp("user1");
        var result = await service.VerifyOtp("user1", otp);
        result.ShouldBe(OtpVerifyResult.Valid);
    }

    [Fact]
    public async Task VerifyOtp_UnknownCode_ReturnsNotFound()
    {
        var (service, _) = BuildService();
        var result = await service.VerifyOtp("user1", "000000");
        result.ShouldBe(OtpVerifyResult.NotFound);
    }

    [Fact]
    public async Task VerifyOtp_WrongUserId_ReturnsWrongUserId()
    {
        var (service, repo) = BuildService();
        var otp = await repo.CreateOtp("user1");
        var result = await service.VerifyOtp("user2", otp);
        result.ShouldBe(OtpVerifyResult.WrongUserId);
    }

    [Fact]
    public async Task VerifyOtp_ValidCode_AutoRemovesOtpByDefault()
    {
        var (service, repo) = BuildService();
        var otp = await repo.CreateOtp("user1");
        await service.VerifyOtp("user1", otp);
        repo.Contains("user1", otp).ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyOtp_ValidCode_PreserveOtp_DoesNotRemove()
    {
        var (service, repo) = BuildService();
        var otp = await repo.CreateOtp("user1");
        await service.VerifyOtp("user1", otp, preserveOtp: true);
        repo.Contains("user1", otp).ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyOtp_InvalidCode_DoesNotRemoveOtp()
    {
        var (service, repo) = BuildService();
        var otp = await repo.CreateOtp("user1");
        await service.VerifyOtp("user1", "wrong");
        repo.Contains("user1", otp).ShouldBeTrue();
    }

    [Fact]
    public async Task RemoveOtp_ValidCode_RemovesOtp()
    {
        var (service, repo) = BuildService();
        var otp = await repo.CreateOtp("user1");
        await service.RemoveOtp("user1", otp);
        repo.Contains("user1", otp).ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyOtp_AfterPreservedVerify_CanVerifyAgain()
    {
        var (service, repo) = BuildService();
        var otp = await repo.CreateOtp("user1");
        await service.VerifyOtp("user1", otp, preserveOtp: true);
        var result = await service.VerifyOtp("user1", otp);
        result.ShouldBe(OtpVerifyResult.Valid);
    }

    [Fact]
    public async Task SendOtpViaEmail_NoEmailService_ThrowsNotSupportedException()
    {
        // Arrange
        var repo = new FakeOtpRepository();
        var service = new OtpService(repo, emailService: null, smsService: null);

        // Act & Assert
        await Should.ThrowAsync<NotSupportedException>(async () =>
            await service.SendOtpViaEmail("user1", otp => Substitute.For<IAddressedEmail>()));
    }

    [Fact]
    public async Task SendOtpViaEmail_Success_CreatesOtpAndSendsEmail()
    {
        // Arrange
        var repo = new FakeOtpRepository();
        var emailService = Substitute.For<IEmailService>();
        var service = new OtpService(repo, emailService: emailService, smsService: null);
        IAddressedEmail? capturedEmail = null;
        string? capturedOtp = null;

        // Act
        await service.SendOtpViaEmail("user1", otp =>
        {
            capturedOtp = otp;
            capturedEmail = Substitute.For<IAddressedEmail>();
            return capturedEmail;
        });

        // Assert
        capturedOtp.ShouldNotBeNull();
        repo.Contains("user1", capturedOtp).ShouldBeTrue();
        await emailService.Received(1).Send(capturedEmail!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOtpViaEmail_SendFails_RemovesOtpAndRethrows()
    {
        // Arrange
        var repo = new FakeOtpRepository();
        var emailService = Substitute.For<IEmailService>();
        var service = new OtpService(repo, emailService: emailService, smsService: null);
        var expectedException = new InvalidOperationException("Email send failed");
        string? capturedOtp = null;

        emailService.Send(Arg.Any<IAddressedEmail>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw expectedException);

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await service.SendOtpViaEmail("user1", otp =>
            {
                capturedOtp = otp;
                return Substitute.For<IAddressedEmail>();
            }));

        ex.ShouldBe(expectedException);
        capturedOtp.ShouldNotBeNull();
        repo.Contains("user1", capturedOtp).ShouldBeFalse();
    }

    [Fact]
    public async Task SendOtpViaSms_NoSmsService_ThrowsNotSupportedException()
    {
        // Arrange
        var repo = new FakeOtpRepository();
        var service = new OtpService(repo, emailService: null, smsService: null);

        // Act & Assert
        await Should.ThrowAsync<NotSupportedException>(async () =>
            await service.SendOtpViaSms("user1", otp => Substitute.For<IAddressedSmsMessage>()));
    }

    [Fact]
    public async Task SendOtpViaSms_Success_CreatesOtpAndSendsSms()
    {
        // Arrange
        var repo = new FakeOtpRepository();
        var smsService = Substitute.For<ISmsService>();
        var service = new OtpService(repo, emailService: null, smsService: smsService);
        IAddressedSmsMessage? capturedSms = null;
        string? capturedOtp = null;

        // Act
        await service.SendOtpViaSms("user1", otp =>
        {
            capturedOtp = otp;
            capturedSms = Substitute.For<IAddressedSmsMessage>();
            return capturedSms;
        });

        // Assert
        capturedOtp.ShouldNotBeNull();
        repo.Contains("user1", capturedOtp).ShouldBeTrue();
        await smsService.Received(1).Send(capturedSms!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendOtpViaSms_SendFails_RemovesOtpAndRethrows()
    {
        // Arrange
        var repo = new FakeOtpRepository();
        var smsService = Substitute.For<ISmsService>();
        var service = new OtpService(repo, emailService: null, smsService: smsService);
        var expectedException = new InvalidOperationException("SMS send failed");
        string? capturedOtp = null;

        smsService.Send(Arg.Any<IAddressedSmsMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw expectedException);

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await service.SendOtpViaSms("user1", otp =>
            {
                capturedOtp = otp;
                return Substitute.For<IAddressedSmsMessage>();
            }));

        ex.ShouldBe(expectedException);
        capturedOtp.ShouldNotBeNull();
        repo.Contains("user1", capturedOtp).ShouldBeFalse();
    }
}

internal class FakeOtpRepository : IOtpRepository
{
    private readonly Dictionary<string, string> _store = new();
    private int _nextCode = 100000;

    public Task<string> CreateOtp(string userIdentifier)
    {
        var otp = (_nextCode++).ToString();
        _store[otp] = userIdentifier.ToLower();
        return Task.FromResult(otp);
    }

    public Task<OtpVerifyResult> VerifyOtp(string userIdentifier, string otp, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(otp, out var storedId)) return Task.FromResult(OtpVerifyResult.NotFound);
        if (storedId != userIdentifier.ToLower()) return Task.FromResult(OtpVerifyResult.WrongUserId);
        return Task.FromResult(OtpVerifyResult.Valid);
    }

    public Task RemoveOtp(string userIdentifier, string otp, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(otp, out var storedId) && storedId == userIdentifier.ToLower())
            _store.Remove(otp);
        return Task.CompletedTask;
    }

    public bool Contains(string userIdentifier, string otp) =>
        _store.TryGetValue(otp, out var storedId) && storedId == userIdentifier.ToLower();
}
