using Microsoft.Extensions.Caching.Distributed;
using RossWright.MetalGuardian.Server.OneTimePassword;
using System.Text.Json;

namespace RossWright.MetalGuardian.OneTimePassword;

internal interface IOtpRepository
{
    Task<string> CreateOtp(string userIdentifier);
    Task<OtpVerifyResult> VerifyOtp(string userIdentifier, string otp, CancellationToken cancellationToken = default);
    Task RemoveOtp(string userIdentifier, string otp, CancellationToken cancellationToken = default);
}

internal class DistributedCacheOtpRepository : IOtpRepository
{
    public DistributedCacheOtpRepository(IDistributedCache cache, int otpLength = 6, int expirationMinutes = 15) =>
        (_cache, _otpLength, _expirationMinutes) = (cache, otpLength, expirationMinutes);
    private readonly IDistributedCache _cache;
    private readonly int _otpLength;
    private readonly int _expirationMinutes;

    // Cache key: "otp:{otp}" → JSON payload of { userIdentifier }
    private static string CacheKey(string otp) => $"otp:{otp}";

    public async Task<string> CreateOtp(string userIdentifier)
    {
        // Try until we find a key not already in the cache (collision avoidance)
        string otp;
        do
        {
            otp = SecurityTools.RandomNumber(_otpLength);
        } while (await _cache.GetStringAsync(CacheKey(otp)) != null);

        var payload = JsonSerializer.Serialize(userIdentifier.ToLower());
        await _cache.SetStringAsync(CacheKey(otp), payload, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_expirationMinutes)
        });
        return otp;
    }

    public async Task<OtpVerifyResult> VerifyOtp(string userIdentifier, string otp, CancellationToken cancellationToken = default)
    {
        var raw = await _cache.GetStringAsync(CacheKey(otp), cancellationToken);
        if (raw == null) return OtpVerifyResult.NotFound;
        var storedIdentifier = JsonSerializer.Deserialize<string>(raw);
        if (storedIdentifier != userIdentifier.ToLower()) return OtpVerifyResult.WrongUserId;
        // TTL is managed by the cache; if the entry exists it is not expired
        return OtpVerifyResult.Valid;
    }

    public async Task RemoveOtp(string userIdentifier, string otp, CancellationToken cancellationToken = default)
    {
        var raw = await _cache.GetStringAsync(CacheKey(otp), cancellationToken);
        if (raw == null) return;
        var storedIdentifier = JsonSerializer.Deserialize<string>(raw);
        if (storedIdentifier == userIdentifier.ToLower())
            await _cache.RemoveAsync(CacheKey(otp), cancellationToken);
    }
}
