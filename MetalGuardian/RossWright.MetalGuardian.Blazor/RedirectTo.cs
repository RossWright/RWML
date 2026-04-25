using Microsoft.AspNetCore.Components;

namespace RossWright.MetalGuardian;

public class RedirectTo : ComponentBase
{
    [Parameter, EditorRequired] public string Url { get; set; } = null!;

    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo(Url, true);
    }
}
