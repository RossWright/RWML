namespace RossWright.MetalGuardian;

/// <summary>
/// Supplies additional claims to include in issued JWTs for a given user.
/// Multiple implementations may be registered; all are called during token generation
/// and their results are merged. Register via
/// <see cref="IMetalGuardianServerOptionBuilder.UseUserClaimsProvider{TUserClaimsProvider}"/>.
/// </summary>
public interface IUserClaimsProvider
{
    /// <summary>
    /// Returns a collection of (claimType, value) pairs to add to the token for
    /// <paramref name="user"/>, or <c>null</c> if this provider has no claims to contribute.
    /// </summary>
    Task<IEnumerable<(string, string)>?> GetClaims(IAuthenticationUser user, CancellationToken cancellationToken);
}
