using Microsoft.JSInterop;

namespace RossWright;

/// <summary>
/// Typed interface over <c>window.localStorage</c> via JS interop, providing strongly-typed get, set, remove, and clear operations.
/// Register via <see cref="MetalCoreBlazorExtensions.AddBrowserLocalStorage"/>.
/// </summary>
public interface IBrowserLocalStorage
{
    /// <summary>Stores a string value in local storage under <paramref name="key"/>.</summary>
    /// <param name="key">The storage key.</param>
    /// <param name="value">The string value to store.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when the value has been stored.</returns>
    ValueTask Set(string key, string value);
    /// <summary>Retrieves the string value stored under <paramref name="key"/>, or <see langword="null"/> if absent.</summary>
    /// <param name="key">The storage key.</param>
    /// <returns>The stored value, or <see langword="null"/> if the key is not found.</returns>
    ValueTask<string?> Get(string key);
    /// <summary>Removes the entry for <paramref name="key"/> from local storage.</summary>
    /// <param name="key">The storage key to remove.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when the entry has been removed.</returns>
    ValueTask Remove(string key);
    /// <summary>Clears all entries from local storage.</summary>
    /// <returns>A <see cref="ValueTask"/> that completes when local storage has been cleared.</returns>
    ValueTask Clear();
}

internal class BrowserLocalStorage(
    IJSRuntime _js)
    : IBrowserLocalStorage
{
    public ValueTask Set(string key, string value) =>
        _js.InvokeVoidAsync("localStorage.setItem", key, value);

    public ValueTask<string?> Get(string key) =>
        _js.InvokeAsync<string?>("localStorage.getItem", key);

    public ValueTask Remove(string key) =>
        _js.InvokeVoidAsync("localStorage.removeItem", key);

    public ValueTask Clear() =>
        _js.InvokeVoidAsync("localStorage.clear");
}
