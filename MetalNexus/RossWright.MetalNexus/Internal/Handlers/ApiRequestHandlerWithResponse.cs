using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;
using System.Net.Http.Json;

namespace RossWright.MetalNexus;

internal class ApiRequestHandlerWithResponse<TRequest, TResponse>(
    IMetalNexusRegistry registry,
    IHttpClientFactory httpClientFactory,
    IMetalNexusClientOptions metalNexusClientOptions) :
    ApiRequestHandlerBase<TRequest>(registry, httpClientFactory, metalNexusClientOptions),
    IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var response = await InnerHandle(null, request, cancellationToken);
        if (typeof(TResponse) == typeof(MetalNexusFile))
        {
            return (TResponse)(object)new MetalNexusFile
            {
                FileName = response.Content.Headers.ContentDisposition!.FileName!,
                ContentType = response.Content.Headers.ContentType!.MediaType!,
                Data = await response.Content.ReadAsByteArrayAsync()
            };
        }
        else
        {
            var result = await response.Content.ReadFromJsonAsync<TResponse>(
                cancellationToken: cancellationToken);
            return result!;
        }
    }
}
