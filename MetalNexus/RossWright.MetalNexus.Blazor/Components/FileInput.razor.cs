using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RossWright.MetalGuardian;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using System.Text.Json;

namespace RossWright.MetalNexus;

/// <summary>
/// Blazor WebAssembly file picker component that creates <see cref="BrowserFile"/> values for MetalNexus file upload endpoints.
/// </summary>
public partial class FileInput : IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private IJsScriptLoaderService ScriptLoader { get; set; } = null!;
    [Inject] private IMetalNexusRegistry MetalNexusRegistry { get; set; } = null!;
    [Inject] private IMetalNexusOptions MetalNexusOptions { get; set; } = null!;
    [Inject] private IMetalGuardianAuthenticationClient? authClient { get; set; } = null!;
    [Inject] private IMetalNexusUrlHelper UrlHelper { get; set; } = null!;

    /// <summary>When <c>true</c>, the file picker allows the user to select more than one file at a time.</summary>
    [Parameter] public bool AllowMultipleFiles { get; set; } = false;
    /// <summary>
    /// An optional comma-separated list of accepted MIME types or file extensions passed to the
    /// browser file input's <c>accept</c> attribute, e.g. <c>"image/*"</c> or <c>".pdf,.docx"</c>.
    /// </summary>
    [Parameter] public string? Accept { get; set; } //"image/*"

    private string Id = Guid.NewGuid().ToString();
    private DotNetObjectReference<FileInput> objRef = null!;
    private bool _filesSelected = false;

    private Task EnsureReady()
    {
        if (ready == null) ready = init();
        return ready;

        async Task init()
        {
            await ScriptLoader.EnsureLoaded(
                "https://code.jquery.com/jquery-3.7.0.min.js", "jQuery",
                "sha256-2Pmvv0kuTBOenSvLm6bvfBSSHrUJ+3A7x6P5Ebd07/g=");
            await ScriptLoader.EnsureLoaded(
                "_content/RossWright.MetalNexus.Blazor/Interop.js",
                "window.RossWrightFileInput");
            if (objRef == null) objRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("RossWrightFileInput.attach", objRef, Id);
        }
    }
    private Task ready = null!;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await EnsureReady();
        }
        if (pendingSuccess != null)
        {
            var result = pendingSuccess;
            pendingSuccess = null;
            await FilesUploaded.InvokeAsync(result);
        }
        else if (pendingError != null)
        {
            var error = pendingError;
            pendingError = null;
            await UploadFailed.InvokeAsync(error);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// Programmatically opens the browser's file picker dialog, as if the user clicked the
    /// underlying file input element.
    /// </summary>
    public async ValueTask OpenFilePicker()
    {
        await EnsureReady();
        _filesSelected = false;
        await JSRuntime.InvokeVoidAsync("RossWrightFileInput.click", Id);
    }

    /// <summary>Raised when the user selects one or more files in the file picker.</summary>
    [Parameter] public EventCallback<IFilesPickedArgs> FilesPicked { get; set; }
    /// <summary>Called by JavaScript interop when files are selected; not intended for direct use.</summary>
    [JSInvokable] public async Task OnFilesSelected(BrowserFile[] files)
    {
        _filesSelected = true;
        if (!isDisposed) await FilesPicked.InvokeAsync(new FilesPickedArgs(this, files));
    }

    /// <summary>
    /// Provides access to the files selected by the user and methods for previewing or uploading them.
    /// </summary>
    public interface IFilesPickedArgs
    {
        /// <summary>The files the user selected, in selection order.</summary>
        BrowserFile[] Files { get; }
        /// <summary>
        /// Displays a preview of an image from a URL inside an <c>&lt;img&gt;</c> or CSS
        /// background-image element identified by <paramref name="imgId"/>.
        /// </summary>
        /// <param name="imgId">The DOM element ID to update.</param>
        /// <param name="imgUrl">The image URL to display.</param>
        /// <param name="asImg">When <c>true</c> (default), sets the <c>src</c> attribute; when <c>false</c>, sets as a CSS background image.</param>
        ValueTask ShowImage(string imgId, string imgUrl, bool asImg = true);
        /// <summary>
        /// Displays a preview of <paramref name="file"/> inside an element without uploading it.
        /// </summary>
        /// <param name="imgId">The DOM element ID to update.</param>
        /// <param name="file">The browser file to preview.</param>
        /// <param name="asImg">When <c>true</c> (default), sets the <c>src</c> attribute; when <c>false</c>, sets as a CSS background image.</param>
        ValueTask ShowImage(string imgId, BrowserFile file, bool asImg = true);
        /// <summary>
        /// Uploads the specified files to the MetalNexus endpoint defined by <typeparamref name="TRequest"/>,
        /// using the provided request instance for additional properties.
        /// </summary>
        /// <typeparam name="TRequest">The <see cref="MetalNexusFileRequest"/>-derived request type that defines the upload endpoint.</typeparam>
        /// <param name="request">The request instance whose non-file properties are sent alongside the upload.</param>
        /// <param name="files">The files to upload.  Must be a subset of <see cref="Files"/>.</param>
        Task UploadFiles<TRequest>(TRequest request, params BrowserFile[] files)
            where TRequest : MetalNexusFileRequest, new();
        /// <summary>
        /// Uploads the specified files to the MetalNexus endpoint defined by <typeparamref name="TRequest"/>
        /// using a default-constructed request instance.
        /// </summary>
        /// <typeparam name="TRequest">The <see cref="MetalNexusFileRequest"/>-derived request type that defines the upload endpoint.</typeparam>
        /// <param name="files">The files to upload.  Must be a subset of <see cref="Files"/>.</param>
        Task UploadFiles<TRequest>(params BrowserFile[] files)
            where TRequest : MetalNexusFileRequest, new();
        /// <summary>
        /// Uploads named slot files and optional unnamed files to the MetalNexus endpoint defined
        /// by <typeparamref name="TRequest"/>.
        /// </summary>
        /// <typeparam name="TRequest">The <see cref="MetalNexusFileRequest"/>-derived request type that defines the upload endpoint.</typeparam>
        /// <param name="request">The request instance whose non-file properties are sent alongside the upload.</param>
        /// <param name="slots">A dictionary mapping slot name to the <see cref="BrowserFile"/> for that slot.</param>
        /// <param name="files">Additional unnamed files placed in <c>Files[]</c> on the server.</param>
        Task UploadFiles<TRequest>(TRequest request, Dictionary<string, BrowserFile> slots, params BrowserFile[] files)
            where TRequest : MetalNexusFileRequest, new();
    }

    private class FilesPickedArgs : IFilesPickedArgs
    {
        public FilesPickedArgs(FileInput fileInput, BrowserFile[] files) =>
            (_fileInput, _files) = (fileInput, files);
        private readonly FileInput _fileInput;
        private readonly BrowserFile[] _files;

        public BrowserFile[] Files => _files;

        public ValueTask ShowImage(string imgId, string imgUrl, bool asImg = true) =>
            _fileInput.ShowImage(imgId, imgUrl, asImg);

        public ValueTask ShowImage(string imgId, BrowserFile file, bool asImg = true) =>
            _fileInput.ShowImage(imgId, file, asImg);

        public Task UploadFiles<TRequest>(TRequest request, params BrowserFile[] files)
            where TRequest : MetalNexusFileRequest, new() =>
            _fileInput.UploadFiles<TRequest>(request, files);

        public Task UploadFiles<TRequest>(params BrowserFile[] files)
            where TRequest : MetalNexusFileRequest, new() =>
            _fileInput.UploadFiles(new TRequest(), files);

        public Task UploadFiles<TRequest>(TRequest request, Dictionary<string, BrowserFile> slots, params BrowserFile[] files)
            where TRequest : MetalNexusFileRequest, new() =>
            _fileInput.UploadFiles<TRequest>(request, slots, files);
    }


    /// <summary>Raised when the file picker is dismissed without a selection.</summary>
    [Parameter] public EventCallback FilePickerCanceled { get; set; }
    /// <summary>Called by JavaScript interop when the file picker focus is lost; not intended for direct use.</summary>
    [JSInvokable] public async Task OnFilePickerFocusLost()
    {
        if (!_filesSelected) await FilePickerCanceled.InvokeAsync();
    }


    /// <summary>
    /// Displays a preview of an image from a URL inside an element identified by <paramref name="imgId"/>.
    /// </summary>
    /// <param name="imgId">The DOM element ID to update.</param>
    /// <param name="imgUrl">The image URL to display.</param>
    /// <param name="asImg">When <c>true</c> (default), sets the <c>src</c> attribute; when <c>false</c>, sets as a CSS background image.</param>
    public ValueTask ShowImage(string imgId, string imgUrl, bool asImg = true) =>
        JSRuntime.InvokeVoidAsync("RossWrightFileInput.showImageUrl", imgId, imgUrl, asImg ? "imgsrc" : "cssbgimg");
    /// <summary>
    /// Displays a preview of a selected browser file inside an element without uploading it.
    /// </summary>
    /// <param name="imgId">The DOM element ID to update.</param>
    /// <param name="file">The browser file to preview.</param>
    /// <param name="asImg">When <c>true</c> (default), sets the <c>src</c> attribute; when <c>false</c>, sets as a CSS background image.</param>
    public ValueTask ShowImage(string imgId, BrowserFile file, bool asImg = true) =>
        JSRuntime.InvokeVoidAsync("RossWrightFileInput.showImage", imgId, file.FileRefId, asImg ? "imgsrc" : "cssbgimg");

    /// <summary>
    /// Uploads the specified files to the MetalNexus endpoint defined by <typeparamref name="TRequest"/>.
    /// </summary>
    /// <typeparam name="TRequest">The <see cref="MetalNexusFileRequest"/>-derived request type that defines the upload endpoint.</typeparam>
    /// <param name="request">The request instance whose non-file properties are sent alongside the files.</param>
    /// <param name="files">The files to upload.</param>
    /// <exception cref="InvalidOperationException">Thrown when no endpoint is registered for <typeparamref name="TRequest"/>.</exception>
    public Task UploadFiles<TRequest>(TRequest request, params BrowserFile[] files)
        where TRequest : MetalNexusFileRequest, new() =>
        UploadFiles<TRequest>(request, null, files);

    /// <summary>
    /// Uploads named slot files and optional unnamed files to the MetalNexus endpoint defined
    /// by <typeparamref name="TRequest"/>.
    /// </summary>
    /// <typeparam name="TRequest">The <see cref="MetalNexusFileRequest"/>-derived request type that defines the upload endpoint.</typeparam>
    /// <param name="request">The request instance whose non-file properties are sent alongside the upload.</param>
    /// <param name="slots">A dictionary mapping slot name to the <see cref="BrowserFile"/> for that slot, or <c>null</c>.</param>
    /// <param name="files">Additional unnamed files placed in <c>Files[]</c> on the server.</param>
    /// <exception cref="InvalidOperationException">Thrown when no endpoint is registered for <typeparamref name="TRequest"/>.</exception>
    public async Task UploadFiles<TRequest>(TRequest request, Dictionary<string, BrowserFile>? slots, params BrowserFile[] files)
        where TRequest : MetalNexusFileRequest, new()
    {
        var endpoint = MetalNexusRegistry.FindEndpoint(typeof(TRequest));
        if (endpoint == null) throw new InvalidOperationException(
            $"Endpoint Schema contains no endpoint for {typeof(TRequest).FullName}");

        var url = UrlHelper.GetUrlFor<TRequest>(request);
        var authInfo = authClient == null ? null
            : await authClient.Authenticate(endpoint.HttpClientName, forceRefesh: true);

        // Build fileEntries: each entry carries the fileRefId and the form-field name.
        // Slot files use the slot name; unnamed files use "files".
        var fileEntries = new List<object>();
        if (slots != null)
            foreach (var (slotName, file) in slots)
                fileEntries.Add(new { fileRefId = file.FileRefId, fieldName = slotName });
        foreach (var file in files)
            fileEntries.Add(new { fileRefId = file.FileRefId, fieldName = "files" });

        startTime = DateTime.Now;
        previousLoaded = 0;
        previousTotalSecs = 0;
        await JSRuntime.InvokeVoidAsync("RossWrightFileInput.uploadFiles", objRef,
            endpoint.HttpMethod.ToString(), url, authInfo?.Token, fileEntries);
    }

    /// <summary>Raised periodically during an upload with progress information.</summary>
    [Parameter] public EventCallback<IProgressArgs> Progress { get; set; }
    /// <summary>Called by JavaScript interop to report upload progress; not intended for direct use.</summary>
    [JSInvokable] public async Task OnProgress(double loaded, double total, FileProgressInfo[] files)
    {
        var currentSecs = (DateTime.Now - startTime).TotalSeconds;
        var updateBytes = loaded - previousLoaded;
        previousLoaded = loaded;
        var updateSecs = currentSecs - previousTotalSecs;
        previousTotalSecs = currentSecs;

        await Progress.InvokeAsync(new ProgressArgs
        {
            Loaded = loaded,
            Total = total,
            UpdateLoaded = updateBytes,
            UpdateSpeedKbps = updateBytes / updateSecs / 1024.0,
            AverageSpeedKbps = loaded / currentSecs / 1024.0,
            Files = files,
        });
    }
    private DateTime startTime;
    private double previousLoaded;
    private double previousTotalSecs;

    /// <summary>Progress information for a single file within a multi-file upload.</summary>
    /// <param name="FileName">The original name of the file.</param>
    /// <param name="Loaded">The number of bytes of this file uploaded so far.</param>
    /// <param name="Total">The total size of this file in bytes.</param>
    public record FileProgressInfo(string FileName, double Loaded, double Total);

    /// <summary>Provides upload progress statistics for a single progress event.</summary>
    public interface IProgressArgs
    {
        /// <summary>The number of bytes uploaded so far.</summary>
        double Loaded { get; }
        /// <summary>The total number of bytes to upload.</summary>
        double Total { get; }
        /// <summary>The upload progress as a fraction between 0.0 and 1.0.</summary>
        double Fraction { get; }
        /// <summary>The average upload speed since the upload began, in kilobytes per second.</summary>
        double AverageSpeedKbps { get; }
        /// <summary>The number of bytes uploaded since the previous progress event.</summary>
        double UpdateLoaded { get; }
        /// <summary>The upload speed over the last progress interval, in kilobytes per second.</summary>
        double UpdateSpeedKbps { get; }
        /// <summary>Per-file progress breakdown.  Empty when per-file data is unavailable.</summary>
        IReadOnlyList<FileProgressInfo> Files { get; }
    }

    private class ProgressArgs : IProgressArgs
    {
        public double Loaded { get; init; }
        public double Total { get; init; }
        public double Fraction => Loaded / Total;
        public double AverageSpeedKbps { get; init; }
        public double UpdateLoaded { get; init; }
        public double UpdateSpeedKbps { get; init; }
        public IReadOnlyList<FileProgressInfo> Files { get; init; } = [];
    }


    /// <summary>Raised when the upload completes successfully.</summary>
    [Parameter] public EventCallback<IFilesUploadedArgs> FilesUploaded { get; set; }
    /// <summary>Called by JavaScript interop when the upload succeeds; not intended for direct use.</summary>
    [JSInvokable] public void OnSuccess(JsonElement? json)
    {
        pendingSuccess = new FilesUploadedArgs(json);
        StateHasChanged();
    }
    private IFilesUploadedArgs? pendingSuccess;

    /// <summary>Provides access to the server response after a successful file upload.</summary>
    public interface IFilesUploadedArgs
    {
        /// <summary>When <c>true</c>, the server returned a non-empty JSON response body.</summary>
        bool HasValue { get; }
        /// <summary>
        /// Deserializes and returns the server response as <typeparamref name="TResponse"/>, or
        /// <c>null</c> when the server returned no body.
        /// </summary>
        /// <typeparam name="TResponse">The expected response type.</typeparam>
        /// <returns>The deserialized response, or <c>null</c> if there is no response body.</returns>
        TResponse? Get<TResponse>() where TResponse : class;
    }

    private class FilesUploadedArgs : IFilesUploadedArgs
    {
        public FilesUploadedArgs(JsonElement? json) => _json = json;
        private readonly JsonElement? _json;
        public bool HasValue => _json != null;
        public TResponse? Get<TResponse>() where TResponse : class => 
            _json == null ? null : JsonSerializer.Deserialize<TResponse>(_json.Value);
    }


    /// <summary>Raised when the upload fails.  If no delegate is set, the exception is rethrown.</summary>
    [Parameter] public EventCallback<Exception> UploadFailed { get; set; }
    /// <summary>Called by JavaScript interop when the upload fails; not intended for direct use.</summary>
    [JSInvokable] public void OnError(int httpStatus, string exceptionJson)
    {
        pendingError = ExceptionResponse
            .Deserialize((System.Net.HttpStatusCode)httpStatus, 
                         exceptionJson,
                         MetalNexusOptions.ServerStackTraceOnExceptionsIncluded);
        if (UploadFailed.HasDelegate)
        {
            StateHasChanged();
        }
        else
        {
            throw pendingError;
        }
    }
    private Exception? pendingError;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        isDisposed = true;
        await JSRuntime.InvokeVoidAsync("RossWrightFileInput.detach", Id);
        objRef?.Dispose();
    }
    private bool isDisposed;
}
