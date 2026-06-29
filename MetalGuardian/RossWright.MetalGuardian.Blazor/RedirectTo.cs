using Microsoft.AspNetCore.Components;

namespace RossWright.MetalGuardian;

/// <summary>
/// A convenience Razor component that immediately issues a client-side navigation
/// redirect to the specified <see cref="Url"/> when rendered.
/// </summary>
public class RedirectTo : ComponentBase
{
    /// <summary>The URL to navigate to.</summary>
    [Parameter, EditorRequired] public string Url { get; set; } = null!;

    /// <summary>The Blazor navigation service used to perform the redirect.</summary>
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo(Url, true);
    }
}
