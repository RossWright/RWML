using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

internal class GetCustomerWithCorrelationHandler(InMemoryRepository repo, IMetalNexusRequestContext requestContext)
    : IRequestHandler<GetCustomerWithCorrelationRequest, CustomerDto>
{
    public Task<CustomerDto> Handle(GetCustomerWithCorrelationRequest request, CancellationToken cancellationToken)
    {
        var customer = repo.GetCustomer(request.Id)
            ?? throw new CustomerNotFoundException(request.Id);

        var dto = CustomerMapping.ToDto(customer);
        // Echo the correlation ID back in the DTO so clients can confirm header round-trip
        _ = requestContext.RequestHeaders.TryGetValue("X-Correlation-Id", out var correlationId);
        dto.CorrelationId = correlationId ?? request.CorrelationId;
        return Task.FromResult(dto);
    }
}
