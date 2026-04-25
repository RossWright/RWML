namespace RossWright.MetalNexus;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpClientTimeoutAttribute : Attribute
{
    public HttpClientTimeoutAttribute(int timeoutSeconds) => TimeoutSeconds = timeoutSeconds;
    public int TimeoutSeconds { get; }
}