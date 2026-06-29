using Microsoft.Extensions.Logging;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;

namespace RossWright.MetalNexus;

internal class ApiRequestHandlerBase<TRequest>(
    IMetalNexusRegistry _registry,
    IHttpClientFactory _httpClientFactory,
    IMetalNexusClientOptions _metalNexusClientOptions,
    ILoggerFactory _loggerFactory)
{
    protected IMetalNexusClientOptions _metalNexusClientOptions { get; } = _metalNexusClientOptions;
    private readonly ILogger _logger = _loggerFactory.CreateLogger("MetalNexus");

    protected internal async Task<HttpResponseMessage> InnerHandle(string? overrideHttpClientName, TRequest request, CancellationToken cancellationToken)
    {
        var endpoint = _registry.FindEndpoint(typeof(TRequest));
        if (endpoint == null) throw new MetalNexusException($"Endpoint for {typeof(TRequest).FullName} not defined");

        HttpContent? content = null;
        string fullPath = endpoint.GetEndpointWithParams<TRequest>(request!);
        if (request is MetalNexusFileRequest fileRequest)
        {
            // Scalar props are already encoded into the URL by GetEndpointWithParams;
            // always attach the multipart body regardless of RequestAsQueryParams.
            content = BuildMultipartContent(request!, fileRequest);
        }
        else if (!endpoint.RequestAsQueryParams)
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
        httpRequestMessage.Headers.Add(MetalNexusConstants.ClientHeader, MetalNexusConstants.ClientHeaderValue);

        if (endpoint.HeaderProperties.Length > 0)
        {
            foreach (var headerPropName in endpoint.HeaderProperties)
            {
                var prop = typeof(TRequest).GetProperty(headerPropName);
                var headerAttr = prop?.GetCustomAttribute<FromHeaderAttribute>();
                var headerName = headerAttr?.HeaderName ?? headerPropName;
                var value = prop?.GetValue(request)?.ToString();
                if (value != null)
                    httpRequestMessage.Headers.TryAddWithoutValidation(headerName, value);
            }
        }

        var connectionName = overrideHttpClientName ??
            endpoint.HttpClientName ??
            _metalNexusClientOptions.DefaultConnectionName ??
            Microsoft.Extensions.Options.Options.DefaultName;
        var httpClient = _httpClientFactory.CreateClient(connectionName);

        _logger?.LogTrace($"Calling {httpRequestMessage.Method} " +
            $"{httpClient.BaseAddress?.OriginalString ?? "<null>"}" +
            $"{httpRequestMessage.RequestUri.OriginalString}" +
            $"{(string.IsNullOrWhiteSpace(connectionName) ? "" : $" on connection {connectionName}")}");

        HttpResponseMessage? response = null;
        try
        {
            CancellationToken sendToken = cancellationToken;
            CancellationTokenSource? timeoutCts = null;
            if (endpoint.HttpClientTimeout.HasValue)
            {
                timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(endpoint.HttpClientTimeout.Value);
                sendToken = timeoutCts.Token;
            }
            try
            {
                response = await httpClient.SendAsync(httpRequestMessage, sendToken);
            }
            finally
            {
                timeoutCts?.Dispose();
            }
        }
        catch (OperationCanceledException)
           when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException();
        }
        if (response?.IsSuccessStatusCode == true)
        {
            _logger?.LogTrace($"Success {endpoint.HttpMethod} {Tools.CombineUrl(httpClient.BaseAddress?.ToString() ?? "", fullPath)}" +
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

    // Encodes a MetalNexusFileRequest as multipart/form-data.
    // Named [FileSlot] properties are sent under their slot name; anonymous Files[] entries
    // are sent under the field name "files". Non-file, non-base-class properties are sent
    // as plain string form fields so path/query params still work server-side.
    private static MultipartFormDataContent BuildMultipartContent(object request, MetalNexusFileRequest fileRequest)
    {
        var multipart = new MultipartFormDataContent();
        var requestType = request.GetType();

        // Collect [FileSlot] property names so we can skip them during the scalar pass
        var slotProps = requestType.GetProperties()
            .Select(p => (Property: p, Slot: p.GetCustomAttribute<FileSlotAttribute>()))
            .Where(x => x.Slot != null)
            .ToList();

        var slotPropNames = new HashSet<string>(slotProps.Select(x => x.Property.Name));

        // Scalar properties on the concrete request type (not MetalNexusFileRequest base members)
        foreach (var prop in requestType.GetProperties()
            .Where(p => p.DeclaringType != typeof(MetalNexusFileRequest)
                     && !slotPropNames.Contains(p.Name)
                     && p.PropertyType.IsSimpleType()))
        {
            var value = prop.GetValue(request);
            if (value != null)
                multipart.Add(new StringContent(value.ToString()!), prop.Name);
        }

        // Named [FileSlot] properties
        foreach (var (prop, slot) in slotProps)
        {
            if (prop.GetValue(request) is MetalNexusFile file)
                AddFilePart(multipart, file, slot!.Name);
        }

        // Anonymous Files[] array
        foreach (var file in fileRequest.Files ?? [])
            AddFilePart(multipart, file, "files");

        return multipart;
    }

    private static void AddFilePart(MultipartFormDataContent multipart, MetalNexusFile file, string fieldName)
    {
        HttpContent fileContent = file.DataStream is not null
            ? new StreamContent(file.DataStream)
            : new ByteArrayContent(file.Data ?? []);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        multipart.Add(fileContent, fieldName, file.FileName);
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
                if (requestProperty.DeclaringType == typeof(MetalNexusFileRequest)) continue;
                if (requestProperty.HasAttribute<FromHeaderAttribute>()) continue;

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

