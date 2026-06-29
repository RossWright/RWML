namespace RossWright.MetalNexus.Testbed.Shared;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public List<CustomerNoteDto> Notes { get; set; } = [];
    public List<CustomerDocumentDto> Documents { get; set; } = [];
    /// <summary>Echoed X-Correlation-Id, populated by GetCustomerWithCorrelationRequest.</summary>
    public string? CorrelationId { get; set; }
}
