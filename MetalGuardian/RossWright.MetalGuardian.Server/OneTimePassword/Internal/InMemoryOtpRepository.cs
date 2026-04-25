using RossWright.MetalGuardian.Server.OneTimePassword;

namespace RossWright.MetalGuardian.OneTimePassword;

internal interface IOtpRepository
{
    Task<string> CreateOtp(string userIdentifier);
    Task<OtpVerifyResult> VerifyOpt(string userIdentifier, string otp);
    Task RemoveOtp(string userIdentifier, string otp);
}

internal class InMemoryOtpRepository : IOtpRepository
{
    public InMemoryOtpRepository(int otpLength = 6, int expirationMinutes = 15) =>
        (_otpLength, _expirationMinutes) = (otpLength, expirationMinutes);
    private readonly int _otpLength;
    private readonly int _expirationMinutes;
    private readonly Dictionary<string, OtpInfo> _otps = new();

    private sealed class OtpInfo
    {
        public string UserIdentifier { get; init; } = null!;
        public DateTime ExpiresAt { get; init; }
    }

    public Task<string> CreateOtp(string userIdentifier)
    {
        _otps.RemoveWhere((k,v) => v.ExpiresAt <= DateTime.UtcNow);

        string otp;
        do
        {
            otp = SecurityTools.RandomNumber(_otpLength);
        } while (_otps.ContainsKey(otp));
        _otps.Add(otp, new OtpInfo
        {
            UserIdentifier = userIdentifier.ToLower(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes)
        });
        return Task.FromResult(otp);
    }

    public Task<OtpVerifyResult> VerifyOpt(string userIdentifier, string otp)
    {
        OtpVerifyResult result = OtpVerifyResult.Valid;
        if (!_otps.TryGetValue(otp, out var otpInfo))
            result = OtpVerifyResult.NotFound;
        else if (otpInfo.UserIdentifier != userIdentifier.ToLower())
            result = OtpVerifyResult.WrongUserId;
        else if (otpInfo.ExpiresAt <= DateTime.UtcNow)
            result = OtpVerifyResult.Expired;
        return Task.FromResult(result);
    }

    public Task RemoveOtp(string userIdentifier, string otp)
    {
        _otps.RemoveWhere((k, v) => v.ExpiresAt <= DateTime.UtcNow || 
            (k == otp && v.UserIdentifier == userIdentifier));
        return Task.CompletedTask;
    }
}
