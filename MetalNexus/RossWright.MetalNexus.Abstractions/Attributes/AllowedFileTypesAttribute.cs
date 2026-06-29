namespace RossWright.MetalNexus;

/// <summary>
/// Restricts uploaded files to a specified set of MIME content types on a
/// <see cref="MetalNexusFileRequest"/>-derived endpoint.
/// </summary>
/// <remarks>
/// <para>
/// When applied to the <b>request class</b>, the restriction applies to every file in the request —
/// both files in the anonymous <see cref="MetalNexusFileRequest.Files"/> array and any named
/// <see cref="FileSlotAttribute"/> slots that do not carry their own <see cref="AllowedFileTypesAttribute"/>.
/// </para>
/// <para>
/// When applied to a <b><see cref="FileSlotAttribute"/> property</b>, the restriction applies only to
/// that slot and overrides any class-level restriction for that file.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [AllowedFileTypes("image/jpeg", "image/png", "image/webp")]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public class AllowedFileTypesAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="AllowedFileTypesAttribute"/> with the specified MIME types.
    /// </summary>
    /// <param name="mimeTypes">
    /// One or more permitted MIME content-type strings, e.g. <c>"image/jpeg"</c>, <c>"application/pdf"</c>.
    /// Comparisons are case-insensitive.
    /// </param>
    public AllowedFileTypesAttribute(params string[] mimeTypes) => MimeTypes = mimeTypes;

    /// <summary>The permitted MIME content-type strings.</summary>
    public string[] MimeTypes { get; }
}
