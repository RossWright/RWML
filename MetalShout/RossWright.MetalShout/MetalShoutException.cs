namespace RossWright.MetalShout;

public class MetalShoutException : Exception
{
    public MetalShoutException() { }
    public MetalShoutException(string message) : base(message) { }
    public MetalShoutException(string message, Exception innerException) : base(message, innerException) { }
}
