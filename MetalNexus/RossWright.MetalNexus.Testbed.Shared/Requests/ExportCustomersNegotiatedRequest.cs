using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Returns customers as JSON or CSV based on the Accept header.</summary>
[ApiRequest]
[Anonymous]
public class ExportCustomersNegotiatedRequest : IRequest<IMetalNexusRawResponse> { }
