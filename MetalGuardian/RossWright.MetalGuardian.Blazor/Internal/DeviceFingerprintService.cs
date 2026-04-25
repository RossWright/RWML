using Microsoft.JSInterop;

namespace RossWright.MetalGuardian;

internal class DeviceFingerprintService(
    IJSRuntime _jsRuntime,
    IJsScriptLoaderService _scriptLoader)
    : IDeviceFingerprintService
{
    public async Task<string> GetFingerprint()
    {
        await _scriptLoader.EnsureLoaded(
            "_content/RossWright.MetalGuardian.Blazor/fingerprint.js",
            "window.RossWrightDeviceFingerprint");
        return await _jsRuntime.InvokeAsync<string>("RossWrightDeviceFingerprint");
    }
}