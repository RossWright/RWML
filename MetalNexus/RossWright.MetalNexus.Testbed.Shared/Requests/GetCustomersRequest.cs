using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Returns all customers. Auto → GET (0 props). No auth required.</summary>
[ApiRequest]
[Anonymous]
public class GetCustomersRequest : IRequest<GetCustomersResponse> { }

public class GetCustomersResponse
{
    public List<CustomerDto> Customers { get; init; } = [];
}
