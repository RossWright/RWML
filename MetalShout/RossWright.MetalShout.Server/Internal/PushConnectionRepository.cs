namespace RossWright.MetalShout;

internal interface IPushConnectionRepository
{
    bool IsUserConnected(Guid userId);
    void AddConnection(string connectionId, Guid userId);
    void RemoveConnection(string connectionId, Guid userId);
}

internal class PushConnectionRepository : IPushConnectionRepository
{
    private readonly Dictionary<Guid, HashSet<string>> _connections = new();
    public bool IsUserConnected(Guid userId) => _connections.ContainsKey(userId);
    public void AddConnection(string connectionId, Guid userId) =>
        _connections.AddToSet(userId, connectionId);
    public void RemoveConnection(string connectionId, Guid userId)=>
        _connections.RemoveFromSet(userId, connectionId, removeEmptySet: true);
}