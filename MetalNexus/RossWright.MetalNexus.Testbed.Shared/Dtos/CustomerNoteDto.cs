namespace RossWright.MetalNexus.Testbed.Shared;

public class CustomerNoteDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
