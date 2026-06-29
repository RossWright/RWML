namespace RossWright.MetalNexus;

/// <summary>
/// Represents a file selected by the user in a <c>FileInput</c> component before it has been
/// uploaded to the server.
/// </summary>
/// <remarks>
/// Instances are created by the JavaScript interop layer when the user picks files via the
/// browser's file picker.  Pass one or more <see cref="BrowserFile"/> instances to
/// <c>IFilesPickedArgs.UploadFiles</c> to transmit the selected files to a MetalNexus endpoint.
/// </remarks>
public class BrowserFile
{
    /// <summary>The original file name as reported by the browser.</summary>
    public string FileName { get; set; } = null!;
    /// <summary>The file size in bytes as reported by the browser.</summary>
    public int Size { get; set; }
    /// <summary>The MIME type of the file as reported by the browser, e.g. <c>image/png</c>.</summary>
    public string ContentType { get; set; } = null!;
    /// <summary>
    /// An opaque integer identifier assigned by the JavaScript interop layer to reference the
    /// in-memory browser <c>File</c> object.  Used internally by <c>FileInput</c> when uploading
    /// or previewing the file; not meaningful outside of the current browser session.
    /// </summary>
    public int FileRefId { get; set; }
}
