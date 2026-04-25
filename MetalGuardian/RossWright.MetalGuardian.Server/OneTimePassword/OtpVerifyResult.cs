namespace RossWright.MetalGuardian.Server.OneTimePassword;

public enum OtpVerifyResult
{
    Valid,
    NotFound,
    WrongUserId,
    Expired
}
