using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalChain;

internal class Mediator(IServiceScopeFactory _serviceScopeFactory, IMetalChainRegistry _registry) : IMediator
{
    public bool HasHandlerFor(Type requestType) => _registry.HasHandlerFor(requestType);
    public bool HasListenerFor(Type requestType) => _registry.HasListenerFor(requestType);

    public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        return await _registry
            .Handle(scope.ServiceProvider, request, cancellationToken);
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        (TResponse)(await Send((object)request, cancellationToken))!;

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
        Send((object)request, cancellationToken);

    public IDisposable Listen<TRequest>(Func<TRequest, CancellationToken, Task> listener) where TRequest : IRequest =>
        new ListenerSubscription<TRequest>(_registry, listener);

    internal class ListenerSubscription<TRequest> : IDisposable
    {
        private IMetalChainRegistry _registry;
        internal Func<object, CancellationToken, Task> _listener;

        public ListenerSubscription(IMetalChainRegistry registry, Func<TRequest, CancellationToken, Task> listener)
        {
            _registry = registry;
            _listener = (r, ct) => listener((TRequest)r, ct);
            _registry.AddListener(typeof(TRequest), _listener);
        }

        public void Dispose()
        {
            if (_registry == null) return;
            _registry.RemoveListener(typeof(TRequest), _listener);
            _registry = null!;
        }
    }
}
