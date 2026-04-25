namespace RossWright;

/// <summary>Marks an entity as having a unique <see cref="Guid"/> identity.</summary>
public interface IHasId
{
    /// <summary>Gets the unique identifier for this entity.</summary>
    Guid Id { get; }
}
