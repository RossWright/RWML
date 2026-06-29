namespace RossWright.MetalNexus;

/// <summary>
/// Removes the multipart upload size limit for this endpoint, allowing arbitrarily large uploads.
/// </summary>
/// <remarks>
/// This attribute overrides both the server-wide limit set via
/// <c>SetMultipartBodyLengthLimit</c> server option and any
/// <see cref="UploadLimitAttribute"/> that might otherwise apply.
/// Use with caution in production environments.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NoUploadLimitAttribute : UploadLimitAttribute
{
    /// <summary>Initializes a new <see cref="NoUploadLimitAttribute"/> with no byte limit.</summary>
    public NoUploadLimitAttribute() : base(null) { }
}
