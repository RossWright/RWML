namespace RossWright.MetalGuardian;

public interface ICurrentUser
{
    bool IsAuthenticated => UserId != Guid.Empty;

    Guid UserId { get; }
    string UserName { get; }

    string? GetClaim(string claimName);
    string?[]? GetClaimValues(string claimName);
    Guid? GetGuidClaim(string claimName);
    Guid?[]? GetGuidClaims(string claimName);
    bool HasRole(string role);
}
