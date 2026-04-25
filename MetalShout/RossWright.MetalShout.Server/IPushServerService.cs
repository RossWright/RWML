namespace RossWright.MetalShout;

public interface IPushServerService 
{
    Task Push(object pushObj, string? refId, Guid[] userIds, CancellationToken cancellationToken);    
    Task PushToAll(object pushObj, CancellationToken cancellationToken);
    void SubscribeUser(Guid userId, Type pushMessageType, string? refId = null, bool forAllRefsAndUsers = false);
    void UnsubscribeUser(Guid userId, Type pushMessageType, string? refId = null);
    void SubscribeCurrentUser(Type pushMessageType, string? refId = null, bool forAllRefsAndUsers = false);
    void UnsubscribeCurrentUser(Type pushMessageType, string? refId = null);
}

public static class IPushServerServiceExtensions
{
    public static Task Push(this IPushServerService pushService, object pushObj, Guid[] userIds, CancellationToken cancellationToken) =>
        pushService.Push(pushObj, null, userIds, cancellationToken);

    public static void SubscribeUser<TPush>(this IPushServerService repo,
        Guid userId, string? refId = null, bool forAllRefsAndUsers = false) =>
        repo.SubscribeUser(userId, typeof(TPush), refId, forAllRefsAndUsers);

    public static void UnsubscribeUser<TPush>(this IPushServerService repo,
        Guid userId, string? refId = null) =>
        repo.UnsubscribeUser(userId, typeof(TPush), refId);

    public static void SubscribeCurrentUser<TPush>(this IPushServerService repo,
        string? refId = null, bool forAllRefsAndUsers = false) =>
        repo.SubscribeCurrentUser(typeof(TPush), refId, forAllRefsAndUsers);

    public static void UnsubscribeCurrentUser<TPush>(this IPushServerService repo,
        string? refId = null) =>
        repo.UnsubscribeCurrentUser(typeof(TPush), refId);
}
