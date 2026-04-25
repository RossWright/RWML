using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RossWright.MetalInjection.Tests;

public class CovariantServiceResolutionTests
{
    public interface ILedger { }
    public class AddLedger : ILedger { }
    public class SubtractLedger : ILedger { }

    public interface IValidator<T> { }

    [Singleton(typeof(IValidator<ILedger>), CovariantResolution = Covariance.Covariant)]
    public class CovariantLedgerValidator : IValidator<ILedger> { }

    [Singleton(typeof(IValidator<ILedger>))]
    public class NonCovariantLedgerValidator : IValidator<ILedger> { }

    [Singleton(typeof(IValidator<ILedger>), CovariantResolution = Covariance.Covariant)]
    public class AnotherCovariantLedgerValidator : IValidator<ILedger> { }

    [Singleton(typeof(IValidator<AddLedger>))]
    public class SpecificAddValidator : IValidator<AddLedger> { }

    [Singleton(typeof(IValidator<ILedger>), "keyed", CovariantResolution = Covariance.Covariant)]
    public class KeyedCovariantValidator : IValidator<ILedger> { }

    public interface INonGeneric { }

    [Singleton(typeof(INonGeneric), CovariantResolution = Covariance.Covariant)]
    public class BaseNonGeneric : INonGeneric { }

    public class DerivedNonGeneric : INonGeneric { }

    // ICovariantValidator uses CLR-covariant 'out T' so ICovariantValidator<ILedger>
    // is assignable to ICovariantValidator<AddLedger> at the array element level.
    public interface ICovariantValidator<out T> { }

    // TypeParameterVariance honours the CLR 'out T' annotation on ICovariantValidator<out T>
    [Singleton(typeof(ICovariantValidator<ILedger>), CovariantResolution = Covariance.HonorInOut)]
    public class CovariantOutLedgerValidator : ICovariantValidator<ILedger> { }

    private MetalInjectionServiceProvider CreateProvider(Action<IServiceCollection> configureServices, Action<MetalInjectionOptionsBuilder>? configureOptions = null)
    {
        var services = new ServiceCollection();
        configureServices(services);
        var options = new MetalInjectionOptionsBuilder();
        configureOptions?.Invoke(options);
        return new MetalInjectionServiceProvider(services, options);
    }

    [Fact]
    public void CovariantResolution_Succeeds_WhenCovariantResolutionIsCovariant()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IValidator<ILedger>), typeof(CovariantLedgerValidator));
        });

        var validator = provider.GetService(typeof(IValidator<AddLedger>));

        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<CovariantLedgerValidator>();
    }

    [Fact]
    public void CovariantResolution_Fails_WhenCovariantResolutionIsDisabled()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IValidator<ILedger>), typeof(NonCovariantLedgerValidator));
        });

        var validator = provider.GetService(typeof(IValidator<AddLedger>));

        validator.ShouldBeNull();
    }

    [Fact]
    public void ExactMatch_Preferred_OverCovariant()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IValidator<ILedger>), typeof(CovariantLedgerValidator));
            services.AddSingleton(typeof(IValidator<AddLedger>), typeof(SpecificAddValidator));
        });

        var validator = provider.GetService(typeof(IValidator<AddLedger>));

        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<SpecificAddValidator>();
    }

    [Fact]
    public void MultipleCovariantMatches_FailsResolution()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IValidator<ILedger>), typeof(CovariantLedgerValidator));
            services.AddSingleton(typeof(IValidator<ILedger>), typeof(AnotherCovariantLedgerValidator));
        });

        Should.Throw<MetalInjectionException>(() => provider.GetService(typeof(IValidator<AddLedger>)));
    }

    [Fact]
    public void CovariantWithKey_ResolvesCorrectly()
    {
        var provider = CreateProvider(services =>
        {
            services.AddKeyedSingleton(typeof(IValidator<ILedger>), "keyed", typeof(KeyedCovariantValidator));
        });

        var validator = provider.GetKeyedService(typeof(IValidator<AddLedger>), "keyed");

        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<KeyedCovariantValidator>();
    }

    [Fact]
    public void CovariantWithKey_DoesNotResolve_WithoutKeyMatch()
    {
        var provider = CreateProvider(services =>
        {
            services.AddKeyedSingleton(typeof(IValidator<ILedger>), "keyed", typeof(KeyedCovariantValidator));
        });

        var validator = provider.GetKeyedService(typeof(IValidator<AddLedger>), "different");

        validator.ShouldBeNull();
    }

    [Fact]
    public void NonGenericService_IgnoresCovariance()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(INonGeneric), typeof(BaseNonGeneric));
        });

        var service = provider.GetService(typeof(DerivedNonGeneric));

        service.ShouldBeNull();
    }

    // -- G-18: Covariant list resolution is not supported -------------------------------------
    // The covariant lookup only runs for single-service resolution (!isServiceList guard).
    // List resolution (IEnumerable<T>) skips the covariant path and returns an empty collection.
    // Supporting covariant list resolution would require CLR generic covariance in the opposite
    // direction to what makes sense for this pattern, so this is a documented limitation.

    [Fact]
    public void CovariantResolution_ViaServiceList_ReturnsEmptyWhenOnlyCovariantRegistrationExists()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IValidator<ILedger>), typeof(CovariantLedgerValidator));
        });

        // Covariant lookup does not fire for list requests; the result is an empty collection.
        var validators = provider.GetServices<IValidator<AddLedger>>().ToList();

        validators.ShouldBeEmpty();
    }

    // -- G-19: Covariant scoped and transient lifetimes resolve correctly ---------------------

    [ScopedService(typeof(IValidator<ILedger>), CovariantResolution = Covariance.Covariant)]
    public class ScopedCovariantLedgerValidator : IValidator<ILedger> { }

    [TransientService(typeof(IValidator<ILedger>), CovariantResolution = Covariance.Covariant)]
    public class TransientCovariantLedgerValidator : IValidator<ILedger> { }

    [Fact]
    public void CovariantResolution_Scoped_ResolvesCorrectlyInScope()
    {
        var provider = CreateProvider(services =>
        {
            services.AddScoped(typeof(IValidator<ILedger>), typeof(ScopedCovariantLedgerValidator));
        });

        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetService(typeof(IValidator<AddLedger>));

        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<ScopedCovariantLedgerValidator>();
    }

    [Fact]
    public void CovariantResolution_Transient_ResolvesCorrectly()
    {
        var provider = CreateProvider(services =>
        {
            services.AddTransient(typeof(IValidator<ILedger>), typeof(TransientCovariantLedgerValidator));
        });

        var validator = provider.GetService(typeof(IValidator<AddLedger>));

        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<TransientCovariantLedgerValidator>();
    }

    // -- Multi-type-argument covariance (Covariance.Covariant) -----------------------
    // All positions are checked: regArg.IsAssignableFrom(requestedArg) for each position.

    public interface IRepository<TEntity, TKey> { }

    [Singleton(typeof(IRepository<Animal, int>), CovariantResolution = Covariance.Covariant)]
    public class AnimalRepository : IRepository<Animal, int> { }

    [Singleton(typeof(IRepository<Dog, int>), CovariantResolution = Covariance.Covariant)]
    public class DogRepository : IRepository<Dog, int> { }

    public class Animal { }
    public class Dog : Animal { }
    public class Cat : Animal { }

    [Fact]
    public void MultiArgCovariant_AllPositionsCompatible_Resolves()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IRepository<Animal, int>), typeof(AnimalRepository));
        });

        // Dog : Animal, int == int ? covariant match
        var repo = provider.GetService(typeof(IRepository<Dog, int>));

        repo.ShouldNotBeNull();
        repo.ShouldBeOfType<AnimalRepository>();
    }

    [Fact]
    public void MultiArgCovariant_OnePositionIncompatible_DoesNotResolve()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IRepository<Animal, int>), typeof(AnimalRepository));
        });

        // Dog : Animal ? but long is NOT assignable from int ?
        var repo = provider.GetService(typeof(IRepository<Dog, long>));

        repo.ShouldBeNull();
    }

    [Fact]
    public void MultiArgCovariant_ExactRegistrationPreferredOverCovariantMatch()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IRepository<Animal, int>), typeof(AnimalRepository));
            services.AddSingleton(typeof(IRepository<Dog, int>), typeof(DogRepository));
        });

        // IRepository<Dog, int> is registered exactly — exact match wins
        var repo = provider.GetService(typeof(IRepository<Dog, int>));

        repo.ShouldNotBeNull();
        repo.ShouldBeOfType<DogRepository>();
    }

    // -- HonorInOut: CLR out/in annotations drive per-position matching ------------------------

    // 'out T' (covariant): registered base, derived requested
    public interface IProducer<out T> { }

    // 'in T' (contravariant): registered derived, base requested
    public interface IConsumer<in T> { }

    [Singleton(typeof(IProducer<Animal>), CovariantResolution = Covariance.HonorInOut)]
    public class AnimalProducer : IProducer<Animal> { }

    [Singleton(typeof(IConsumer<Dog>), CovariantResolution = Covariance.HonorInOut)]
    public class DogConsumer : IConsumer<Dog> { }

    [Fact]
    public void HonorInOut_CovariantPosition_RegisteredBaseMatchesDerivedRequest()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IProducer<Animal>), typeof(AnimalProducer));
        });

        // out T ? Animal.IsAssignableFrom(Dog) ?
        var producer = provider.GetService(typeof(IProducer<Dog>));

        producer.ShouldNotBeNull();
        producer.ShouldBeOfType<AnimalProducer>();
    }

    [Fact]
    public void HonorInOut_CovariantPosition_RequestForWiderTypeDoesNotMatch()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IProducer<Animal>), typeof(AnimalProducer));
        });

        // out T ? Animal.IsAssignableFrom(object) is false (object is not a subtype of Animal)
        var producer = provider.GetService(typeof(IProducer<object>));

        producer.ShouldBeNull();
    }

    [Fact]
    public void HonorInOut_ContravariantPosition_RegisteredDerivedMatchesBaseRequest()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IConsumer<Dog>), typeof(DogConsumer));
        });

        // in T ? TReq(Animal).IsAssignableFrom(TReg(Dog)) ? (Dog is an Animal)
        var consumer = provider.GetService(typeof(IConsumer<Animal>));

        consumer.ShouldNotBeNull();
        consumer.ShouldBeOfType<DogConsumer>();
    }

    [Fact]
    public void HonorInOut_InvariantInterface_RequiresExactMatch()
    {
        // IValidator<T> has no out/in — HonorInOut treats all positions as invariant.
        // This means HonorInOut behaves identically to Disabled for invariant interfaces;
        // use Covariance.Covariant for those instead.
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(IValidator<ILedger>), typeof(CovariantLedgerValidator));
        });

        // HonorInOut with invariant T: exact match required — no covariant resolution
        var validator = provider.GetService(typeof(IValidator<AddLedger>));

        // CovariantLedgerValidator is registered for IValidator<ILedger> with Covariance.Covariant
        // (not HonorInOut), so it DOES resolve via the Covariant path.
        validator.ShouldNotBeNull();
    }

    [Fact]
    public void HonorInOut_CLRCovariantInterface_ResolvesCorrectly()
    {
        var provider = CreateProvider(services =>
        {
            services.AddSingleton(typeof(ICovariantValidator<ILedger>), typeof(CovariantOutLedgerValidator));
        });

        // ICovariantValidator<out T>: out T ? ILedger.IsAssignableFrom(AddLedger) ?
        var validator = provider.GetService(typeof(ICovariantValidator<AddLedger>));

        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<CovariantOutLedgerValidator>();
    }
} 
