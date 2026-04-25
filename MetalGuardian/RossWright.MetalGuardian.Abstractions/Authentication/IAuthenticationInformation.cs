using System.Security.Claims;

namespace RossWright.MetalGuardian;

public interface IAuthenticationInformation
{
    string Token { get; }
    DateTimeOffset ExpiresOn { get; }
    Guid UserId { get; }
    public string? UserName { get; }
    bool IsProvisional { get; }
    bool? IsKnownDevice { get; }
    string? GetAdditionalClaim(string claimType);
    ClaimsIdentity AsClaimsIdentity();
}