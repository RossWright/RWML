using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;
using System.Net.Http.Json;

namespace RossWright.MetalNexus;

internal class SendViaRequestHandler<TRequest>(
    IMetalNexusRegistry registry,
    IHttpClientFactory httpClientFactory,
    IMetalNexusClientOptions metalNexusClientOptions)
    : ApiRequestHandlerBase<TRequest>(registry, httpClientFactory, metalNexusClientOptions),
    IRequestHandler<SendVia<TRequest>>
    where TRequest : IRequest
{
    public async Task Handle(SendVia<TRequest> request, CancellationToken cancellationToken)
    {
        await InnerHandle(request.ConnectionName, (TRequest)request.Request, cancellationToken);
    }
}

internal class SendViaRequestHandler<TRequest, TResponse>(
    IMetalNexusRegistry registry,
    IHttpClientFactory httpClientFactory,
    IMetalNexusClientOptions metalNexusClientOptions) 
    : ApiRequestHandlerBase<TRequest>(registry, httpClientFactory, metalNexusClientOptions),
    IRequestHandler<SendVia<TRequest, TResponse>, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(SendVia<TRequest, TResponse> request, CancellationToken cancellationToken)
    {
        var response = await InnerHandle(request.ConnectionName, (TRequest)request.Request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TResponse>(
            cancellationToken: cancellationToken);
        return result!;
    }
}