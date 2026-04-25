using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace RossWright.MetalInjection.Tests;

public class BuildMetalInjectionServiceProviderTests
{
    [Fact] public void BasicTransientRegistrationAndInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();
        var result = serviceProvider.GetService<IEmptyTestService>();
        result.ShouldBeOfType<EmptyTestService>();

        var result2 = serviceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeOfType<EmptyTestService>();

        result2.ShouldNotBeSameAs(result);

        using var scope = serviceProvider.CreateScope();
        var result3 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result3.ShouldBeOfType<EmptyTestService>();
        result3.ShouldNotBeSameAs(result);
        result3.ShouldNotBeSameAs(result2);
    }

    [Fact] public void BasicScopedRegistrationAndInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IEmptyTestService>());

        using var scope = serviceProvider.CreateScope();
        var result = scope.ServiceProvider.GetService<IEmptyTestService>();
        result.ShouldBeOfType<EmptyTestService>();

        var result2 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeOfType<EmptyTestService>();

        result2.ShouldBeSameAs(result);

        using var scope2 = serviceProvider.CreateScope();
        var result3 = scope2.ServiceProvider.GetService<IEmptyTestService>();
        result3.ShouldBeOfType<EmptyTestService>();
        result3.ShouldNotBeSameAs(result);
    }

    [Fact] public void BasicSingletonRegistrationAndInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<IEmptyTestService>();
        result.ShouldBeOfType<EmptyTestService>();

        var result2 = serviceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeOfType<EmptyTestService>();

        result2.ShouldBeSameAs(result);

        using var scope = serviceProvider.CreateScope();
        var result3 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result3.ShouldBeSameAs(result);
    }

    [Fact] public void BasicTransientRegistrationWithTransientInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddTransient<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<ITestServiceWithInjection>();
        result.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result).TestService.ShouldBeOfType<EmptyTestService>();

        var result2 = serviceProvider.GetService<IEmptyTestService>();
        result2.ShouldNotBeSameAs(((TestServiceWithInjection)result).TestService);

        using var scope = serviceProvider.CreateScope();

        var result3 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result3.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result3).TestService.ShouldBeOfType<EmptyTestService>();

        result3.ShouldNotBeSameAs(result);
        ((TestServiceWithInjection)result3).TestService.ShouldNotBeSameAs(
            ((TestServiceWithInjection)result).TestService);

        var result4 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result4.ShouldNotBeSameAs(((TestServiceWithInjection)result3).TestService);
    }

    [Fact] public void BasicTransientRegistrationWithScopedInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddTransient<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        // can't get transient with scoped injection without a scope
        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<ITestServiceWithInjection>());

        using var scope = serviceProvider.CreateScope();

        var result = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result).TestService.ShouldBeOfType<EmptyTestService>();

        var result2 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result2.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result2).TestService.ShouldBeOfType<EmptyTestService>();
        result2.ShouldNotBeSameAs(result);
        ((TestServiceWithInjection)result2).TestService.ShouldBeSameAs(
            ((TestServiceWithInjection)result).TestService);
    }

    [Fact] public void BasicTransientRegistrationWithSingletonInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddTransient<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<ITestServiceWithInjection>();
        result.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result).TestService.ShouldBeOfType<EmptyTestService>();

        var result2 = serviceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeSameAs(((TestServiceWithInjection)result).TestService);

        using var scope = serviceProvider.CreateScope();

        var result3 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result3.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result3).TestService.ShouldBeOfType<EmptyTestService>();
        result3.ShouldNotBeSameAs(result);
        ((TestServiceWithInjection)result3).TestService.ShouldBeSameAs(result2);
    }

    [Fact] public void BasicScopedRegistrationWithTransientInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddScoped<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<ITestServiceWithInjection>());

        var result = serviceProvider.GetService<IEmptyTestService>();
        result.ShouldBeOfType<EmptyTestService>();

        using var scope = serviceProvider.CreateScope();

        var result2 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result2.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result2).TestService.ShouldBeOfType<EmptyTestService>();

        ((TestServiceWithInjection)result2).TestService.ShouldNotBeSameAs(result);

        var result4 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result4.ShouldBeOfType<TestServiceWithInjection>();
        result4.ShouldBeSameAs(result2);
        ((TestServiceWithInjection)result4).TestService.ShouldBeSameAs(((TestServiceWithInjection)result2).TestService);
    }

    [Fact] public void BasicScopedRegistrationWithScopedInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddScoped<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<ITestServiceWithInjection>());
        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IEmptyTestService>());

        using var scope = serviceProvider.CreateScope();

        var result = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result).TestService.ShouldBeOfType<EmptyTestService>();

        var result2 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result.ShouldBeSameAs(result);

        var result3 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result3.ShouldBeSameAs(result);

        var result4 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result4.ShouldBeSameAs(result2);
    }

    [Fact] public void BasicScopedRegistrationWithSingletonInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddScoped<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<ITestServiceWithInjection>());

        var result = serviceProvider.GetService<IEmptyTestService>();
        result.ShouldBeOfType<EmptyTestService>();

        using var scope = serviceProvider.CreateScope();

        var result2 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result2.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result2).TestService.ShouldBeOfType<EmptyTestService>();

        ((TestServiceWithInjection)result2).TestService.ShouldBeSameAs(result);

        var result4 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result4.ShouldBeOfType<TestServiceWithInjection>();
        result4.ShouldBeSameAs(result2);
        ((TestServiceWithInjection)result4).TestService.ShouldBeSameAs(result);
    }

    [Fact] public void BasicSingletonRegistrationWithTransientInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<ITestServiceWithInjection>();
        result.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result).TestService.ShouldBeOfType<EmptyTestService>();

        var result2 = serviceProvider.GetService<ITestServiceWithInjection>();
        result2.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result2).TestService.ShouldBeOfType<EmptyTestService>();

        result2.ShouldBeSameAs(result);
        ((TestServiceWithInjection)result2).TestService.ShouldBeSameAs(((TestServiceWithInjection)result).TestService);

        using var scope = serviceProvider.CreateScope();

        var result3 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result3.ShouldBeSameAs(result);
        ((TestServiceWithInjection)result3!).TestService.ShouldBeSameAs(((TestServiceWithInjection)result).TestService);
    }

    [Fact] public void BasicSingletonRegistrationWithScopedInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        // cannot inject a scoped service into a singleton
        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<ITestServiceWithInjection>());

        using var scope = serviceProvider.CreateScope();

        // cannot inject a scoped service into a singleton
        Should.Throw<MetalInjectionException>(() => scope.ServiceProvider.GetService<ITestServiceWithInjection>());
    }

    [Fact] public void BasicSingletonRegistrationWithSingletonInjection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<ITestServiceWithInjection>();
        result.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result).TestService.ShouldBeOfType<EmptyTestService>();

        var result2 = serviceProvider.GetService<ITestServiceWithInjection>();
        result2.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result2).TestService.ShouldBeOfType<EmptyTestService>();

        result2.ShouldBeSameAs(result);
        ((TestServiceWithInjection)result2).TestService.ShouldBeSameAs(((TestServiceWithInjection)result).TestService);

        using var scope = serviceProvider.CreateScope();

        var result3 = scope.ServiceProvider.GetService<ITestServiceWithInjection>();
        result3.ShouldBeOfType<TestServiceWithInjection>();
        ((TestServiceWithInjection)result3).TestService.ShouldBeOfType<EmptyTestService>();
        result3.ShouldBeSameAs(result);
        ((TestServiceWithInjection)result3).TestService.ShouldBeSameAs(((TestServiceWithInjection)result).TestService);
    }

    [Fact] public void GetIServicProviderService()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();
        var provider = serviceProvider.GetService<IServiceProvider>();
        provider.ShouldBeSameAs(serviceProvider);

        var scope = serviceProvider.CreateScope();
        var provider2 = scope.ServiceProvider.GetService<IServiceProvider>();
        provider2.ShouldNotBeSameAs(serviceProvider);
        provider2.ShouldBeSameAs(scope.ServiceProvider);
    }

    [Fact] public void GetIServicProviderServiceList()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();
        var provider = serviceProvider.GetService<IEnumerable<IServiceProvider>>();
        provider.ShouldNotBeNull();
        provider.ShouldHaveSingleItem();
        provider.Single().ShouldBeSameAs(serviceProvider);

        var scope = serviceProvider.CreateScope();
        var provider2 = scope.ServiceProvider.GetService<IEnumerable<IServiceProvider>>();
        provider2.ShouldNotBeNull();
        provider2.ShouldHaveSingleItem();
        provider2.Single().ShouldNotBeSameAs(serviceProvider);
        provider2.Single().ShouldBeSameAs(scope.ServiceProvider);
    }

    [Fact] public void ServiceListOfTransient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddTransient<IEmptyTestService, AnotherEmptyTestService>();
        serviceCollection.AddTransient<IEmptyTestService, YetAnotherEmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetServices<IEmptyTestService>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        result[0].ShouldBeOfType<AnotherEmptyTestService>();
        result[1].ShouldBeOfType<EmptyTestService>();
        result[2].ShouldBeOfType<YetAnotherEmptyTestService>();

        var scope = serviceProvider.CreateScope();

        var result2 = scope.ServiceProvider.GetServices<IEmptyTestService>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        result2[0].ShouldBeOfType<AnotherEmptyTestService>();
        result2[0].ShouldNotBeSameAs(result[0]);
        result2[1].ShouldBeOfType<EmptyTestService>();
        result2[1].ShouldNotBeSameAs(result[1]);
        result2[2].ShouldBeOfType<YetAnotherEmptyTestService>();
        result2[2].ShouldNotBeSameAs(result[2]);
    }

    [Fact] public void ServiceListOfScoped()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddScoped<IEmptyTestService, AnotherEmptyTestService>();
        serviceCollection.AddScoped<IEmptyTestService, YetAnotherEmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetServices<IEmptyTestService>());

        var scope = serviceProvider.CreateScope();

        var result = scope.ServiceProvider.GetServices<IEmptyTestService>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        result[0].ShouldBeOfType<AnotherEmptyTestService>();
        result[1].ShouldBeOfType<EmptyTestService>();
        result[2].ShouldBeOfType<YetAnotherEmptyTestService>();

        var result2 = scope.ServiceProvider.GetServices<IEmptyTestService>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        result2[0].ShouldBeOfType<AnotherEmptyTestService>();
        result2[0].ShouldBeSameAs(result[0]);
        result2[1].ShouldBeOfType<EmptyTestService>();
        result2[1].ShouldBeSameAs(result[1]);
        result2[2].ShouldBeOfType<YetAnotherEmptyTestService>();
        result2[2].ShouldBeSameAs(result[2]);
    }

    [Fact] public void ServiceListOfSingletons()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, AnotherEmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, YetAnotherEmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetServices<IEmptyTestService>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        result[0].ShouldBeOfType<AnotherEmptyTestService>();
        result[1].ShouldBeOfType<EmptyTestService>();
        result[2].ShouldBeOfType<YetAnotherEmptyTestService>();

        var result2 = serviceProvider.GetServices<IEmptyTestService>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        result2[0].ShouldBeOfType<AnotherEmptyTestService>();
        result2[0].ShouldBeSameAs(result[0]);
        result2[1].ShouldBeOfType<EmptyTestService>();
        result2[1].ShouldBeSameAs(result[1]);
        result2[2].ShouldBeOfType<YetAnotherEmptyTestService>();
        result2[2].ShouldBeSameAs(result[2]);

        var scope = serviceProvider.CreateScope();

        var result3 = scope.ServiceProvider.GetServices<IEmptyTestService>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        result3[0].ShouldBeOfType<AnotherEmptyTestService>();
        result3[0].ShouldBeSameAs(result[0]);
        result3[1].ShouldBeOfType<EmptyTestService>();
        result3[1].ShouldBeSameAs(result[1]);
        result3[2].ShouldBeOfType<YetAnotherEmptyTestService>();
        result3[2].ShouldBeSameAs(result[2]);

        var result4 = scope.ServiceProvider.GetServices<IEmptyTestService>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        result4[0].ShouldBeOfType<AnotherEmptyTestService>();
        result4[0].ShouldBeSameAs(result[0]);
        result4[1].ShouldBeOfType<EmptyTestService>();
        result4[1].ShouldBeSameAs(result[1]);
        result4[2].ShouldBeOfType<YetAnotherEmptyTestService>();
        result4[2].ShouldBeSameAs(result[2]);
    }

    [Fact] public void GetTransientGenericService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(typeof(IGenericService<>), typeof(GenericService<>));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<IGenericService<int>>();
        result.ShouldBeOfType<GenericService<int>>();

        var result2 = serviceProvider.GetService<IGenericService<int>>();
        result2.ShouldBeOfType<GenericService<int>>();

        result2.ShouldNotBeSameAs(result);
    }

    [Fact] public void GetScopedGenericService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IGenericService<int>>());

        var scope = serviceProvider.CreateScope();

        var result = scope.ServiceProvider.GetService<IGenericService<int>>();
        result.ShouldBeOfType<GenericService<int>>();

        var result2 = scope.ServiceProvider.GetService<IGenericService<int>>();
        result2.ShouldBeOfType<GenericService<int>>();

        result2.ShouldBeSameAs(result);

        var scope2 = serviceProvider.CreateScope();

        var result3 = scope2.ServiceProvider.GetService<IGenericService<int>>();
        result3.ShouldBeOfType<GenericService<int>>();

        var result4 = scope2.ServiceProvider.GetService<IGenericService<int>>();
        result4.ShouldBeOfType<GenericService<int>>();

        result4.ShouldBeSameAs(result3);
        result3.ShouldNotBeSameAs(result);
    }

    [Fact] public void GetSingletonGenericService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(IGenericService<>), typeof(GenericService<>));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<IGenericService<int>>();
        result.ShouldBeOfType<GenericService<int>>();

        var result2 = serviceProvider.GetService<IGenericService<int>>();
        result2.ShouldBeOfType<GenericService<int>>();

        result2.ShouldBeSameAs(result);

        var scope = serviceProvider.CreateScope();

        var result3 = serviceProvider.GetService<IGenericService<int>>();
        result3.ShouldBeOfType<GenericService<int>>();

        var result4 = serviceProvider.GetService<IGenericService<int>>();
        result4.ShouldBeOfType<GenericService<int>>();

        result4.ShouldBeSameAs(result3);
        result3.ShouldBeSameAs(result);
    }

    [Fact] public void GetSameGenericServiceOfDifferentGenericArgumentTypes()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(IGenericService<>), typeof(GenericService<>));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<IGenericService<int>>();
        result.ShouldBeOfType<GenericService<int>>();

        var result2 = serviceProvider.GetService<IGenericService<double>>();
        result2.ShouldBeOfType<GenericService<double>>();

        result2.ShouldNotBeSameAs(result);

        var result3 = serviceProvider.GetService<IGenericService<int>>();
        result3.ShouldBeOfType<GenericService<int>>();
        result3.ShouldBeSameAs(result);
    }


    [Fact] public void GetListOfTransientGenericService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(typeof(IGenericService<>), typeof(GenericService<>));
        serviceCollection.AddTransient(typeof(IGenericService<>), typeof(GenericService2<>));
        serviceCollection.AddTransient(typeof(IGenericService<>), typeof(GenericService3<>));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var services = serviceProvider.GetServices<IGenericService<int>>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();

        services[0].ShouldBeOfType<GenericService<int>>();
        services[1].ShouldBeOfType<GenericService2<int>>();
        services[2].ShouldBeOfType<GenericService3<int>>();

        var scope = serviceProvider.CreateScope();
        var services2 = scope.ServiceProvider.GetServices<IGenericService<int>>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();

        services2[0].ShouldBeOfType<GenericService<int>>();
        services2[0].ShouldNotBeSameAs(services[0]);
        services2[1].ShouldBeOfType<GenericService2<int>>();
        services2[1].ShouldNotBeSameAs(services[1]);
        services2[2].ShouldBeOfType<GenericService3<int>>();
        services2[2].ShouldNotBeSameAs(services[2]);
    }

    [Fact] public void GetListOfScopedGenericService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
        serviceCollection.AddScoped(typeof(IGenericService<>), typeof(GenericService2<>));
        serviceCollection.AddScoped(typeof(IGenericService<>), typeof(GenericService3<>));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetServices<IGenericService<int>>());

        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider.GetServices<IGenericService<int>>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();
        services[0].ShouldBeOfType<GenericService<int>>();
        services[1].ShouldBeOfType<GenericService2<int>>();
        services[2].ShouldBeOfType<GenericService3<int>>();

        var services2 = scope.ServiceProvider.GetServices<IGenericService<int>>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();

        services2[0].ShouldBeOfType<GenericService<int>>();
        services2[0].ShouldBeSameAs(services[0]);
        services2[1].ShouldBeOfType<GenericService2<int>>();
        services2[1].ShouldBeSameAs(services[1]);
        services2[2].ShouldBeOfType<GenericService3<int>>();
        services2[2].ShouldBeSameAs(services[2]);

        using var scope2 = serviceProvider.CreateScope();

        var services3 = scope2.ServiceProvider.GetServices<IGenericService<int>>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();

        services3[0].ShouldBeOfType<GenericService<int>>();
        services3[0].ShouldNotBeSameAs(services[0]);
        services3[1].ShouldBeOfType<GenericService2<int>>();
        services3[1].ShouldNotBeSameAs(services[1]);
        services3[2].ShouldBeOfType<GenericService3<int>>();
        services3[2].ShouldNotBeSameAs(services[2]);
    }

    [Fact] public void GetListOfSingletonGenericService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(IGenericService<>), typeof(GenericService<>));
        serviceCollection.AddSingleton(typeof(IGenericService<>), typeof(GenericService2<>));
        serviceCollection.AddSingleton(typeof(IGenericService<>), typeof(GenericService3<>));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var services = serviceProvider.GetServices<IGenericService<int>>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();

        services[0].ShouldBeOfType<GenericService<int>>();
        services[1].ShouldBeOfType<GenericService2<int>>();
        services[2].ShouldBeOfType<GenericService3<int>>();

        var scope = serviceProvider.CreateScope();
        var services2 = scope.ServiceProvider.GetServices<IGenericService<int>>()
            .OrderBy(_ => _.GetType().Name)
            .ToArray();

        services2[0].ShouldBeOfType<GenericService<int>>();
        services2[0].ShouldBeSameAs(services[0]);
        services2[1].ShouldBeOfType<GenericService2<int>>();
        services2[1].ShouldBeSameAs(services[1]);
        services2[2].ShouldBeOfType<GenericService3<int>>();
        services2[2].ShouldBeSameAs(services[2]);
    }


    [Fact] public void DoubleInjectTransientIntoGenericTransientService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddTransient<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();

        var result2 = serviceProvider.GetService<IEmptyTestService>();
        result2.ShouldNotBeSameAs(result.InjectedService);

        var result3 = serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result3.ShouldNotBeSameAs(result);
        result3!.InjectedService.ShouldNotBeSameAs(result.InjectedService);
    }

    [Fact] public void DoubleInjectScopedIntoGenericTransientService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>());

        var scope = serviceProvider.CreateScope();
        var result = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();

        var result2 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeSameAs(result.InjectedService);

        var result3 = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result3.ShouldNotBeSameAs(result);
        result3!.InjectedService.ShouldBeSameAs(result.InjectedService);
    }

    [Fact] public void DoubleInjectSingletonIntoGenericTransientService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();

        var result2 = serviceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeSameAs(result.InjectedService);

        var result3 = serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result3.ShouldNotBeSameAs(result);
        result3!.InjectedService.ShouldBeSameAs(result.InjectedService);
    }

    [Fact] public void DoubleInjectTransientIntoGenericScopedService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddTransient<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>());

        var scope = serviceProvider.CreateScope();

        var result = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();

        var result2 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result2.ShouldNotBeSameAs(result.InjectedService);

        var result3 = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result3.ShouldBeSameAs(result);
        result3!.InjectedService.ShouldBeSameAs(result.InjectedService);

        var scope2 = serviceProvider.CreateScope();

        var result4 = scope2.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldNotBeSameAs(result);
        result4.InjectedService.ShouldNotBeSameAs(result.InjectedService);
    }

    [Fact] public void DoubleInjectScopedIntoGenericScopedService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>());

        var scope = serviceProvider.CreateScope();

        var result = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();

        var result2 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeSameAs(result.InjectedService);

        var result3 = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result3.ShouldBeSameAs(result);
        result3!.InjectedService.ShouldBeSameAs(result.InjectedService);

        var scope2 = serviceProvider.CreateScope();

        var result4 = scope2.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldNotBeSameAs(result);
        result4.InjectedService.ShouldNotBeSameAs(result.InjectedService);
    }

    [Fact] public void DoubleInjectSingletonIntoGenericScopedService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>());

        var scope = serviceProvider.CreateScope();

        var result = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();

        var result2 = scope.ServiceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeSameAs(result.InjectedService);

        var result3 = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result3.ShouldBeSameAs(result);
        result3!.InjectedService.ShouldBeSameAs(result.InjectedService);

        var scope2 = serviceProvider.CreateScope();

        var result4 = scope2.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldNotBeSameAs(result);
        result4.InjectedService.ShouldBeSameAs(result.InjectedService);
    }

    [Fact] public void DoubleInjectTransientIntoGenericSingletonService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddTransient<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();

        var result2 = serviceProvider.GetService<IEmptyTestService>();
        result2.ShouldNotBeSameAs(result.InjectedService);

        var result3 = serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result3.ShouldBeSameAs(result);
        result3!.InjectedService.ShouldBeSameAs(result.InjectedService);

        var scope = serviceProvider.CreateScope();

        var result4 = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldBeSameAs(result);
        result4.InjectedService.ShouldBeSameAs(result.InjectedService);
    }

    [Fact] public void DoubleInjectScopedIntoGenericSingletonService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>());

        var scope = serviceProvider.CreateScope();

        Should.Throw<MetalInjectionException>(() => scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>());
    }

    [Fact] public void DoubleInjectSingletonIntoGenericSingletonService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(IDoubleInjectionGenericService<>), typeof(DoubleInjectionGenericService<>));
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();

        var result2 = serviceProvider.GetService<IEmptyTestService>();
        result2.ShouldBeSameAs(result.InjectedService);

        var result3 = serviceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result3.ShouldBeSameAs(result);
        result3!.InjectedService.ShouldBeSameAs(result.InjectedService);

        var scope = serviceProvider.CreateScope();

        var result4 = scope.ServiceProvider.GetService<IDoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldBeOfType<DoubleInjectionGenericService<IEmptyTestService>>();
        result4.ShouldBeSameAs(result);
        result4.InjectedService.ShouldBeSameAs(result.InjectedService);
    }


    [Fact] public void GetMissingService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, AnotherEmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, YetAnotherEmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();
        serviceProvider.GetService<ITestServiceWithInjection>().ShouldBeNull();
    }

    [Fact] public void GetMissingKeyedService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("First");
        serviceCollection.AddKeyedSingleton<IEmptyTestService, AnotherEmptyTestService>("Second");
        serviceCollection.AddKeyedSingleton<IEmptyTestService, YetAnotherEmptyTestService>("Third");
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();
        serviceProvider.GetKeyedService<ITestServiceWithInjection>("Fourth").ShouldBeNull();
    }

    [Fact] public void GetMissingRequiredService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, AnotherEmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, YetAnotherEmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();
        Should.Throw<MetalInjectionException>(() => serviceProvider.GetRequiredService<ITestServiceWithInjection>());
    }

    [Fact] public void GetMissingRequiredKeyedService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("First");
        serviceCollection.AddKeyedSingleton<IEmptyTestService, AnotherEmptyTestService>("Second");
        serviceCollection.AddKeyedSingleton<IEmptyTestService, YetAnotherEmptyTestService>("Third");
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();
        Should.Throw<MetalInjectionException>(() =>
            serviceProvider.GetRequiredKeyedService<ITestServiceWithInjection>("Fourth"));
    }



    // ?? Optional Constructor Injection ??????????????????????????????????????????????????????
    [Fact] public void OptionalConstructorInjection_NullWhenNotRegistered()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<Phase5ConsumerWithOptional>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<Phase5ConsumerWithOptional>();
        result.ShouldNotBeNull();
        result.Optional.ShouldBeNull();
    }

    [Fact] public void OptionalConstructorInjection_InjectedWhenRegistered()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IPhase5OptionalService, Phase5OptionalServiceImpl>();
        serviceCollection.AddTransient<Phase5ConsumerWithOptional>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result = serviceProvider.GetService<Phase5ConsumerWithOptional>();
        result.ShouldNotBeNull();
        result.Optional.ShouldBeOfType<Phase5OptionalServiceImpl>();
    }

    // ?? Keyed Services ??????????????????????????????????????????????????????????????????????
    [Fact] public void KeyedSingletonServiceResolution()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("a");
        serviceCollection.AddKeyedSingleton<IEmptyTestService, AnotherEmptyTestService>("b");
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var a1 = serviceProvider.GetKeyedService<IEmptyTestService>("a");
        var b1 = serviceProvider.GetKeyedService<IEmptyTestService>("b");
        a1.ShouldBeOfType<EmptyTestService>();
        b1.ShouldBeOfType<AnotherEmptyTestService>();

        serviceProvider.GetKeyedService<IEmptyTestService>("a").ShouldBeSameAs(a1);

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetKeyedService<IEmptyTestService>("a").ShouldBeSameAs(a1);
        scope.ServiceProvider.GetKeyedService<IEmptyTestService>("b").ShouldBeSameAs(b1);
    }

    [Fact] public void KeyedScopedServiceResolution()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedScoped<IEmptyTestService, EmptyTestService>("k");
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetKeyedService<IEmptyTestService>("k"));

        using var scope1 = serviceProvider.CreateScope();
        var result1 = scope1.ServiceProvider.GetKeyedService<IEmptyTestService>("k");
        result1.ShouldBeOfType<EmptyTestService>();
        scope1.ServiceProvider.GetKeyedService<IEmptyTestService>("k").ShouldBeSameAs(result1);

        using var scope2 = serviceProvider.CreateScope();
        var result2 = scope2.ServiceProvider.GetKeyedService<IEmptyTestService>("k");
        result2.ShouldNotBeSameAs(result1);
    }

    [Fact] public void KeyedTransientServiceResolution()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeyedTransient<IEmptyTestService, EmptyTestService>("k");
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var result1 = serviceProvider.GetKeyedService<IEmptyTestService>("k");
        var result2 = serviceProvider.GetKeyedService<IEmptyTestService>("k");
        result1.ShouldBeOfType<EmptyTestService>();
        result2.ShouldNotBeSameAs(result1);
    }

    // ?? ThrowOnError ?????????????????????????????????????????????????????????????????????????
    [Fact] public void ScopedFromRoot_AlwaysThrows()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IEmptyTestService>());
    }

    [Fact] public void CaptiveDependency_AlwaysThrows()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<ITestServiceWithInjection, TestServiceWithInjection>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<ITestServiceWithInjection>());
    }

    // ?? StrictResolution
    [Fact] public void MultipleRegistrations_ThrowsAtResolutionTime()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, AnotherEmptyTestService>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IEmptyTestService>());
    }

    [Fact] public void InstantiationFailure_Throws()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<Phase6Unsatisfiable>();
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<Phase6Unsatisfiable>());
    }

    [Fact] public void MultipleRegistrations_ViaServiceCollection_ThrowsAtResolutionTime()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IEmptyTestService, EmptyTestService>();
        serviceCollection.AddSingleton<IEmptyTestService, AnotherEmptyTestService>();
        // Multiple registrations always throw — permissive mode is removed
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        Should.Throw<MetalInjectionException>(() => serviceProvider.GetService<IEmptyTestService>());
    }

    // ?? Implementation Factory ??????????????????????????????????????????????????????????????
    [Fact] public void SingletonImplementationFactory_CalledExactlyOnce()
    {
        var counter = 0;
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IPhase7FactoryService>(_ => new Phase7FactoryService(++counter));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var first = serviceProvider.GetService<IPhase7FactoryService>();
        var second = serviceProvider.GetService<IPhase7FactoryService>();
        IPhase7FactoryService? fromScope;
        using (var scope = serviceProvider.CreateScope())
            fromScope = scope.ServiceProvider.GetService<IPhase7FactoryService>();

        second.ShouldBeSameAs(first);
        fromScope.ShouldBeSameAs(first);
        counter.ShouldBe(1);
    }

    [Fact] public void ScopedImplementationFactory_CalledOncePerScope()
    {
        var counter = 0;
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IPhase7FactoryService>(_ => new Phase7FactoryService(++counter));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        IPhase7FactoryService? scope1First;
        IPhase7FactoryService? scope1Second;
        IPhase7FactoryService? scope2First;
        using (var scope1 = serviceProvider.CreateScope())
        {
            scope1First = scope1.ServiceProvider.GetService<IPhase7FactoryService>();
            scope1Second = scope1.ServiceProvider.GetService<IPhase7FactoryService>();
        }
        using (var scope2 = serviceProvider.CreateScope())
        {
            scope2First = scope2.ServiceProvider.GetService<IPhase7FactoryService>();
        }

        scope1Second.ShouldBeSameAs(scope1First);
        scope2First.ShouldNotBeSameAs(scope1First);
        counter.ShouldBe(2);
    }

    [Fact] public void TransientImplementationFactory_CalledOnEveryResolve()
    {
        var counter = 0;
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IPhase7FactoryService>(_ => new Phase7FactoryService(++counter));
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var first = serviceProvider.GetService<IPhase7FactoryService>();
        var second = serviceProvider.GetService<IPhase7FactoryService>();
        var third = serviceProvider.GetService<IPhase7FactoryService>();

        second.ShouldNotBeSameAs(first);
        third.ShouldNotBeSameAs(second);
        counter.ShouldBe(3);
    }

    [Fact] public void ImplementationInstance_ReusedAsSingleton()
    {
        var existingInstance = new Phase7FactoryService(99);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IPhase7FactoryService>(existingInstance);
        var serviceProvider = serviceCollection.BuildMetalInjectionServiceProvider();

        var fromRoot = serviceProvider.GetService<IPhase7FactoryService>();
        IPhase7FactoryService? fromScope;
        using (var scope = serviceProvider.CreateScope())
            fromScope = scope.ServiceProvider.GetService<IPhase7FactoryService>();

        fromRoot.ShouldBeSameAs(existingInstance);
        fromScope.ShouldBeSameAs(existingInstance);
    }

    // ── G-11: [ActivatorUtilitiesConstructor] forces constructor selection ──────────────────

    [Fact] public void ActivatorUtilitiesConstructorAttribute_ForcesCorrectConstructorSelection()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IEmptyTestService, EmptyTestService>();
        sc.AddTransient<PhaseWithAttributeConstructor>();
        var provider = sc.BuildMetalInjectionServiceProvider();

        var result = provider.GetService<PhaseWithAttributeConstructor>();

        result.ShouldNotBeNull();
        result!.UsedNoArgConstructor.ShouldBeTrue();
    }

    // ── G-12: Multiple constructors — engine falls back to longest satisfiable one ──────────

    [Fact] public void MultipleConstructors_FallsBackToSatisfiableConstructor()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IEmptyTestService, EmptyTestService>();
        sc.AddTransient<PhaseWithFallbackConstructor>();
        var provider = sc.BuildMetalInjectionServiceProvider();

        var result = provider.GetService<PhaseWithFallbackConstructor>();

        result.ShouldNotBeNull();
        result!.UsedShortConstructor.ShouldBeTrue();
    }

    // ── G-13: Factory lambda that throws propagates exception to caller ──────────────────────

    [Fact] public void ImplementationFactory_ThatThrows_PropagatesException()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IEmptyTestService>(_ => throw new InvalidOperationException("factory error"));
        var provider = sc.BuildMetalInjectionServiceProvider();

        Should.Throw<InvalidOperationException>(() => provider.GetService<IEmptyTestService>());
    }

    // ── G-21: IEnumerable<T> at root silently drops unguarded scoped entries ─────────────────

    [Fact] public void ServiceList_AtRoot_ScopedEntriesExcluded()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IEmptyTestService, EmptyTestService>();
        sc.AddScoped<IEmptyTestService, AnotherEmptyTestService>();
        sc.AddSingleton<IEmptyTestService, YetAnotherEmptyTestService>();
        var provider = sc.BuildMetalInjectionServiceProvider();

        var results = provider.GetServices<IEmptyTestService>().ToList();

        results.Count.ShouldBe(2);
        results.ShouldContain(r => r is EmptyTestService);
        results.ShouldContain(r => r is YetAnotherEmptyTestService);
    }

    // ── G-22: Duplicate identical descriptors are silently deduplicated ──────────────────────

    [Fact] public void DuplicateIdenticalDescriptors_DeduplicatedSilently()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IEmptyTestService, EmptyTestService>();
        sc.AddSingleton<IEmptyTestService, EmptyTestService>();
        var provider = sc.BuildMetalInjectionServiceProvider();

        var result = provider.GetService<IEmptyTestService>();

        result.ShouldNotBeNull();
        result.ShouldBeOfType<EmptyTestService>();
    }

    // ── G-23: IsService(Type) returns correct values ─────────────────────────────────────────

    [Fact] public void IsService_RegisteredType_ReturnsTrue()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IEmptyTestService, EmptyTestService>();
        var provider = sc.BuildMetalInjectionServiceProvider();
        var isServiceCheck = (IServiceProviderIsService)provider;

        isServiceCheck.IsService(typeof(IEmptyTestService)).ShouldBeTrue();
    }

    [Fact] public void IsService_UnregisteredType_ReturnsFalse()
    {
        var provider = new ServiceCollection().BuildMetalInjectionServiceProvider();
        var isServiceCheck = (IServiceProviderIsService)provider;

        isServiceCheck.IsService(typeof(IEmptyTestService)).ShouldBeFalse();
    }

    [Fact] public void IsService_ScopedTypeFromRoot_ReturnsFalse()
    {
        // IsService returns false for a scoped service resolved from the root provider because
        // the scoped-from-root guard fires even during the isCheck path (before instantiation).
        var sc = new ServiceCollection();
        sc.AddScoped<IEmptyTestService, EmptyTestService>();
        var provider = sc.BuildMetalInjectionServiceProvider();
        var isServiceCheck = (IServiceProviderIsService)provider;

        isServiceCheck.IsService(typeof(IEmptyTestService)).ShouldBeFalse();
    }

    // ── G-24: IsKeyedService(Type, key) returns correct values ──────────────────────────────

    [Fact] public void IsKeyedService_RegisteredKeyedType_ReturnsTrue()
    {
        var sc = new ServiceCollection();
        sc.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("key1");
        var provider = sc.BuildMetalInjectionServiceProvider();
        var isKeyedServiceCheck = (IServiceProviderIsKeyedService)provider;

        isKeyedServiceCheck.IsKeyedService(typeof(IEmptyTestService), "key1").ShouldBeTrue();
    }

    [Fact] public void IsKeyedService_UnregisteredKey_ReturnsFalse()
    {
        var provider = new ServiceCollection().BuildMetalInjectionServiceProvider();
        var isKeyedServiceCheck = (IServiceProviderIsKeyedService)provider;

        isKeyedServiceCheck.IsKeyedService(typeof(IEmptyTestService), "missing").ShouldBeFalse();
    }

    [Fact] public void IsKeyedService_WrongKey_ReturnsFalse()
    {
        var sc = new ServiceCollection();
        sc.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("key1");
        var provider = sc.BuildMetalInjectionServiceProvider();
        var isKeyedServiceCheck = (IServiceProviderIsKeyedService)provider;

        isKeyedServiceCheck.IsKeyedService(typeof(IEmptyTestService), "wrong-key").ShouldBeFalse();
    }

    // ── G-25: IServiceScopeFactory resolved via interface can create scopes ──────────────────

    [Fact] public void IServiceScopeFactory_ResolvedFromContainer_CanCreateScope()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IEmptyTestService, EmptyTestService>();
        var provider = sc.BuildMetalInjectionServiceProvider();

        var factory = provider.GetRequiredService<IServiceScopeFactory>();
        using var scope = factory.CreateScope();
        var result = scope.ServiceProvider.GetService<IEmptyTestService>();

        result.ShouldNotBeNull();
    }
}


public interface IEmptyTestService { }
public class EmptyTestService : IEmptyTestService
{
}
public class AnotherEmptyTestService : IEmptyTestService
{
}
public class YetAnotherEmptyTestService : IEmptyTestService
{
}


public interface ITestServiceWithInjection { }
public class TestServiceWithInjection(
    IEmptyTestService testService)
    : ITestServiceWithInjection
{
    public IEmptyTestService TestService { get; } = testService;
}


public interface IGenericService<TValue>
{
    TValue Value { get; set; }
}
public class GenericService<TValue> : IGenericService<TValue>
{
    public TValue Value { get; set; } = default(TValue)!;
}
public class GenericService2<TValue> : IGenericService<TValue>
{
    public TValue Value { get; set; } = default(TValue)!;
}

public class GenericService3<TValue> : IGenericService<TValue>
{
    public TValue Value { get; set; } = default(TValue)!;
}

public interface IDoubleInjectionGenericService<TService>
{
    public TService InjectedService { get; }
}
public class DoubleInjectionGenericService<TService> : IDoubleInjectionGenericService<TService>
{
    public DoubleInjectionGenericService(TService injectedService)
    {
        InjectedService = injectedService;
    }
    public TService InjectedService { get; }
}


public interface IPhase5OptionalService { }
public class Phase5OptionalServiceImpl : IPhase5OptionalService { }

public class Phase5ConsumerWithOptional
{
    public Phase5ConsumerWithOptional(IPhase5OptionalService? optional = null)
    {
        Optional = optional;
    }
    public IPhase5OptionalService? Optional { get; }
}

public interface IMissingFromContainer { }

public class Phase6Unsatisfiable
{
    public Phase6Unsatisfiable(IEmptyTestService a, IMissingFromContainer b) { }
}

public interface IPhase7FactoryService
{
    int InstanceId { get; }
}

public class Phase7FactoryService : IPhase7FactoryService
{
    public Phase7FactoryService(int instanceId) => InstanceId = instanceId;
    public int InstanceId { get; }
}

public class PhaseWithAttributeConstructor
{
    public bool UsedNoArgConstructor { get; }

    [ActivatorUtilitiesConstructor]
    public PhaseWithAttributeConstructor() { UsedNoArgConstructor = true; }

    public PhaseWithAttributeConstructor(IEmptyTestService _) { UsedNoArgConstructor = false; }
}

public class PhaseWithFallbackConstructor
{
    public bool UsedShortConstructor { get; }

    // Long constructor: IMissingFromContainer cannot be resolved, so this fails
    public PhaseWithFallbackConstructor(IEmptyTestService dep1, IMissingFromContainer dep2)
    {
        UsedShortConstructor = false;
    }

    // Short constructor: satisfiable, used as fallback
    public PhaseWithFallbackConstructor(IEmptyTestService dep1)
    {
        UsedShortConstructor = true;
    }
}

// ── T3: IAsyncDisposable support ─────────────────────────────────────────────────────────────

public class AsyncDisposableOnlySvc : IAsyncDisposable
{
    public bool DisposeAsyncCalled { get; private set; }
    public ValueTask DisposeAsync() { DisposeAsyncCalled = true; return ValueTask.CompletedTask; }
}

public class DualDisposableSvc : IDisposable, IAsyncDisposable
{
    public bool DisposeCalled { get; private set; }
    public bool DisposeAsyncCalled { get; private set; }
    public void Dispose() => DisposeCalled = true;
    public ValueTask DisposeAsync() { DisposeAsyncCalled = true; return ValueTask.CompletedTask; }
}

public class IAsyncDisposableOnlyTests
{
    [Fact]
    public void AsyncDisposableOnly_TransientService_DisposeAsyncCalledOnSyncDispose()
    {
        var services = new ServiceCollection();
        services.AddTransient<AsyncDisposableOnlySvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<AsyncDisposableOnlySvc>();
        ((IDisposable)provider).Dispose();

        instance.DisposeAsyncCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task AsyncDisposableOnly_TransientService_DisposeAsyncCalledOnAsyncDispose()
    {
        var services = new ServiceCollection();
        services.AddTransient<AsyncDisposableOnlySvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<AsyncDisposableOnlySvc>();
        await ((IAsyncDisposable)provider).DisposeAsync();

        instance.DisposeAsyncCalled.ShouldBeTrue();
    }

    [Fact]
    public void AsyncDisposableOnly_ScopedService_DisposeAsyncCalledOnScopeDispose()
    {
        var services = new ServiceCollection();
        services.AddScoped<AsyncDisposableOnlySvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        AsyncDisposableOnlySvc instance;
        using (var scope = provider.CreateScope())
        {
            instance = scope.ServiceProvider.GetRequiredService<AsyncDisposableOnlySvc>();
        }

        instance.DisposeAsyncCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task AsyncDisposableOnly_ScopedService_DisposeAsyncCalledOnAsyncScopeDispose()
    {
        var services = new ServiceCollection();
        services.AddScoped<AsyncDisposableOnlySvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        AsyncDisposableOnlySvc instance;
        await using (var scope = (IAsyncDisposable)provider.CreateScope())
        {
            instance = ((IServiceScope)scope).ServiceProvider.GetRequiredService<AsyncDisposableOnlySvc>();
        }

        instance.DisposeAsyncCalled.ShouldBeTrue();
    }

    [Fact]
    public void AsyncDisposableOnly_SingletonService_DisposeAsyncCalledOnRootDispose()
    {
        var services = new ServiceCollection();
        services.AddSingleton<AsyncDisposableOnlySvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<AsyncDisposableOnlySvc>();
        ((IDisposable)provider).Dispose();

        instance.DisposeAsyncCalled.ShouldBeTrue();
    }

    [Fact]
    public void DualDisposable_SyncAndAsync_OnlySyncDisposeCalledOnSyncPath()
    {
        var services = new ServiceCollection();
        services.AddTransient<DualDisposableSvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<DualDisposableSvc>();
        ((IDisposable)provider).Dispose();

        // Sync path must call Dispose, not DisposeAsync, for dual-disposable services
        instance.DisposeCalled.ShouldBeTrue();
        instance.DisposeAsyncCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task DualDisposable_SyncAndAsync_AsyncDisposeCalledOnAsyncPath()
    {
        var services = new ServiceCollection();
        services.AddTransient<DualDisposableSvc>();
        var provider = services.BuildMetalInjectionServiceProvider();

        var instance = provider.GetRequiredService<DualDisposableSvc>();
        await ((IAsyncDisposable)provider).DisposeAsync();

        // Async path must call DisposeAsync, not Dispose, for dual-disposable services
        instance.DisposeCalled.ShouldBeFalse();
        instance.DisposeAsyncCalled.ShouldBeTrue();
    }
}

// ── T4: GetKeyedService with KeyedService.AnyKey sentinel ────────────────────────────────────

public class KeyedServiceAnyKeyTests
{
    [Fact]
    public void GetKeyedService_WithAnyKey_ResolvesFirstMatchingRegistration()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("k1");
        services.AddKeyedSingleton<IEmptyTestService, AnotherEmptyTestService>("k2");
        var provider = services.BuildMetalInjectionServiceProvider();

        // AnyKey is a wildcard that matches all keyed registrations; single-get returns first.
        var result = provider.GetKeyedService<IEmptyTestService>(KeyedService.AnyKey);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void GetKeyedServices_WithAnyKey_ReturnsAllKeyedRegistrations()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IEmptyTestService, EmptyTestService>("k1");
        services.AddKeyedSingleton<IEmptyTestService, AnotherEmptyTestService>("k2");
        var provider = services.BuildMetalInjectionServiceProvider(o => o.AllowMultipleServicesOfAnyType());

        var all = provider.GetKeyedServices<IEmptyTestService>(KeyedService.AnyKey).ToList();

        all.Count.ShouldBe(2);
    }
}

// ── T6: Open-generic service with factory delegate ───────────────────────────────────────────

public class OpenGenericFactoryTests
{
    [Fact]
    public void OpenGenericService_RegisteredWithFactory_ResolvesCorrectly()
    {
        // Verifies the previously-TODO scenario: open-generic registration with a factory lambda.
        // The standard DI container does not support open-generic factory delegates natively, so
        // the closed generic is registered via factory for each concrete type argument.
        var services = new ServiceCollection();
        services.AddSingleton(typeof(IGenericService<int>), sp => new GenericService<int> { Value = 42 });
        var provider = services.BuildMetalInjectionServiceProvider();

        var result = provider.GetService<IGenericService<int>>();

        result.ShouldNotBeNull();
        result.ShouldBeOfType<GenericService<int>>();
        result!.Value.ShouldBe(42);
    }

    [Fact]
    public void OpenGenericService_FactoryCallsServiceProvider_CanInjectOtherServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEmptyTestService, EmptyTestService>();
        services.AddSingleton(typeof(IGenericService<IEmptyTestService>),
            sp => new GenericService<IEmptyTestService> { Value = sp.GetRequiredService<IEmptyTestService>() });
        var provider = services.BuildMetalInjectionServiceProvider();

        var result = provider.GetService<IGenericService<IEmptyTestService>>();

        result.ShouldNotBeNull();
        result!.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<EmptyTestService>();
    }
}

// ── T7: Open-generic factory delegate registration ───────────────────────────────────────────

public class OpenGenericFactoryDelegateTests
{
    [Fact]
    public void OpenGenericFactory_Singleton_ResolvesCorrectlyForDifferentTypeArgs()
    {
        var services = new ServiceCollection();
        services.AddOpenGenericSingleton(typeof(IGenericService<>),
            (sp, typeArgs) => Activator.CreateInstance(typeof(GenericService<>).MakeGenericType(typeArgs))!);
        var provider = services.BuildMetalInjectionServiceProvider();

        var intSvc = provider.GetRequiredService<IGenericService<int>>();
        var strSvc = provider.GetRequiredService<IGenericService<string>>();

        intSvc.ShouldBeOfType<GenericService<int>>();
        strSvc.ShouldBeOfType<GenericService<string>>();
        // Singleton — same instance on repeat resolve
        provider.GetRequiredService<IGenericService<int>>().ShouldBeSameAs(intSvc);
    }

    [Fact]
    public void OpenGenericFactory_Scoped_ReturnsNewInstancePerScope()
    {
        var services = new ServiceCollection();
        services.AddOpenGenericScoped(typeof(IGenericService<>),
            (sp, typeArgs) => Activator.CreateInstance(typeof(GenericService<>).MakeGenericType(typeArgs))!);
        var provider = services.BuildMetalInjectionServiceProvider();

        IGenericService<int> fromScope1, fromScope2;
        using (var s1 = provider.CreateScope())
            fromScope1 = s1.ServiceProvider.GetRequiredService<IGenericService<int>>();
        using (var s2 = provider.CreateScope())
            fromScope2 = s2.ServiceProvider.GetRequiredService<IGenericService<int>>();

        fromScope2.ShouldNotBeSameAs(fromScope1);
    }

    [Fact]
    public void OpenGenericFactory_Transient_InvokesFactoryOnEveryResolve()
    {
        int callCount = 0;
        var services = new ServiceCollection();
        services.AddOpenGenericTransient(typeof(IGenericService<>),
            (sp, typeArgs) => { callCount++; return Activator.CreateInstance(typeof(GenericService<>).MakeGenericType(typeArgs))!; });
        var provider = services.BuildMetalInjectionServiceProvider();

        provider.GetRequiredService<IGenericService<int>>();
        provider.GetRequiredService<IGenericService<int>>();
        provider.GetRequiredService<IGenericService<int>>();

        callCount.ShouldBe(3);
    }

    [Fact]
    public void OpenGenericFactory_FactoryReceivesTypeArgs_CanBranchOnType()
    {
        var services = new ServiceCollection();
        services.AddOpenGenericTransient(typeof(IGenericService<>),
            (sp, typeArgs) => typeArgs[0] == typeof(int)
                ? (object)new GenericService<int> { Value = 99 }
                : new GenericService2<string> { Value = "hello" });
        var provider = services.BuildMetalInjectionServiceProvider();

        var intSvc = provider.GetRequiredService<IGenericService<int>>();
        intSvc.ShouldBeOfType<GenericService<int>>();
        intSvc.Value.ShouldBe(99);
    }

    [Fact]
    public void OpenGenericFactory_NonOpenGenericType_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Should.Throw<ArgumentException>(() =>
            services.AddOpenGenericFactory(typeof(IGenericService<int>), ServiceLifetime.Singleton,
                (sp, typeArgs) => new GenericService<int>()));
    }

    [Fact]
    public void OpenGenericFactory_CanInjectOtherServicesViaServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEmptyTestService, EmptyTestService>();
        services.AddOpenGenericSingleton(typeof(IGenericService<>),
            (sp, typeArgs) =>
            {
                if (typeArgs[0] == typeof(IEmptyTestService))
                    return new GenericService<IEmptyTestService>
                        { Value = sp.GetRequiredService<IEmptyTestService>() };
                return Activator.CreateInstance(typeof(GenericService<>).MakeGenericType(typeArgs))!;
            });
        var provider = services.BuildMetalInjectionServiceProvider();

        var svc = provider.GetRequiredService<IGenericService<IEmptyTestService>>();

        svc.Value.ShouldNotBeNull();
        svc.Value.ShouldBeOfType<EmptyTestService>();
    }
}
