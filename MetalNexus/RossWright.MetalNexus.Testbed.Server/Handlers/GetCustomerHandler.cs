using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

internal class GetCustomerHandler(InMemoryRepository repo)
    : IRequestHandler<GetCustomerRequest, CustomerDto>
{
    public Task<CustomerDto> Handle(GetCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = repo.GetCustomer(request.Id)
            ?? throw new CustomerNotFoundException(request.Id);
        return Task.FromResult(CustomerMapping.ToDto(customer));
    }
}
