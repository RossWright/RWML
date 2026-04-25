namespace RossWright.MetalNexus;

public class MetalNexusException : Exception
{
    public MetalNexusException() { }
    public MetalNexusException(string message) : base(message) { }
    public MetalNexusException(string message, Exception? inner = null) : base(message, inner) { }
}
