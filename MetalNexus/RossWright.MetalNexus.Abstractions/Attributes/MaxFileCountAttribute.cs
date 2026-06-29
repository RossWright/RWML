namespace RossWright.MetalNexus;

/// <summary>
/// Enforces a maximum total number of uploaded files across the entire request on a
/// <see cref="MetalNexusFileRequest"/>-derived endpoint.
/// </summary>
/// <remarks>
/// The count includes all files: named <see cref="FileSlotAttribute"/> slots that are populated
/// plus every file in the anonymous <see cref="MetalNexusFileRequest.Files"/> array.
/// This attribute is class-level only — it has no meaningful per-slot interpretation.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MaxFileCountAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="MaxFileCountAttribute"/> with the specified maximum count.
    /// </summary>
    /// <param name="maxCount">The maximum total number of files allowed in the request.</param>
    public MaxFileCountAttribute(int maxCount) => MaxCount = maxCount;

    /// <summary>The maximum total number of files permitted across the entire request.</summary>
    public int MaxCount { get; }
}
