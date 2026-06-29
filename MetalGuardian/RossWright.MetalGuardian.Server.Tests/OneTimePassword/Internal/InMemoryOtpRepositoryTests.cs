using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using RossWright.MetalGuardian.OneTimePassword;
using RossWright.MetalGuardian.Server.OneTimePassword;
using Shouldly;
using System.Text;
using System.Text.Json;

namespace RossWright.MetalGuardian.Server.Tests.OneTimePassword.Internal;

public class InMemoryOtpRepositoryTests
{
    [Fact]
    public async Task CreateOtp_GeneratesOtpOfSpecifiedLength()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 8, expirationMinutes: 15);

        var otp = await repository.CreateOtp("user@example.com");

        otp.Length.ShouldBe(8);
        otp.ShouldMatch(@"^\d+$");
    }

    [Fact]
    public async Task CreateOtp_StoresOtpInCacheWithCorrectKey()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        var otp = await repository.CreateOtp("TestUser");

        await cache.Received(1).SetAsync(
            $"otp:{otp}",
            Arg.Any<byte[]>(),
            Arg.Is<DistributedCacheEntryOptions>(opts => 
                opts.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(15)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOtp_StoresUserIdentifierAsLowercase()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        await repository.CreateOtp("USER@EXAMPLE.COM");

        await cache.Received(1).SetAsync(
            Arg.Any<string>(),
            Arg.Is<byte[]>(payload => Encoding.UTF8.GetString(payload).Contains("user@example.com")),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOtp_AvoidsCollisions_RetriesUntilUniqueOtpFound()
    {
        var cache = Substitute.For<IDistributedCache>();
        var callCount = 0;
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                callCount++;
                return callCount <= 2 ? Encoding.UTF8.GetBytes("existing") : null;
            });
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        var otp = await repository.CreateOtp("user@example.com");

        otp.ShouldNotBeNullOrEmpty();
        await cache.Received(3).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyOtp_ReturnsValid_WhenOtpExistsAndUserIdentifierMatches()
    {
        var cache = Substitute.For<IDistributedCache>();
        var userIdentifier = "user@example.com";
        var otp = "123456";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userIdentifier));
        cache.GetAsync($"otp:{otp}", Arg.Any<CancellationToken>()).Returns(payload);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        var result = await repository.VerifyOtp(userIdentifier, otp);

        result.ShouldBe(OtpVerifyResult.Valid);
    }

    [Fact]
    public async Task VerifyOtp_ReturnsNotFound_WhenOtpDoesNotExist()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        var result = await repository.VerifyOtp("user@example.com", "999999");

        result.ShouldBe(OtpVerifyResult.NotFound);
    }

    [Fact]
    public async Task VerifyOtp_ReturnsWrongUserId_WhenOtpExistsButUserIdentifierDoesNotMatch()
    {
        var cache = Substitute.For<IDistributedCache>();
        var otp = "123456";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize("user1@example.com"));
        cache.GetAsync($"otp:{otp}", Arg.Any<CancellationToken>()).Returns(payload);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        var result = await repository.VerifyOtp("user2@example.com", otp);

        result.ShouldBe(OtpVerifyResult.WrongUserId);
    }

    [Fact]
    public async Task VerifyOtp_IsCaseInsensitive_ForUserIdentifier()
    {
        var cache = Substitute.For<IDistributedCache>();
        var otp = "123456";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize("user@example.com"));
        cache.GetAsync($"otp:{otp}", Arg.Any<CancellationToken>()).Returns(payload);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        var result = await repository.VerifyOtp("USER@EXAMPLE.COM", otp);

        result.ShouldBe(OtpVerifyResult.Valid);
    }

    [Fact]
    public async Task VerifyOtp_PassesCancellationTokenToCache()
    {
        var cache = Substitute.For<IDistributedCache>();
        var otp = "123456";
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);
        var cts = new CancellationTokenSource();

        await repository.VerifyOtp("user@example.com", otp, cts.Token);

        await cache.Received(1).GetAsync($"otp:{otp}", cts.Token);
    }

    [Fact]
    public async Task RemoveOtp_RemovesOtpFromCache_WhenUserIdentifierMatches()
    {
        var cache = Substitute.For<IDistributedCache>();
        var userIdentifier = "user@example.com";
        var otp = "123456";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userIdentifier));
        cache.GetAsync($"otp:{otp}", Arg.Any<CancellationToken>()).Returns(payload);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        await repository.RemoveOtp(userIdentifier, otp);

        await cache.Received(1).RemoveAsync($"otp:{otp}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveOtp_DoesNotRemoveOtp_WhenUserIdentifierDoesNotMatch()
    {
        var cache = Substitute.For<IDistributedCache>();
        var otp = "123456";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize("user1@example.com"));
        cache.GetAsync($"otp:{otp}", Arg.Any<CancellationToken>()).Returns(payload);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        await repository.RemoveOtp("user2@example.com", otp);

        await cache.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveOtp_DoesNothing_WhenOtpDoesNotExist()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        await repository.RemoveOtp("user@example.com", "999999");

        await cache.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveOtp_IsCaseInsensitive_ForUserIdentifier()
    {
        var cache = Substitute.For<IDistributedCache>();
        var otp = "123456";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize("user@example.com"));
        cache.GetAsync($"otp:{otp}", Arg.Any<CancellationToken>()).Returns(payload);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);

        await repository.RemoveOtp("USER@EXAMPLE.COM", otp);

        await cache.Received(1).RemoveAsync($"otp:{otp}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveOtp_PassesCancellationTokenToCache()
    {
        var cache = Substitute.For<IDistributedCache>();
        var userIdentifier = "user@example.com";
        var otp = "123456";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userIdentifier));
        cache.GetAsync($"otp:{otp}", Arg.Any<CancellationToken>()).Returns(payload);
        var repository = new DistributedCacheOtpRepository(cache, otpLength: 6, expirationMinutes: 15);
        var cts = new CancellationTokenSource();

        await repository.RemoveOtp(userIdentifier, otp, cts.Token);

        await cache.Received(1).GetAsync($"otp:{otp}", cts.Token);
        await cache.Received(1).RemoveAsync($"otp:{otp}", cts.Token);
    }
}
