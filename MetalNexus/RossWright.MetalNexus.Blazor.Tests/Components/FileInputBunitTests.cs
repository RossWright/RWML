using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Shouldly;
using System.Reflection;
using System.Text.Json;

namespace RossWright.MetalNexus.Blazor.UnitTests.Components;

/// <summary>
/// bUnit component-level tests for <see cref="FileInput"/>.
/// These tests render the component through the Blazor lifecycle so that
/// OnAfterRenderAsync, JS interop calls, and event callbacks are exercised
/// exactly as they would be in a browser.
/// </summary>
public class FileInputBunitTests : Bunit.BunitContext
{
    private readonly IJsScriptLoaderService _scriptLoader;
    private readonly IMetalNexusRegistry _registry;
    private readonly IMetalNexusOptions _options;
    private readonly IMetalNexusUrlHelper _urlHelper;

    public FileInputBunitTests()
    {
        _scriptLoader = Substitute.For<IJsScriptLoaderService>();
        _registry = Substitute.For<IMetalNexusRegistry>();
        _options = Substitute.For<IMetalNexusOptions>();
        _urlHelper = Substitute.For<IMetalNexusUrlHelper>();

        _scriptLoader.EnsureLoaded(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .Returns(Task.CompletedTask);

        // Use bUnit's built-in JSInterop rather than substituting IJSRuntime directly
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddSingleton(_scriptLoader);
        Services.AddSingleton(_registry);
        Services.AddSingleton(_options);
        Services.AddSingleton(_urlHelper);
        Services.AddSingleton(Substitute.For<IMetalGuardianAuthenticationClient>());
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_RendersHiddenFileInput()
    {
        var cut = Render<FileInput>();

        var input = cut.Find("input[type=file]");
        input.ShouldNotBeNull();
        input.HasAttribute("hidden").ShouldBeTrue();
    }

    [Fact]
    public void Render_DefaultsToSingleFileSelection()
    {
        var cut = Render<FileInput>();

        var input = cut.Find("input[type=file]");
        // multiple="False" is how Blazor renders a false bool parameter
        var multipleAttr = input.GetAttribute("multiple");
        (multipleAttr == null || multipleAttr == "False").ShouldBeTrue();
    }

    [Fact]
    public void Render_AllowMultipleFiles_SetsMultipleAttribute()
    {
        var cut = Render<FileInput>(p => p.Add(x => x.AllowMultipleFiles, true));

        var input = cut.Find("input[type=file]");
        // Blazor renders a true bool attribute as present with an empty string value
        input.HasAttribute("multiple").ShouldBeTrue();
    }

    [Fact]
    public void Render_AcceptParameter_SetsAcceptAttribute()
    {
        var cut = Render<FileInput>(p => p.Add(x => x.Accept, "image/*"));

        var input = cut.Find("input[type=file]");
        input.GetAttribute("accept").ShouldBe("image/*");
    }

    // -------------------------------------------------------------------------
    // First render – JS initialisation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnAfterRenderAsync_FirstRender_LoadsJQueryThenInteropJs()
    {
        Render<FileInput>();
        // bUnit renders synchronously but we need to let the async lifecycle complete
        await Task.Delay(50);

        Received.InOrder(() =>
        {
            _scriptLoader.EnsureLoaded(
                Arg.Is<string>(s => s.Contains("jquery")),
                Arg.Any<string>(),
                Arg.Any<string?>());
            _scriptLoader.EnsureLoaded(
                Arg.Is<string>(s => s.Contains("Interop.js")),
                Arg.Any<string>(),
                Arg.Any<string?>());
        });
    }

    [Fact]
    public async Task OnAfterRenderAsync_FirstRender_AttachesComponentViaJsInterop()
    {
        Render<FileInput>();
        await Task.Delay(50);

        JSInterop.VerifyInvoke("RossWrightFileInput.attach", 1);
    }

    [Fact]
    public async Task OnAfterRenderAsync_SubsequentRender_DoesNotReattach()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        // Trigger a re-render via bUnit's Render method
        cut.Render();
        await Task.Delay(50);

        // attach must only have been called once (first render)
        JSInterop.VerifyInvoke("RossWrightFileInput.attach", 1);
    }

    // -------------------------------------------------------------------------
    // OpenFilePicker
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OpenFilePicker_CallsJsClickWithComponentId()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        await cut.Instance.OpenFilePicker();

        JSInterop.VerifyInvoke("RossWrightFileInput.click", 1);
    }

    [Fact]
    public async Task OpenFilePicker_ResetsFilesSelectedFlag()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        // Simulate files having been selected previously
        SetPrivateField(cut.Instance, "_filesSelected", true);

        await cut.Instance.OpenFilePicker();

        GetPrivateField(cut.Instance, "_filesSelected").ShouldBe(false);
    }

    // -------------------------------------------------------------------------
    // OnFilesSelected (JS invokable)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnFilesSelected_SetsFilesSelectedFlag()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        await cut.Instance.OnFilesSelected(Array.Empty<BrowserFile>());

        GetPrivateField(cut.Instance, "_filesSelected").ShouldBe(true);
    }

    [Fact]
    public async Task OnFilesSelected_InvokesFilesPicked()
    {
        FileInput.IFilesPickedArgs? capturedArgs = null;
        var cut = Render<FileInput>(p =>
            p.Add(x => x.FilesPicked, EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
                this, args => capturedArgs = args)));
        await Task.Delay(50);

        var files = new[] { new BrowserFile { FileName = "photo.jpg", Size = 1024, ContentType = "image/jpeg", FileRefId = 1 } };
        await cut.Instance.OnFilesSelected(files);

        capturedArgs.ShouldNotBeNull();
        capturedArgs.Files.ShouldContain(f => f.FileName == "photo.jpg");
    }

    [Fact]
    public async Task OnFilesSelected_WhenDisposed_DoesNotInvokeCallback()
    {
        FileInput.IFilesPickedArgs? capturedArgs = null;
        var cut = Render<FileInput>(p =>
            p.Add(x => x.FilesPicked, EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
                this, args => capturedArgs = args)));
        await Task.Delay(50);

        await cut.Instance.DisposeAsync();
        await cut.Instance.OnFilesSelected(Array.Empty<BrowserFile>());

        capturedArgs.ShouldBeNull();
    }

    // -------------------------------------------------------------------------
    // OnFilePickerFocusLost (JS invokable)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnFilePickerFocusLost_WhenNoFilesSelected_InvokesFilePickerCanceled()
    {
        var canceled = false;
        var cut = Render<FileInput>(p =>
            p.Add(x => x.FilePickerCanceled, EventCallback.Factory.Create(this, () => canceled = true)));
        await Task.Delay(50);

        await cut.Instance.OnFilePickerFocusLost();

        canceled.ShouldBeTrue();
    }

    [Fact]
    public async Task OnFilePickerFocusLost_WhenFilesAlreadySelected_DoesNotInvokeCallback()
    {
        var canceled = false;
        var cut = Render<FileInput>(p =>
            p.Add(x => x.FilePickerCanceled, EventCallback.Factory.Create(this, () => canceled = true)));
        await Task.Delay(50);

        // Simulate files having been selected so the flag is set
        SetPrivateField(cut.Instance, "_filesSelected", true);
        await cut.Instance.OnFilePickerFocusLost();

        canceled.ShouldBeFalse();
    }

    // -------------------------------------------------------------------------
    // ShowImage (JS interop passthrough)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ShowImage_WithUrl_AsImg_InvokesShowImageUrl_WithImgsrc()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        await cut.Instance.ShowImage("myImg", "https://example.com/photo.jpg", asImg: true);

        JSInterop.VerifyInvoke("RossWrightFileInput.showImageUrl");
        var inv = JSInterop.Invocations["RossWrightFileInput.showImageUrl"][0];
        inv.Arguments[0].ShouldBe("myImg");
        inv.Arguments[2].ShouldBe("imgsrc");
    }

    [Fact]
    public async Task ShowImage_WithUrl_NotAsImg_InvokesShowImageUrl_WithCssbgimg()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        await cut.Instance.ShowImage("myDiv", "https://example.com/bg.jpg", asImg: false);

        JSInterop.VerifyInvoke("RossWrightFileInput.showImageUrl");
        var inv = JSInterop.Invocations["RossWrightFileInput.showImageUrl"][0];
        inv.Arguments[2].ShouldBe("cssbgimg");
    }

    [Fact]
    public async Task ShowImage_WithBrowserFile_InvokesShowImage_WithFileRefId()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        var file = new BrowserFile { FileName = "x.jpg", FileRefId = 99 };
        await cut.Instance.ShowImage("el", file, asImg: true);

        JSInterop.VerifyInvoke("RossWrightFileInput.showImage");
        var inv = JSInterop.Invocations["RossWrightFileInput.showImage"][0];
        inv.Arguments[1].ShouldBe(99);
        inv.Arguments[2].ShouldBe("imgsrc");
    }

    // -------------------------------------------------------------------------
    // OnSuccess (JS invokable)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnSuccess_WithJsonResponse_InvokesFilesUploaded()
    {
        FileInput.IFilesUploadedArgs? uploadedArgs = null;
        var cut = Render<FileInput>(p =>
            p.Add(x => x.FilesUploaded, EventCallback.Factory.Create<FileInput.IFilesUploadedArgs>(
                this, args => uploadedArgs = args)));
        await Task.Delay(50);

        var json = JsonDocument.Parse("{\"Id\":42}").RootElement;
        // OnSuccess calls StateHasChanged; must be dispatched on the Blazor renderer thread
        await cut.InvokeAsync(() => cut.Instance.OnSuccess(json));
        await Task.Delay(50);

        uploadedArgs.ShouldNotBeNull();
        uploadedArgs.HasValue.ShouldBeTrue();
        uploadedArgs.Get<TestResponse>()!.Id.ShouldBe(42);
    }

    [Fact]
    public async Task OnSuccess_WithNullJson_InvokesFilesUploaded_WithNoValue()
    {
        FileInput.IFilesUploadedArgs? uploadedArgs = null;
        var cut = Render<FileInput>(p =>
            p.Add(x => x.FilesUploaded, EventCallback.Factory.Create<FileInput.IFilesUploadedArgs>(
                this, args => uploadedArgs = args)));
        await Task.Delay(50);

        await cut.InvokeAsync(() => cut.Instance.OnSuccess(null));
        await Task.Delay(50);

        uploadedArgs.ShouldNotBeNull();
        uploadedArgs.HasValue.ShouldBeFalse();
    }

    // -------------------------------------------------------------------------
    // OnError (JS invokable)
    // -------------------------------------------------------------------------

    [Fact]
    public void OnError_WithNoUploadFailedDelegate_Throws()
    {
        var cut = Render<FileInput>();
        // No UploadFailed delegate registered

        Should.Throw<Exception>(() => cut.Instance.OnError(500, "{}"));
    }

    [Fact]
    public async Task OnError_WithUploadFailedDelegate_InvokesCallback()
    {
        Exception? capturedEx = null;
        var cut = Render<FileInput>(p =>
            p.Add(x => x.UploadFailed, EventCallback.Factory.Create<Exception>(
                this, ex => capturedEx = ex)));
        await Task.Delay(50);

        await cut.InvokeAsync(() => cut.Instance.OnError(400, "{}"));
        await Task.Delay(50);

        capturedEx.ShouldNotBeNull();
    }

    // -------------------------------------------------------------------------
    // OnProgress (JS invokable)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnProgress_InvokesProgressCallback_WithCorrectValues()
    {
        FileInput.IProgressArgs? capturedProgress = null;
        var cut = Render<FileInput>(p =>
            p.Add(x => x.Progress, EventCallback.Factory.Create<FileInput.IProgressArgs>(
                this, args => capturedProgress = args)));
        await Task.Delay(50);

        var fileProgress = new[] { new FileInput.FileProgressInfo("a.jpg", 500, 1000) };
        await cut.Instance.OnProgress(500, 1000, fileProgress);

        capturedProgress.ShouldNotBeNull();
        capturedProgress.Loaded.ShouldBe(500);
        capturedProgress.Total.ShouldBe(1000);
        capturedProgress.Fraction.ShouldBe(0.5);
        capturedProgress.Files.Count.ShouldBe(1);
        capturedProgress.Files[0].FileName.ShouldBe("a.jpg");
    }

    // -------------------------------------------------------------------------
    // DisposeAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DisposeAsync_CallsJsDetachWithId()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        var id = GetPrivateField(cut.Instance, "Id") as string;
        await cut.Instance.DisposeAsync();

        JSInterop.VerifyInvoke("RossWrightFileInput.detach");
        var inv = JSInterop.Invocations["RossWrightFileInput.detach"][0];
        inv.Arguments[0].ShouldBe(id);
    }

    [Fact]
    public async Task DisposeAsync_SetsIsDisposedFlag()
    {
        var cut = Render<FileInput>();
        await Task.Delay(50);

        await cut.Instance.DisposeAsync();

        GetPrivateField(cut.Instance, "isDisposed").ShouldBe(true);
    }

    // -------------------------------------------------------------------------
    // Helper methods
    // -------------------------------------------------------------------------

    private static object? GetPrivateField(object obj, string fieldName) =>
        obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj);

    private static void SetPrivateField(object obj, string fieldName, object? value) =>
        obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(obj, value);

    // -------------------------------------------------------------------------
    // Helper types
    // -------------------------------------------------------------------------

    private record TestResponse(int Id);
}
