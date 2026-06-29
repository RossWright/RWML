namespace RossWright.MetalNexus;

/// <summary>
/// Marks a <see cref="MetalNexusFile"/> property on a <see cref="MetalNexusFileRequest"/>-derived
/// class as a named upload slot.
/// </summary>
/// <remarks>
/// The client sends the file using the slot name as the multipart form-field name.
/// The server routes the file to this property instead of the anonymous <see cref="MetalNexusFileRequest.Files"/> array.
/// Any files whose form-field name does not match a slot are still collected in <see cref="MetalNexusFileRequest.Files"/>.
/// </remarks>
/// <example>
/// <code>
/// [ApiRequest(HttpProtocol.PostViaQuery)]
/// public class UpdateProfileRequest : MetalNexusFileRequest, IRequest
/// {
///     public int UserId { get; set; }
///
///     [FileSlot("avatar")]  public MetalNexusFile? Avatar { get; set; }
///     [FileSlot("banner")]  public MetalNexusFile? Banner { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FileSlotAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="FileSlotAttribute"/> with the specified slot name.
    /// </summary>
    /// <param name="name">
    /// The multipart form-field name the client uses to send the file for this slot.
    /// Case-insensitive when matched on the server.
    /// </param>
    public FileSlotAttribute(string name) => Name = name;

    /// <summary>The form-field name that identifies this slot on the wire.</summary>
    public string Name { get; }
}
