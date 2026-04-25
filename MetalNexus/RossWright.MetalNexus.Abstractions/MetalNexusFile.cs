namespace RossWright.MetalNexus;

public class MetalNexusFileRequest
{
    public MetalNexusFile[] Files { get; set; } = null!;
}

public class MetalNexusFile
{
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
    public bool IsAttachment { get; set; } = true;
}
