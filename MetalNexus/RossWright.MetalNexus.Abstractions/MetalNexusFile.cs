namespace RossWright.MetalNexus;

/// <summary>
/// Base request type for endpoints that accept one or more file uploads via multipart form data.
/// </summary>
/// <remarks>
/// Derive from this class (or use it directly with <c>IRequest</c>) to expose a file-upload
/// endpoint.  MetalNexus deserializes each uploaded part into a <see cref="MetalNexusFile"/>
/// and populates <see cref="Files"/> before invoking the handler.
/// To control the maximum upload size, apply <see cref="UploadLimitAttribute"/> or
/// <see cref="NoUploadLimitAttribute"/> to the request class.
/// </remarks>
public class MetalNexusFileRequest
{
    /// <summary>The files received from the multipart upload, in order of submission.</summary>
    public MetalNexusFile[] Files { get; set; } = null!;
}

/// <summary>
/// Represents a single file, either received from an upload or returned as a download response.
/// </summary>
public class MetalNexusFile
{
    /// <summary>The MIME type of the file, e.g. <c>image/png</c> or <c>application/pdf</c>.</summary>
    public string ContentType { get; set; } = null!;
    /// <summary>The original file name as provided by the client.</summary>
    public string FileName { get; set; } = null!;
    /// <summary>
    /// The file data as a byte array. Populated when the file is received from a client upload
    /// or when the handler constructs a download response directly from bytes.
    /// Null when <see cref="DataStream"/> is set.
    /// </summary>
    public byte[]? Data { get; set; }
    /// <summary>
    /// When <c>true</c> (the default), the server sets <c>Content-Disposition: attachment</c>
    /// so browsers prompt a file save dialog.  Set to <c>false</c> to use
    /// <c>Content-Disposition: inline</c> and allow the browser to display the file.
    /// </summary>
    public bool IsAttachment { get; set; } = true;
    /// <summary>
    /// An optional stream for the file data. When set on an uploaded file, the handler can read
    /// directly from the underlying form stream without buffering the entire file into memory.
    /// Set <see cref="Data"/> instead when returning a download response from a handler.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Stream? DataStream { get; set; }
}
