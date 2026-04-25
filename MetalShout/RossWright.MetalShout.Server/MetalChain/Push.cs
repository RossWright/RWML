using RossWright.MetalChain;

namespace RossWright.MetalShout;

public class Push<TRequest> : IRequest where TRequest : IRequest
{
    public Push() { }

    public Push(TRequest request, params Guid[] userIds)
    {
        Request = request;
        UserIds = userIds;
    }

    public Push(TRequest request, string refId, params Guid[] userIds)
    {
        Request = request;
        UserIds = userIds;
    }

    public required Guid[] UserIds { get; init; }
    public required TRequest Request { get; init; }
    public string? RefId { get; init;  }
}
