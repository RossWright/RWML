namespace RossWright.MetalNexus;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NoUploadLimitAttribute : UploadLimitAttribute
{
    public NoUploadLimitAttribute() : base(null) { }
}
