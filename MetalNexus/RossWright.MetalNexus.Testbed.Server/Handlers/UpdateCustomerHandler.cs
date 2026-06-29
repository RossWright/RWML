using System.Net;
using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

internal class UpdateCustomerHandler(InMemoryRepository repo, IMetalNexusResponseContext responseContext)
    : IRequestHandler<UpdateCustomerRequest, CustomerDto>
{
    public Task<CustomerDto> Handle(UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (repo.GetCustomer(request.Id) is null)
            throw new CustomerNotFoundException(request.Id);

        if (repo.EmailExists(request.Email, excludeId: request.Id))
            throw new DuplicateEmailException(request.Email);

        var customer = repo.UpdateCustomer(request.Id, request.Name, request.Email, request.Phone)
            ?? throw new CustomerNotFoundException(request.Id);

        responseContext.StatusCode = HttpStatusCode.OK;
        responseContext.Location = $"/api/getcustomer?id={customer.Id}";

        return Task.FromResult(CustomerMapping.ToDto(customer));
    }
}
