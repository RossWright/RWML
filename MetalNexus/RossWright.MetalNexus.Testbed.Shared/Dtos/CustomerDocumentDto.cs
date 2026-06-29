namespace RossWright.MetalNexus.Testbed.Shared;

public class CustomerDocumentDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
