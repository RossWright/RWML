using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RossWright.MetalChain;

namespace RossWright.MetalShout;

[Authorize]
internal class PushServiceHub(
    IMediator _mediator,
    IPushConnectionRepository _pushConnectionRepository,
    IEnumerable<IPushConnectionObserver> _pushConnectionObservers)
    : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Guid.TryParse(Context.UserIdentifier, out var userId))
        {
            var isFirstConnection = !_pushConnectionRepository.IsUserConnected(userId);
            _pushConnectionRepository.AddConnection(Context.ConnectionId, userId);
            await _mediator.Send(new UserConnected 
            { 
                UserId = userId, 
                IsFirstConnection = isFirstConnection 
            });
            foreach (var observer in _pushConnectionObservers)
            {
                await observer.OnConnected(userId, isFirstConnection);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Guid.TryParse(Context.UserIdentifier, out var userId))
        {
            _pushConnectionRepository.RemoveConnection(Context.ConnectionId, userId);
            var wasLastConnection = !_pushConnectionRepository.IsUserConnected(userId);
            await _mediator.Send(new UserDisconnected 
            { 
                UserId = userId, 
                Exception = exception, 
                WasLastConnection = wasLastConnection 
            });
            foreach (var observer in _pushConnectionObservers)
            {
                await observer.OnDisconnected(userId, exception,wasLastConnection);
            }
        }
    }
}
