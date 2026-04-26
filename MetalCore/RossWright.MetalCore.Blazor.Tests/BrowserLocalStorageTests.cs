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

    [Fact]
    public async Task Set_WithEmptyKey_InvokesLocalStorageSetItem()
    {
        var js = CreateJs();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.setItem", Arg.Any<object[]>())
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var storage = CreateStorage(js);

        await storage.Set("", "value1");

        await js.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.setItem", Arg.Is<object[]>(a => (string)a[0] == "" && (string)a[1] == "value1"));
    }

    [Fact]
    public async Task Set_WithEmptyValue_InvokesLocalStorageSetItem()
    {
        var js = CreateJs();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.setItem", Arg.Any<object[]>())
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var storage = CreateStorage(js);

        await storage.Set("key1", "");

        await js.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.setItem", Arg.Is<object[]>(a => (string)a[0] == "key1" && (string)a[1] == ""));
    }

    [Fact]
    public async Task Set_WithSpecialCharacters_InvokesLocalStorageSetItem()
    {
        var js = CreateJs();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.setItem", Arg.Any<object[]>())
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var storage = CreateStorage(js);

        await storage.Set("key:with.special-chars", "value with spaces & symbols!");

        await js.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.setItem", Arg.Is<object[]>(a => (string)a[0] == "key:with.special-chars" && (string)a[1] == "value with spaces & symbols!"));
    }

    [Fact]
    public async Task Get_WithEmptyKey_InvokesLocalStorageGetItem()
    {
        var js = CreateJs();
        js.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>("result"));

        var storage = CreateStorage(js);

        var result = await storage.Get("");

        result.ShouldBe("result");
        await js.Received(1).InvokeAsync<string?>(
            "localStorage.getItem", Arg.Is<object[]>(a => (string)a[0] == ""));
    }

    [Fact]
    public async Task Get_ReturnsEmptyString()
    {
        var js = CreateJs();
        js.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(""));

        var storage = CreateStorage(js);

        var result = await storage.Get("key1");

        result.ShouldBe("");
    }

    [Fact]
    public async Task Get_WithSpecialCharacters_InvokesLocalStorageGetItem()
    {
        var js = CreateJs();
        js.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>("stored value"));

        var storage = CreateStorage(js);

        var result = await storage.Get("key:with.special-chars");

        result.ShouldBe("stored value");
        await js.Received(1).InvokeAsync<string?>(
            "localStorage.getItem", Arg.Is<object[]>(a => (string)a[0] == "key:with.special-chars"));
    }

    [Fact]
    public async Task Remove_WithEmptyKey_InvokesLocalStorageRemoveItem()
    {
        var js = CreateJs();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.removeItem", Arg.Any<object[]>())
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var storage = CreateStorage(js);

        await storage.Remove("");

        await js.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.removeItem", Arg.Is<object[]>(a => (string)a[0] == ""));
    }

    [Fact]
    public async Task Remove_WithSpecialCharacters_InvokesLocalStorageRemoveItem()
    {
        var js = CreateJs();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.removeItem", Arg.Any<object[]>())
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var storage = CreateStorage(js);

        await storage.Remove("key:with.special-chars");

        await js.Received(1).InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "localStorage.removeItem", Arg.Is<object[]>(a => (string)a[0] == "key:with.special-chars"));
    }
}
