namespace RossWright.MetalNexus;

public class InternalServerErrorException : Exception
{
    public InternalServerErrorException() { }
    public InternalServerErrorException(string? message) : base(message) { }
    public InternalServerErrorException(string? message, Exception? innerException) : base(message, innerException) { }
}
