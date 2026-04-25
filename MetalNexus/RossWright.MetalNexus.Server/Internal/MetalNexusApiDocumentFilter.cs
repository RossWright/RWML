using Microsoft.OpenApi;
using RossWright.MetalNexus.Schemna;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace RossWright.MetalNexus;

internal class MetalNexusApiDocumentFilter(IMetalNexusRegistry _registry) : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        document.Tags = _registry.Endpoints
            .Select(_ => _.Tag)
            .Distinct()
            .Select(_ => new OpenApiTag { Name = _ })
            .ToHashSet();

        foreach (var requestApiInfo in _registry.Endpoints)
        {
            if (document.Paths.TryGetValue(requestApiInfo.Path, out var pathItem)) continue;
            
            pathItem = new OpenApiPathItem
            {
                Operations = new Dictionary<HttpMethod, OpenApiOperation>()
            };
            document.Paths.Add(requestApiInfo.Path, pathItem);
            
            var operation = new OpenApiOperation
            {
                Summary = requestApiInfo.RequestType.Name,
                Deprecated = requestApiInfo.RequestType.GetCustomAttribute<ObsoleteAttribute>() != null,
                Tags = requestApiInfo.Tag == null ? null :
                    new HashSet<OpenApiTagReference>()
                    {
                        new OpenApiTagReference(requestApiInfo.Tag!)
                    },
                Security = !requestApiInfo.RequiresAuthentication ? null : new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("MetalGuardian", document)] = new List<string>()
                    }
                },
                Responses = new OpenApiResponses()
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = MakeMediaDict(MakeSchema(requestApiInfo.ResponseType))
                    },
                    ["400"] = new OpenApiResponse
                    {
                        Description = "Error",
                        Content = MakeMediaDict(MakeSchema(typeof(string)))
                    }
                }
            };
            pathItem.Operations![requestApiInfo.HttpMethod] = operation;

            if (requestApiInfo.RequestAsQueryParams)
            {
                operation.Parameters = requestApiInfo.RequestType
                    .GetProperties()
                    .Where(property => property.DeclaringType != typeof(MetalNexusFileRequest))
                    .Select(property => 
                        new OpenApiParameter
                        {
                            Name = property.Name!,
                            In = ParameterLocation.Query,
                            Schema = MakeSchema(property.PropertyType),
                            Required = new NullabilityInfoContext().Create(property).WriteState is not NullabilityState.Nullable,
                        })
                    .ToList<IOpenApiParameter>();
            }
            else
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = MakeMediaDict(MakeSchema(requestApiInfo.RequestType))
                };
            }

            if (requestApiInfo.RequiresAuthentication)
            {
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized"
                });
            }
            if (requestApiInfo.AuthorizedRoles != null)
            {
                operation.Responses.Add("403", new OpenApiResponse
                {
                    Description = "Forbidden"
                });
            }
        
            IOpenApiSchema? MakeSchema(Type? type)
            {
                if (type == null) return null;
                return context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
            }

            Dictionary<string, OpenApiMediaType>? MakeMediaDict(IOpenApiSchema? schema)
            {
                if (schema == null) return null;
                var media = new OpenApiMediaType { Schema = schema };
                return new Dictionary<string, OpenApiMediaType>()
                {
                    ["application/json"] = media
                };
            }
        }
    }
}
