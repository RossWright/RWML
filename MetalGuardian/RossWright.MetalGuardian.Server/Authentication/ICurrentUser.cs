namespace RossWright.MetalGuardian;

/// <summary>
/// Provides access to the identity and claims of the currently authenticated user for
/// the current HTTP request. Resolved from the request's JWT bearer token.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Returns <c>true</c> when <see cref="UserId"/> is not <see cref="Guid.Empty"/>;
    /// <c>false</c> for unauthenticated requests.
    /// </summary>
    bool IsAuthenticated => UserId != Guid.Empty;

    /// <summary>
    /// The unique identifier of the authenticated user, taken from the
    /// <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/> claim.
    /// Returns <see cref="Guid.Empty"/> when not authenticated.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// The display name of the authenticated user, taken from the
    /// <see cref="System.Security.Claims.ClaimTypes.Name"/> claim.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// Returns the value of the first claim with the specified name, or <c>null</c>
    /// if no such claim is present.
    /// </summary>
    string? GetClaim(string claimName);

    /// <summary>
    /// Returns all values for claims with the specified name, or <c>null</c> if the
    /// claim is not present.
    /// </summary>
    string?[]? GetClaimValues(string claimName);

    /// <summary>
    /// Returns the value of the first claim with the specified name parsed as a
    /// <see cref="Guid"/>, or <c>null</c> if the claim is absent or not a valid GUID.
    /// </summary>
    Guid? GetGuidClaim(string claimName);

    /// <summary>
    /// Returns all values for claims with the specified name parsed as <see cref="Guid"/>s.
    /// Non-parseable values are returned as <c>null</c> entries. Returns <c>null</c> if the
    /// claim is not present.
    /// </summary>
    Guid?[]? GetGuidClaims(string claimName);

    /// <summary>
    /// Returns <c>true</c> if the authenticated user holds the specified role claim.
    /// </summary>
    bool HasRole(string role);
}
