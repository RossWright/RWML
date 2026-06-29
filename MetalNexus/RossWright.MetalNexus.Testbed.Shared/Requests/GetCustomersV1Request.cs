using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Legacy list endpoint. Deprecated — use GetCustomersRequest instead.</summary>
[ApiRequest]
[Anonymous]
[Obsolete("Use GetCustomersRequest instead.")]
public class GetCustomersV1Request : IRequest<GetCustomersResponse> { }
