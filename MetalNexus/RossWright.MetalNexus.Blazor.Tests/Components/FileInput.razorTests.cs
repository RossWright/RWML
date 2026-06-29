using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NSubstitute;
using RossWright.MetalGuardian;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Shouldly;

namespace RossWright.MetalNexus.Blazor.UnitTests.Components;

public class FileInputTests
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IJsScriptLoaderService _scriptLoader;
    private readonly IMetalNexusRegistry _registry;
    private readonly IMetalNexusOptions _options;
    private readonly IMetalGuardianAuthenticationClient _authClient;
    private readonly IMetalNexusUrlHelper _urlHelper;

    private FileInput CreateFileInput()
    {
        var fileInput = new FileInput();
        
        // Use reflection to set private properties
        SetPrivateProperty(fileInput, "JSRuntime", _jsRuntime);
        SetPrivateProperty(fileInput, "ScriptLoader", _scriptLoader);
        SetPrivateProperty(fileInput, "MetalNexusRegistry", _registry);
        SetPrivateProperty(fileInput, "MetalNexusOptions", _options);
        SetPrivateProperty(fileInput, "authClient", _authClient);
        SetPrivateProperty(fileInput, "UrlHelper", _urlHelper);
        
        return fileInput;
    }

    private void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        property?.SetValue(obj, value);
    }

    private object? GetPrivateField(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(obj);
    }

    private void SetPrivateField(object obj, string fieldName, object? value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(obj, value);
    }

    public FileInputTests()
    {
        _jsRuntime = Substitute.For<IJSRuntime>();
        _scriptLoader = Substitute.For<IJsScriptLoaderService>();
        _registry = Substitute.For<IMetalNexusRegistry>();
        _options = Substitute.For<IMetalNexusOptions>();
        _authClient = Substitute.For<IMetalGuardianAuthenticationClient>();
        _urlHelper = Substitute.For<IMetalNexusUrlHelper>();
    }

    [Fact]
    public async Task OnAfterRenderAsync_FirstRender_CallsEnsureReady()
    {
        // Arrange
        var fileInput = CreateFileInput();
        _scriptLoader.EnsureLoaded(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act
        var method = typeof(FileInput).GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(fileInput, new object[] { true })!;

        // Assert
        await _scriptLoader.Received(2).EnsureLoaded(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>());
        await _jsRuntime.Received(1).InvokeVoidAsync("RossWrightFileInput.attach", Arg.Any<object[]>());
    }

    [Fact]
    public async Task OnAfterRenderAsync_NotFirstRender_DoesNotCallEnsureReady()
    {
        // Arrange
        var fileInput = CreateFileInput();

        // Act
        var method = typeof(FileInput).GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(fileInput, new object[] { false })!;

        // Assert
        await _scriptLoader.DidNotReceive().EnsureLoaded(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task OnAfterRenderAsync_PendingSuccessSet_InvokesFilesUploadedAndClearsPending()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filesUploadedInvoked = false;
        FileInput.IFilesUploadedArgs? receivedArgs = null;
        
        var filesUploadedCallback = EventCallback.Factory.Create<FileInput.IFilesUploadedArgs>(
            this, 
            (FileInput.IFilesUploadedArgs args) => 
            { 
                filesUploadedInvoked = true; 
                receivedArgs = args;
            });
        SetPrivateProperty(fileInput, "FilesUploaded", filesUploadedCallback);

        // Create a pending success
        var pendingSuccessType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var pendingSuccess = Activator.CreateInstance(pendingSuccessType!, new object?[] { null });
        SetPrivateField(fileInput, "pendingSuccess", pendingSuccess);

        // Act
        var method = typeof(FileInput).GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(fileInput, new object[] { false })!;

        // Assert
        filesUploadedInvoked.ShouldBeTrue();
        receivedArgs.ShouldNotBeNull();
        var pendingSuccessAfter = GetPrivateField(fileInput, "pendingSuccess");
        pendingSuccessAfter.ShouldBeNull();
    }

    [Fact]
    public async Task OnAfterRenderAsync_PendingErrorSet_InvokesUploadFailedAndClearsPending()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var uploadFailedInvoked = false;
        Exception? receivedError = null;
        
        var uploadFailedCallback = EventCallback.Factory.Create<Exception>(
            this, 
            (Exception error) => 
            { 
                uploadFailedInvoked = true; 
                receivedError = error;
            });
        SetPrivateProperty(fileInput, "UploadFailed", uploadFailedCallback);

        var testException = new Exception("Test error");
        SetPrivateField(fileInput, "pendingError", testException);

        // Act
        var method = typeof(FileInput).GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(fileInput, new object[] { false })!;

        // Assert
        uploadFailedInvoked.ShouldBeTrue();
        receivedError.ShouldBe(testException);
        var pendingErrorAfter = GetPrivateField(fileInput, "pendingError");
        pendingErrorAfter.ShouldBeNull();
    }

    [Fact]
    public async Task OnAfterRenderAsync_PendingSuccessTakesPrecedenceOverPendingError()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filesUploadedInvoked = false;
        var uploadFailedInvoked = false;
        
        var filesUploadedCallback = EventCallback.Factory.Create<FileInput.IFilesUploadedArgs>(
            this, 
            (FileInput.IFilesUploadedArgs args) => { filesUploadedInvoked = true; });
        SetPrivateProperty(fileInput, "FilesUploaded", filesUploadedCallback);

        var uploadFailedCallback = EventCallback.Factory.Create<Exception>(
            this, 
            (Exception error) => { uploadFailedInvoked = true; });
        SetPrivateProperty(fileInput, "UploadFailed", uploadFailedCallback);

        var pendingSuccessType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var pendingSuccess = Activator.CreateInstance(pendingSuccessType!, new object?[] { null });
        SetPrivateField(fileInput, "pendingSuccess", pendingSuccess);
        SetPrivateField(fileInput, "pendingError", new Exception("Error"));

        // Act
        var method = typeof(FileInput).GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(fileInput, new object[] { false })!;

        // Assert
        filesUploadedInvoked.ShouldBeTrue();
        uploadFailedInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task OpenFilePicker_CallsEnsureReadyAndSetsFilesSelectedToFalse()
    {
        // Arrange
        var fileInput = CreateFileInput();
        _scriptLoader.EnsureLoaded(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);
        
        SetPrivateField(fileInput, "_filesSelected", true);
        var id = GetPrivateField(fileInput, "Id") as string;

        // Act
        await fileInput.OpenFilePicker();

        // Assert
        var filesSelected = (bool)GetPrivateField(fileInput, "_filesSelected")!;
        filesSelected.ShouldBeFalse();
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.click", 
            Arg.Is<object[]>(args => args.Length == 1 && args[0].Equals(id)));
    }

    [Fact]
    public async Task OnFilesSelected_SetsFilesSelectedToTrueAndInvokesCallback()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filesPickedInvoked = false;
        FileInput.IFilesPickedArgs? receivedArgs = null;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => 
            { 
                filesPickedInvoked = true; 
                receivedArgs = args;
            });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] 
        { 
            new BrowserFile { FileName = "test.txt", Size = 100, ContentType = "text/plain", FileRefId = 1 } 
        };

        // Act
        await fileInput.OnFilesSelected(files);

        // Assert
        var filesSelected = (bool)GetPrivateField(fileInput, "_filesSelected")!;
        filesSelected.ShouldBeTrue();
        filesPickedInvoked.ShouldBeTrue();
        receivedArgs.ShouldNotBeNull();
        receivedArgs.Files.ShouldBe(files);
    }

    [Fact]
    public async Task OnFilesSelected_WhenDisposed_DoesNotInvokeCallback()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filesPickedInvoked = false;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => { filesPickedInvoked = true; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        SetPrivateField(fileInput, "isDisposed", true);

        var files = new[] 
        { 
            new BrowserFile { FileName = "test.txt", Size = 100, ContentType = "text/plain", FileRefId = 1 } 
        };

        // Act
        await fileInput.OnFilesSelected(files);

        // Assert
        var filesSelected = (bool)GetPrivateField(fileInput, "_filesSelected")!;
        filesSelected.ShouldBeTrue();
        filesPickedInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithUrl_CallsJSRuntime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("img1", "http://example.com/image.jpg", true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 && 
                args[0].Equals("img1") && 
                args[1].Equals("http://example.com/image.jpg") && 
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithUrl_AsImgFalse_PassesCssbgimg()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("img1", "http://example.com/image.jpg", false);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 && 
                args[0].Equals("img1") && 
                args[1].Equals("http://example.com/image.jpg") && 
                args[2].Equals("cssbgimg")));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithBrowserFile_CallsJSRuntime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var file = new BrowserFile { FileName = "test.jpg", FileRefId = 42 };
        var files = new[] { file };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("img1", file, true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImage",
            Arg.Is<object[]>(args => args.Length == 3 && 
                args[0].Equals("img1") && 
                args[1].Equals(42) && 
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithBrowserFile_AsImgFalse_PassesCssbgimg()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var file = new BrowserFile { FileName = "test.jpg", FileRefId = 99 };
        var files = new[] { file };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("img1", file, false);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImage",
            Arg.Is<object[]>(args => args.Length == 3 && 
                args[0].Equals("img1") && 
                args[1].Equals(99) && 
                args[2].Equals("cssbgimg")));
    }

    [Fact]
    public async Task OnAfterRenderAsync_WithBothPendingSuccessAndError_OnlyProcessesSuccess()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filesUploadedInvoked = false;
        var uploadFailedInvoked = false;
        
        var filesUploadedCallback = EventCallback.Factory.Create<FileInput.IFilesUploadedArgs>(
            this, 
            (FileInput.IFilesUploadedArgs args) => { filesUploadedInvoked = true; });
        SetPrivateProperty(fileInput, "FilesUploaded", filesUploadedCallback);

        var uploadFailedCallback = EventCallback.Factory.Create<Exception>(
            this, 
            (Exception error) => { uploadFailedInvoked = true; });
        SetPrivateProperty(fileInput, "UploadFailed", uploadFailedCallback);

        var pendingSuccessType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var pendingSuccess = Activator.CreateInstance(pendingSuccessType!, new object?[] { null });
        SetPrivateField(fileInput, "pendingSuccess", pendingSuccess);
        SetPrivateField(fileInput, "pendingError", new Exception("Error"));

        // Act
        var method = typeof(FileInput).GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(fileInput, new object[] { false })!;

        // Assert
        filesUploadedInvoked.ShouldBeTrue();
        uploadFailedInvoked.ShouldBeFalse();
        var pendingErrorAfter = GetPrivateField(fileInput, "pendingError");
        pendingErrorAfter.ShouldNotBeNull(); // Error should not be cleared since success was processed
    }

    [Fact]
    public async Task OnFilesSelected_WithEmptyFilesArray_StillSetsFilesSelectedAndInvokesCallback()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filesPickedInvoked = false;
        FileInput.IFilesPickedArgs? receivedArgs = null;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => 
            { 
                filesPickedInvoked = true; 
                receivedArgs = args;
            });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = Array.Empty<BrowserFile>();

        // Act
        await fileInput.OnFilesSelected(files);

        // Assert
        var filesSelected = (bool)GetPrivateField(fileInput, "_filesSelected")!;
        filesSelected.ShouldBeTrue();
        filesPickedInvoked.ShouldBeTrue();
        receivedArgs.ShouldNotBeNull();
        receivedArgs.Files.Length.ShouldBe(0);
    }

    [Fact]
    public async Task OnFilesSelected_WithMultipleFiles_PassesAllFilesToCallback()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? receivedArgs = null;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => { receivedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] 
        { 
            new BrowserFile { FileName = "file1.txt", Size = 100, ContentType = "text/plain", FileRefId = 1 },
            new BrowserFile { FileName = "file2.jpg", Size = 200, ContentType = "image/jpeg", FileRefId = 2 },
            new BrowserFile { FileName = "file3.pdf", Size = 300, ContentType = "application/pdf", FileRefId = 3 }
        };

        // Act
        await fileInput.OnFilesSelected(files);

        // Assert
        receivedArgs.ShouldNotBeNull();
        receivedArgs.Files.Length.ShouldBe(3);
        receivedArgs.Files[0].FileName.ShouldBe("file1.txt");
        receivedArgs.Files[1].FileName.ShouldBe("file2.jpg");
        receivedArgs.Files[2].FileName.ShouldBe("file3.pdf");
    }

    [Fact]
    public async Task IFilesPickedArgs_Files_ReturnsCorrectFiles()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;
        
        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this, 
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var file1 = new BrowserFile { FileName = "test1.txt", FileRefId = 10 };
        var file2 = new BrowserFile { FileName = "test2.txt", FileRefId = 20 };
        var files = new[] { file1, file2 };

        // Act
        await fileInput.OnFilesSelected(files);

        // Assert
        pickedArgs.ShouldNotBeNull();
        pickedArgs.Files.ShouldBe(files);
        pickedArgs.Files.Length.ShouldBe(2);
    }

    [Fact]
    public async Task OpenFilePicker_MultipleCallsInSequence_ResetsFilesSelectedEachTime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        _scriptLoader.EnsureLoaded(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act - first call
        await fileInput.OpenFilePicker();
        SetPrivateField(fileInput, "_filesSelected", true);
        
        // Act - second call
        await fileInput.OpenFilePicker();

        // Assert
        var filesSelected = (bool)GetPrivateField(fileInput, "_filesSelected")!;
        filesSelected.ShouldBeFalse();
    }

    [Fact]
    public async Task OnAfterRenderAsync_NoPendingStates_DoesNotInvokeCallbacks()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filesUploadedInvoked = false;
        var uploadFailedInvoked = false;
        
        var filesUploadedCallback = EventCallback.Factory.Create<FileInput.IFilesUploadedArgs>(
            this, 
            (FileInput.IFilesUploadedArgs args) => { filesUploadedInvoked = true; });
        SetPrivateProperty(fileInput, "FilesUploaded", filesUploadedCallback);

        var uploadFailedCallback = EventCallback.Factory.Create<Exception>(
            this, 
            (Exception error) => { uploadFailedInvoked = true; });
        SetPrivateProperty(fileInput, "UploadFailed", uploadFailedCallback);

        // Act
        var method = typeof(FileInput).GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method!.Invoke(fileInput, new object[] { false })!;

        // Assert
        filesUploadedInvoked.ShouldBeFalse();
        uploadFailedInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task FilesPickedArgs_UploadFiles_WithRequestParameter_CallsFileInputUploadFiles()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var testRequest = new TestFileRequest();
        var uploadFiles = new[] { new BrowserFile { FileName = "upload.txt", FileRefId = 2 } };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(testRequest).Returns("http://test.com/upload");

        // Act
        await pickedArgs!.UploadFiles(testRequest, uploadFiles);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task FilesPickedArgs_UploadFiles_WithoutRequestParameter_CreatesNewRequestAndCallsUpload()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var uploadFiles = new[] { new BrowserFile { FileName = "upload.txt", FileRefId = 2 } };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor(Arg.Any<TestFileRequest>()).Returns("http://test.com/upload");

        // Act
        await pickedArgs!.UploadFiles<TestFileRequest>(uploadFiles);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Any<object[]>());
    }

    [Fact]
    public void FilesPickedArgs_Constructor_InitializesFieldsCorrectly()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var files = new[] 
        { 
            new BrowserFile { FileName = "test.txt", FileRefId = 1 },
            new BrowserFile { FileName = "test2.txt", FileRefId = 2 }
        };

        // Act
        var filesPickedArgsType = typeof(FileInput).GetNestedType("FilesPickedArgs", BindingFlags.NonPublic);
        var constructor = filesPickedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance, 
            null, 
            new[] { typeof(FileInput), typeof(BrowserFile[]) }, 
            null);
        var pickedArgs = constructor!.Invoke(new object[] { fileInput, files }) as FileInput.IFilesPickedArgs;

        // Assert
        pickedArgs.ShouldNotBeNull();
        pickedArgs.Files.ShouldBe(files);
        pickedArgs.Files.Length.ShouldBe(2);
    }

    [Fact]
    public void FilesPickedArgs_Files_ReturnsSameArrayPassedToConstructor()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var file1 = new BrowserFile { FileName = "file1.txt", FileRefId = 10 };
        var file2 = new BrowserFile { FileName = "file2.txt", FileRefId = 20 };
        var file3 = new BrowserFile { FileName = "file3.txt", FileRefId = 30 };
        var files = new[] { file1, file2, file3 };

        // Act
        var filesPickedArgsType = typeof(FileInput).GetNestedType("FilesPickedArgs", BindingFlags.NonPublic);
        var constructor = filesPickedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(FileInput), typeof(BrowserFile[]) },
            null);
        var pickedArgs = constructor!.Invoke(new object[] { fileInput, files }) as FileInput.IFilesPickedArgs;

        // Assert
        pickedArgs.ShouldNotBeNull();
        var returnedFiles = pickedArgs.Files;
        returnedFiles.ShouldBeSameAs(files);
        returnedFiles.Length.ShouldBe(3);
        returnedFiles[0].ShouldBe(file1);
        returnedFiles[1].ShouldBe(file2);
        returnedFiles[2].ShouldBe(file3);
    }

    [Fact]
    public async Task FilesPickedArgs_ShowImage_WithUrl_DelegatesToFileInput()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("imgId", "http://example.com/img.jpg", true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("imgId") &&
                args[1].Equals("http://example.com/img.jpg") &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task FilesPickedArgs_ShowImage_WithUrlAndAsImgFalse_PassesCssbgimg()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("bgId", "http://example.com/bg.jpg", false);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("bgId") &&
                args[1].Equals("http://example.com/bg.jpg") &&
                args[2].Equals("cssbgimg")));
    }

    [Fact]
    public async Task FilesPickedArgs_UploadFiles_WithMultipleFiles_PassesAllFileRefIds()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var testRequest = new TestFileRequest();
        var uploadFiles = new[]
        {
            new BrowserFile { FileName = "file1.txt", FileRefId = 10 },
            new BrowserFile { FileName = "file2.txt", FileRefId = 20 },
            new BrowserFile { FileName = "file3.txt", FileRefId = 30 }
        };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(testRequest).Returns("http://test.com/upload");

        // Act
        await pickedArgs!.UploadFiles(testRequest, uploadFiles);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Any<object[]>());
    }

    [Fact]
    public void FilesPickedArgs_Constructor_WithEmptyFilesArray_InitializesCorrectly()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var files = Array.Empty<BrowserFile>();

        // Act
        var filesPickedArgsType = typeof(FileInput).GetNestedType("FilesPickedArgs", BindingFlags.NonPublic);
        var constructor = filesPickedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(FileInput), typeof(BrowserFile[]) },
            null);
        var pickedArgs = constructor!.Invoke(new object[] { fileInput, files }) as FileInput.IFilesPickedArgs;

        // Assert
        pickedArgs.ShouldNotBeNull();
        pickedArgs.Files.ShouldBe(files);
        pickedArgs.Files.Length.ShouldBe(0);
    }

    private class TestFileRequest : MetalNexusFileRequest
    {
    }

    [Fact]
    public async Task OnFilePickerFocusLost_WhenFilesNotSelected_InvokesFilePickerCanceledCallback()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filePickerCanceledInvoked = false;

        var filePickerCanceledCallback = EventCallback.Factory.Create(
            this,
            () => { filePickerCanceledInvoked = true; });
        SetPrivateProperty(fileInput, "FilePickerCanceled", filePickerCanceledCallback);

        SetPrivateField(fileInput, "_filesSelected", false);

        // Act
        await fileInput.OnFilePickerFocusLost();

        // Assert
        filePickerCanceledInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task OnFilePickerFocusLost_WhenFilesSelected_DoesNotInvokeFilePickerCanceledCallback()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var filePickerCanceledInvoked = false;

        var filePickerCanceledCallback = EventCallback.Factory.Create(
            this,
            () => { filePickerCanceledInvoked = true; });
        SetPrivateProperty(fileInput, "FilePickerCanceled", filePickerCanceledCallback);

        SetPrivateField(fileInput, "_filesSelected", true);

        // Act
        await fileInput.OnFilePickerFocusLost();

        // Assert
        filePickerCanceledInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task OnFilePickerFocusLost_WhenFilesNotSelectedAndNoCallback_DoesNotThrow()
    {
        // Arrange
        var fileInput = CreateFileInput();
        SetPrivateField(fileInput, "_filesSelected", false);

        // Act & Assert - should not throw
        await fileInput.OnFilePickerFocusLost();
    }

    [Fact]
    public async Task ShowImage_WithUrl_CallsJSRuntimeWithCorrectParametersWhenAsImgTrue()
    {
        // Arrange
        var fileInput = CreateFileInput();

        // Act
        await fileInput.ShowImage("testImgId", "http://example.com/image.png", true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("testImgId") &&
                args[1].Equals("http://example.com/image.png") &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task ShowImage_WithUrl_CallsJSRuntimeWithCorrectParametersWhenAsImgFalse()
    {
        // Arrange
        var fileInput = CreateFileInput();

        // Act
        await fileInput.ShowImage("testBgId", "http://example.com/background.jpg", false);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("testBgId") &&
                args[1].Equals("http://example.com/background.jpg") &&
                args[2].Equals("cssbgimg")));
    }

    [Fact]
    public async Task ShowImage_WithUrl_DefaultAsImgParameter_UsesImgsrc()
    {
        // Arrange
        var fileInput = CreateFileInput();

        // Act
        await fileInput.ShowImage("imgId", "http://example.com/default.png");

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("imgId") &&
                args[1].Equals("http://example.com/default.png") &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task ShowImage_WithBrowserFile_AsImgTrue_CallsJSRuntimeWithCorrectParameters()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var file = new BrowserFile { FileName = "test.jpg", FileRefId = 42 };

        // Act
        await fileInput.ShowImage("imgId", file, true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImage",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("imgId") &&
                args[1].Equals(42) &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task ShowImage_WithBrowserFile_AsImgFalse_CallsJSRuntimeWithCssbgimg()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var file = new BrowserFile { FileName = "bg.jpg", FileRefId = 99 };

        // Act
        await fileInput.ShowImage("bgId", file, false);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImage",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("bgId") &&
                args[1].Equals(99) &&
                args[2].Equals("cssbgimg")));
    }

    [Fact]
    public async Task ShowImage_WithBrowserFile_DefaultAsImgParameter_UsesImgsrc()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var file = new BrowserFile { FileName = "default.jpg", FileRefId = 123 };

        // Act
        await fileInput.ShowImage("defaultId", file);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImage",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("defaultId") &&
                args[1].Equals(123) &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task UploadFiles_WithAuthClient_CallsAuthenticateAndJSRuntime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var files = new[] 
        { 
            new BrowserFile { FileName = "file1.txt", FileRefId = 10 },
            new BrowserFile { FileName = "file2.txt", FileRefId = 20 }
        };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com/upload");

        var authInfo = Substitute.For<IAuthenticationInformation>();
        authInfo.Token.Returns("test-token");
        _authClient.Authenticate("TestClient", forceRefesh: true).Returns(authInfo);

        // Act
        await fileInput.UploadFiles(request, files);

        // Assert
        await _authClient.Received(1).Authenticate("TestClient", forceRefesh: true);
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => args.Length == 5 &&
                args[0] == objRef &&
                args[1].Equals("POST") &&
                args[2].Equals("http://test.com/upload") &&
                args[3].Equals("test-token") &&
                GetFileRefIds(args[4]).SequenceEqual(new[] { 10, 20 })));
    }

    [Fact]
    public async Task UploadFiles_WithoutAuthClient_PassesNullToken()
    {
        // Arrange
        var fileInputNoAuth = new FileInput();
        SetPrivateProperty(fileInputNoAuth, "JSRuntime", _jsRuntime);
        SetPrivateProperty(fileInputNoAuth, "MetalNexusRegistry", _registry);
        SetPrivateProperty(fileInputNoAuth, "UrlHelper", _urlHelper);
        SetPrivateProperty(fileInputNoAuth, "authClient", null!);

        var objRef = DotNetObjectReference.Create(fileInputNoAuth);
        SetPrivateField(fileInputNoAuth, "objRef", objRef);

        var request = new TestFileRequest();
        var files = new[] { new BrowserFile { FileName = "file.txt", FileRefId = 5 } };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Put);
        endpoint.HttpClientName.Returns("Client");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://api.com/files");

        // Act
        await fileInputNoAuth.UploadFiles(request, files);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => args.Length == 5 &&
                args[3] == null &&
                GetFileRefIds(args[4]).SequenceEqual(new[] { 5 })));
    }

    [Fact]
    public async Task UploadFiles_EndpointNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var request = new TestFileRequest();
        var files = new[] { new BrowserFile { FileName = "file.txt", FileRefId = 1 } };

        _registry.FindEndpoint(typeof(TestFileRequest)).Returns((IEndpoint?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await fileInput.UploadFiles(request, files));
        exception.Message.ShouldContain("Endpoint Schema contains no endpoint for");
        exception.Message.ShouldContain(typeof(TestFileRequest).FullName ?? string.Empty);
    }

    [Fact]
    public async Task UploadFiles_SetsStartTimeAndResetsTracking()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var files = new[] { new BrowserFile { FileName = "file.txt", FileRefId = 1 } };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("Client");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com");

        SetPrivateField(fileInput, "previousLoaded", 999.0);
        SetPrivateField(fileInput, "previousTotalSecs", 888.0);

        var beforeTime = DateTime.Now;

        // Act
        await fileInput.UploadFiles(request, files);

        var afterTime = DateTime.Now;

        // Assert
        var startTime = (DateTime)GetPrivateField(fileInput, "startTime")!;
        startTime.ShouldBeGreaterThanOrEqualTo(beforeTime);
        startTime.ShouldBeLessThanOrEqualTo(afterTime);

        var previousLoaded = (double)GetPrivateField(fileInput, "previousLoaded")!;
        previousLoaded.ShouldBe(0.0);

        var previousTotalSecs = (double)GetPrivateField(fileInput, "previousTotalSecs")!;
        previousTotalSecs.ShouldBe(0.0);
    }

    [Fact]
    public async Task UploadFiles_WithMultipleFiles_PassesAllFileRefIds()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var files = new[] 
        { 
            new BrowserFile { FileName = "file1.txt", FileRefId = 100 },
            new BrowserFile { FileName = "file2.txt", FileRefId = 200 },
            new BrowserFile { FileName = "file3.txt", FileRefId = 300 },
            new BrowserFile { FileName = "file4.txt", FileRefId = 400 }
        };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("Client");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com");

        // Act
        await fileInput.UploadFiles(request, files);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => GetFileRefIds(args[4]).SequenceEqual(new[] { 100, 200, 300, 400 })));
    }

    [Fact]
    public async Task UploadFiles_WithEmptyFilesArray_PassesEmptyFileRefIds()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var files = Array.Empty<BrowserFile>();

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("Client");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com");

        // Act
        await fileInput.UploadFiles(request, files);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => GetFileRefIds(args[4]).Count == 0));
    }

    [Fact]
    public async Task OnProgress_CalculatesProgressAndInvokesCallback()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IProgressArgs? receivedArgs = null;

        var progressCallback = EventCallback.Factory.Create<FileInput.IProgressArgs>(
            this,
            (FileInput.IProgressArgs args) => { receivedArgs = args; });
        SetPrivateProperty(fileInput, "Progress", progressCallback);

        var startTime = DateTime.Now.AddSeconds(-10);
        SetPrivateField(fileInput, "startTime", startTime);
        SetPrivateField(fileInput, "previousLoaded", 0.0);
        SetPrivateField(fileInput, "previousTotalSecs", 0.0);

        // Act
        await fileInput.OnProgress(1024000.0, 2048000.0, []);

        // Assert
        receivedArgs.ShouldNotBeNull();
        receivedArgs.Loaded.ShouldBe(1024000.0);
        receivedArgs.Total.ShouldBe(2048000.0);
        receivedArgs.UpdateLoaded.ShouldBe(1024000.0);
        receivedArgs.UpdateSpeedKbps.ShouldBeGreaterThan(0);
        receivedArgs.AverageSpeedKbps.ShouldBeGreaterThan(0);

        var previousLoaded = (double)GetPrivateField(fileInput, "previousLoaded")!;
        previousLoaded.ShouldBe(1024000.0);

        var previousTotalSecs = (double)GetPrivateField(fileInput, "previousTotalSecs")!;
        previousTotalSecs.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task OnProgress_UpdatesStateCorrectly()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IProgressArgs? receivedArgs = null;

        var progressCallback = EventCallback.Factory.Create<FileInput.IProgressArgs>(
            this,
            (FileInput.IProgressArgs args) => { receivedArgs = args; });
        SetPrivateProperty(fileInput, "Progress", progressCallback);

        var startTime = DateTime.Now.AddSeconds(-5);
        SetPrivateField(fileInput, "startTime", startTime);
        SetPrivateField(fileInput, "previousLoaded", 500000.0);
        SetPrivateField(fileInput, "previousTotalSecs", 2.5);

        // Act
        await fileInput.OnProgress(1000000.0, 2000000.0, []);

        // Assert
        receivedArgs.ShouldNotBeNull();
        receivedArgs.UpdateLoaded.ShouldBe(500000.0);

        var previousLoaded = (double)GetPrivateField(fileInput, "previousLoaded")!;
        previousLoaded.ShouldBe(1000000.0);
    }

    [Fact]
    public async Task OnProgress_CalculatesAverageSpeedCorrectly()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IProgressArgs? receivedArgs = null;

        var progressCallback = EventCallback.Factory.Create<FileInput.IProgressArgs>(
            this,
            (FileInput.IProgressArgs args) => { receivedArgs = args; });
        SetPrivateProperty(fileInput, "Progress", progressCallback);

        var startTime = DateTime.Now.AddSeconds(-10);
        SetPrivateField(fileInput, "startTime", startTime);
        SetPrivateField(fileInput, "previousLoaded", 0.0);
        SetPrivateField(fileInput, "previousTotalSecs", 0.0);

        // Act
        await fileInput.OnProgress(10240.0, 20480.0, []);

        // Assert
        receivedArgs.ShouldNotBeNull();
        var expectedAvgSpeed = 10240.0 / 10.0 / 1024.0;
        receivedArgs.AverageSpeedKbps.ShouldBe(expectedAvgSpeed, 0.1);
    }

    [Fact]
    public async Task OnProgress_CalculatesUpdateSpeedCorrectly()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IProgressArgs? receivedArgs = null;

        var progressCallback = EventCallback.Factory.Create<FileInput.IProgressArgs>(
            this,
            (FileInput.IProgressArgs args) => { receivedArgs = args; });
        SetPrivateProperty(fileInput, "Progress", progressCallback);

        var startTime = DateTime.Now.AddSeconds(-5);
        SetPrivateField(fileInput, "startTime", startTime);
        SetPrivateField(fileInput, "previousLoaded", 0.0);
        SetPrivateField(fileInput, "previousTotalSecs", 0.0);

        // Act
        await fileInput.OnProgress(5120.0, 10240.0, []);

        // Assert
        receivedArgs.ShouldNotBeNull();
        var currentSecs = (DateTime.Now - startTime).TotalSeconds;
        var expectedUpdateSpeed = 5120.0 / currentSecs / 1024.0;
        receivedArgs.UpdateSpeedKbps.ShouldBe(expectedUpdateSpeed, 0.5);
    }

    [Fact]
    public void ProgressArgs_Fraction_CalculatesCorrectly()
    {
        // Arrange
        var progressArgsType = typeof(FileInput).GetNestedType("ProgressArgs", BindingFlags.NonPublic);
        var progressArgs = Activator.CreateInstance(progressArgsType!);

        var loadedProperty = progressArgsType!.GetProperty("Loaded");
        var totalProperty = progressArgsType.GetProperty("Total");

        loadedProperty!.SetValue(progressArgs, 500.0);
        totalProperty!.SetValue(progressArgs, 1000.0);

        // Act
        var fractionProperty = progressArgsType.GetProperty("Fraction");
        var fraction = (double)fractionProperty!.GetValue(progressArgs)!;

        // Assert
        fraction.ShouldBe(0.5);
    }

    [Fact]
    public void ProgressArgs_Fraction_WithZeroTotal_ReturnsInfinity()
    {
        // Arrange
        var progressArgsType = typeof(FileInput).GetNestedType("ProgressArgs", BindingFlags.NonPublic);
        var progressArgs = Activator.CreateInstance(progressArgsType!);

        var loadedProperty = progressArgsType!.GetProperty("Loaded");
        var totalProperty = progressArgsType.GetProperty("Total");

        loadedProperty!.SetValue(progressArgs, 100.0);
        totalProperty!.SetValue(progressArgs, 0.0);

        // Act
        var fractionProperty = progressArgsType.GetProperty("Fraction");
        var fraction = (double)fractionProperty!.GetValue(progressArgs)!;

        // Assert
        double.IsPositiveInfinity(fraction).ShouldBeTrue();
    }

    [Fact]
    public void ProgressArgs_Fraction_WithZeroLoadedAndZeroTotal_ReturnsNaN()
    {
        // Arrange
        var progressArgsType = typeof(FileInput).GetNestedType("ProgressArgs", BindingFlags.NonPublic);
        var progressArgs = Activator.CreateInstance(progressArgsType!);

        var loadedProperty = progressArgsType!.GetProperty("Loaded");
        var totalProperty = progressArgsType.GetProperty("Total");

        loadedProperty!.SetValue(progressArgs, 0.0);
        totalProperty!.SetValue(progressArgs, 0.0);

        // Act
        var fractionProperty = progressArgsType.GetProperty("Fraction");
        var fraction = (double)fractionProperty!.GetValue(progressArgs)!;

        // Assert
        double.IsNaN(fraction).ShouldBeTrue();
    }

    [Fact]
    public void ProgressArgs_Fraction_WithFullProgress_ReturnsOne()
    {
        // Arrange
        var progressArgsType = typeof(FileInput).GetNestedType("ProgressArgs", BindingFlags.NonPublic);
        var progressArgs = Activator.CreateInstance(progressArgsType!);

        var loadedProperty = progressArgsType!.GetProperty("Loaded");
        var totalProperty = progressArgsType.GetProperty("Total");

        loadedProperty!.SetValue(progressArgs, 2048.0);
        totalProperty!.SetValue(progressArgs, 2048.0);

        // Act
        var fractionProperty = progressArgsType.GetProperty("Fraction");
        var fraction = (double)fractionProperty!.GetValue(progressArgs)!;

        // Assert
        fraction.ShouldBe(1.0);
    }

    [Fact]
    public async Task OnSuccess_WithJsonElement_SetsPendingSuccessAndCallsStateHasChanged()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var renderer = InitializeRenderHandle(fileInput);
        
        var jsonString = "{\"result\":\"success\"}";
        var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        // Act
        await renderer.Dispatcher.InvokeAsync(() => fileInput.OnSuccess(jsonElement));

        // Assert
        var pendingSuccess = GetPrivateField(fileInput, "pendingSuccess");
        pendingSuccess.ShouldNotBeNull();
        pendingSuccess.ShouldBeAssignableTo<FileInput.IFilesUploadedArgs>();
    }

    [Fact]
    public async Task OnSuccess_WithNullJsonElement_SetsPendingSuccessToNonNullValue()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var renderer = InitializeRenderHandle(fileInput);

        // Act
        await renderer.Dispatcher.InvokeAsync(() => fileInput.OnSuccess(null));

        // Assert
        var pendingSuccess = GetPrivateField(fileInput, "pendingSuccess");
        pendingSuccess.ShouldNotBeNull();
        pendingSuccess.ShouldBeAssignableTo<FileInput.IFilesUploadedArgs>();
    }

    [Fact]
    public async Task OnSuccess_CreatesFilesUploadedArgsWithCorrectJsonElement()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var renderer = InitializeRenderHandle(fileInput);
        
        var jsonString = "{\"id\":123,\"name\":\"test\"}";
        var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        // Act
        await renderer.Dispatcher.InvokeAsync(() => fileInput.OnSuccess(jsonElement));

        // Assert
        var pendingSuccess = GetPrivateField(fileInput, "pendingSuccess") as FileInput.IFilesUploadedArgs;
        pendingSuccess.ShouldNotBeNull();
        pendingSuccess!.HasValue.ShouldBeTrue();
    }

    private TestRenderer InitializeRenderHandle(FileInput fileInput)
    {
        var renderHandleField = typeof(Microsoft.AspNetCore.Components.ComponentBase)
            .GetField("_renderHandle", BindingFlags.NonPublic | BindingFlags.Instance);
        
        var renderer = new TestRenderer(Substitute.For<IServiceProvider>());
        
        if (renderHandleField != null)
        {
            var renderHandleType = typeof(Microsoft.AspNetCore.Components.RenderHandle);
            var renderHandleCtor = renderHandleType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(Microsoft.AspNetCore.Components.RenderTree.Renderer), typeof(int) },
                null);

            if (renderHandleCtor != null)
            {
                var renderHandle = renderHandleCtor.Invoke(new object[] { renderer, 0 });
                renderHandleField.SetValue(fileInput, renderHandle);
            }
        }
        
        return renderer;
    }

#pragma warning disable BL0006 // Do not use RenderTree types
    private class TestRenderer : Microsoft.AspNetCore.Components.RenderTree.Renderer
    {
        private readonly Microsoft.AspNetCore.Components.Dispatcher _dispatcher;

        public TestRenderer(IServiceProvider serviceProvider) 
            : base(serviceProvider, Substitute.For<Microsoft.Extensions.Logging.ILoggerFactory>())
        {
            _dispatcher = Microsoft.AspNetCore.Components.Dispatcher.CreateDefault();
        }

        public override Microsoft.AspNetCore.Components.Dispatcher Dispatcher => _dispatcher;

        protected override void HandleException(Exception exception)
        {
        }

        protected override Task UpdateDisplayAsync(in Microsoft.AspNetCore.Components.RenderTree.RenderBatch renderBatch)
        {
            return Task.CompletedTask;
        }
    }
#pragma warning restore BL0006

    [Fact]
    public void FilesUploadedArgs_Constructor_WithNullJsonElement_InitializesCorrectly()
    {
        // Arrange & Act
        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { null }) as FileInput.IFilesUploadedArgs;

        // Assert
        args.ShouldNotBeNull();
        args.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void FilesUploadedArgs_Constructor_WithJsonElement_InitializesCorrectly()
    {
        // Arrange
        var jsonString = "{\"result\":\"success\"}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        // Act
        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Assert
        args.ShouldNotBeNull();
        args.HasValue.ShouldBeTrue();
    }

    [Fact]
    public void FilesUploadedArgs_HasValue_WithNull_ReturnsFalse()
    {
        // Arrange
        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { null }) as FileInput.IFilesUploadedArgs;

        // Act
        var hasValue = args!.HasValue;

        // Assert
        hasValue.ShouldBeFalse();
    }

    [Fact]
    public void FilesUploadedArgs_HasValue_WithJsonElement_ReturnsTrue()
    {
        // Arrange
        var jsonString = "{\"id\":42}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var hasValue = args!.HasValue;

        // Assert
        hasValue.ShouldBeTrue();
    }

    [Fact]
    public void FilesUploadedArgs_Get_WithNullJson_ReturnsNull()
    {
        // Arrange
        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { null }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<TestResponse>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void FilesUploadedArgs_Get_WithJsonElement_DeserializesCorrectly()
    {
        // Arrange
        var jsonString = "{\"Id\":123,\"Name\":\"test\"}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<TestResponse>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(123);
        result.Name.ShouldBe("test");
    }

    [Fact]
    public void FilesUploadedArgs_Get_WithComplexJson_DeserializesCorrectly()
    {
        // Arrange
        var jsonString = "{\"Id\":999,\"Name\":\"complex\",\"Nested\":{\"Value\":42}}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<ComplexTestResponse>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(999);
        result.Name.ShouldBe("complex");
        result.Nested.ShouldNotBeNull();
        result.Nested.Value.ShouldBe(42);
    }

    [Fact]
    public void FilesUploadedArgs_Get_WithEmptyJson_DeserializesWithDefaultValues()
    {
        // Arrange
        var jsonString = "{}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<TestResponse>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(0);
        result.Name.ShouldBeNull();
    }

    [Fact]
    public async Task OnError_WithDelegateSet_SetsPendingErrorAndCallsStateHasChanged()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var renderer = InitializeRenderHandle(fileInput);

        var uploadFailedInvoked = false;
        var uploadFailedCallback = EventCallback.Factory.Create<Exception>(
            this,
            (Exception error) => { uploadFailedInvoked = true; });
        SetPrivateProperty(fileInput, "UploadFailed", uploadFailedCallback);

        var exceptionJson = "{\"Message\":\"Test error\"}";
        _options.ServerStackTraceOnExceptionsIncluded.Returns(false);

        // Act
        await renderer.Dispatcher.InvokeAsync(() => fileInput.OnError(500, exceptionJson));

        // Assert
        var pendingError = GetPrivateField(fileInput, "pendingError");
        pendingError.ShouldNotBeNull();
        uploadFailedInvoked.ShouldBeFalse(); // Should not be invoked immediately, only StateHasChanged is called
    }

    [Fact]
    public void OnError_WithoutDelegate_ThrowsException()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var exceptionJson = "{\"Message\":\"Test error\"}";
        _options.ServerStackTraceOnExceptionsIncluded.Returns(false);

        // Act & Assert
        Should.Throw<Exception>(() => fileInput.OnError(400, exceptionJson));
    }

    [Fact]
    public async Task OnError_WithDelegate_SetsPendingErrorFromHttpStatus()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var renderer = InitializeRenderHandle(fileInput);

        var uploadFailedCallback = EventCallback.Factory.Create<Exception>(this, (Exception error) => { });
        SetPrivateProperty(fileInput, "UploadFailed", uploadFailedCallback);

        var exceptionJson = "{\"Message\":\"Not found\"}";
        _options.ServerStackTraceOnExceptionsIncluded.Returns(false);

        // Act
        await renderer.Dispatcher.InvokeAsync(() => fileInput.OnError(404, exceptionJson));

        // Assert
        var pendingError = GetPrivateField(fileInput, "pendingError") as Exception;
        pendingError.ShouldNotBeNull();
    }

    [Fact]
    public async Task OnError_WithServerStackTraceEnabled_PassesCorrectParameter()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var renderer = InitializeRenderHandle(fileInput);

        var uploadFailedCallback = EventCallback.Factory.Create<Exception>(this, (Exception error) => { });
        SetPrivateProperty(fileInput, "UploadFailed", uploadFailedCallback);

        var exceptionJson = "{\"Message\":\"Error with stack\",\"StackTrace\":\"at SomeMethod()\"}";
        _options.ServerStackTraceOnExceptionsIncluded.Returns(true);

        // Act
        await renderer.Dispatcher.InvokeAsync(() => fileInput.OnError(500, exceptionJson));

        // Assert
        var pendingError = GetPrivateField(fileInput, "pendingError") as Exception;
        pendingError.ShouldNotBeNull();
    }

    [Fact]
    public void OnError_WithDifferentHttpStatusCodes_CreatesException()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var exceptionJson = "{\"Message\":\"Unauthorized\"}";
        _options.ServerStackTraceOnExceptionsIncluded.Returns(false);

        // Act & Assert
        var exception = Should.Throw<Exception>(() => fileInput.OnError(401, exceptionJson));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task OnError_Multiple_CallsStateHasChangedEachTime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var renderer = InitializeRenderHandle(fileInput);

        var uploadFailedCallback = EventCallback.Factory.Create<Exception>(this, (Exception error) => { });
        SetPrivateProperty(fileInput, "UploadFailed", uploadFailedCallback);

        var exceptionJson1 = "{\"Message\":\"Error 1\"}";
        var exceptionJson2 = "{\"Message\":\"Error 2\"}";
        _options.ServerStackTraceOnExceptionsIncluded.Returns(false);

        // Act
        await renderer.Dispatcher.InvokeAsync(() => fileInput.OnError(500, exceptionJson1));
        var pendingError1 = GetPrivateField(fileInput, "pendingError");

        await renderer.Dispatcher.InvokeAsync(() => fileInput.OnError(500, exceptionJson2));
        var pendingError2 = GetPrivateField(fileInput, "pendingError");

        // Assert
        pendingError1.ShouldNotBeNull();
        pendingError2.ShouldNotBeNull();
    }

    [Fact]
    public void IFilesUploadedArgs_Get_WithNullJson_ReturnsNull()
    {
        // Arrange
        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { null }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<TestResponse>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void IFilesUploadedArgs_Get_WithValidJson_ReturnsDeserializedObject()
    {
        // Arrange
        var jsonString = "{\"Id\":456,\"Name\":\"interface test\"}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<TestResponse>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(456);
        result.Name.ShouldBe("interface test");
    }

    private class TestResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class ComplexTestResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public NestedResponse? Nested { get; set; }
    }

    private class NestedResponse
    {
        public int Value { get; set; }
    }

    [Fact]
    public async Task DisposeAsync_WhenCalled_SetsIsDisposedToTrue()
    {
        // Arrange
        var fileInput = CreateFileInput();
        SetPrivateField(fileInput, "isDisposed", false);

        // Act
        await fileInput.DisposeAsync();

        // Assert
        var isDisposed = (bool)GetPrivateField(fileInput, "isDisposed")!;
        isDisposed.ShouldBeTrue();
    }

    [Fact]
    public async Task DisposeAsync_WhenCalled_InvokesJSRuntimeDetach()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var id = GetPrivateField(fileInput, "Id") as string;

        // Act
        await fileInput.DisposeAsync();

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.detach",
            Arg.Is<object[]>(args => args.Length == 1 && args[0].Equals(id)));
    }

    [Fact]
    public async Task DisposeAsync_WhenObjRefIsNotNull_DisposesObjRef()
    {
        // Arrange
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        // Act
        await fileInput.DisposeAsync();

        // Assert - Verify no exception was thrown and objRef is still set
        var objRefAfter = GetPrivateField(fileInput, "objRef");
        objRefAfter.ShouldBe(objRef);
    }

    [Fact]
    public async Task DisposeAsync_WhenObjRefIsNull_DoesNotThrow()
    {
        // Arrange
        var fileInput = CreateFileInput();
        SetPrivateField(fileInput, "objRef", null);

        // Act & Assert
        await Should.NotThrowAsync(async () => await fileInput.DisposeAsync());
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithEmptyImgId_CallsJSRuntime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("", "http://example.com/image.jpg", true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("") &&
                args[1].Equals("http://example.com/image.jpg") &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithEmptyUrl_CallsJSRuntime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("imgId", "", true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("imgId") &&
                args[1].Equals("") &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithBrowserFileZeroFileRefId_CallsJSRuntime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var file = new BrowserFile { FileName = "test.jpg", FileRefId = 0 };
        var files = new[] { file };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("img1", file, true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImage",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("img1") &&
                args[1].Equals(0) &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task IFilesPickedArgs_UploadFiles_WithEmptyFilesArray_CallsFileInputUploadFiles()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var testRequest = new TestFileRequest();
        var uploadFiles = Array.Empty<BrowserFile>();

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(testRequest).Returns("http://test.com/upload");

        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        // Act
        await pickedArgs!.UploadFiles(testRequest, uploadFiles);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => args.Length == 5 && GetFileRefIds(args[4]).Count == 0));
    }

    [Fact]
    public async Task IFilesPickedArgs_UploadFiles_WithoutRequest_SingleFile_CallsFileInputUploadFiles()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var uploadFile = new BrowserFile { FileName = "upload.txt", FileRefId = 50 };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor(Arg.Any<TestFileRequest>()).Returns("http://test.com/upload");

        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        // Act
        await pickedArgs!.UploadFiles<TestFileRequest>(uploadFile);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => args.Length == 5 && GetFileRefIds(args[4]).SequenceEqual(new[] { 50 })));
    }

    [Fact]
    public async Task FilesPickedArgs_UploadFiles_WithRequest_VerifiesDelegationToFileInput()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var testRequest = new TestFileRequest();
        var uploadFile = new BrowserFile { FileName = "upload.txt", FileRefId = 77 };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(testRequest).Returns("http://test.com/upload");

        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        // Act
        await pickedArgs!.UploadFiles(testRequest, uploadFile);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => args.Length == 5 &&
                args[0] == objRef &&
                args[1].Equals("POST") &&
                args[2].Equals("http://test.com/upload") &&
                GetFileRefIds(args[4]).SequenceEqual(new[] { 77 })));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithUrl_DefaultParameter_UsesImgsrcAsCssMode()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("testId", "http://example.com/test.png");

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("testId") &&
                args[1].Equals("http://example.com/test.png") &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithBrowserFile_DefaultParameter_UsesImgsrcAsCssMode()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var file = new BrowserFile { FileName = "test.jpg", FileRefId = 123 };
        var files = new[] { file };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("testId", file);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImage",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("testId") &&
                args[1].Equals(123) &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task IFilesPickedArgs_UploadFiles_WithoutRequest_EmptyFilesArray_CallsFileInputUploadFiles()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor(Arg.Any<TestFileRequest>()).Returns("http://test.com/upload");

        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        // Act
        await pickedArgs!.UploadFiles<TestFileRequest>();

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => args.Length == 5 && GetFileRefIds(args[4]).Count == 0));
    }

    [Fact]
    public async Task IFilesPickedArgs_UploadFiles_WithRequest_NoFilesProvided_CallsFileInputUploadFiles()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var testRequest = new TestFileRequest();

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(testRequest).Returns("http://test.com/upload");

        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        // Act
        await pickedArgs!.UploadFiles<TestFileRequest>(testRequest);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => args.Length == 5 && GetFileRefIds(args[4]).Count == 0));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithSpecialCharactersInUrl_CallsJSRuntime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("img1", "http://example.com/path/to/image%20with%20spaces.jpg?param=value&other=123", true);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImageUrl",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("img1") &&
                args[1].Equals("http://example.com/path/to/image%20with%20spaces.jpg?param=value&other=123") &&
                args[2].Equals("imgsrc")));
    }

    [Fact]
    public async Task IFilesPickedArgs_ShowImage_WithNegativeFileRefId_CallsJSRuntime()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var file = new BrowserFile { FileName = "test.jpg", FileRefId = -1 };
        var files = new[] { file };
        await fileInput.OnFilesSelected(files);

        // Act
        await pickedArgs!.ShowImage("img1", file, false);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.showImage",
            Arg.Is<object[]>(args => args.Length == 3 &&
                args[0].Equals("img1") &&
                args[1].Equals(-1) &&
                args[2].Equals("cssbgimg")));
    }

    [Fact]
    public async Task FilesPickedArgs_UploadFiles_WithoutRequestParameter_MultipleFiles_CreatesNewRequestAndCallsUpload()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var uploadFiles = new[]
        {
            new BrowserFile { FileName = "file1.txt", FileRefId = 10 },
            new BrowserFile { FileName = "file2.txt", FileRefId = 20 },
            new BrowserFile { FileName = "file3.txt", FileRefId = 30 }
        };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor(Arg.Any<TestFileRequest>()).Returns("http://test.com/upload");

        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        // Act
        await pickedArgs!.UploadFiles<TestFileRequest>(uploadFiles);

        // Assert
        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => args.Length == 5 && GetFileRefIds(args[4]).SequenceEqual(new[] { 10, 20, 30 })));
    }

    [Fact]
    public async Task FilesPickedArgs_UploadFiles_WithoutRequestParameter_CreatesNewRequestInstance()
    {
        // Arrange
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var files = new[] { new BrowserFile { FileName = "test.txt", FileRefId = 1 } };
        await fileInput.OnFilesSelected(files);

        var uploadFiles = new[] { new BrowserFile { FileName = "upload.txt", FileRefId = 5 } };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor(Arg.Any<TestFileRequest>()).Returns("http://test.com/upload");

        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        // Act
        await pickedArgs!.UploadFiles<TestFileRequest>(uploadFiles);

        // Assert - Verify that GetUrlFor was called, which means a new request was created
        _urlHelper.Received(1).GetUrlFor(Arg.Any<TestFileRequest>());
    }

    [Fact]
    public void FilesUploadedArgs_Get_WithInvalidJsonStructure_ReturnsNullOrPartialObject()
    {
        // Arrange
        var jsonString = "{\"WrongProperty\":\"value\"}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<TestResponse>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(0);
        result.Name.ShouldBeNull();
    }

    [Fact]
    public void FilesUploadedArgs_Get_WithArrayJson_CanDeserializeToList()
    {
        // Arrange
        var jsonString = "[{\"Id\":1,\"Name\":\"first\"},{\"Id\":2,\"Name\":\"second\"}]";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<List<TestResponse>>();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe(1);
        result[0].Name.ShouldBe("first");
        result[1].Id.ShouldBe(2);
        result[1].Name.ShouldBe("second");
    }

    [Fact]
    public void FilesUploadedArgs_Get_WithNullPropertiesInJson_DeserializesWithNulls()
    {
        // Arrange
        var jsonString = "{\"Id\":42,\"Name\":null}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result = args!.Get<TestResponse>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(42);
        result.Name.ShouldBeNull();
    }

    [Fact]
    public void FilesUploadedArgs_Get_CalledMultipleTimes_ReturnsSameDeserializedData()
    {
        // Arrange
        var jsonString = "{\"Id\":99,\"Name\":\"persistent\"}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result1 = args!.Get<TestResponse>();
        var result2 = args.Get<TestResponse>();

        // Assert
        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result1.Id.ShouldBe(99);
        result2.Id.ShouldBe(99);
        result1.Name.ShouldBe("persistent");
        result2.Name.ShouldBe("persistent");
    }

    [Fact]
    public void FilesUploadedArgs_Get_DifferentTypes_DeserializesSameJsonToDifferentStructures()
    {
        // Arrange
        var jsonString = "{\"Id\":100,\"Name\":\"test\",\"Extra\":\"value\"}";
        var jsonDoc = JsonDocument.Parse(jsonString);
        var jsonElement = jsonDoc.RootElement;

        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { jsonElement }) as FileInput.IFilesUploadedArgs;

        // Act
        var result1 = args!.Get<TestResponse>();
        var result2 = args.Get<ExtendedTestResponse>();

        // Assert
        result1.ShouldNotBeNull();
        result1.Id.ShouldBe(100);
        result1.Name.ShouldBe("test");

        result2.ShouldNotBeNull();
        result2.Id.ShouldBe(100);
        result2.Name.ShouldBe("test");
        result2.Extra.ShouldBe("value");
    }

    [Fact]
    public void IFilesUploadedArgs_Get_MultipleCallsWithNull_AlwaysReturnsNull()
    {
        // Arrange
        var filesUploadedArgsType = typeof(FileInput).GetNestedType("FilesUploadedArgs", BindingFlags.NonPublic);
        var constructor = filesUploadedArgsType!.GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(JsonElement?) },
            null);
        var args = constructor!.Invoke(new object?[] { null }) as FileInput.IFilesUploadedArgs;

        // Act
        var result1 = args!.Get<TestResponse>();
        var result2 = args.Get<ComplexTestResponse>();

        // Assert
        result1.ShouldBeNull();
        result2.ShouldBeNull();
    }

    private class ExtendedTestResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Extra { get; set; }
    }

    // ── New multi-file slot upload tests ──────────────────────────────────────────

    [Fact]
    public async Task UploadFiles_WithSlotDictionary_SlotsUseSlotNamesAsFieldName()
    {
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var avatarFile = new BrowserFile { FileName = "photo.jpg", FileRefId = 10 };
        var resumeFile = new BrowserFile { FileName = "cv.pdf", FileRefId = 20 };
        var slots = new Dictionary<string, BrowserFile>
        {
            ["avatar"] = avatarFile,
            ["resume"] = resumeFile
        };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com/upload");

        await fileInput.UploadFiles<TestFileRequest>(request, slots);

        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args =>
                GetFileEntry(args[4], 0, "fileRefId")!.Equals(10) &&
                GetFileEntry(args[4], 0, "fieldName")!.Equals("avatar") &&
                GetFileEntry(args[4], 1, "fileRefId")!.Equals(20) &&
                GetFileEntry(args[4], 1, "fieldName")!.Equals("resume")));
    }

    [Fact]
    public async Task UploadFiles_UnnamedFiles_UseFilesAsFieldName()
    {
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var file1 = new BrowserFile { FileName = "a.txt", FileRefId = 5 };
        var file2 = new BrowserFile { FileName = "b.txt", FileRefId = 6 };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com/upload");

        await fileInput.UploadFiles(request, file1, file2);

        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args =>
                GetFileEntry(args[4], 0, "fieldName")!.Equals("files") &&
                GetFileEntry(args[4], 0, "fileRefId")!.Equals(5) &&
                GetFileEntry(args[4], 1, "fieldName")!.Equals("files") &&
                GetFileEntry(args[4], 1, "fileRefId")!.Equals(6)));
    }

    [Fact]
    public async Task UploadFiles_SlotAndUnnamedFiles_SlotsFirstThenFiles()
    {
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var slotFile = new BrowserFile { FileName = "photo.jpg", FileRefId = 100 };
        var extra = new BrowserFile { FileName = "extra.txt", FileRefId = 200 };
        var slots = new Dictionary<string, BrowserFile> { ["avatar"] = slotFile };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com/upload");

        await fileInput.UploadFiles<TestFileRequest>(request, slots, extra);

        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args =>
                GetFileEntries(args[4]).Count == 2 &&
                GetFileEntry(args[4], 0, "fieldName")!.Equals("avatar") &&
                GetFileEntry(args[4], 0, "fileRefId")!.Equals(100) &&
                GetFileEntry(args[4], 1, "fieldName")!.Equals("files") &&
                GetFileEntry(args[4], 1, "fileRefId")!.Equals(200)));
    }

    [Fact]
    public async Task UploadFiles_WithNullSlots_TreatsLikeNoSlots()
    {
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var file = new BrowserFile { FileName = "doc.pdf", FileRefId = 7 };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com/upload");

        await fileInput.UploadFiles<TestFileRequest>(request, (Dictionary<string, BrowserFile>?)null, file);

        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args =>
                GetFileEntries(args[4]).Count == 1 &&
                GetFileEntry(args[4], 0, "fieldName")!.Equals("files") &&
                GetFileEntry(args[4], 0, "fileRefId")!.Equals(7)));
    }

    [Fact]
    public async Task UploadFiles_WithEmptySlotsAndNoFiles_SendsEmptyEntries()
    {
        var fileInput = CreateFileInput();
        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        var request = new TestFileRequest();
        var slots = new Dictionary<string, BrowserFile>();

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("TestClient");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com/upload");

        await fileInput.UploadFiles<TestFileRequest>(request, slots);

        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args => GetFileEntries(args[4]).Count == 0));
    }

    [Fact]
    public async Task OnProgress_WithFileProgressInfo_PropagatesFilesCollection()
    {
        var fileInput = CreateFileInput();
        FileInput.IProgressArgs? receivedArgs = null;

        var progressCallback = EventCallback.Factory.Create<FileInput.IProgressArgs>(
            this,
            (FileInput.IProgressArgs args) => { receivedArgs = args; });
        SetPrivateProperty(fileInput, "Progress", progressCallback);

        SetPrivateField(fileInput, "startTime", DateTime.Now.AddSeconds(-2));
        SetPrivateField(fileInput, "previousLoaded", 0.0);
        SetPrivateField(fileInput, "previousTotalSecs", 0.0);

        var fileInfos = new[]
        {
            new FileInput.FileProgressInfo("photo.jpg", 512, 1024),
            new FileInput.FileProgressInfo("doc.pdf", 256, 512)
        };

        await fileInput.OnProgress(768.0, 1536.0, fileInfos);

        receivedArgs.ShouldNotBeNull();
        receivedArgs.Files.Count.ShouldBe(2);
        receivedArgs.Files[0].FileName.ShouldBe("photo.jpg");
        receivedArgs.Files[0].Loaded.ShouldBe(512);
        receivedArgs.Files[0].Total.ShouldBe(1024);
        receivedArgs.Files[1].FileName.ShouldBe("doc.pdf");
        receivedArgs.Files[1].Loaded.ShouldBe(256);
        receivedArgs.Files[1].Total.ShouldBe(512);
    }

    [Fact]
    public async Task OnProgress_WithEmptyFileProgressInfo_PropagatesEmptyCollection()
    {
        var fileInput = CreateFileInput();
        FileInput.IProgressArgs? receivedArgs = null;

        var progressCallback = EventCallback.Factory.Create<FileInput.IProgressArgs>(
            this,
            (FileInput.IProgressArgs args) => { receivedArgs = args; });
        SetPrivateProperty(fileInput, "Progress", progressCallback);

        SetPrivateField(fileInput, "startTime", DateTime.Now.AddSeconds(-1));
        SetPrivateField(fileInput, "previousLoaded", 0.0);
        SetPrivateField(fileInput, "previousTotalSecs", 0.0);

        await fileInput.OnProgress(100.0, 200.0, []);

        receivedArgs.ShouldNotBeNull();
        receivedArgs.Files.Count.ShouldBe(0);
    }

    [Fact]
    public async Task IFilesPickedArgs_UploadFiles_WithSlots_RoutesSlotFilesByName()
    {
        var fileInput = CreateFileInput();
        FileInput.IFilesPickedArgs? pickedArgs = null;

        var filesPickedCallback = EventCallback.Factory.Create<FileInput.IFilesPickedArgs>(
            this,
            (FileInput.IFilesPickedArgs args) => { pickedArgs = args; });
        SetPrivateProperty(fileInput, "FilesPicked", filesPickedCallback);

        var file = new BrowserFile { FileName = "photo.jpg", FileRefId = 77 };
        await fileInput.OnFilesSelected(new[] { file });

        var request = new TestFileRequest();
        var slots = new Dictionary<string, BrowserFile> { ["profilePic"] = file };

        var endpoint = Substitute.For<IEndpoint>();
        endpoint.HttpMethod.Returns(HttpMethod.Post);
        endpoint.HttpClientName.Returns("Client");
        _registry.FindEndpoint(typeof(TestFileRequest)).Returns(endpoint);
        _urlHelper.GetUrlFor<TestFileRequest>(request).Returns("http://test.com/upload");

        var objRef = DotNetObjectReference.Create(fileInput);
        SetPrivateField(fileInput, "objRef", objRef);

        await pickedArgs!.UploadFiles(request, slots);

        await _jsRuntime.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "RossWrightFileInput.uploadFiles",
            Arg.Is<object[]>(args =>
                GetFileEntries(args[4]).Count == 1 &&
                GetFileEntry(args[4], 0, "fieldName")!.Equals("profilePic") &&
                GetFileEntry(args[4], 0, "fileRefId")!.Equals(77)));
    }

    // ── Helpers for the new fileEntries payload ───────────────────────────────────

    /// <summary>Extracts file reference IDs from the list of fileEntry objects sent to JS.</summary>
    private static System.Collections.IList GetFileEntries(object fileEntriesArg)
    {
        return (System.Collections.IList)fileEntriesArg;
    }

    /// <summary>Returns all fileRefId values from the fileEntries list, in order.</summary>
    private static IReadOnlyList<int> GetFileRefIds(object fileEntriesArg)
    {
        var list = GetFileEntries(fileEntriesArg);
        var ids = new List<int>();
        foreach (var entry in list)
        {
            var type = entry.GetType();
            var prop = type.GetProperty("fileRefId") ?? type.GetProperty("FileRefId");
            ids.Add((int)prop!.GetValue(entry)!);
        }
        return ids;
    }

    /// <summary>Returns a named property value from a specific fileEntry by index.</summary>
    private static object? GetFileEntry(object fileEntriesArg, int index, string propName)
    {
        var list = GetFileEntries(fileEntriesArg);
        var entry = list[index]!;
        var type = entry.GetType();
        var prop = type.GetProperty(propName) ?? type.GetProperty(char.ToUpper(propName[0]) + propName[1..]);
        return prop?.GetValue(entry);
    }
}
