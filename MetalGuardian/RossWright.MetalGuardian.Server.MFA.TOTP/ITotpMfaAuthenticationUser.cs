namespace RossWright.MetalGuardian;

/// <summary>
/// Extends <see cref="IAuthenticationUser"/> with the TOTP MFA state properties
/// that the host application's user entity must expose.
/// </summary>
public interface ITotpMfaAuthenticationUser : IAuthenticationUser
{
    /// <summary>The Base32-encoded TOTP secret key for this user, or <c>null</c> if TOTP has not been set up.</summary>
    public string? MfaTotpSecret { get; set; }

    /// <summary>Indicates whether TOTP MFA is currently active for this user.</summary>
    public bool IsMfaTotpEnabled { get; set; }

    /// <summary>Indicates whether TOTP MFA is required for this user and cannot be bypassed.</summary>
    bool IsMfaTotpRequired { get; }
}