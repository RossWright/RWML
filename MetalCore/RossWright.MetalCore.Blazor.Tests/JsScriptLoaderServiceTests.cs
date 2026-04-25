using Microsoft.Extensions.DependencyInjection;

namespace RossWright.Blazor.Tests;

public class JsScriptLoaderServiceTests
{
    private static IJSRuntime CreateJsRuntime() => Substitute.For<IJSRuntime>();

    private static IBrowserLocalStorage CreateStorageWith(IJSRuntime js)
    {
        var services = new ServiceCollection();
        services.AddSingleton(js);
        services.AddBrowserLocalStorage();
        return services.BuildServiceProvider().GetRequiredService<IBrowserLocalStorage>();
    }

    private static IJsScriptLoaderService CreateLoaderWith(IJSRuntime js)
    {
        var services = new ServiceCollection();
        services.AddSingleton(js);
        services.AddJsScriptLoader();
        return services.BuildServiceProvider().GetRequiredService<IJsScriptLoaderService>();
    }

    [Fact]
    public async Task EnsureLoaded_FirstCall_InvokesJsRuntime()
    {
        var js = CreateJsRuntime();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(callInfo => new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var service = CreateLoaderWith(js);

        await service.EnsureLoaded("/app.js", "MyLib");

        await js.Received().InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "eval", Arg.Any<object[]>());
    }

    [Fact]
    public async Task EnsureLoaded_WithNullFileHash_DoesNotIncludeIntegrityInArgs()
    {
        var capturedArgs = new List<object[]>();
        var js = CreateJsRuntime();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            Arg.Any<string>(), Arg.Do<object[]>(a => capturedArgs.Add(a)))
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var service = CreateLoaderWith(js);
        await service.EnsureLoaded("/app.js", "MyLib", null);

        // The loadScriptIfNotExists call should have null as the integrity argument
        var loadCall = capturedArgs.FirstOrDefault(a =>
            a.Length >= 2 && a[0] is string path && path == "/app.js");
        loadCall.ShouldNotBeNull();
        loadCall![1].ShouldBeNull(); // fileHash argument is null
    }

    [Fact]
    public async Task EnsureLoaded_WithFileHash_PassesHashAsSecondArgument()
    {
        var capturedArgs = new List<object[]>();
        var js = CreateJsRuntime();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            Arg.Any<string>(), Arg.Do<object[]>(a => capturedArgs.Add(a)))
            .Returns(new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>()));

        var service = CreateLoaderWith(js);
        const string hash = "sha384-abc123";
        await service.EnsureLoaded("/app.js", "MyLib", hash);

        var loadCall = capturedArgs.FirstOrDefault(a =>
            a.Length >= 2 && a[0] is string path && path == "/app.js");
        loadCall.ShouldNotBeNull();
        loadCall![1].ShouldBe(hash);
    }

    [Fact]
    public async Task EnsureLoaded_DifferentPaths_InvokesJsForEachPath()
    {
        var invokeCount = 0;
        var js = CreateJsRuntime();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            Arg.Any<string>(), Arg.Any<object[]>())
            .Returns(callInfo =>
            {
                invokeCount++;
                return new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                    Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>());
            });

        var service = CreateLoaderWith(js);

        await service.EnsureLoaded("/first.js", "First");
        await service.EnsureLoaded("/second.js", "Second");

        // Each path triggers eval + loadScriptIfNotExists = 2 calls each = 4 total
        invokeCount.ShouldBeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task EnsureLoaded_ConcurrentCallsSamePath_OnlyOneLoadExecutes()
    {
        var evalCount = 0;
        var tcs = new TaskCompletionSource();

        var js = Substitute.For<IJSRuntime>();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "eval", Arg.Any<object[]>())
            .Returns(callInfo =>
            {
                Interlocked.Increment(ref evalCount);
                return new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                    Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>());
            });
        var voidResult = Substitute.For<Microsoft.JSInterop.Infrastructure.IJSVoidResult>();
        js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
            "loadScriptIfNotExists", Arg.Any<object[]>())
            .Returns(callInfo => new ValueTask<Microsoft.JSInterop.Infrastructure.IJSVoidResult>(
                tcs.Task.ContinueWith(_ => voidResult)));

        var services = new ServiceCollection();
        services.AddSingleton(js);
        services.AddJsScriptLoader();
        var service = services.BuildServiceProvider().GetRequiredService<IJsScriptLoaderService>();

        // Fire two concurrent calls for the same path before the first completes
        var t1 = service.EnsureLoaded("/concurrent.js", "ConcLib");
        var t2 = service.EnsureLoaded("/concurrent.js", "ConcLib");

        tcs.SetResult();
        await Task.WhenAll(t1, t2);

        // eval should only have been called once (LoadGuard deduplication)
        evalCount.ShouldBe(1);
    }
}
