using RossWright.Messaging;
using RossWright.MetalGuardian.Server.OneTimePassword;

namespace RossWright.MetalGuardian;

public interface IOtpService
{
    Task SendOtpViaEmail(string userIdentifier, Func<string, IAddressedEmail> makeEmail, CancellationToken cancellationToken = default);
    Task SendOtpViaSms(string userIdentifier, Func<string, IAddressedSmsMessage> makeSms, CancellationToken cancellationToken = default);
    Task<OtpVerifyResult> VerifyOtp(string userIdentifier, string otp);
    Task RemoveOtp(string userIdentifier, string otp);
}
