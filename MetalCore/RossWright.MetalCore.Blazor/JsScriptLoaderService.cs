using Microsoft.JSInterop;

namespace RossWright;

/// <summary>
/// Loads a JavaScript file exactly once per path, guarded against concurrent or duplicate calls via <see cref="LoadGuard"/>.
/// Register via <see cref="MetalCoreBlazorExtensions.AddJsScriptLoader"/>.
/// </summary>
public interface IJsScriptLoaderService
{
    /// <summary>
    /// Ensures the script at <paramref name="path"/> is loaded exactly once per path/existence-object pair.
    /// Concurrent callers for the same key await the same underlying load operation.
    /// </summary>
    /// <param name="path">The URL or relative path of the JavaScript file to load.</param>
    /// <param name="existenceObject">A JavaScript global name or expression defined by the script; used to skip loading if the script is already present.</param>
    /// <param name="fileHash">An optional SRI integrity hash applied to the script element.</param>
    Task EnsureLoaded(string path, string existenceObject, string? fileHash = null);
}

internal class JsScriptLoaderService(
    IJSRuntime _jsRuntime) 
    : IJsScriptLoaderService
{
    private readonly LoadGuard _loadGuard = new();

    public Task EnsureLoaded(string path, string existenceObject, string? fileHash = null) =>
        _loadGuard.Load($"EnsureScriptsLoadedAsync_{path}_{existenceObject}", async () =>
        {
            // Define the JS helper if not already (call this once, e.g., in app startup or here)
            await _jsRuntime.InvokeVoidAsync("eval", @"
                    if (typeof window.loadScriptIfNotExists === 'undefined') {                
                        window.loadScriptIfNotExists = function(url, integrity, checkExpression) {
                            return new Promise((resolve, reject) => {
                                if (checkExpression && eval(checkExpression)) {
                                    resolve();
                                    return;
                                }
                                var script = document.createElement('script');
                                script.src = url;
                                if (integrity) script.integrity = integrity;
                                script.crossOrigin = 'anonymous';
                                script.onload = resolve;
                                script.onerror = reject;
                                document.head.appendChild(script);
                            });
                        };
                    }
                ");

            await _jsRuntime.InvokeVoidAsync("loadScriptIfNotExists", path, fileHash,
                $"typeof {existenceObject} !== 'undefined'");
        });
}
