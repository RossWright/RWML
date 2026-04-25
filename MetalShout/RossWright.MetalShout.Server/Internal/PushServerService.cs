using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using RossWright.MetalGuardian;

namespace RossWright.MetalShout;

internal class PushServerService : IPushServerService
{
    public PushServerService(
        IHubContext<PushServiceHub> hub,
        IPushSubscriptionRepository subscriptionRepository,
        IHttpContextAccessor httpCtx) 
    { 
        _hubContext = hub;
        _subscriptionRepository = subscriptionRepository;
        _httpCtx = httpCtx;
    }
    private readonly IPushSubscriptionRepository _subscriptionRepository;
    private readonly IHubContext<PushServiceHub> _hubContext;
    private readonly IHttpContextAccessor _httpCtx;

    public async Task Push(object pushObj, string? refId, Guid[] userIds, CancellationToken cancellationToken)
    {
        var subscriberUserIds = _subscriptionRepository
            .GetSubscribers(pushObj.GetType(), refId);
        if (userIds != null) subscriberUserIds.AddRange(userIds);
        if (subscriberUserIds.Any())
        {
            await _hubContext.Clients
                .Users(subscriberUserIds.Select(_ => _.ToString()))
                .SendAsync("Push", pushObj.GetType().AssemblyQualifiedName, pushObj, cancellationToken);
        }
    }

    public Task PushToAll(object pushObj, CancellationToken cancellationToken) => _hubContext.Clients.All
            .SendAsync("Push", pushObj.GetType().AssemblyQualifiedName, pushObj, cancellationToken);

    public void SubscribeUser(Guid userId, Type pushMessageType, string? refId = null, bool forAllRefsAndUsers = false) =>
        _subscriptionRepository.SubscribeUser(new Subscription
        {
            UserId = userId,
            PushMessageType = pushMessageType,
            RefId = refId,
            ForAllRefsAndUsers = forAllRefsAndUsers
        });

    public void UnsubscribeUser(Guid userId, Type pushMessageType, string? refId = null) =>
        _subscriptionRepository.UnsubscribeUser(new Subscription
        {
            UserId = userId,
            PushMessageType = pushMessageType,
            RefId = refId
        });

    public void SubscribeCurrentUser(Type pushMessageType, string? refId = null, bool forAllRefsAndUsers = false)
    {
        var userId = _httpCtx.GetUserId();
        if (userId == null) throw new MetalShoutException("Cannot subscribe a user if UserId is not in http context authentication claims");
        _subscriptionRepository.SubscribeUser(new Subscription
        {
            UserId = userId.Value,
            PushMessageType = pushMessageType,
            RefId = refId,
            ForAllRefsAndUsers = forAllRefsAndUsers
        });
    }

    public void UnsubscribeCurrentUser(Type pushMessageType, string? refId = null)
    {
        var userId = _httpCtx.GetUserId();
        if (userId == null) throw new MetalShoutException("Cannot subscribe a user if UserId is not in http context authentication claims");
        _subscriptionRepository.UnsubscribeUser(new Subscription
        {
            UserId = userId.Value,
            PushMessageType = pushMessageType,
            RefId = refId
        });
    }
}
