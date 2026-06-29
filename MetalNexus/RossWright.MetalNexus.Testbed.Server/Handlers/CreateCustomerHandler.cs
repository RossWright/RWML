using System.Net;
using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

internal class CreateCustomerHandler(InMemoryRepository repo, IMetalNexusResponseContext responseContext)
    : IRequestHandler<CreateCustomerRequest, CustomerDto>
{
    public Task<CustomerDto> Handle(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (repo.EmailExists(request.Email))
            throw new DuplicateEmailException(request.Email);

        var customer = repo.CreateCustomer(request.Name, request.Email, request.Phone);

        responseContext.StatusCode = HttpStatusCode.Created;
        responseContext.Location = $"/api/getcustomer?id={customer.Id}";

        return Task.FromResult(CustomerMapping.ToDto(customer));
    }
}
