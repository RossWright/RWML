namespace RossWright.MetalChain.Tests;

public static class InterceptingQuery
{
    public class Request<TResponse>(int Value, IRequest<TResponse> InnerRequest)
        : IRequest<TResponse>
    {
        public int Value { get; set; } = Value;
        public IRequest<TResponse> InnerRequest { get; set; } = InnerRequest;
    }

    public class Handler<TResponse>(IMediator _mediator)
        : IRequestHandler<Request<TResponse>, TResponse>
    {
        public static int LastValue { get; private set; }
        public Task<TResponse> Handle(
            Request<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            LastValue = request.Value;
            return _mediator.Send(request.InnerRequest, cancellationToken);
        }
    }
}
