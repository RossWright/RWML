using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RossWright.MetalGuardian;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;
using System.Text.Json;

namespace RossWright.MetalNexus;

public partial class FileInput : IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private IJsScriptLoaderService ScriptLoader { get; set; } = null!;
    [Inject] private IMetalNexusRegistry MetalNexusRegistry { get; set; } = null!;
    [Inject] private IMetalNexusOptions MetalNexusOptions { get; set; } = null!;
    [Inject] private IMetalGuardianAuthenticationClient? authClient { get; set; } = null!;
    [Inject] private IMetalNexusUrlHelper UrlHelper { get; set; } = null!;

    [Parameter] public bool AllowMultipleFiles { get; set; } = false;
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

    public async ValueTask OpenFilePicker()
    {
        await EnsureReady();
        _filesSelected = false;
        await JSRuntime.InvokeVoidAsync("RossWrightFileInput.click", Id);
    }

    [Parameter] public EventCallback<IFilesPickedArgs> FilesPicked { get; set; }
    [JSInvokable] public async Task OnFilesSelected(BrowserFile[] files)
    {
        _filesSelected = true;
        if (!isDisposed) await FilesPicked.InvokeAsync(new FilesPickedArgs(this, files));
    }

    public interface IFilesPickedArgs
    {
        BrowserFile[] Files { get; }
        ValueTask ShowImage(string imgId, string imgUrl, bool asImg = true);
        ValueTask ShowImage(string imgId, BrowserFile file, bool asImg = true);
        Task UploadFiles<TRequest>(TRequest request, params BrowserFile[] files)
            where TRequest : MetalNexusFileRequest, new();
        Task UploadFiles<TRequest>(params BrowserFile[] files)
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
    }


    [Parameter] public EventCallback FilePickerCanceled { get; set; }
    [JSInvokable] public async Task OnFilePickerFocusLost()
    {
        if (!_filesSelected) await FilePickerCanceled.InvokeAsync();
    }


    public ValueTask ShowImage(string imgId, string imgUrl, bool asImg = true) =>
        JSRuntime.InvokeVoidAsync("RossWrightFileInput.showImageUrl", imgId, imgUrl, asImg ? "imgsrc" : "cssbgimg");
    public ValueTask ShowImage(string imgId, BrowserFile file, bool asImg = true) =>
        JSRuntime.InvokeVoidAsync("RossWrightFileInput.showImage", imgId, file.FileRefId, asImg ? "imgsrc" : "cssbgimg");

    public async Task UploadFiles<TRequest>(TRequest request, params BrowserFile[] files)
        where TRequest : MetalNexusFileRequest, new()
    {
        var endpoint = MetalNexusRegistry.FindEndpoint(typeof(TRequest));
        if (endpoint == null) throw new InvalidOperationException(
            $"Endpoint Schema contains no endpoint for {typeof(TRequest).FullName}");

        var url = UrlHelper.GetUrlFor<TRequest>(request);
        var authInfo = authClient == null ? null
            : await authClient.Authenticate(endpoint.HttpClientName, forceRefesh: true);
        var fileRefIds = files.Select(_ => _.FileRefId).ToArray();
        startTime = DateTime.Now;
        previousLoaded = 0;
        previousTotalSecs = 0;
        await JSRuntime.InvokeVoidAsync("RossWrightFileInput.uploadFiles", objRef,
            endpoint.HttpMethod.ToString(), url, authInfo?.Token, fileRefIds);
    }

    [Parameter] public EventCallback<IProgressArgs> Progress { get; set; }
    [JSInvokable] public async Task OnProgress(double loaded, double total)
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
        });
    }
    private DateTime startTime;
    private double previousLoaded;
    private double previousTotalSecs;

    public interface IProgressArgs
    {
        double Loaded { get; }
        double Total { get; }
        double Fraction { get; }
        double AverageSpeedKbps { get; }
        double UpdateLoaded { get; }
        double UpdateSpeedKbps { get; }
    }

    private class ProgressArgs : IProgressArgs
    {
        public double Loaded { get; init; }
        public double Total { get; init; }
        public double Fraction => Loaded / Total;
        public double AverageSpeedKbps { get; init; }
        public double UpdateLoaded { get; init; }
        public double UpdateSpeedKbps { get; init; }
    }


    [Parameter] public EventCallback<IFilesUploadedArgs> FilesUploaded { get; set; }
    [JSInvokable] public void OnSuccess(JsonElement? json)
    {
        pendingSuccess = new FilesUploadedArgs(json);
        StateHasChanged();
    }
    private IFilesUploadedArgs? pendingSuccess;

    public interface IFilesUploadedArgs
    {
        bool HasValue { get; }
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


    [Parameter] public EventCallback<Exception> UploadFailed { get; set; }
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

    public async ValueTask DisposeAsync()
    {
        isDisposed = true;
        await JSRuntime.InvokeVoidAsync("RossWrightFileInput.detach", Id);
        objRef?.Dispose();
    }
    private bool isDisposed;
}