using RossWright.MetalChain;

namespace RossWright.MetalShout;

public class UserConnected : IRequest
{
    public required Guid UserId { get; init; }
    public bool IsFirstConnection { get; init; }
}
