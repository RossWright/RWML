using Microsoft.Extensions.DependencyInjection;

namespace RossWright.Blazor.Tests;

public class BrowserLocalStorageTests
{
    private static IBrowserLocalStorage CreateStorage(IJSRuntime js)
    {
        var services = new ServiceCollection();
        services.AddSingleton(js);
        services.AddBrowserLocalStorage();
        return services.BuildServiceProvider().GetRequiredService<IBrowserLocalStorage>();
    }

    private static IJSRuntime CreateJs() => Substitute.For<IJSRuntime>();

    [Fact]
    public async Task Set_InvokesLocalStorageSetItem()
    {
        var js = CreateJs();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.setItem", Arg.Any<object[]>())
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var storage = CreateStorage(js);

        await storage.Set("key1", "value1");

        await js.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.setItem", Arg.Is<object[]>(a => (string)a[0] == "key1" && (string)a[1] == "value1"));
    }

    [Fact]
    public async Task Get_InvokesLocalStorageGetItem()
    {
        var js = CreateJs();
        js.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>("value1"));

        var storage = CreateStorage(js);

        var result = await storage.Get("key1");

        result.ShouldBe("value1");
        await js.Received(1).InvokeAsync<string?>(
            "localStorage.getItem", Arg.Is<object[]>(a => (string)a[0] == "key1"));
    }

    [Fact]
    public async Task Get_ForAbsentKey_ReturnsNull()
    {
        var js = CreateJs();
        js.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(default(string)));

        var storage = CreateStorage(js);

        var result = await storage.Get("missing");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Remove_InvokesLocalStorageRemoveItem()
    {
        var js = CreateJs();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.removeItem", Arg.Any<object[]>())
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var storage = CreateStorage(js);

        await storage.Remove("key1");

        await js.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.removeItem", Arg.Is<object[]>(a => (string)a[0] == "key1"));
    }

    [Fact]
    public async Task Clear_InvokesLocalStorageClear()
    {
        var js = CreateJs();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.clear", Arg.Any<object[]>())
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var storage = CreateStorage(js);

        await storage.Clear();

        await js.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.clear", Arg.Any<object[]>());
    }
}
