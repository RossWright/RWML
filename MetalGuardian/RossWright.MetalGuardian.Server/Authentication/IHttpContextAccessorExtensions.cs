using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RossWright.MetalGuardian;

/// <summary>
/// Convenience extensions on <see cref="IHttpContextAccessor"/> for reading common
/// MetalGuardian identity values from the current request's JWT claims.
/// </summary>
public static class IHttpContextAccessorExtensions
{
    /// <summary>
    /// Returns the authenticated user's ID from the
    /// <see cref="ClaimTypes.NameIdentifier"/> claim, or <c>null</c> if not present or
    /// not a valid GUID.
    /// </summary>
    public static Guid? GetUserId(this IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.GetGuidClaim(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Returns the authenticated user's display name from the
    /// <see cref="ClaimTypes.Name"/> claim, or <c>null</c> if not present.
    /// </summary>
    public static string? GetUserName(this IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.GetClaim(ClaimTypes.Name);

    /// <summary>
    /// Returns <c>true</c> if the authenticated user holds the specified
    /// <see cref="ClaimTypes.Role"/> claim value.
    /// </summary>
    public static bool HasRole(this IHttpContextAccessor httpContextAccessor, string role) =>
        httpContextAccessor.HttpContext?.User?.Claims
            .Any(_ => _.Type == ClaimTypes.Role && _.Value == role) ?? false;

    /// <summary>
    /// Returns the value of the first claim matching <paramref name="claimName"/>,
    /// or <c>null</c> if not present.
    /// </summary>
    public static string? GetClaim(this IHttpContextAccessor httpContextAccessor, string claimName)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var claims = user?.Claims.ToArray();
        return claims
            ?.FirstOrDefault(_ => _.Type == claimName)
            ?.Value;
    }

    /// <summary>
    /// Returns all values for claims matching <paramref name="claimName"/>,
    /// or <c>null</c> if there is no HTTP context.
    /// </summary>
    public static string?[]? GetClaimValues(this IHttpContextAccessor httpContextAccessor, string claimName) =>
        httpContextAccessor
            .HttpContext?
            .User?
            .Claims
            .Where(_ => _.Type == claimName)
            .Select(_ => _.Value)
            .ToArray();

    /// <summary>
    /// Returns the value of the first claim matching <paramref name="claimName"/> parsed
    /// as a <see cref="Guid"/>, or <c>null</c> if the claim is absent or not a valid GUID.
    /// </summary>
    public static Guid? GetGuidClaim(this IHttpContextAccessor httpContextAccessor, string claimName)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var claims = user?.Claims.ToArray();
        var value = claims?.FirstOrDefault(_ => _.Type == claimName)?.Value;
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    /// <summary>
    /// Returns all values for claims matching <paramref name="claimName"/> parsed as
    /// <see cref="Guid"/>s. Non-parseable values are returned as <c>null</c> entries.
    /// Returns <c>null</c> if there is no HTTP context.
    /// </summary>
    public static Guid?[]? GetGuidClaims(this IHttpContextAccessor httpContextAccessor, string claimName) =>
        httpContextAccessor
            .HttpContext?
            .User?
            .Claims
            .Where(_ => _.Type == claimName)
            .Select(_ => Guid.TryParse(_.Value, out var guid) ? guid : (Guid?)null)
            .ToArray();
}
