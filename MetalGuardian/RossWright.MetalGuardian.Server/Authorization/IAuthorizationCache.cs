namespace RossWright.MetalGuardian.Authorization;

public interface IAuthorizationCache
{
    /// <summary>
    /// Resets caching in the authorization system. Will only partially reset if context if provided
    /// </summary>
    /// <param name="userId">The user affected by the authorization change</param>
    /// <param name="entityId">The entity affected by the authorization change</param>
    void BustCache(Guid? userId = null, Guid? entityId = null);
}
