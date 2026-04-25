namespace RossWright.MetalNexus;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UploadLimitAttribute : Attribute
{
    public UploadLimitAttribute(long byteLimit) => ByteLimit = byteLimit;
    protected UploadLimitAttribute(long? byteLimit) => ByteLimit = byteLimit;
    public long? ByteLimit { get; }
}