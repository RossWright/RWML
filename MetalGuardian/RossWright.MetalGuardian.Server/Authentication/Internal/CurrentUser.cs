using Microsoft.AspNetCore.Http;

namespace RossWright.MetalGuardian.Authentication;

internal class CurrentUser(IHttpContextAccessor _httpCtx) : ICurrentUser
{
    public Guid UserId => _httpCtx.GetUserId() ?? Guid.Empty;
    public string UserName => _httpCtx.GetUserName()!;
    public string? GetClaim(string claimName) => _httpCtx.GetClaim(claimName);
    public string?[]? GetClaimValues(string claimName) => _httpCtx.GetClaimValues(claimName);
    public Guid? GetGuidClaim(string claimName) => _httpCtx.GetGuidClaim(claimName);
    public Guid?[]? GetGuidClaims(string claimName) => _httpCtx.GetGuidClaims(claimName);
    public bool HasRole(string role) => _httpCtx.HasRole(role);
}