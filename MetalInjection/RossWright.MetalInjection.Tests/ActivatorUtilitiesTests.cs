using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RossWright.MetalInjection.Tests;

#pragma warning disable CS9113

public class ActivatorUtilitiesTests
{
    public ActivatorUtilitiesTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ITestService, TestService>();
        serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();
    }
    private readonly IServiceProvider serviceProvider;

    [Fact] public void HappyPath()
    {
        ActivatorUtilities.CreateInstance(serviceProvider, typeof(Test), 1);
    }

    [Fact] public void MissingParameter()
    {
        Should.Throw<InvalidOperationException>(() => ActivatorUtilities.CreateInstance(serviceProvider, typeof(Test)));
    }

    [Fact] public void MismaatchedParameter()
    {
        Should.Throw<InvalidOperationException>(() => ActivatorUtilities.CreateInstance(serviceProvider, typeof(Test), "bad"));
    }

    [Fact] public void OptionalParameterConstructor()
    {
        ActivatorUtilities.CreateInstance(serviceProvider, typeof(TestWithOptionalParameter), 1);
    }

    // -- Property Injection -------------------------------------------------------------------

    [Fact]
    public void CreateInstance_ResolvesPropertyInjection()
    {
        var obj = ActivatorUtilities.CreateInstance<Phase9Object>(serviceProvider);

        obj.PropService.ShouldNotBeNull();
    }

    // -- IServiceProvider.CreateInstance Extension --------------------------------------------

    [Fact]
    public void ServiceProviderCreateInstance_InjectsServicesAndExtraParams()
    {
        var obj = serviceProvider.CreateInstance<Phase9ObjectWithExtras>(42);

        obj.CtorService.ShouldNotBeNull();
        obj.Count.ShouldBe(42);
    }

    [Fact]
    public void ServiceProviderCreateInstance_ResolvesPropertyInjection()
    {
        var obj = serviceProvider.CreateInstance<Phase9Object>();

        obj.PropService.ShouldNotBeNull();
    }

    // -- G-01: GetServiceOrCreateInstance ----------------------------------------------------

    [Fact]
    public void GetServiceOrCreateInstance_ServiceRegistered_ReturnsResolvedInstance()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ITestService, TestService>();
        sc.AddSingleton<Phase9Object>();
        var provider = sc.BuildMetalInjectionServiceProvider();

        var registered = provider.GetRequiredService<Phase9Object>();
        var result = ActivatorUtilities.GetServiceOrCreateInstance<Phase9Object>(provider);

        result.ShouldBeSameAs(registered);
    }

    [Fact]
    public void GetServiceOrCreateInstance_ServiceNotRegistered_CreatesInstance()
    {
        var result = ActivatorUtilities.GetServiceOrCreateInstance<Phase9Object>(serviceProvider);

        result.ShouldNotBeNull();
        result.CtorService.ShouldNotBeNull();
    }

    [Fact]
    public void GetServiceOrCreateInstance_ResolvesPropertyInjection()
    {
        var result = ActivatorUtilities.GetServiceOrCreateInstance<Phase9Object>(serviceProvider);

        result.PropService.ShouldNotBeNull();
    }

    // -- G-02: CreateFactory ------------------------------------------------------------------

    [Fact]
    public void CreateFactory_NonGeneric_ProducesWorkingFactory()
    {
        var factory = ActivatorUtilities.CreateFactory(typeof(Phase9Object), Type.EmptyTypes);
        var result = (Phase9Object)factory(serviceProvider, null);

        result.CtorService.ShouldNotBeNull();
    }

    [Fact]
    public void CreateFactory_Generic_ProducesWorkingFactory()
    {
        var factory = ActivatorUtilities.CreateFactory<Phase9Object>(Type.EmptyTypes);
        var result = factory(serviceProvider, null);

        result.CtorService.ShouldNotBeNull();
    }

    [Fact]
    public void CreateFactory_Generic_ResolvesPropertyInjection()
    {
        var factory = ActivatorUtilities.CreateFactory<Phase9Object>(Type.EmptyTypes);
        var result = factory(serviceProvider, null);

        result.PropService.ShouldNotBeNull();
    }

    // -- G-03: ServiceProviderExtensions.CreateInstance non-generic overload -----------------

    [Fact]
    public void ServiceProviderCreateInstance_NonGenericOverload_InjectsServices()
    {
        var obj = (Phase9Object)serviceProvider.CreateInstance(typeof(Phase9Object));

        obj.CtorService.ShouldNotBeNull();
        obj.PropService.ShouldNotBeNull();
    }

    // -- G-04: InjectProperties is a no-op on non-MetalInjection providers -------------------

    [Fact]
    public void InjectProperties_NonMetalProvider_ReturnsObjectUnchanged()
    {
        var builtIn = new ServiceCollection()
            .AddSingleton<ITestService, TestService>()
            .BuildServiceProvider();
        var obj = new Phase9Object(builtIn.GetRequiredService<ITestService>());

        var result = builtIn.InjectProperties(obj);

        result.ShouldBeSameAs(obj);
        result.PropService.ShouldBeNull();
    }

    public interface ITestService { }
    public class TestService : ITestService { }
    public class Test(ITestService svc, int count) { }
    public interface IMissingTestService { }
    public class TestWithOptionalParameter(ITestService svc, int count, IMissingTestService? missingSvc = null) { }

    public class Phase9Object(ITestService svc)
    {
        public ITestService CtorService { get; } = svc;
        [Inject] public ITestService PropService { get; set; } = null!;
    }

    public class Phase9ObjectWithExtras(ITestService svc, int count)
    {
        public ITestService CtorService { get; } = svc;
        public int Count { get; } = count;
        [Inject] public ITestService PropService { get; set; } = null!;
    }
}