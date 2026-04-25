namespace RossWright.MetalGuardian.Authorization;

public interface IHierarchialAuthorizationRepository<TPrivilege, TRole>  // Role + Permission on Entities
    : IEntityAuthorizationRepository<TPrivilege, TRole>
{
    /// <summary>
    /// Returns the ids of the ancestors (parent, grandparent, etc.) in order of distance.
    /// i.e. the last element of the returned array is the entity's parent, 
    /// the next to last element is the parent's parent, etc.
    /// and the first element of the array is the root of the entity hierarchy
    /// </summary>
    Task<Guid[]> GetAncestry(Guid securedEntityId);
}