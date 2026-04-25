namespace RossWright.MetalChain.Tests;

public static class BasicQuery
{
    public class Request(int Value) : IRequest<Response>
    {
        public int Value { get; set; } = Value;
    }

    public class Response
    {
        public int Result { get; set; }
    }

    public class Handler : IRequestHandler<Request, Response>
    {
        public static int LastValue { get; private set; }
        public Task<Response> Handle(
            Request request,
            CancellationToken cancellationToken = default)
        {
            LastValue = request.Value;
            return Task.FromResult(new Response { Result = request.Value + 1 });
        }
    }
}