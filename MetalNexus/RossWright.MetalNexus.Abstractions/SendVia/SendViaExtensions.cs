using RossWright.MetalChain;
using System.Reflection;

namespace RossWright.MetalNexus;

/// <summary>
/// Extension methods on <see cref="IMediator"/> for dispatching a request through a specific
/// named <see cref="HttpClient"/> connection.
/// </summary>
public static class SendViaExtensions
{
    /// <summary>
    /// Dispatches a fire-and-forget <paramref name="request"/> through the named
    /// <paramref name="connectionName"/> connection.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="connectionName">The named <see cref="HttpClient"/> connection to use.</param>
    /// <param name="request">The request to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that completes when the request has been handled.</returns>
    public static Task SendVia(this IMediator mediator, string connectionName, IRequest request, CancellationToken cancellationToken)
    {
        var sendViaRequestType = typeof(SendVia<>).MakeGenericType(request.GetType());
        var sendViaRequest = MetalActivator.CreateInstance(sendViaRequestType, connectionName, request)!;
        return mediator.Send(sendViaRequest, cancellationToken);
    }

    /// <summary>
    /// Dispatches a <paramref name="request"/> through the named <paramref name="connectionName"/>
    /// connection and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="connectionName">The named <see cref="HttpClient"/> connection to use.</param>
    /// <param name="request">The request to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is the deserialized response, or <c>null</c>
    /// if the server returned no body.
    /// </returns>
    public static async Task<TResponse?> SendVia<TResponse>(this IMediator mediator, string connectionName, IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var sendViaRequestType = typeof(SendVia<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var sendViaRequest = MetalActivator.CreateInstance(sendViaRequestType, connectionName, request)!;
        return (TResponse?) await mediator.Send(sendViaRequest, cancellationToken);
    }
}
