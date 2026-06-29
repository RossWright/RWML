using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Returns a customer, forwarding X-Correlation-Id as a request header.</summary>
[ApiRequest]
[Anonymous]
[ProducesError<CustomerNotFoundException>]
public class GetCustomerWithCorrelationRequest : IRequest<CustomerDto>
{
    public int Id { get; set; }

    [FromHeader("X-Correlation-Id")]
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
