namespace RossWright.MetalGuardian;

public interface IUserClaimsProvider
{
    Task<IEnumerable<(string, string)>?> GetClaims(IAuthenticationUser user, CancellationToken cancellationToken);
}
