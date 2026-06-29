using Microsoft.JSInterop;

namespace RossWright.MetalGuardian.Blazor.Tests.Internal;

public class DeviceFingerprintServiceTests
{
    [Fact]
    public async Task GetFingerprint_EnsuresScriptLoaded_WithCorrectParameters()
    {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        var scriptLoader = Substitute.For<IJsScriptLoaderService>();
        var service = new DeviceFingerprintService(jsRuntime, scriptLoader);
        
        jsRuntime.InvokeAsync<string>("RossWrightDeviceFingerprint")
            .Returns(new ValueTask<string>("test-fingerprint"));

        // Act
        await service.GetFingerprint();

        // Assert
        await scriptLoader.Received(1).EnsureLoaded(
            "_content/RossWright.MetalGuardian.Blazor/fingerprint.js",
            "window.RossWrightDeviceFingerprint");
    }

    [Fact]
    public async Task GetFingerprint_InvokesJavaScriptFunction_WithCorrectName()
    {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        var scriptLoader = Substitute.For<IJsScriptLoaderService>();
        var service = new DeviceFingerprintService(jsRuntime, scriptLoader);
        
        jsRuntime.InvokeAsync<string>("RossWrightDeviceFingerprint")
            .Returns(new ValueTask<string>("test-fingerprint"));

        // Act
        await service.GetFingerprint();

        // Assert
        await jsRuntime.Received(1).InvokeAsync<string>("RossWrightDeviceFingerprint");
    }

    [Fact]
    public async Task GetFingerprint_ReturnsValueFromJavaScript()
    {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        var scriptLoader = Substitute.For<IJsScriptLoaderService>();
        var service = new DeviceFingerprintService(jsRuntime, scriptLoader);
        
        const string expectedFingerprint = "unique-device-fingerprint-abc123";
        jsRuntime.InvokeAsync<string>("RossWrightDeviceFingerprint")
            .Returns(new ValueTask<string>(expectedFingerprint));

        // Act
        var result = await service.GetFingerprint();

        // Assert
        result.ShouldBe(expectedFingerprint);
    }

    [Fact]
    public async Task GetFingerprint_EnsuresScriptLoadedBeforeInvokingJavaScript()
    {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        var scriptLoader = Substitute.For<IJsScriptLoaderService>();
        var service = new DeviceFingerprintService(jsRuntime, scriptLoader);
        
        var callOrder = new List<string>();
        
        scriptLoader.When(x => x.EnsureLoaded(Arg.Any<string>(), Arg.Any<string>()))
            .Do(_ => callOrder.Add("EnsureLoaded"));
        
        jsRuntime.InvokeAsync<string>(Arg.Any<string>())
            .Returns(callInfo =>
            {
                callOrder.Add("InvokeAsync");
                return new ValueTask<string>("fingerprint");
            });

        // Act
        await service.GetFingerprint();

        // Assert
        callOrder.ShouldBe(new[] { "EnsureLoaded", "InvokeAsync" });
    }

    [Fact]
    public async Task GetFingerprint_ReturnsEmptyString_WhenJavaScriptReturnsEmpty()
    {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        var scriptLoader = Substitute.For<IJsScriptLoaderService>();
        var service = new DeviceFingerprintService(jsRuntime, scriptLoader);
        
        jsRuntime.InvokeAsync<string>("RossWrightDeviceFingerprint")
            .Returns(new ValueTask<string>(string.Empty));

        // Act
        var result = await service.GetFingerprint();

        // Assert
        result.ShouldBe(string.Empty);
    }
}
