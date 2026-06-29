using RossWright.MetalNexus.Testbed.Shared;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>
/// Adds a text note to a customer using HTTP PATCH.
/// Demonstrates PatchViaBody and authenticated endpoints.
/// </summary>
internal class AddCustomerNoteHandler(InMemoryRepository repo)
    : IRequestHandler<AddCustomerNoteRequest, CustomerNoteDto>
{
    public Task<CustomerNoteDto> Handle(AddCustomerNoteRequest request, CancellationToken cancellationToken)
    {
        var note = repo.AddNote(request.CustomerId, request.Text)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        return Task.FromResult(new CustomerNoteDto
        {
            Id = note.Id,
            CustomerId = note.CustomerId,
            Text = note.Text,
            CreatedAt = note.CreatedAt
        });
    }
}
