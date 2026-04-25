using RossWright.MetalChain;

namespace RossWright.MetalShout;

public class UserDisconnected : IRequest
{
    public required Guid UserId { get; init; }
    public Exception? Exception { get; init; }
    public bool WasLastConnection { get; init; }
}