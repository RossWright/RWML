using Microsoft.Extensions.Logging;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;

namespace RossWright.MetalNexus;

internal class ApiRequestHandler<TRequest>(
        IMetalNexusRegistry registry,
        IHttpClientFactory httpClientFactory,
        IMetalNexusClientOptions metalNexusClientOptions,
        ILoggerFactory loggerFactory) :
        ApiRequestHandlerBase<TRequest>(
            registry,
            httpClientFactory,
            metalNexusClientOptions,
            loggerFactory),
        IRequestHandler<TRequest>
        where TRequest : IRequest
{
    public async Task Handle(TRequest request, CancellationToken cancellationToken) =>
        await InnerHandle(null, request, cancellationToken);
}
