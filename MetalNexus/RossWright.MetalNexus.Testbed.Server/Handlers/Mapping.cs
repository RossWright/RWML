using RossWright.MetalNexus.Testbed.Shared;

namespace RossWright.MetalNexus.Testbed.Server.Handlers;

/// <summary>Shared entity-to-DTO mapping helpers used by all handler files.</summary>
internal static class CustomerMapping
{
    public static CustomerDto ToDto(Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Email = c.Email,
        Phone = c.Phone,
        CreatedAt = c.CreatedAt,
        AvatarUrl = c.AvatarUrl,
        Notes = c.Notes.Select(n => new CustomerNoteDto
        {
            Id = n.Id,
            CustomerId = n.CustomerId,
            Text = n.Text,
            CreatedAt = n.CreatedAt
        }).ToList(),
        Documents = c.Documents.Select(d => new CustomerDocumentDto
        {
            Id = d.Id,
            CustomerId = d.CustomerId,
            FileName = d.FileName,
            ContentType = d.ContentType,
            UploadedAt = d.UploadedAt
        }).ToList()
    };
}
