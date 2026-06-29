using Microsoft.Extensions.Logging;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using System.Net.Http.Json;

namespace RossWright.MetalNexus;

internal class SendViaRequestHandler<TRequest>(
    IMetalNexusRegistry registry,
    IHttpClientFactory httpClientFactory,
    IMetalNexusClientOptions metalNexusClientOptions,
    ILoggerFactory loggerFactory)
    : ApiRequestHandlerBase<TRequest>(
        registry, 
        httpClientFactory, 
        metalNexusClientOptions,
        loggerFactory),
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
    IMetalNexusClientOptions metalNexusClientOptions,
    ILoggerFactory loggerFactory) 
    : ApiRequestHandlerBase<TRequest>(
        registry, 
        httpClientFactory, 
        metalNexusClientOptions,
        loggerFactory),
    IRequestHandler<SendVia<TRequest, TResponse>, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(SendVia<TRequest, TResponse> request, CancellationToken cancellationToken)
    {
        var response = await InnerHandle(request.ConnectionName, (TRequest)request.Request, cancellationToken);
        if (typeof(TResponse) == typeof(MetalNexusFile))
        {
            var disposition = response.Content.Headers.ContentDisposition;
            return (TResponse)(object)new MetalNexusFile
            {
                FileName = disposition?.FileName ?? string.Empty,
                ContentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty,
                Data = await response.Content.ReadAsByteArrayAsync(cancellationToken),
                IsAttachment = string.Equals(disposition?.DispositionType, "attachment",
                    StringComparison.OrdinalIgnoreCase)
            };
        }
        var result = await response.Content.ReadFromJsonAsync<TResponse>(
            cancellationToken: cancellationToken);
        return result!;
    }
}