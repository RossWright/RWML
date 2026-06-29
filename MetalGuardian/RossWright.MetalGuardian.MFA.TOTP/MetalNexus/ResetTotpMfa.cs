using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalGuardian;

/// <summary>
/// MetalNexus request type for resetting a user's TOTP MFA configuration (<c>/Authentication/ResetTotp</c>).
/// Typically called by an administrator to allow the user to re-enroll their authenticator app.
/// </summary>
public static class ResetTotpMfa
{
    /// <summary>Resets the TOTP MFA secret for the specified user, requiring them to go through setup again.</summary>
    [ApiRequest(path: "/Authentication/ResetTotp", tag: "Authentication")] 
    [Authenticated]
    public class Request : IRequest
    {
        /// <summary>The unique identifier of the user whose TOTP MFA should be reset.</summary>
        public Guid UserId { get; init; }
    }
}