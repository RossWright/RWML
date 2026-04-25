using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;

namespace RossWright.MetalNexus;

internal class ApiRequestHandler<TRequest> :
        ApiRequestHandlerBase<TRequest>,
        IRequestHandler<TRequest>,
        IRequestHandler<SendVia<TRequest>>
        where TRequest : IRequest
{
    public ApiRequestHandler(
        IMetalNexusRegistry registry,
        IHttpClientFactory httpClientFactory,
        IMetalNexusClientOptions metalNexusClientOptions)
        : base(registry, httpClientFactory, metalNexusClientOptions) { }

    public async Task Handle(TRequest request, CancellationToken cancellationToken) =>
        await InnerHandle(null, request, cancellationToken);
    public async Task Handle(SendVia<TRequest> request, CancellationToken cancellationToken) =>
        await InnerHandle(request.ConnectionName, request.Request, cancellationToken);
}
