using RossWright.MetalChain;

namespace RossWright.MetalNexus;

/// <summary>
/// Wraps a fire-and-forget <see cref="IRequest"/> so it is dispatched through a specific
/// named <see cref="HttpClient"/> connection rather than the default one.
/// </summary>
/// <typeparam name="TRequest">The underlying request type to dispatch.</typeparam>
/// <remarks>
/// Use <see cref="SendViaExtensions.SendVia(IMediator, string, IRequest, CancellationToken)"/>
/// instead of constructing this type directly.
/// </remarks>
public class SendVia<TRequest>(string connectionName, TRequest request) 
    : IRequest
    where TRequest : IRequest
{
    /// <summary>The named <see cref="HttpClient"/> connection to use for this dispatch.</summary>
    public string ConnectionName => connectionName;
    /// <summary>The underlying request to dispatch.</summary>
    public TRequest Request => request;
}

/// <summary>
/// Wraps a <see cref="IRequest{TResponse}"/> so it is dispatched through a specific
/// named <see cref="HttpClient"/> connection rather than the default one.
/// </summary>
/// <typeparam name="TRequest">The underlying request type to dispatch.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
/// <remarks>
/// Use <see cref="SendViaExtensions.SendVia{TResponse}(IMediator, string, IRequest{TResponse}, CancellationToken)"/>
/// instead of constructing this type directly.
/// </remarks>
public class SendVia<TRequest, TResponse>(string connectionName, TRequest request)
    : IRequest<TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>The named <see cref="HttpClient"/> connection to use for this dispatch.</summary>
    public string ConnectionName => connectionName;
    /// <summary>The underlying request to dispatch.</summary>
    public TRequest Request => request;
}