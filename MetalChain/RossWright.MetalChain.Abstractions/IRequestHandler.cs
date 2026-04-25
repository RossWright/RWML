namespace RossWright.MetalChain;

/// <summary>Handles a query of type <typeparamref name="TRequest"/> and returns a <typeparamref name="TResponse"/>.</summary>
/// <typeparam name="TRequest">The query type this handler processes.</typeparam>
/// <typeparam name="TResponse">The response type returned by this handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    /// <summary>Processes <paramref name="request"/> and returns the response.</summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>Handles a command of type <typeparamref name="TRequest"/> with no return value.</summary>
/// <typeparam name="TRequest">The command type this handler processes.</typeparam>
public interface IRequestHandler<in TRequest> 
    where TRequest : IRequest
{
    /// <summary>Processes <paramref name="request"/>.</summary>
    /// <param name="request">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task Handle(TRequest request, CancellationToken cancellationToken);
}