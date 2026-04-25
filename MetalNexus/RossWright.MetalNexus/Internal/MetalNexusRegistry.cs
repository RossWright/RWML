using RossWright.MetalNexus.Schemna;
using System.Collections.Concurrent;
using System.Reflection;

namespace RossWright.MetalNexus;

internal class MetalNexusRegistry(
    Action<Type> _addHandler,
    IEndpointSchemaOptions _endpointSchema,
    ICustomEndpointSchema? _customEndpointSchema,
    bool _isServer,
    ILoadLog? _loadLog)
    : IMetalNexusRegistry
{
    private readonly ConcurrentDictionary<Type, IEndpoint> _endpoints = new();

    public IEnumerable<IEndpoint> Endpoints => _endpoints.Values;

    public IEndpoint? FindEndpoint(Type requestType) =>
        _endpoints.GetValueOrDefault(requestType);

    public void AddEndpoints(params Type[] types)
    {
        foreach (var type in types)
        {
            var endpoint = DefineEndpoint(type);
            if (endpoint == null) continue;
            _endpoints.AddOrUpdate(endpoint.RequestType, endpoint, (_, _) => endpoint);
            if (_isServer)
            {
                _addHandler(type);
            }
            else
            {
                Type proxyHandlerType;
                if (endpoint.ResponseType == null)
                {
                    proxyHandlerType = typeof(ApiRequestHandler<>)
                        .MakeGenericType(endpoint.RequestType);
                }
                else
                {
                    proxyHandlerType = typeof(ApiRequestHandlerWithResponse<,>)
                        .MakeGenericType(endpoint.RequestType, endpoint.ResponseType);
                }
                _addHandler(proxyHandlerType);
            }
        }
    }

    public IEndpoint? DefineEndpoint(Type type)
    {
        // Find the actual request type if this is a handler type and on the server side
        Type? requestType = type;
        if (_isServer)
        {
            var handlerType = requestType.GetInterfaces()
                .Where(_ => _.IsGenericType)
                .FirstOrDefault(_ =>
                    _.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                    _.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            requestType = handlerType?.GetGenericArguments()[0];
        }
        var apiRequestAttribute = requestType?.GetCustomAttribute<ApiRequestAttribute>();
        if (requestType == null || apiRequestAttribute == null) return null;

        // Ensure the request type implements IRequest or IRequest<T>
        var requestInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i == typeof(IRequest));
        if (requestInterface == null)
        {
            requestInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IRequest<>));
        }
        if (requestInterface == null)
        {
            throw new MetalNexusException($"Invalid use of ApiRequest on {requestType.Name}, attribute is only valid for MetalChain Requests");
        }
        var responseType = requestInterface.IsGenericType ? requestInterface.GetGenericArguments()[0] : null;


        Endpoint endpoint = new()
        {
            RequestType = requestType,
            ResponseType = responseType,
        };

        (endpoint.Path, endpoint.HasPathParams) = DeterminePath(requestType, apiRequestAttribute);

        endpoint.Tag = DetermineTag(requestType, apiRequestAttribute, endpoint.Path);

        (endpoint.HttpMethod, endpoint.RequestAsQueryParams) = DetermineHttpProtocol(requestType, apiRequestAttribute);

        endpoint.HttpClientName = apiRequestAttribute.ConnectionName;

        var authAttribute = requestType.GetCustomAttribute<AuthenticatedAttribute>();
        (endpoint.RequiresAuthentication, endpoint.AuthorizedRoles) = DetermineAuthentication(requestType, apiRequestAttribute, authAttribute);
        endpoint.AllowProvisional = authAttribute?.AllowProvisional ?? false;

        var httpClientTimeoutAttribute = requestType.GetCustomAttribute<HttpClientTimeoutAttribute>();
        endpoint.HttpClientTimeout = httpClientTimeoutAttribute == null ? null
                : TimeSpan.FromSeconds(httpClientTimeoutAttribute.TimeoutSeconds);

        endpoint.HeaderProperties = requestType.GetProperties()
                .Where(_ => _.HasAttribute<FromHeaderAttribute>())
                .ToArray(_ => _.Name);

        _loadLog?.LogTrace($"{requestType.FullName} {endpoint.HttpMethod} {endpoint.Path} " +
            $"{(endpoint.RequestAsQueryParams ? "using query parameters" : "using ApiRequestAttribute body")} " +
            $"{(endpoint.RequiresAuthentication ? "Authenticated" : "Anonymous")}");

        return endpoint;
    }

    internal (string, bool) DeterminePath(Type requestType, ApiRequestAttribute apiRequestAttribute)
    {
        var path = apiRequestAttribute.Path;
        if (path == null)
        {
            path = _endpointSchema.PathStrategy?.Trim(requestType);

            var requestName = requestType.Name;
            foreach (var suffix in _endpointSchema.RequestSuffixesToTrim
                .Where(suffix => requestName.ToLower().EndsWith(suffix.ToLower())))
            {
                requestName = requestName.Substring(0, requestName.Length - suffix.Length);
            }

            path = _endpointSchema.ApiPathPrefix + path != null
                ? $"/{path}/{requestName}"
                : $"/{requestName}";
            if (_endpointSchema.ApiPathToLower) path = path.ToLower();
        }
        if (_customEndpointSchema != null)
        {
            try
            {
                path = _customEndpointSchema.DeterminePath(requestType, path);
            }
            catch (Exception ex)
            {
                throw new MetalNexusException($"Failed to determine path for {requestType}", ex);
            }
        }
        path = path.Replace('\\', '/');
        if (!path.StartsWith('/')) path = "/" + path;

        bool hasPathParams = path.Contains('{');
        if (hasPathParams && !IsValidBracketUrlParsing(path, requestType))
        {
            throw new MetalNexusException($"Path has invalid brackets for {requestType}");
        }

        return (path, hasPathParams);
    }
    internal static bool IsValidBracketUrlParsing(string value, Type type)
    {
        var propNames = type
            .GetProperties()
            .Where(_ => _.PropertyType.IsSimpleType())
            .Select(_ => _.Name)
            .ToList();

        int bracketStart = -1;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] == '{')
            {
                if (bracketStart != -1) return false;
                if (i > 0 && !value[i - 1].In('/', '\\')) return false;
                bracketStart = i;
            }
            else if (value[i] == '}')
            {
                if (bracketStart == -1) return false;
                if (i != value.Length - 1 && !value[i + 1].In('/', '\\')) return false;
                var name = value.Substring(bracketStart + 1, i - bracketStart - 1);
                if (!propNames.Contains(name)) return false;
                bracketStart = -1;
            }
        }
        return bracketStart == -1;
    }

    internal string DetermineTag(Type requestType, ApiRequestAttribute attribute, string path)
    {
        string? tag = attribute.Tag;
        if (tag == null)
        {
            tag = path;
            if (tag.ToLower().StartsWith(_endpointSchema.ApiPathPrefix.ToLower()))
                tag = tag.Substring(_endpointSchema.ApiPathPrefix.Length);
            var parts = tag.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                tag = string.Join('/', parts.Take(parts.Length - 1).Select(_ => _.CapFirst()));
            else
                tag = path.Trim('/').CapFirst()!;
        }
        if (_customEndpointSchema != null)
        {
            try
            {
                path = _customEndpointSchema.DeterminePath(requestType, tag);
            }
            catch (Exception ex)
            {
                throw new MetalNexusException($"Failed to determine path for {requestType}", ex);
            }
        }
        return tag;
    }

    internal (HttpMethod, bool) DetermineHttpProtocol(Type requestType, ApiRequestAttribute attribute)
    {
        var requestProperties = requestType.GetProperties();
        var requestMustUseBody =
            requestType.IsAssignableTo(typeof(MetalNexusFileRequest)) ||
            requestProperties.Length > _endpointSchema.MaximumRequestParameters ||
            requestProperties.Any(requestProperty =>
            {
                if (requestProperty.PropertyType.IsArray)
                {
                    return requestProperty.PropertyType.GetArrayRank() > 1 ||
                        !requestProperty.PropertyType.GetElementType()!.IsSimpleType();
                }
                else
                {
                    return !requestProperty.PropertyType.IsSimpleType();
                }
            });

        HttpProtocol httpProtocol = attribute.HttpProtocol;
        if (httpProtocol == HttpProtocol.Auto)
        {
            if (requestMustUseBody)
            {
                httpProtocol = HttpProtocol.PostViaBody;
            }
            else if (_endpointSchema.DefaultHttpProtocol != HttpProtocol.Auto)
            {
                httpProtocol = _endpointSchema.DefaultHttpProtocol;
            }
            else
            {
                httpProtocol = HttpProtocol.Get;
            }
        }
        if (_customEndpointSchema != null)
        {
            try
            {
                httpProtocol = _customEndpointSchema.DetermineHttpProtocol(requestType, httpProtocol);
            }
            catch (Exception ex)
            {
                throw new MetalNexusException($"Failed to determine HttpMethod or if using query params for {requestType}", ex);
            }
        }
        if (requestMustUseBody && httpProtocol.UsesQueryParams())
        {
            throw new MetalNexusException($"{requestType.Name} cannot use query params with a complex request or one that contains files");
        }
        return (httpProtocol.ToHttpMethod(), httpProtocol.UsesQueryParams());
    }

    internal (bool, string[]?) DetermineAuthentication(Type requestType, ApiRequestAttribute attribute, AuthenticatedAttribute? authAttribute)
    {
        bool requiresAuthentication = _endpointSchema.RequiresAuthenticationByDefault;
        string[]? authorizedRoles = null;
        if (authAttribute != null)
        {
            if (requestType.HasAttribute<AnonymousAttribute>())
            {
                throw new MetalNexusException($"Request {requestType.Name} cannot have both Authenticated and Anonymous attributes");
            }
            requiresAuthentication = true;
            authorizedRoles = authAttribute.AuthorizedRoles;
        }
        else if (requestType.HasAttribute<AnonymousAttribute>())
        {
            requiresAuthentication = false;
        }
        if (_customEndpointSchema != null)
        {
            try
            {
                requiresAuthentication = _customEndpointSchema.DetermineRequiresAuthentication(requestType, requiresAuthentication);
                if (requiresAuthentication)
                {
                    authorizedRoles = _customEndpointSchema.DetermineAuthorizedRoles(requestType, authorizedRoles);
                }
            }
            catch (Exception ex)
            {
                throw new MetalNexusException($"Failed to determine authentication requirement for {requestType}", ex);
            }
        }
        return (requiresAuthentication, authorizedRoles);
    }

    internal class Endpoint : IEndpoint
    {
        public string Path { get; set; } = null!;
        public HttpMethod HttpMethod { get; set; } = null!;
        public bool RequestAsQueryParams { get; set; }
        public bool HasPathParams { get; set; }
        public string? HttpClientName { get; set; }
        public Type RequestType { get; set; } = null!;
        public Type? ResponseType { get; set; }
        public string? Tag { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string[]? AuthorizedRoles { get; set; }
        public bool AllowProvisional { get; set; }
        public TimeSpan? HttpClientTimeout { get; set; }
        public string[] HeaderProperties { get; set; } = [];
    }
}
