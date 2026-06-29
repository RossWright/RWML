using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

internal class DeleteCustomerHandler(InMemoryRepository repo)
    : IRequestHandler<DeleteCustomerRequest>
{
    public Task Handle(DeleteCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!repo.DeleteCustomer(request.Id))
            throw new CustomerNotFoundException(request.Id);
        return Task.CompletedTask;
    }
}
