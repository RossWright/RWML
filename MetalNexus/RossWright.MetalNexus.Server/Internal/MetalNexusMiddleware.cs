using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalChain;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

                if (endpoint.AuthorizedRoles?.Length > 0 &&
                    false == endpoint.AuthorizedRoles.Any(_ => ctx.User.IsInRole(_)))
                {
                    ctx.Response.StatusCode = 403;
                    return;
                }

                if (endpoint.AuthorizationPolicy != null)
                {
                    var authorizationService = ctx.RequestServices.GetRequiredService<IAuthorizationService>();
                    var policyResult = await authorizationService.AuthorizeAsync(ctx.User, endpoint.AuthorizationPolicy);
                    if (!policyResult.Succeeded)
                    {
                        ctx.Response.StatusCode = 403;
                        return;
                    }
                }
            }

            object request = await HydrateRequest(ctx, endpoint);

            if (endpoint.RequestType.IsAssignableTo(typeof(MetalNexusFileRequest)))
                ValidateFiles(endpoint.RequestType, (MetalNexusFileRequest)request);

            var mediator = ctx.RequestServices.GetRequiredService<IMediator>();

            object? response;
            using (MetalNexusRequestContext.Begin(ctx.Request))
            using (MetalNexusResponseContext.Begin())
            {
                response = await mediator.Send(request);

                // Success path only — read context before the Begin() scope is cleared.
                // Option D (ambient, set by handler) wins; fall back to Option E (attribute); fall back to 200.
                var ambientCtx = MetalNexusResponseContext.Current;
                var successCode = ambientCtx?.IsStatusCodeSet == true
                    ? (int)ambientCtx.StatusCode
                    : endpoint.SuccessStatusCode.HasValue
                        ? (int)endpoint.SuccessStatusCode.Value
                        : 200;
                ctx.Response.StatusCode = successCode;

                var location = ambientCtx?.Location;
                if (!string.IsNullOrEmpty(location))
                    ctx.Response.Headers.Location = location;
            }

            var responseType = endpoint.ResponseType;
            if (responseType != null)
            {
                if (response is MetalNexusFile fileResponse)
                {
                    ctx.Response.ContentType = fileResponse.ContentType;
                    ctx.Response.Headers.ContentDisposition =
                        $"{(fileResponse.IsAttachment ? "attachment" : "inline")}; filename=\"{fileResponse.FileName}\"";
                    if (fileResponse.DataStream != null)
                    {
                        await using var stream = fileResponse.DataStream;
                        await stream.CopyToAsync(ctx.Response.Body);
                        await ctx.Response.Body.FlushAsync();
                    }
                    else
                    {
                        ctx.Response.ContentLength = fileResponse.Data!.Length;
                        await ctx.Response.BodyWriter.WriteAsync(fileResponse.Data);
                        await ctx.Response.Body.FlushAsync();
                    }
                }
                else if (response is IMetalNexusRawResponse rawResponse)
                {
                    ctx.Response.ContentType = rawResponse.ContentType;
                    if (rawResponse.DataStream != null)
                    {
                        await using var stream = rawResponse.DataStream;
                        await stream.CopyToAsync(ctx.Response.Body);
                        await ctx.Response.Body.FlushAsync();
                    }
                    else if (rawResponse.Data != null)
                    {
                        ctx.Response.ContentLength = rawResponse.Data.Length;
                        await ctx.Response.BodyWriter.WriteAsync(rawResponse.Data);
                        await ctx.Response.Body.FlushAsync();
                    }
                }
                else
                {
                    var json = JsonSerializer.Serialize(response);
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
                    await ctx.Response.WriteAsync(json);
                }
            }
        }
        catch (Exception exception)
        {
            if (exception is TargetInvocationException tie && tie.InnerException != null)
                exception = tie.InnerException;
            var exceptionResponse = new ExceptionResponse(exception,
                options.ServerStackTraceOnExceptionsIncluded,
                options.DefaultToBadRequest);
            ctx.Response.StatusCode = (int)exceptionResponse.StatusCode;

            var isMetalNexusClient =
                ctx.Request.Headers.TryGetValue(MetalNexusConstants.ClientHeader, out _);

            if (isMetalNexusClient)
            {
                var json = JsonSerializer.Serialize(exceptionResponse);
                ctx.Response.ContentType = "application/json";
                ctx.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
                await ctx.Response.WriteAsync(json);
            }
            else
            {
                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = (int)exceptionResponse.StatusCode,
                    Title = exceptionResponse.TypeName
                        ?.Split('.')?.Last()
                        ?? "Error",
                    Detail = exceptionResponse.Message,
                    Type = $"https://httpstatuses.com/{(int)exceptionResponse.StatusCode}",
                };
                var json = JsonSerializer.Serialize(problemDetails);
                ctx.Response.ContentType = "application/problem+json";
                ctx.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
                await ctx.Response.WriteAsync(json);
            }
        }
    }

    internal static async Task<object> HydrateRequest(HttpContext ctx, IEndpoint endpoint)
    {
        var requestType = endpoint.RequestType;
        object request = null!;
        if (endpoint.RequestAsQueryParams)
        {
            var excludedProps = requestType.GetProperties()
                .Where(p => p.DeclaringType == typeof(MetalNexusFileRequest) ||
                            p.IsDefined(typeof(FromHeaderAttribute), false) ||
                            p.IsDefined(typeof(FileSlotAttribute), false))
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var jsonObj = BuildJsonObjectFromQuery(
                ctx.Request.Query, requestType, excludedProps);
            request = JsonSerializer.Deserialize(jsonObj, requestType, queryParamJsonOptions)!;

            if (ctx.Request.HasFormContentType &&
                endpoint.RequestType.IsAssignableTo(typeof(MetalNexusFileRequest)))
            {
                var uploadAttr = endpoint.RequestType.GetCustomAttribute<UploadLimitAttribute>();
                if (uploadAttr != null)
                {
                    var bodySizeFeature = ctx.Features.Get<IHttpMaxRequestBodySizeFeature>();
                    if (bodySizeFeature != null)
                        bodySizeFeature.MaxRequestBodySize = uploadAttr.ByteLimit;
                }

                // Build a lookup of [FileSlot] properties keyed by slot name (case-insensitive).
                var slotProperties = requestType.GetProperties()
                    .Select(p => (Property: p, Slot: p.GetCustomAttribute<FileSlotAttribute>()))
                    .Where(x => x.Slot != null)
                    .ToDictionary(x => x.Slot!.Name, x => x.Property, StringComparer.OrdinalIgnoreCase);

                List<MetalNexusFile> files = new();
                foreach (var formFile in ctx.Request.Form.Files)
                {
                    var metalNexusFile = new MetalNexusFile
                    {
                        ContentType = formFile.ContentType,
                        FileName = formFile.FileName,
                        DataStream = formFile.OpenReadStream()
                    };

                    if (slotProperties.TryGetValue(formFile.Name, out var slotProp))
                        slotProp.SetValue(request, metalNexusFile);
                    else
                        files.Add(metalNexusFile);
                }
                ((MetalNexusFileRequest)request).Files = files.ToArray();
            }
        }
        else
        {
            using var reader = new StreamReader(ctx.Request.Body);
            var body = await reader.ReadToEndAsync(ctx.RequestAborted);
            if (requestType.IsAssignableTo(typeof(IMetalNexusRawRequest)))
            {
                request = MetalInjection.ActivatorUtilities
                    .CreateInstance(ctx.RequestServices, requestType);
                ((IMetalNexusRawRequest)request).RawRequestBody = body;
            }
            else if (requestType.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IMetalNexusRawRequest<>)))
            {
                request = MetalInjection.ActivatorUtilities
                    .CreateInstance(ctx.RequestServices, requestType);
                requestType.GetProperty(nameof(IMetalNexusRawRequest.RawRequestBody))!
                    .SetValue(request, body);
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

    private static JsonSerializerOptions queryParamJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    // Builds a JsonObject from the query string so that JsonSerializer.Deserialize
    // can populate both regular classes and init-only / record request types.
    // Nested objects are handled by projecting each property's own sub-type.
    internal static JsonObject BuildJsonObjectFromQuery(
        IQueryCollection query, Type type, IReadOnlySet<string>? excludedProps = null)
    {
        var node = new JsonObject();
        foreach (var property in type.GetProperties())
        {
            if (excludedProps != null && excludedProps.Contains(property.Name))
                continue;

            if (!property.PropertyType.IsSimpleType() && !property.PropertyType.IsArray)
            {
                node[property.Name] = BuildJsonObjectFromQuery(query, property.PropertyType);
            }
            else if (query.TryGetValue(property.Name, out var values))
            {
                if (property.PropertyType.IsArray)
                {
                    var arr = new JsonArray();
                    foreach (var v in values)
                        arr.Add(JsonValue.Create(v));
                    node[property.Name] = arr;
                }
                else
                {
                    node[property.Name] = JsonValue.Create(values[0]);
                }
            }
        }
        return node;
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

    internal static void ValidateFiles(Type requestType, MetalNexusFileRequest request)
    {
        var classMaxSize = requestType.GetCustomAttribute<MaxFileSizeAttribute>();
        var classAllowedTypes = requestType.GetCustomAttribute<AllowedFileTypesAttribute>();
        var maxCountAttr = requestType.GetCustomAttribute<MaxFileCountAttribute>();

        var errors = new List<string>();

        // Validate each [FileSlot] property, with property-level attrs overriding class-level.
        var slotProperties = requestType.GetProperties()
            .Select(p => (Property: p, Slot: p.GetCustomAttribute<FileSlotAttribute>()))
            .Where(x => x.Slot != null);

        int totalFileCount = request.Files?.Length ?? 0;

        foreach (var (prop, slot) in slotProperties)
        {
            var file = prop.GetValue(request) as MetalNexusFile;
            if (file == null) continue;

            totalFileCount++;

            var effectiveMaxSize = prop.GetCustomAttribute<MaxFileSizeAttribute>() ?? classMaxSize;
            var effectiveAllowedTypes = prop.GetCustomAttribute<AllowedFileTypesAttribute>() ?? classAllowedTypes;

            ValidateSingleFile(file, slot!.Name, effectiveMaxSize, effectiveAllowedTypes, errors);
        }

        // Validate unnamed files in Files[] against class-level attributes.
        if (request.Files != null)
        {
            foreach (var file in request.Files)
                ValidateSingleFile(file, file.FileName, classMaxSize, classAllowedTypes, errors);
        }

        // Validate total file count.
        if (maxCountAttr != null && totalFileCount > maxCountAttr.MaxCount)
            errors.Add($"Too many files: {totalFileCount} uploaded, maximum is {maxCountAttr.MaxCount}.");

        if (errors.Count > 0)
            throw new System.ComponentModel.DataAnnotations.ValidationException(
                string.Join(" ", errors));
    }

    private static void ValidateSingleFile(
        MetalNexusFile file,
        string label,
        MaxFileSizeAttribute? maxSizeAttr,
        AllowedFileTypesAttribute? allowedTypesAttr,
        List<string> errors)
    {
        if (maxSizeAttr != null && file.DataStream != null && file.DataStream.Length > maxSizeAttr.MaxBytes)
            errors.Add($"File '{label}' exceeds the maximum allowed size of {maxSizeAttr.MaxBytes:N0} bytes.");

        if (allowedTypesAttr != null &&
            !allowedTypesAttr.MimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            errors.Add($"File '{label}' has content type '{file.ContentType}', which is not permitted. " +
                       $"Allowed types: {string.Join(", ", allowedTypesAttr.MimeTypes)}.");
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
