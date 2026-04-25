namespace RossWright.MetalShout;

internal interface IPushSubscriptionRepository
{
    void SubscribeUser(Subscription subscription);
    void UnsubscribeUser(Subscription subscription);
    HashSet<Guid> GetSubscribers(Type pushMessageType, string? refId = null);
}

internal class Subscription
{
    public Guid UserId { get; set; }
    public Type PushMessageType { get; set; } = null!;
    public string? RefId { get; set; }
    public bool ForAllRefsAndUsers { get; set; }
}

internal class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly List<Subscription> _subscriptions = new();

    public void SubscribeUser(Subscription subscription) => _subscriptions.Add(subscription);

    public void UnsubscribeUser(Subscription subscription) => _subscriptions
        .RemoveAll(_ => _.UserId == subscription.UserId && 
                        _.PushMessageType == subscription.PushMessageType &&
                        (subscription.RefId == null || _.RefId == subscription.RefId));

    public HashSet<Guid> GetSubscribers(Type pushMessageType, string? refId = null) => _subscriptions
        .Where(_ => _.PushMessageType == pushMessageType &&
                    (_.ForAllRefsAndUsers || _.RefId == null || _.RefId == refId))
        .Select(_ => _.UserId)
        .ToHashSet();
}