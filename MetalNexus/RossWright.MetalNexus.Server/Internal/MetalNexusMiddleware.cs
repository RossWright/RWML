using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace RossWright.MetalNexus.Server;

internal class MetalNexusMiddleware
{
    public MetalNexusMiddleware(
        IMetalNexusOptions options,
        bool authServiceInstalled,
        IEnumerable<IEndpoint> handledEndpoints)
    {
        this.options = options;        
        this.authServiceInstalled = authServiceInstalled;

        endpointByPath = new Dictionary<(string method, string path), IEndpoint>();
        pathParamEndpoints = new List<(string Method, string Path, IEndpoint Endpoint)>();
        foreach (var endpoint in handledEndpoints)
        {
            var pathKey = endpoint.Path
                .Replace('\\', '/')
                .TrimStart('/')
                .TrimEnd('/')
                .ToLower();
            if (endpoint.HasPathParams)
                pathParamEndpoints.Add(new(
                    endpoint.HttpMethod.ToString(),
                    CollapseBrackets(pathKey),
                    endpoint));
            else
                endpointByPath.Add((endpoint.HttpMethod.ToString(), pathKey), endpoint);
        }
        pathParamEndpoints = pathParamEndpoints
            .OrderBy(_ => _.Path.Take(_.Path.IndexOf('{')).Count(_ => _ == '/'))
            .ToList();
    }
    private readonly IMetalNexusOptions options;
    private readonly Dictionary<(string method, string path), IEndpoint> endpointByPath;
    private readonly List<(string Method, string Path, IEndpoint Endpoint)> pathParamEndpoints;
    private readonly bool authServiceInstalled = false;

    public async Task Handle(HttpContext ctx, RequestDelegate next)
    {
        if (ctx.Request.Path.HasValue)
        {
            var cleanPath = CleanPath(ctx.Request.Path.Value);
            if (endpointByPath.TryGetValue(
                (ctx.Request.Method, cleanPath),
                out var endpoint))
            {
                await HandleRequest(ctx, endpoint, authServiceInstalled, options);
                return;
            }
            foreach (var pathParamEndpoint in pathParamEndpoints)
            {
                if (pathParamEndpoint.Method == ctx.Request.Method &&
                    IsBracketUrlMatch(pathParamEndpoint.Path, cleanPath))
                {
                    await HandleRequest(ctx, pathParamEndpoint.Endpoint, authServiceInstalled, options);
                    return;
                }
            }
        }
        await next.Invoke(ctx);
    }


    internal static string CleanPath(string path) =>
        path.Replace('\\', '/').TrimStart('/').TrimEnd('/').ToLower();

    internal static async Task HandleRequest(HttpContext ctx, IEndpoint endpoint, 
        bool authServiceInstalled, IMetalNexusOptions options)
    {
        try
        {
            if (endpoint.RequiresAuthentication)
            {
                if (!authServiceInstalled)
                {
                    ctx.Response.StatusCode = 401;
                    return;
                }

                var authResult = await ctx.AuthenticateAsync();
                if (!authResult.Succeeded)
                {
                    ctx.Response.StatusCode = 401;
                    return;
                }

                if (ctx.User.HasClaim("Provisional", "true") &&
                    !endpoint.AllowProvisional)
                {
                    ctx.Response.StatusCode = 401;
                    return;
                }

                if (false == endpoint.AuthorizedRoles?.Any(_ => ctx.User.IsInRole(_)))
                {
                    ctx.Response.StatusCode = 403;
                    return;
                }
            }

            object request = await HydrateRequest(ctx, endpoint);

            var mediator = ctx.RequestServices.GetRequiredService<IMediator>();
            var response = await mediator.Send(request);

            ctx.Response.StatusCode = 200;
            var responseType = endpoint.ResponseType;
            if (responseType != null)
            {
                if (response is MetalNexusFile fileResponse)
                {
                    ctx.Response.ContentType = fileResponse.ContentType;
                    ctx.Response.ContentLength = fileResponse.Data.Length;
                    ctx.Response.Headers.ContentDisposition =
                        $"{(fileResponse.IsAttachment ? "attachment" : "inline")}; filename=\"{fileResponse.FileName}\"";
                    await ctx.Response.BodyWriter.WriteAsync(fileResponse.Data);
                    await ctx.Response.Body.FlushAsync();
                }
                else
                {
                    var json = JsonSerializer.Serialize(response);
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.ContentLength = json.Length;
                    await ctx.Response.WriteAsync(json);
                }
            }
        }
        catch (Exception exception)
        {
            if (exception is TargetInvocationException) exception = exception.InnerException!;
            var exceptionResponse = new ExceptionResponse(exception, 
                options.ServerStackTraceOnExceptionsIncluded,
                options.DefaultToBadRequest);
            ctx.Response.StatusCode = (int)exceptionResponse.StatusCode;
            var json = JsonSerializer.Serialize(exceptionResponse);
            ctx.Response.ContentType = "application/json";
            ctx.Response.ContentLength = json.Length;
            await ctx.Response.WriteAsync(json);
        }
    }

    internal static async Task<object> HydrateRequest(HttpContext ctx, IEndpoint endpoint)
    {
        var requestType = endpoint.RequestType;
        object request = null!;
        if (endpoint.RequestAsQueryParams)
        {
            request = Activator.CreateInstance(requestType,
                BindingFlags.CreateInstance | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                null, null, CultureInfo.CurrentCulture)!;
            foreach (var requestProperty in requestType.GetProperties()
                .Where(_ => _.DeclaringType != typeof(MetalNexusFileRequest)))
            {
                ResolveQueryParam(requestProperty, request, ctx.Request.Query);
            }
            if (ctx.Request.HasFormContentType &&
                endpoint.RequestType.IsAssignableTo(typeof(MetalNexusFileRequest)))
            {
                var uploadAttr = endpoint.RequestType.GetCustomAttribute<UploadLimitAttribute>();
                if (uploadAttr != null)
                {
                    ctx.Features.Get<IHttpMaxRequestBodySizeFeature>()!
                        .MaxRequestBodySize = uploadAttr.ByteLimit;
                }

                List<MetalNexusFile> files = new();
                foreach (var formFile in ctx.Request.Form.Files)
                {
                    using var memoryStream = new MemoryStream((int)formFile.Length);
                    await formFile.CopyToAsync(memoryStream);
                    files.Add(new MetalNexusFile
                    {
                        ContentType = formFile.ContentType,
                        FileName = formFile.FileName,
                        Data = memoryStream.ToArray()
                    });
                }
                ((MetalNexusFileRequest)request).Files = files.ToArray();
            }
        }
        else
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var body = await reader.ReadToEndAsync();
            if (requestType.IsAssignableTo(typeof(IMetalNexusRawRequest)))
            {
                request = MetalInjection.ActivatorUtilities
                    .CreateInstance(ctx.RequestServices, requestType);
                ((IMetalNexusRawRequest)request).RawRequestBody = body;
            }
            else
            {
                request = JsonSerializer.Deserialize(body, requestType, jsonOptions)!;
            }
        }

        if (endpoint.HasPathParams)
        {
            FillSlotValues(endpoint, ctx.Request.Path, request);
        }

        foreach (var headerProperty in endpoint.HeaderProperties)
        {
            var propInfo = requestType.GetProperty(headerProperty)!;
            var headerName = propInfo.GetCustomAttribute<FromHeaderAttribute>()!.HeaderName;
            var headerValue = ctx.Request.Headers[headerName].ToString();
            propInfo.SetValue(request, headerValue);
        }

        return request;
    }

    private static JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    internal static void ResolveQueryParam(PropertyInfo property, object obj, IQueryCollection queryCollection)
    {
        if (!property.PropertyType.IsSimpleType() && !property.PropertyType.IsArray)
        {
            var value = Activator.CreateInstance(property.PropertyType)!;
            foreach (var paramProp in property.PropertyType.GetProperties())
            {
                ResolveQueryParam(paramProp, value, queryCollection);
            }
            property.SetValue(obj, value);
        }
        else if (queryCollection.TryGetValue(property.Name, out var queryValues))
        {
            if (property.PropertyType.IsArray)
            {
                var elementType = property.PropertyType.GetElementType()!;
                var array = Array.CreateInstance(elementType, queryValues.Count);
                var converter = TypeDescriptor.GetConverter(elementType);
                for (var i = 0; i < queryValues.Count; i++)
                    array.SetValue(converter.ConvertFromString(queryValues[i]!), i);
                property.SetValue(obj, array);
            }
            else if (property.PropertyType.IsSimpleType())
            {
                var propValue = TypeDescriptor
                    .GetConverter(property.PropertyType)
                    .ConvertFromString(queryValues[0]!);
                property.SetValue(obj, propValue);
            }
        }
    }

    internal static void FillSlotValues(IEndpoint endpoint, string path, object obj)
    {
        var splitPropPath = endpoint.Path.Split('/');
        var splitValuePath = path.Split('/');
        if (splitPropPath.Length != splitValuePath.Length)
            throw new MetalNexusException("Bracketed Path Mismatch");
        foreach ((var slot, var index) in splitPropPath
            .WithIndex().Where(_ => _.item.StartsWith('{')))
        {
            var propName = slot.Trim('{', '}');
            var property = endpoint.RequestType.GetProperty(propName)!;
            var propValue = TypeDescriptor
                .GetConverter(property.PropertyType)
                .ConvertFromString(splitValuePath[index]);
            property.SetValue(obj, propValue);
        }
    }

    internal static string CollapseBrackets(string input)
    {
        StringBuilder key = new();
        bool inBrackets = false;
        foreach (var c in input)
        {
            if (c == '{')
            {
                key.Append('{');
                inBrackets = true;
            }
            else if (c == '}')
            {
                key.Append('}');
                inBrackets = false;
            }
            else if (!inBrackets)
            {
                key.Append(c);
            }
        }
        return key.ToString();
    }

    internal static bool IsBracketUrlMatch(string candidate, string received)
    {
        var splitReceivedPath = received.Split('/');
        var splitCandidatePath = candidate.Split('/');
        return splitReceivedPath.Length == splitCandidatePath.Length &&
            splitCandidatePath
                .Zip(splitReceivedPath)
                .All(_ => _.First.StartsWith('{') ||
                          _.First.ToLower() == _.Second.ToLower());
    }
}
