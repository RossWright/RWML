namespace RossWright.MetalNexus;

/// <summary>
/// Sets a maximum allowed upload size (in bytes) for multipart file upload endpoints.
/// </summary>
/// <remarks>
/// Apply this attribute to a <see cref="MetalNexusFileRequest"/>-derived request type to
/// override the server-wide multipart body length limit configured via
/// <c>SetMultipartBodyLengthLimit</c>.
/// To remove the limit entirely, use <see cref="NoUploadLimitAttribute"/> instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UploadLimitAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="UploadLimitAttribute"/> with the specified byte limit.
    /// </summary>
    /// <param name="byteLimit">The maximum number of bytes allowed in the multipart request body.</param>
    public UploadLimitAttribute(long byteLimit) => ByteLimit = byteLimit;
    /// <summary>Protected constructor used by <see cref="NoUploadLimitAttribute"/> to signal no limit.</summary>
    /// <param name="byteLimit"><c>null</c> to indicate no limit.</param>
    protected UploadLimitAttribute(long? byteLimit) => ByteLimit = byteLimit;
    /// <summary>
    /// The maximum number of bytes permitted in the multipart request body, or <c>null</c> when
    /// there is no limit (set by <see cref="NoUploadLimitAttribute"/>).
    /// </summary>
    public long? ByteLimit { get; }
}
