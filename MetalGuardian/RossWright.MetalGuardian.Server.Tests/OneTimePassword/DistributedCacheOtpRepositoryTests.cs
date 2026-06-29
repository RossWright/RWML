using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalGuardian.OneTimePassword;
using RossWright.MetalGuardian.Server.OneTimePassword;
using Shouldly;

namespace RossWright.MetalGuardian.Server.Tests.OneTimePassword;

public class DistributedCacheOtpRepositoryTests
{
    private static IOtpRepository BuildRepository(int otpLength = 6, int expirationMinutes = 15)
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        var sp = services.BuildServiceProvider();
        return new DistributedCacheOtpRepository(sp.GetRequiredService<IDistributedCache>(), otpLength, expirationMinutes);
    }

    [Fact]
    public async Task CreateOtp_ReturnsCodeOfConfiguredLength()
    {
        var repo = BuildRepository(otpLength: 6);
        var otp = await repo.CreateOtp("user1");
        otp.Length.ShouldBe(6);
    }

    [Fact]
    public async Task VerifyOtp_ValidCode_ReturnsValid()
    {
        var repo = BuildRepository();
        var otp = await repo.CreateOtp("user1");
        var result = await repo.VerifyOtp("user1", otp);
        result.ShouldBe(OtpVerifyResult.Valid);
    }

    [Fact]
    public async Task VerifyOtp_UnknownCode_ReturnsNotFound()
    {
        var repo = BuildRepository();
        var result = await repo.VerifyOtp("user1", "999999");
        result.ShouldBe(OtpVerifyResult.NotFound);
    }

    [Fact]
    public async Task VerifyOtp_WrongUserId_ReturnsWrongUserId()
    {
        var repo = BuildRepository();
        var otp = await repo.CreateOtp("user1");
        var result = await repo.VerifyOtp("user2", otp);
        result.ShouldBe(OtpVerifyResult.WrongUserId);
    }

    [Fact]
    public async Task VerifyOtp_IsCaseInsensitiveForUserId()
    {
        var repo = BuildRepository();
        var otp = await repo.CreateOtp("User1");
        var result = await repo.VerifyOtp("user1", otp);
        result.ShouldBe(OtpVerifyResult.Valid);
    }

    [Fact]
    public async Task RemoveOtp_ValidCode_SubsequentVerifyReturnsNotFound()
    {
        var repo = BuildRepository();
        var otp = await repo.CreateOtp("user1");
        await repo.RemoveOtp("user1", otp);
        var result = await repo.VerifyOtp("user1", otp);
        result.ShouldBe(OtpVerifyResult.NotFound);
    }

    [Fact]
    public async Task RemoveOtp_WrongUserId_DoesNotRemove()
    {
        var repo = BuildRepository();
        var otp = await repo.CreateOtp("user1");
        await repo.RemoveOtp("user2", otp);
        var result = await repo.VerifyOtp("user1", otp);
        result.ShouldBe(OtpVerifyResult.Valid);
    }

    [Fact]
    public async Task CreateOtp_MultipleUsers_IndependentCodes()
    {
        var repo = BuildRepository();
        var otp1 = await repo.CreateOtp("user1");
        var otp2 = await repo.CreateOtp("user2");
        (await repo.VerifyOtp("user1", otp1)).ShouldBe(OtpVerifyResult.Valid);
        (await repo.VerifyOtp("user2", otp2)).ShouldBe(OtpVerifyResult.Valid);
    }
}
