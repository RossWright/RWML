using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RossWright.MetalInjection.Tests;

public class PropertyInjectionUnitTests
{
    [Fact]
    public void HappyPath()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<PropertyInjectionTestService>();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<PropertyInjectionTestObject>(serviceProvider);
        obj.Service.ShouldNotBeNull();
    }

    // ── Alternate Inject Attribute ───────────────────────────────────────────────────────────

    [Fact]
    public void SetAlternateInjectAttribute_ResolvesProperties()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<PropertyInjectionTestService>();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        optionsBuilder.SetAlternateInjectAttribute(typeof(MyInjectAttribute));
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<Phase8WithAlternateAttribute>(serviceProvider);

        obj.AltProp.ShouldNotBeNull();
        obj.InjectProp.ShouldNotBeNull();
    }

    [Fact]
    public void SetAlternateInjectAttribute_WithKeyFunc_ResolvesKeyedProperties()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("k");

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        optionsBuilder.SetAlternateInjectAttribute<MyKeyedInjectAttribute>(_ => _.Key);
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<Phase8WithKeyedAttribute>(serviceProvider);

        obj.KeyedProp.ShouldBeOfType<EmptyTestService>();
    }

    [Fact]
    public void AspNetCoreInjectAttribute_ResolvesProperties()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<PropertyInjectionTestService>();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        optionsBuilder.SetAlternateInjectAttribute(typeof(Microsoft.AspNetCore.Components.InjectAttribute));
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<AspNetCoreInjectTestObject>(serviceProvider);

        obj.Service.ShouldNotBeNull();
    }

    // ── Optional Property Injection ──────────────────────────────────────────────────────────

    [Fact]
    public void OptionalPropertyInjection_NullWhenNotRegistered()
    {
        var serviceCollection = new ServiceCollection();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<Phase8WithOptionalProp>(serviceProvider);

        obj.Prop.ShouldBeNull();
    }

    [Fact]
    public void OptionalPropertyInjection_InjectedWhenRegistered()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<Phase8WithOptionalProp>(serviceProvider);

        obj.Prop.ShouldNotBeNull();
    }

    // ── IEnumerable Property Injection ───────────────────────────────────────────────────────

    [Fact]
    public void PropertyInjection_IEnumerable_InjectsAllRegistered()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, AnotherEmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, YetAnotherEmptyTestService>();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<Phase8WithEnumerableProp>(serviceProvider);

        obj.Services.ShouldNotBeNull();
        obj.Services!.Count().ShouldBe(3);
    }

    [Fact]
    public void PropertyInjection_IEnumerable_EmptyCollectionWhenNoneRegistered()
    {
        var serviceCollection = new ServiceCollection();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<Phase8WithEnumerableProp>(serviceProvider);

        obj.Services.ShouldNotBeNull();
        obj.Services!.ShouldBeEmpty();
    }

    // ── IMetalInjectionServiceProvider.InjectProperties ─────────────────────────────────────

    [Fact]
    public void InjectProperties_ResolvesInjectableProperties()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<PropertyInjectionTestService>();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = new Phase8ForInjectProperties();
        serviceProvider.InjectProperties(obj);

        obj.Prop.ShouldNotBeNull();
    }

    // ── [Inject] on a read-only property throws InvalidOperationException ────────────────────────

    [Fact]
    public void PropertyInjection_ReadOnlyProperty_ThrowsInvalidOperationException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<PropertyInjectionTestService>();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = new Phase8WithReadOnlyInject();
        Should.Throw<InvalidOperationException>(() => serviceProvider.InjectProperties(obj));
    }

    // ── [Inject] on an inherited property is resolved (FlattenHierarchy) ───────────────────────

    [Fact]
    public void PropertyInjection_InheritedProperty_IsResolved()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<PropertyInjectionTestService>();

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<Phase8Derived>(serviceProvider);

        obj.InheritedProp.ShouldNotBeNull();
    }

    // ── [Inject("key")] on a property resolves a keyed service ─────────────────────────────────

    [Fact]
    public void PropertyInjection_KeyedInjectAttribute_ResolvesKeyedService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("my-key");

        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        var obj = ActivatorUtilities.CreateInstance<Phase8WithKeyedInject>(serviceProvider);

        obj.KeyedService.ShouldNotBeNull();
        obj.KeyedService.ShouldBeOfType<EmptyTestService>();
    }

    // ── InjectProperties(null) is a no-op ───────────────────────────────────────────────────────

    [Fact]
    public void InjectProperties_NullObject_DoesNotThrow()
    {
        var serviceCollection = new ServiceCollection();
        var optionsBuilder = new MetalInjectionOptionsBuilder();
        var serviceProvider = new MetalInjectionServiceProvider(serviceCollection, optionsBuilder);

        serviceProvider.InjectProperties(null);
    }

    // ── SetAlternateInjectAttribute with a non-Attribute type throws ArgumentException ──────────

    [Fact]
    public void SetAlternateInjectAttribute_NonAttributeType_ThrowsArgumentException()
    {
        var optionsBuilder = new MetalInjectionOptionsBuilder();
        Should.Throw<ArgumentException>(() => optionsBuilder.SetAlternateInjectAttribute(typeof(string)));
    }
}

public class PropertyInjectionTestService
{
}

public class PropertyInjectionTestObject
{
    [Inject] public PropertyInjectionTestService Service { get; set; } = null!;
}

public class MyInjectAttribute : Attribute { }

public class MyKeyedInjectAttribute : Attribute
{
    public string Key { get; set; } = "";
}

public class Phase8WithAlternateAttribute
{
    [MyInject] public PropertyInjectionTestService? AltProp { get; set; }
    [Inject] public PropertyInjectionTestService? InjectProp { get; set; }
}

public class Phase8WithKeyedAttribute
{
    [MyKeyedInject(Key = "k")] public IEmptyTestService? KeyedProp { get; set; }
}

public class Phase8WithOptionalProp
{
    [Inject] public IEmptyTestService? Prop { get; set; }
}

public class Phase8WithEnumerableProp
{
    [Inject] public IEnumerable<IEmptyTestService>? Services { get; set; }
}

public class Phase8ForInjectProperties
{
    [Inject] public PropertyInjectionTestService? Prop { get; set; }
}

public class AspNetCoreInjectTestObject
{
    [Microsoft.AspNetCore.Components.Inject] public PropertyInjectionTestService? Service { get; set; }
}

public class Phase8WithReadOnlyInject
{
    [Inject] public PropertyInjectionTestService ReadOnly { get; } = null!;
}

public class Phase8Base
{
    [Inject] public PropertyInjectionTestService? InheritedProp { get; set; }
}

public class Phase8Derived : Phase8Base { }

public class Phase8WithKeyedInject
{
    [Inject("my-key")] public IEmptyTestService? KeyedService { get; set; }
}