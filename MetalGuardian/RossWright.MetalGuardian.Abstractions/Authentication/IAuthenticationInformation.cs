using System.Security.Claims;

namespace RossWright.MetalGuardian;

/// <summary>
/// Represents the authentication state for a successfully authenticated user,
/// decoded from the current access token.
/// </summary>
public interface IAuthenticationInformation
{
    /// <summary>The raw JWT access token string.</summary>
    string Token { get; }

    /// <summary>The UTC date and time at which the access token expires.</summary>
    DateTimeOffset ExpiresOn { get; }

    /// <summary>The unique identifier of the authenticated user.</summary>
    Guid UserId { get; }

    /// <summary>The username of the authenticated user, if present in the token.</summary>
    public string? UserName { get; }

    /// <summary>
    /// Indicates that the login is provisional, meaning MFA verification is still pending.
    /// A provisional token grants access only to MFA-completion endpoints.
    /// </summary>
    bool IsProvisional { get; }

    /// <summary>
    /// Indicates whether the current device is recognized as a trusted device.
    /// <c>null</c> means device fingerprinting is not enabled or no fingerprint was provided;
    /// <c>true</c> means the device is known and trusted;
    /// <c>false</c> means the device is unrecognized.
    /// </summary>
    bool? IsKnownDevice { get; }

    /// <summary>Returns the value of an additional claim by its claim type URI, or <c>null</c> if not present.</summary>
    string? GetAdditionalClaim(string claimType);

    /// <summary>Returns a <see cref="ClaimsIdentity"/> populated from the claims in the access token.</summary>
    ClaimsIdentity AsClaimsIdentity();
}