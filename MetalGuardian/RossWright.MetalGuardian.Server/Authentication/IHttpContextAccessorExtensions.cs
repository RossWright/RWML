using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RossWright.MetalGuardian;

public static class IHttpContextAccessorExtensions
{
    public static Guid? GetUserId(this IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.GetGuidClaim(ClaimTypes.NameIdentifier);
    public static string? GetUserName(this IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.GetClaim(ClaimTypes.Name);
    public static bool HasRole(this IHttpContextAccessor httpContextAccessor, string role) =>
        httpContextAccessor.HttpContext?.User.Claims
            .Any(_ => _.Type == ClaimTypes.Role && _.Value == role) ?? false;

    public static string? GetClaim(this IHttpContextAccessor httpContextAccessor, string claimName)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var claims = user?.Claims.ToArray();
        return claims
            ?.FirstOrDefault(_ => _.Type == claimName)
            ?.Value;
    }

    public static string?[]? GetClaimValues(this IHttpContextAccessor httpContextAccessor, string claimName) =>
        httpContextAccessor
            .HttpContext?
            .User?
            .Claims
            .Where(_ => _.Type == claimName)
            .Select(_ => _.Value)
            .ToArray();

    public static Guid? GetGuidClaim(this IHttpContextAccessor httpContextAccessor, string claimName)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var claims = user?.Claims.ToArray();
        var value = claims?.FirstOrDefault(_ => _.Type == claimName)?.Value;
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    public static Guid?[]? GetGuidClaims(this IHttpContextAccessor httpContextAccessor, string claimName) =>
        httpContextAccessor
            .HttpContext?
            .User?
            .Claims
            .Where(_ => _.Type == claimName)
            .Select(_ => Guid.TryParse(_.Value, out var guid) ? guid : (Guid?)null)
            .ToArray();
}
