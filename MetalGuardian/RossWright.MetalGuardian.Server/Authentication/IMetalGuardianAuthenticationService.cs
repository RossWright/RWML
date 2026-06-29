namespace RossWright.MetalGuardian;

/// <summary>
/// Core server-side authentication service. Handles credential-based login, MFA-bypass login,
/// logout, and token refresh operations.
/// </summary>
public interface IMetalGuardianAuthenticationService
{
    /// <summary>
    /// Authenticates a user by identity and password. If any registered
    /// <see cref="IMultifactorAuthenticationProvider"/> returns <c>true</c> for the user,
    /// a provisional (MFA-pending) access token is returned; otherwise a full token is issued.
    /// </summary>
    Task<AuthenticationTokens> Login(string userIdentity, string password, string? deviceFingerprint, CancellationToken cancellationToken);

    /// <summary>
    /// Issues a full (non-provisional) access token for a user the host application
    /// has already authenticated by its own means — for example, after verifying a
    /// TOTP code, an SMS OTP, or confirming an impersonation grant.
    ///
    /// This overload bypasses all <see cref="IMultifactorAuthenticationProvider"/> checks.
    /// The caller is responsible for having already satisfied any required authentication
    /// before calling this method.
    /// </summary>
    Task<AuthenticationTokens> Login(IAuthenticationUser user, CancellationToken cancellationToken);

    /// <summary>
    /// Invalidates the refresh token contained in <paramref name="tokens"/>, preventing
    /// further token refreshes for that session.
    /// </summary>
    Task Logout(AuthenticationTokens tokens, CancellationToken cancellationToken);

    /// <summary>
    /// Exchanges a valid refresh token for a new access/refresh token pair. The previous
    /// refresh token is invalidated and replaced.
    /// </summary>
    Task<AuthenticationTokens> Refresh(AuthenticationTokens tokens, CancellationToken cancellationToken);
}
