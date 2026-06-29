using RossWright.Messaging;
using RossWright.MetalGuardian.Server.OneTimePassword;

namespace RossWright.MetalGuardian;

/// <summary>
/// Generates, delivers, and verifies one-time passwords (OTPs). Register via
/// <see cref="IMetalGuardianServerOptionBuilder.UseOneTimePassword"/>. Requires
/// <c>AddDistributedMemoryCache()</c> (or a distributed cache) to be registered in the
/// service collection.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates an OTP for <paramref name="userIdentifier"/> and sends it as an email
    /// using the message produced by <paramref name="makeEmail"/>, which receives the
    /// generated OTP code as its argument.
    /// </summary>
    Task SendOtpViaEmail(string userIdentifier, Func<string, IAddressedEmail> makeEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an OTP for <paramref name="userIdentifier"/> and sends it as an SMS
    /// using the message produced by <paramref name="makeSms"/>, which receives the
    /// generated OTP code as its argument.
    /// </summary>
    Task SendOtpViaSms(string userIdentifier, Func<string, IAddressedSmsMessage> makeSms, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that <paramref name="otp"/> is the active code for <paramref name="userIdentifier"/>.
    /// Returns an <see cref="OtpVerifyResult"/> indicating the outcome.
    /// By default (<paramref name="preserveOtp"/> = <c>false</c>) a valid code is consumed
    /// and removed; pass <c>true</c> to keep the code in place for subsequent verification
    /// calls (useful in multi-step flows where the code must be verified before being used).
    /// </summary>
    Task<OtpVerifyResult> VerifyOtp(string userIdentifier, string otp,
        bool preserveOtp = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly removes the OTP for <paramref name="userIdentifier"/> without verifying it.
    /// Use after a successful multi-step flow where <c>preserveOtp: true</c> was used during
    /// intermediate verification.
    /// </summary>
    Task RemoveOtp(string userIdentifier, string otp,
        CancellationToken cancellationToken = default);
}
