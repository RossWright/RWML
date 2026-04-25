namespace RossWright.MetalShout;

public interface IPushClientService
{
    Task Subscribe<TMessage>(IPushSubscriber<TMessage> subscriber, string? connectionName = null, CancellationToken cancellationToken = default) where TMessage : class;
    Task Unsubscribe<TMessage>(IPushSubscriber<TMessage>? subscriber, string? connectionName = null, CancellationToken cancellationToken = default) where TMessage : class;
}

public interface IPushSubscriber<in TMessage>
{
    Task OnPushReceived(TMessage message);
}

public static class IPushClientServiceExtensions
{
    public static async Task<IAsyncDisposable> Subscribe<TMessage>(
        this IPushClientService pushService,
        Func<TMessage, Task> onPush,
        string? connectionName = null,
        CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var subscriber = new PushSubscriber<TMessage>(onPush);
        await pushService.Subscribe(subscriber, connectionName, cancellationToken);
        subscriber.OnDispose = () => pushService.Unsubscribe(subscriber, connectionName);
        return subscriber;
    }

    private sealed class PushSubscriber<TMessage> : IPushSubscriber<TMessage>, IAsyncDisposable
    {
        public PushSubscriber(Func<TMessage, Task> onPush) => _onPush = onPush;
        private readonly Func<TMessage, Task> _onPush;
        public Task OnPushReceived(TMessage message) => _onPush(message);
        public Func<Task> OnDispose = null!;
        public async ValueTask DisposeAsync() => await OnDispose();
    }
}