using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalInjection;
using Shouldly;
using System.Reflection;

namespace RossWright.MetalInjection.Server.UnitTests;

public class MetalInjectionControllerActivatorTests
{
    // ── Constructor Tests ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithServiceProvider_StoresServiceProvider()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();

        // Act
        var activator = new MetalInjectionControllerActivator(serviceProvider);

        // Assert
        activator.ShouldNotBeNull();
    }

    // ── Create Tests ───────────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidContext_CreatesController()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var scopeServiceProvider = Substitute.For<IServiceProvider>();
        
        scope.ServiceProvider.Returns(scopeServiceProvider);
        scopeFactory.CreateScope().Returns(scope);
        serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
        scopeServiceProvider.GetService(typeof(TestController)).Returns(new TestController());

        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(TestController));

        // Act
        var result = activator.Create(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestController>();
    }

    [Fact]
    public void Create_WithValidContext_CreatesScopeAndStoresInHttpContext()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var scopeServiceProvider = Substitute.For<IServiceProvider>();
        
        scope.ServiceProvider.Returns(scopeServiceProvider);
        scopeFactory.CreateScope().Returns(scope);
        serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
        scopeServiceProvider.GetService(typeof(TestController)).Returns(new TestController());

        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(TestController));

        // Act
        activator.Create(context);

        // Assert
        scopeFactory.Received(1).CreateScope();
        context.HttpContext.Items[typeof(IServiceScope)].ShouldBe(scope);
    }

    [Fact]
    public void Create_WithControllerWithDependencies_ResolvesFromScopedServiceProvider()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var scopeServiceProvider = Substitute.For<IServiceProvider>();
        var dependency = new TestDependency();
        
        scope.ServiceProvider.Returns(scopeServiceProvider);
        scopeFactory.CreateScope().Returns(scope);
        serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
        scopeServiceProvider.GetService(typeof(ControllerWithDependencies)).Returns((ControllerWithDependencies?)null);
        scopeServiceProvider.GetService(typeof(TestDependency)).Returns(dependency);

        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(ControllerWithDependencies));

        // Act
        var result = activator.Create(context);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ControllerWithDependencies>();
        var controller = (ControllerWithDependencies)result;
        controller.Dependency.ShouldBe(dependency);
    }

    // ── Release Tests ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Release_WithScopeInContext_DisposesScope()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var scope = Substitute.For<IServiceScope>();
        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(TestController));
        var controller = new TestController();
        
        context.HttpContext.Items[typeof(IServiceScope)] = scope;

        // Act
        activator.Release(context, controller);

        // Assert
        scope.Received(1).Dispose();
    }

    [Fact]
    public void Release_WithScopeInContext_RemovesScopeFromItems()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var scope = Substitute.For<IServiceScope>();
        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(TestController));
        var controller = new TestController();
        
        context.HttpContext.Items[typeof(IServiceScope)] = scope;

        // Act
        activator.Release(context, controller);

        // Assert
        context.HttpContext.Items.ContainsKey(typeof(IServiceScope)).ShouldBeFalse();
    }

    [Fact]
    public void Release_WithoutScopeInContext_DoesNotThrow()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(TestController));
        var controller = new TestController();

        // Act & Assert
        Should.NotThrow(() => activator.Release(context, controller));
    }

    [Fact]
    public void Release_WithDisposableController_DisposesController()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(DisposableController));
        var controller = new DisposableController();

        // Act
        activator.Release(context, controller);

        // Assert
        controller.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public void Release_WithNonDisposableController_DoesNotThrow()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(TestController));
        var controller = new TestController();

        // Act & Assert
        Should.NotThrow(() => activator.Release(context, controller));
    }

    [Fact]
    public void Release_WithScopeAndDisposableController_DisposesBoth()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var scope = Substitute.For<IServiceScope>();
        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(DisposableController));
        var controller = new DisposableController();
        
        context.HttpContext.Items[typeof(IServiceScope)] = scope;

        // Act
        activator.Release(context, controller);

        // Assert
        scope.Received(1).Dispose();
        controller.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public void Release_WithNonScopeObjectInItems_DoesNotAttemptDispose()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var activator = new MetalInjectionControllerActivator(serviceProvider);
        var context = CreateControllerContext(typeof(TestController));
        var controller = new TestController();
        
        context.HttpContext.Items[typeof(IServiceScope)] = new object();

        // Act & Assert
        Should.NotThrow(() => activator.Release(context, controller));
        context.HttpContext.Items.ContainsKey(typeof(IServiceScope)).ShouldBeTrue();
    }

    // ── Helper Methods ─────────────────────────────────────────────────────────────────────

    private static ControllerContext CreateControllerContext(Type controllerType)
    {
        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerTypeInfo = controllerType.GetTypeInfo()
        };
        
        return new ControllerContext
        {
            HttpContext = httpContext,
            ActionDescriptor = actionDescriptor
        };
    }

    // ── Test Classes ───────────────────────────────────────────────────────────────────────

    private class TestController
    {
    }

    private class TestDependency
    {
    }

    private class ControllerWithDependencies
    {
        public TestDependency Dependency { get; }

        public ControllerWithDependencies(TestDependency dependency)
        {
            Dependency = dependency;
        }
    }

    private class DisposableController : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
