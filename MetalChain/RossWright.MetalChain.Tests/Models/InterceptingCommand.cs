namespace RossWright.MetalChain.Tests;

public static class InterceptingCommand
{
    public class Request(int Value, IRequest InnerRequest) : IRequest
    {
        public int Value { get; set; } = Value;
        public object InnerRequest { get; set; } = InnerRequest;
    }

    public class Handler(IMediator _mediator)
        : IRequestHandler<Request>
    {
        public static int LastValue { get; private set; }
        public Task Handle(Request request, CancellationToken cancellationToken = default)
        {
            LastValue = request.Value;
            return _mediator.Send(request.InnerRequest, cancellationToken);
        }
    }
}