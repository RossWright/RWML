using Microsoft.Extensions.Logging;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using System.Net.Http.Json;

namespace RossWright.MetalNexus;

internal class ApiRequestHandlerWithResponse<TRequest, TResponse>(
    IMetalNexusRegistry registry,
    IHttpClientFactory httpClientFactory,
    IMetalNexusClientOptions _metalNexusClientOptions,
    ILoggerFactory loggerFactory) :
    ApiRequestHandlerBase<TRequest>(
        registry, 
        httpClientFactory, 
        _metalNexusClientOptions,
        loggerFactory),
    IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var response = await InnerHandle(null, request, cancellationToken);
        if (typeof(TResponse) == typeof(MetalNexusFile))
        {
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                // The server returned JSON when binary was expected — surface it as an exception
                // rather than silently producing a garbage MetalNexusFile with JSON bytes as Data.
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw ExceptionResponse.Deserialize(response.StatusCode, body,
                    _metalNexusClientOptions.ServerStackTraceOnExceptionsIncluded);
            }
            var disposition = response.Content.Headers.ContentDisposition;
            return (TResponse)(object)new MetalNexusFile
            {
                FileName = disposition?.FileName ?? string.Empty,
                ContentType = mediaType ?? string.Empty,
                Data = await response.Content.ReadAsByteArrayAsync(cancellationToken),
                IsAttachment = string.Equals(disposition?.DispositionType, "attachment",
                    StringComparison.OrdinalIgnoreCase)
            };
        }
        else if (typeof(IMetalNexusRawResponse).IsAssignableFrom(typeof(TResponse)))
        {
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (typeof(TResponse).IsInterface)
                return (TResponse)(IMetalNexusRawResponse)new RawResponseImpl(contentType, bytes);

            // Concrete class implementing IMetalNexusRawResponse — create and populate Data
            var instance = Activator.CreateInstance<TResponse>();
            typeof(TResponse).GetProperty(nameof(IMetalNexusRawResponse.Data))
                ?.SetValue(instance, bytes);
            return instance;
        }
        else
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent
                && response.Content.Headers.ContentLength is null or 0)
                return default!;

            var result = await response.Content.ReadFromJsonAsync<TResponse>(
                cancellationToken: cancellationToken);
            return result!;
        }
    }

    private sealed class RawResponseImpl(string contentType, byte[]? data) : IMetalNexusRawResponse
    {
        public string ContentType => contentType;
        public byte[]? Data => data;
        public Stream? DataStream => null;
    }
}
