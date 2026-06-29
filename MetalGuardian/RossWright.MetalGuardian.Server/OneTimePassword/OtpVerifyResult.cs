namespace RossWright.MetalGuardian.Server.OneTimePassword;

/// <summary>
/// Represents the outcome of an OTP verification attempt via
/// <see cref="RossWright.MetalGuardian.IOtpService.VerifyOtp"/>.
/// </summary>
public enum OtpVerifyResult
{
    /// <summary>The supplied code matched the stored OTP for the given user identifier.</summary>
    Valid,

    /// <summary>No active OTP was found for the given user identifier.</summary>
    NotFound,

    /// <summary>
    /// An OTP was found but it was issued for a different user identifier than supplied.
    /// </summary>
    WrongUserId,

    /// <summary>An OTP was found but its expiry time has passed.</summary>
    Expired
}
