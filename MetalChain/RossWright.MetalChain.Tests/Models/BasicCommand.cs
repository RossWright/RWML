namespace RossWright.MetalChain.Tests;

public static class BasicCommand
{
    public class Request(int Value) : IRequest
    {
        public int Value { get; set; } = Value;
    }

    public class Handler : IRequestHandler<Request>
    {
        public static int LastValue { get; private set; }
        public Task Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            LastValue = request.Value;
            return Task.CompletedTask;
        }
    }
}