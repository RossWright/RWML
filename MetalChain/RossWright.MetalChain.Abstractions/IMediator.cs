namespace RossWright.MetalChain;

/// <summary>
/// Dispatches requests to their registered handlers and manages in-process listeners.
/// Each <see cref="Send{TResponse}(IRequest{TResponse}, CancellationToken)"/> call (in any overload)
/// resolves handlers in a newly created DI scope, so scoped services injected into handlers
/// receive a fresh instance per dispatch. The scope is disposed when the handler returns.
/// </summary>
public interface IMediator
{
    /// <summary>Returns <see langword="true"/> if a handler is registered for <paramref name="requestType"/>.</summary>
    /// <param name="requestType">The request type to check.</param>
    bool HasHandlerFor(Type requestType);
    /// <summary>Returns <see langword="true"/> if at least one listener is registered for <paramref name="requestType"/>.</summary>
    /// <param name="requestType">The request type to check.</param>
    bool HasListenerFor(Type requestType);

    /// <summary>
    /// Sends a query and returns the response produced by its registered handler.
    /// Each call creates a new DI scope so injected scoped services are isolated per dispatch.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The query to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response produced by the registered handler.</returns>
    /// <exception cref="MetalChainException">Thrown when no handler is registered and the request type or global options do not allow unhandled queries.</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Sends a command to its registered handler.
    /// Each call creates a new DI scope so injected scoped services are isolated per dispatch.
    /// </summary>
    /// <typeparam name="TRequest">The command type.</typeparam>
    /// <param name="request">The command to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="MetalChainException">Thrown when no handler and no active listener are registered and the request type or global options do not allow unhandled commands.</exception>
    Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest;
    /// <summary>
    /// Sends an untyped request to its registered handler.
    /// Each call creates a new DI scope so injected scoped services are isolated per dispatch.
    /// The concrete request type is resolved at runtime; use when the type is not known at compile time.
    /// </summary>
    /// <param name="request">The request object to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The handler's response as <see cref="object"/>, or <see langword="null"/> for commands.</returns>
    /// <exception cref="MetalChainException">Thrown when no handler is registered for the runtime request type.</exception>
    Task<object?> Send(object request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers an in-process listener for <typeparamref name="TRequest"/> and returns a handle that unregisters it on disposal.
    /// Listener tasks are started concurrently with the registered handler(s) and awaited after all handlers complete.
    /// A throwing listener will not interrupt handler execution; its exception surfaces afterward.
    /// Works for both command and query request types; for queries, the listener cannot influence the response.
    /// </summary>
    /// <typeparam name="TRequest">The request type to listen for.</typeparam>
    /// <param name="listener">The async callback invoked for each dispatched request.</param>
    /// <returns>An <see cref="IDisposable"/> that unregisters the listener when disposed.</returns>
    IDisposable Listen<TRequest>(Func<TRequest, CancellationToken, Task> listener) where TRequest : IRequest;
}

/// <summary>Convenience extension methods for <see cref="IMediator"/>.</summary>
public static class IMediatorExtensions
{
    /// <summary>Returns <see langword="true"/> if a handler is registered for <typeparamref name="TRequestType"/>.</summary>
    /// <typeparam name="TRequestType">The request type to check.</typeparam>
    /// <param name="mediator">The mediator instance to query.</param>
    public static bool HasHandlerFor<TRequestType>(this IMediator mediator) =>
        mediator.HasHandlerFor(typeof(TRequestType));

    /// <summary>Returns <see langword="true"/> if at least one listener is registered for <typeparamref name="TRequestType"/>.</summary>
    /// <typeparam name="TRequestType">The request type to check.</typeparam>
    /// <param name="mediator">The mediator instance to query.</param>
    public static bool HasListenerFor<TRequestType>(this IMediator mediator) =>
        mediator.HasListenerFor(typeof(TRequestType));

    /// <summary>Registers an <see cref="IRequestHandler{TRequest}"/> as a listener and returns a handle that unregisters it on disposal.</summary>
    /// <typeparam name="TRequest">The request type the handler processes.</typeparam>
    /// <param name="mediator">The mediator instance to register the listener on.</param>
    /// <param name="handler">The handler instance whose <see cref="IRequestHandler{TRequest}.Handle"/> method is invoked for each dispatched request.</param>
    public static IDisposable Listen<TRequest>(this IMediator mediator, IRequestHandler<TRequest> handler)
        where TRequest : IRequest => 
        mediator.Listen<TRequest>((r,ct) => handler.Handle(r,ct));

    /// <summary>
    /// Sends a query and returns <see langword="default"/> if no handler is registered,
    /// regardless of global settings or type attributes. Never throws for a missing handler.
    /// If a handler is registered and throws, the exception propagates normally.
    /// </summary>
    /// <typeparam name="TResponse">The response type returned by the query handler.</typeparam>
    /// <param name="mediator">The mediator instance used to dispatch the request.</param>
    /// <param name="request">The query to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The handler's response, or <see langword="default"/> when no handler is registered.</returns>
    public static async Task<TResponse?> SendOrDefault<TResponse>(
        this IMediator mediator,
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (!mediator.HasHandlerFor(request.GetType()))
            return default;
        return await mediator.Send(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a command and completes silently if no handler and no listener are registered,
    /// regardless of global settings or <see cref="RequireHandlerAttribute"/> on the type.
    /// </summary>
    /// <typeparam name="TRequest">The command type to dispatch.</typeparam>
    /// <param name="mediator">The mediator instance used to dispatch the request.</param>
    /// <param name="request">The command to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public static Task SendOrIgnore<TRequest>(
        this IMediator mediator,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        if (!mediator.HasHandlerFor(typeof(TRequest)) && !mediator.HasListenerFor(typeof(TRequest)))
            return Task.CompletedTask;
        return mediator.Send(request, cancellationToken);
    }
}