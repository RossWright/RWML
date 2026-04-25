namespace RossWright.MetalGuardian;

public interface IMetalGuardianTotpMfaService
{
    Task<string> GetSetupQrCode(Guid userId, CancellationToken cancellationToken);
    Task<AuthenticationTokens?> VerifyCode(Guid userId, string code, string? deviceFingerprint, CancellationToken cancellationToken);
    Task ResetUser(Guid userId, CancellationToken cancellationToken);
}
