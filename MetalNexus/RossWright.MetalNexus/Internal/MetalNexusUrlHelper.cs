using RossWright.MetalNexus.Schema;

namespace RossWright.MetalNexus;

internal class MetalNexusUrlHelper(
    IMetalNexusRegistry _registry,
    IHttpClientFactory _httpClientFactory)
    : IMetalNexusUrlHelper
{
    public string GetUrlFor<TRequest>(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var endpoint = _registry.FindEndpoint(typeof(TRequest));
        if (endpoint == null) throw new InvalidOperationException(
            $"Endpoint Schema contains no endpoint for {typeof(TRequest).FullName}");
        using var httpClient = _httpClientFactory
            .CreateClient(endpoint.HttpClientName ?? string.Empty);
        return Tools.CombineUrl(
            httpClient.BaseAddress?.OriginalString ?? string.Empty,
            endpoint.GetEndpointWithParams<TRequest>(request));
    }
}
