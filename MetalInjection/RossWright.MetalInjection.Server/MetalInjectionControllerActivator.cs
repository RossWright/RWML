using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalInjection;

internal class MetalInjectionControllerActivator : IControllerActivator
{
    private readonly IServiceProvider _serviceProvider;

    public MetalInjectionControllerActivator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object Create(ControllerContext context)
    {
        var controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();
        var scope = _serviceProvider.CreateScope();
        var controller = ActivatorUtilities.CreateInstance(scope.ServiceProvider, controllerType);
        context.HttpContext.Items[typeof(IServiceScope)] = scope;
        return controller;
    }

    public void Release(ControllerContext context, object controller)
    {
        if (context.HttpContext.Items.TryGetValue(typeof(IServiceScope), out var scopeObj) && 
            scopeObj is IServiceScope scope)
        {
            scope.Dispose();
            context.HttpContext.Items.Remove(typeof(IServiceScope));
        }

        (controller as IDisposable)?.Dispose();
    }
}