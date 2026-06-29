namespace RossWright.MetalGuardian;

/// <summary>
/// Server-side service for managing TOTP MFA operations.
/// </summary>
public interface IMetalGuardianTotpMfaService
{
    /// <summary>
    /// Generates a new TOTP secret for the user and returns the QR code data URI for the authenticator app setup.
    /// Throws if TOTP MFA is already enabled for the user.
    /// </summary>
    Task<string> GetSetupQrCode(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies the provided TOTP <paramref name="code"/> for the user.
    /// Returns a new set of full (non-provisional) authentication tokens on success,
    /// or <c>null</c> if the code is incorrect.
    /// Throws if the user is not found.
    /// </summary>
    Task<AuthenticationTokens?> VerifyCode(Guid userId, string code, string? deviceFingerprint, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the TOTP MFA configuration for the specified user, requiring them to re-enroll their authenticator app.
    /// </summary>
    Task ResetUser(Guid userId, CancellationToken cancellationToken);
}
