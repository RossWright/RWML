using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RossWright;
using RossWright.MetalCore.Blazor.Testbed;

await WebAssemblyHostBuilder
    .CreateDefault(args)
    .AddRootComponents(_ =>
    {
        _.Add<App>("#app");
        _.Add<HeadOutlet>("head::after");
    })
    .AddServices(_ => _
        .AddJsScriptLoader()
        .AddBrowserLocalStorage()
        .AddMudServices())
    .Build()
    .RunAsync();
