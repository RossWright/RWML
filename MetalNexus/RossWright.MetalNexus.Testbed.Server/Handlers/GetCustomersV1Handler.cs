using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Legacy list endpoint that simply delegates to the current implementation.
/// The [Obsolete] attribute on the request type causes MetalNexus to mark
/// this operation as deprecated in the Swagger/OpenAPI document
/// (deprecated: true) without any extra attributes required.
/// </summary>
#pragma warning disable CS0618 // intentionally implementing the deprecated request type
internal class GetCustomersV1Handler(InMemoryRepository repo)
    : IRequestHandler<GetCustomersV1Request, GetCustomersResponse>
{
    public Task<GetCustomersResponse> Handle(GetCustomersV1Request request, CancellationToken cancellationToken)
    {
        var customers = repo.GetAllCustomers();
        return Task.FromResult(new GetCustomersResponse
        {
            Customers = customers.Select(CustomerMapping.ToDto).ToList()
        });
    }
}
#pragma warning restore CS0618
