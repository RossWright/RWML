using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCore.Blazor.Tests;

public class WebAssemblyHostBuilderExtensionsTests
{
    private static WebAssemblyHostBuilder CreateUninitializedBuilder()
    {
        var builder = (WebAssemblyHostBuilder)RuntimeHelpers.GetUninitializedObject(typeof(WebAssemblyHostBuilder));
        
        // Use reflection to set the Services property
        var servicesField = typeof(WebAssemblyHostBuilder).GetField("_services", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (servicesField != null)
        {
            servicesField.SetValue(builder, new ServiceCollection());
        }
        
        // Use reflection to set the RootComponents property
        var rootComponentsField = typeof(WebAssemblyHostBuilder).GetField("_rootComponents",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (rootComponentsField != null)
        {
            var rootComponents = (RootComponentMappingCollection)RuntimeHelpers.GetUninitializedObject(typeof(RootComponentMappingCollection));
            rootComponentsField.SetValue(builder, rootComponents);
        }
        
        return builder;
    }

    private static WebAssemblyHost CreateUninitializedHost()
    {
        var host = (WebAssemblyHost)RuntimeHelpers.GetUninitializedObject(typeof(WebAssemblyHost));
        return host;
    }

    [Fact]
    public void AddRootComponents_InvokesActionWithRootComponents_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var actionInvoked = false;
        RootComponentMappingCollection? passedComponents = null;

        Action<RootComponentMappingCollection> action = components =>
        {
            actionInvoked = true;
            passedComponents = components;
        };

        // Act
        var result = builder.AddRootComponents(action);

        // Assert
        actionInvoked.ShouldBeTrue();
        passedComponents.ShouldBe(builder.RootComponents);
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddServices_InvokesActionWithServices_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var actionInvoked = false;
        IServiceCollection? passedServices = null;

        Action<IServiceCollection> action = services =>
        {
            actionInvoked = true;
            passedServices = services;
        };

        // Act
        var result = builder.AddServices(action);

        // Assert
        actionInvoked.ShouldBeTrue();
        passedServices.ShouldBe(builder.Services);
        result.ShouldBe(builder);
    }

    [Fact]
    public void UseApp_InvokesActionWithApp_ReturnsApp()
    {
        // Arrange
        var app = CreateUninitializedHost();
        var actionInvoked = false;
        WebAssemblyHost? passedApp = null;

        Action<WebAssemblyHost> action = host =>
        {
            actionInvoked = true;
            passedApp = host;
        };

        // Act
        var result = app.UseApp(action);

        // Assert
        actionInvoked.ShouldBeTrue();
        passedApp.ShouldBe(app);
        result.ShouldBe(app);
    }

    [Fact]
    public async Task RunAsync_InvokesButFirstBeforeRunAsync()
    {
        // Arrange
        var app = CreateUninitializedHost();
        var callOrder = new List<string>();
        var butFirstInvoked = false;
        WebAssemblyHost? passedApp = null;

        Func<WebAssemblyHost, Task> butFirst = async host =>
        {
            await Task.Yield();
            callOrder.Add("butFirst");
            butFirstInvoked = true;
            passedApp = host;
        };

        // Act & Assert
        // The extension method will call butFirst, then try to call app.RunAsync()
        // Since app is uninitialized, RunAsync will throw, but we can verify butFirst was called
        try
        {
            await app.RunAsync(butFirst);
        }
        catch
        {
            // Expected - app.RunAsync() will fail on uninitialized object
            // But butFirst should have been called first
        }

        // Assert
        butFirstInvoked.ShouldBeTrue();
        passedApp.ShouldBe(app);
        callOrder[0].ShouldBe("butFirst");
    }

    [Fact]
    public void AddRootComponents_WithMultipleInvocations_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var invocationCount = 0;

        Action<RootComponentMappingCollection> action = components =>
        {
            invocationCount++;
        };

        // Act
        var result = builder.AddRootComponents(action).AddRootComponents(action);

        // Assert
        invocationCount.ShouldBe(2);
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddServices_WithMultipleInvocations_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var invocationCount = 0;

        Action<IServiceCollection> action = services =>
        {
            invocationCount++;
        };

        // Act
        var result = builder.AddServices(action).AddServices(action);

        // Assert
        invocationCount.ShouldBe(2);
        result.ShouldBe(builder);
    }

    [Fact]
    public void UseApp_WithMultipleInvocations_ReturnsApp()
    {
        // Arrange
        var app = CreateUninitializedHost();
        var invocationCount = 0;

        Action<WebAssemblyHost> action = host =>
        {
            invocationCount++;
        };

        // Act
        var result = app.UseApp(action).UseApp(action);

        // Assert
        invocationCount.ShouldBe(2);
        result.ShouldBe(app);
    }

    [Fact]
    public void UseApp_ActionReceivesCorrectHost_HostIsCorrect()
    {
        // Arrange
        var app = CreateUninitializedHost();
        var receivedCorrectHost = false;

        Action<WebAssemblyHost> action = host =>
        {
            receivedCorrectHost = ReferenceEquals(host, app);
        };

        // Act
        app.UseApp(action);

        // Assert
        receivedCorrectHost.ShouldBeTrue();
    }

    [Fact]
    public async Task RunAsync_ButFirstIsAsync_WaitsForCompletion()
    {
        // Arrange
        var app = CreateUninitializedHost();
        var completed = false;

        Func<WebAssemblyHost, Task> butFirst = async host =>
        {
            await Task.Delay(10);
            completed = true;
        };

        // Act
        try
        {
            await app.RunAsync(butFirst);
        }
        catch
        {
            // Expected - app.RunAsync() will fail on uninitialized object
        }

        // Assert
        completed.ShouldBeTrue();
    }

    [Fact]
    public void AddRootComponents_ChainedWithAddServices_BothExecute()
    {
        // Arrange
        var builder = CreateUninitializedBuilder();
        var rootComponentsActionInvoked = false;
        var servicesActionInvoked = false;

        Action<RootComponentMappingCollection> rootComponentsAction = components =>
        {
            rootComponentsActionInvoked = true;
        };

        Action<IServiceCollection> servicesAction = services =>
        {
            servicesActionInvoked = true;
        };

        // Act
        builder.AddRootComponents(rootComponentsAction)
               .AddServices(servicesAction);

        // Assert
        rootComponentsActionInvoked.ShouldBeTrue();
        servicesActionInvoked.ShouldBeTrue();
    }
}
