using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalInjection.Tests.Internal;

public class MetalInjectionServiceProviderTests
{
    [Fact]
    public async Task DisposeAsync_WithDisposableTransientInstances_DisposesAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<DisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance1 = provider.GetRequiredService<DisposableService>();
        var instance2 = provider.GetRequiredService<DisposableService>();

        // Act
        await ((IAsyncDisposable)provider).DisposeAsync();

        // Assert
        instance1.DisposeCalled.ShouldBeTrue();
        instance2.DisposeCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task DisposeAsync_WithScopedDisposableInstances_DisposesAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<DisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        DisposableService instance;
        await using (var scope = (IAsyncDisposable)provider.CreateScope())
        {
            instance = ((IServiceScope)scope).ServiceProvider.GetRequiredService<DisposableService>();
        }

        // Assert
        instance.DisposeCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task DisposeAsync_WithSingletonDisposableInstances_DisposesAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<DisposableService>();

        // Act
        await ((IAsyncDisposable)provider).DisposeAsync();

        // Assert
        instance.DisposeCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task DisposeAsync_WithSingletonAsyncDisposableInstances_DisposesAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<AsyncDisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<AsyncDisposableService>();

        // Act
        await ((IAsyncDisposable)provider).DisposeAsync();

        // Assert
        instance.DisposeAsyncCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task DisposeAsync_WithMultipleSingletonTypes_DisposesAllCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DisposableService>();
        services.AddSingleton<AsyncDisposableService>();
        services.AddSingleton<DualDisposableService>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var disposable = provider.GetRequiredService<DisposableService>();
        var asyncDisposable = provider.GetRequiredService<AsyncDisposableService>();
        var dualDisposable = provider.GetRequiredService<DualDisposableService>();

        // Act
        await ((IAsyncDisposable)provider).DisposeAsync();

        // Assert
        disposable.DisposeCalled.ShouldBeTrue();
        asyncDisposable.DisposeAsyncCalled.ShouldBeTrue();
        dualDisposable.DisposeAsyncCalled.ShouldBeTrue();
        dualDisposable.DisposeCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task DisposeAsync_OnScopedProvider_DoesNotDisposeSingletons()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DisposableService>();
        services.AddScoped<DisposableService2>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var singleton = provider.GetRequiredService<DisposableService>();
        DisposableService2 scoped;

        await using (var scope = (IAsyncDisposable)provider.CreateScope())
        {
            scoped = ((IServiceScope)scope).ServiceProvider.GetRequiredService<DisposableService2>();
        }

        // Assert
        scoped.DisposeCalled.ShouldBeTrue();
        singleton.DisposeCalled.ShouldBeFalse();
    }

    private class DisposableService : IDisposable
    {
        public bool DisposeCalled { get; private set; }
        public void Dispose() => DisposeCalled = true;
    }

    private class DisposableService2 : IDisposable
    {
        public bool DisposeCalled { get; private set; }
        public void Dispose() => DisposeCalled = true;
    }

    private class AsyncDisposableService : IAsyncDisposable
    {
        public bool DisposeAsyncCalled { get; private set; }
        public ValueTask DisposeAsync()
        {
            DisposeAsyncCalled = true;
            return ValueTask.CompletedTask;
        }
    }

    private class DualDisposableService : IDisposable, IAsyncDisposable
    {
        public bool DisposeCalled { get; private set; }
        public bool DisposeAsyncCalled { get; private set; }
        public void Dispose() => DisposeCalled = true;
        public ValueTask DisposeAsync()
        {
            DisposeAsyncCalled = true;
            return ValueTask.CompletedTask;
        }
    }
}
