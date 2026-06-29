namespace RossWright.MetalNexus;

/// <summary>
/// Enforces a maximum byte size for each uploaded file on a <see cref="MetalNexusFileRequest"/>-derived endpoint.
/// </summary>
/// <remarks>
/// <para>
/// When applied to the <b>request class</b>, the limit applies to every file in the request —
/// both files in the anonymous <see cref="MetalNexusFileRequest.Files"/> array and any named
/// <see cref="FileSlotAttribute"/> slots that do not carry their own <see cref="MaxFileSizeAttribute"/>.
/// </para>
/// <para>
/// When applied to a <b><see cref="FileSlotAttribute"/> property</b>, the limit applies only to
/// that slot and overrides any class-level limit for that file.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public class MaxFileSizeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="MaxFileSizeAttribute"/> with the specified byte limit.
    /// </summary>
    /// <param name="maxBytes">The maximum number of bytes allowed for a single file.</param>
    public MaxFileSizeAttribute(long maxBytes) => MaxBytes = maxBytes;

    /// <summary>The maximum number of bytes permitted for a single uploaded file.</summary>
    public long MaxBytes { get; }
}
