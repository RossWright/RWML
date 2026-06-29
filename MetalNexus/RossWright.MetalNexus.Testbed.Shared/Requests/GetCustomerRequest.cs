using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Returns a single customer by ID. Auto → GET with query param.</summary>
[ApiRequest]
[Anonymous]
[ProducesError<CustomerNotFoundException>]
public class GetCustomerRequest : IRequest<CustomerDto>
{
    public int Id { get; set; }
}
