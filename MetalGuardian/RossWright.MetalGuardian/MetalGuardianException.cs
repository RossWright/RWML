namespace RossWright.MetalGuardian;

public class MetalGuardianException : Exception
{
    public MetalGuardianException() { }
    public MetalGuardianException(string message) : base(message) { }
    public MetalGuardianException(string message, Exception innerException) : base(message, innerException) { }
}
