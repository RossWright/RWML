using RossWright.MetalChain;

namespace RossWright.MetalShout;

internal class PushRequestHandler<TRequest>(
    IPushServerService pushSvc)
    : IRequestHandler<Push<TRequest>>
    where TRequest : IRequest
{
    public Task Handle(Push<TRequest> request, CancellationToken cancellationToken) =>
        pushSvc.Push(request.Request, request.RefId, request.UserIds, cancellationToken);
}