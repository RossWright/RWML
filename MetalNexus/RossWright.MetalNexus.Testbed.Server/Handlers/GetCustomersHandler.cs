using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

internal class GetCustomersHandler(InMemoryRepository repo)
    : IRequestHandler<GetCustomersRequest, GetCustomersResponse>
{
    public Task<GetCustomersResponse> Handle(GetCustomersRequest request, CancellationToken cancellationToken)
    {
        var customers = repo.GetAllCustomers();
        return Task.FromResult(new GetCustomersResponse
        {
            Customers = customers.Select(CustomerMapping.ToDto).ToList()
        });
    }
}
