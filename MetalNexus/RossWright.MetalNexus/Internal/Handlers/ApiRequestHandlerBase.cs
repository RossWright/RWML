using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;

namespace RossWright.MetalNexus;

internal class ApiRequestHandlerBase<TRequest>(
    IMetalNexusRegistry _registry,
    IHttpClientFactory _httpClientFactory,
    IMetalNexusClientOptions _metalNexusClientOptions)
{ 
    protected internal async Task<HttpResponseMessage> InnerHandle(string? overrideHttpClientName, TRequest request, CancellationToken cancellationToken)
    {
        var log = _metalNexusClientOptions.LoadLog;

        var endpoint = _registry.FindEndpoint(typeof(TRequest));
        if (endpoint == null) throw new MetalNexusException($"Endpoint for {typeof(TRequest).FullName} not defined");

        HttpContent? content = null;
        string fullPath = endpoint.GetEndpointWithParams<TRequest>(request!);
        if (!endpoint.RequestAsQueryParams)
        {
            var json = JsonSerializer.Serialize(request, _metalNexusClientOptions.RequestBodyJsonSerializerOptions);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = endpoint.HttpMethod,
            RequestUri = new Uri(fullPath, UriKind.RelativeOrAbsolute),
            Content = content,
        };

        var connectionName = overrideHttpClientName ??
            endpoint.HttpClientName ??
            _metalNexusClientOptions.DefaultConnectionName ??
            Microsoft.Extensions.Options.Options.DefaultName;
        var httpClient = _httpClientFactory.CreateClient(connectionName);

        log?.LogTrace($"Calling {httpRequestMessage.Method} " +
            $"{httpClient.BaseAddress?.OriginalString ?? "<null>"}" +
            $"{httpRequestMessage.RequestUri.OriginalString}" +
            $"{(string.IsNullOrWhiteSpace(connectionName) ? "" : $" on connection {connectionName}")}");

        if (endpoint.HttpClientTimeout.HasValue &&
            httpClient.Timeout != endpoint.HttpClientTimeout)
        {
            httpClient.Timeout = endpoint.HttpClientTimeout.Value;
        }
        HttpResponseMessage? response = null;
        try
        {
            response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
        }
        catch (OperationCanceledException)
           when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException();
        }
        if (response?.IsSuccessStatusCode == true)
        {
            log?.LogTrace($"Success {endpoint.HttpMethod} {Tools.CombineUrl(httpClient.BaseAddress?.ToString() ?? "", fullPath)}" +
                $"{(string.IsNullOrWhiteSpace(connectionName) ? null : $" on connection {connectionName}")}");
            return response;
        }

        if (response == null)
        {
            throw new MetalNexusException(
                $"{httpRequestMessage.RequestUri} failed without response");
        }
        else
        {
            throw ExceptionResponse.Deserialize(response.StatusCode,
                await response.Content.ReadAsStringAsync(),
                _metalNexusClientOptions.ServerStackTraceOnExceptionsIncluded);
        }
    }
}

internal static class IEndpointExtensions
{
    public static string GetEndpointWithParams<TRequest>(this IEndpoint endpoint, object request)
    {
        var path = endpoint.Path;
        if (!path.StartsWith('/') &&
            !path.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
        {
            path = $"/{path}";
        }

        var pathParamsIncluded = new List<PropertyInfo>();
        if (endpoint.HasPathParams)
        {
            var splitPropPath = endpoint.Path.Split('/');
            foreach ((var slot, var index) in splitPropPath
                .WithIndex().Where(_ => _.item.StartsWith('{')))
            {
                var propName = slot.Trim('{', '}');
                var property = endpoint.RequestType.GetProperty(propName)!;
                pathParamsIncluded.Add(property);
                splitPropPath[index] = property.GetValue(request)?.ToString()!;
            }
            path = string.Join('/', splitPropPath);
        }

        List<string> queryParams = new();
        void Add(string name, object value) => queryParams.Add(
            $"{HttpUtility.UrlEncode(name)}=" +
            HttpUtility.UrlEncode(value.ToString()));
        if (endpoint.RequestAsQueryParams)
        {
            foreach (var requestProperty in typeof(TRequest).GetProperties()
                .Where(_ => !pathParamsIncluded.Contains(_)))
            {


                if (requestProperty.PropertyType.DeclaringType == typeof(MetalNexusFileRequest)) continue;

                var requestPropertyValue = requestProperty.GetValue(request);
                if (requestPropertyValue == null) continue;

                if (requestProperty.PropertyType.IsArray)
                {
                    foreach (var elementValue in (Array)requestPropertyValue)
                    {
                        if (elementValue != null)
                        {
                            Add(requestProperty.Name, elementValue);
                        }
                    }
                }
                else if (requestProperty.PropertyType.IsSimpleType())
                {
                    Add(requestProperty.Name, requestPropertyValue);
                }
                else
                {
                    foreach (var subProperty in requestProperty.PropertyType.GetProperties())
                    {
                        var subPropertyValue = subProperty.GetValue(requestPropertyValue);
                        if (subPropertyValue == null) continue;
                        if (subProperty.PropertyType.IsArray)
                        {
                            foreach (var elementValue in (Array)subPropertyValue)
                            {
                                if (elementValue != null)
                                {
                                    Add(subProperty.Name, elementValue);
                                }
                            }
                        }
                        else
                        {
                            Add(subProperty.Name, subPropertyValue);
                        }
                    }
                }
            }
        }
        if (queryParams.Any())
        {
            path = path.TrimEnd('/') + $"?{string.Join('&', queryParams)}";
        }
        return path;
    }
}

