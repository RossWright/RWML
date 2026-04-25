namespace RossWright.MetalNexus;

[AttributeUsage(AttributeTargets.Property)]
public class FromHeaderAttribute(string headerName) : Attribute
{
    public string HeaderName => headerName;
}
