using RossWright.MetalChain;

namespace RossWright.MetalNexus;

public interface IMetalNexusRawRequest : IRequest
{
    string? RawRequestBody { get; set; }
}

public interface IMetalNexusRawRequest<out TResponse>
    : IRequest<TResponse>
{
    string? RawRequestBody { get; set; }
}