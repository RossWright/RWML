namespace RossWright.MetalGuardian;

/// <summary>
/// Determines whether a user should receive a provisional access token at login,
/// requiring further authentication before a full token is issued.
///
/// Implement this interface to integrate any MFA scheme into MetalGuardian's login flow.
/// Multiple implementations may be registered; a provisional token is issued if any
/// provider returns <c>true</c>.
///
/// Once the host application has completed MFA verification, issue a full token by
/// calling <see cref="IMetalGuardianAuthenticationService.Login(IAuthenticationUser, CancellationToken)"/>.
/// </summary>
public interface IMultifactorAuthenticationProvider
{
    /// <summary>
    /// Returns whether the user should receive a provisional token and complete MFA before
    /// receiving a full access token.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="isKnownDevice">
    /// Whether the login came from a known device, or <c>null</c> when device tracking is unavailable.
    /// </param>
    /// <returns><c>true</c> to require MFA; otherwise <c>false</c>.</returns>
    bool ShouldLoginAsProvisional(IAuthenticationUser user, bool? isKnownDevice);
}
