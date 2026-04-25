namespace RossWright.MetalChain.Tests;

public static class OpenCommand
{
    public class Request<TThing>(TThing Thing)
        : IRequest
    {
        public TThing Thing { get; set; } = Thing;
    }

    public class Handler<TThing> : IRequestHandler<Request<TThing>>
    {
        public static TThing? LastValue { get; private set; }
        public Task Handle(Request<TThing> request,
            CancellationToken cancellationToken = default)
        {
            LastValue = request.Thing;
            return Task.CompletedTask;
        }
    }
}