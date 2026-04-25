namespace RossWright.MetalInjection;

/// <summary>
/// Controls how MetalInjection resolves a generic service when the exact closed type is not
/// registered. Applied via the <see cref="AutoServiceAttributeBase.CovariantResolution"/>
/// property on <see cref="SingletonAttribute"/>, <see cref="ScopedServiceAttribute"/>, and
/// <see cref="TransientServiceAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Background: what "covariance" means in this context</strong><br/>
/// Normally, DI registrations are exact: if you register <c>IValidator&lt;Animal&gt;</c>,
/// a request for <c>IValidator&lt;Dog&gt;</c> returns <see langword="null"/>, even though
/// <c>Dog</c> derives from <c>Animal</c>. MetalInjection's covariant resolution lets a
/// single "wider" registration satisfy requests for "narrower" closed types.
/// </para>
/// <para>
/// There are two distinct strategies, selected by this enum. The right choice depends on
/// whether the interface itself carries CLR <c>out</c>/<c>in</c> variance annotations.
/// </para>
/// </remarks>
public enum Covariance
{
    /// <summary>
    /// No covariant resolution. Only an exact type-argument match satisfies the request.
    /// This is the default and matches the behaviour of the standard
    /// <c>Microsoft.Extensions.DependencyInjection</c> container.
    /// </summary>
    /// <example>
    /// <code>
    /// public interface IValidator&lt;T&gt; { }
    ///
    /// // Registered as IValidator&lt;Animal&gt;
    /// [Singleton(typeof(IValidator&lt;Animal&gt;))]   // Covariance = Disabled (default)
    /// public class AnimalValidator : IValidator&lt;Animal&gt; { }
    ///
    /// // IValidator&lt;Dog&gt; → null  (Dog is not Animal for DI purposes)
    /// // IValidator&lt;Animal&gt; → AnimalValidator ✓
    /// </code>
    /// </example>
    Disabled = 0,

    /// <summary>
    /// MetalInjection-defined covariance: every type-argument position is treated as
    /// covariant. A registered <c>IFoo&lt;TReg&gt;</c> (or <c>IFoo&lt;TReg1, TReg2&gt;</c>)
    /// is a candidate for a requested <c>IFoo&lt;TReq&gt;</c> when
    /// <c>TReg.IsAssignableFrom(TReq)</c> is <see langword="true"/> for every position —
    /// that is, the requested type is the same as or derives from / implements the registered type
    /// at each position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This mode ignores any CLR <c>out</c>/<c>in</c> annotations on the interface. It applies
    /// uniform covariant ("wider base registered, narrower derived requested") semantics to
    /// all type parameters. Use it when you own the interface and want DI-level covariance
    /// regardless of CLR annotations, including multi-type-argument generics.
    /// </para>
    /// <para>
    /// MetalInjection enforces the single-winner rule: if more than one registered
    /// closed type is a valid covariant match for the requested type, resolution fails
    /// with a <see cref="MetalInjectionException"/>. Eliminate ambiguity by registering
    /// exact closed types for the cases that need different implementations, or by ensuring
    /// only one registered type covariantly covers any given request.
    /// </para>
    /// <para>
    /// <strong>Single type argument</strong>
    /// </para>
    /// <code>
    /// public interface IValidator&lt;T&gt; { }
    ///
    /// // Registers a validator that handles any ILedger (and subtypes of ILedger).
    /// [Singleton(typeof(IValidator&lt;ILedger&gt;), CovariantResolution = Covariance.Covariant)]
    /// public class LedgerValidator : IValidator&lt;ILedger&gt; { }
    ///
    /// // IValidator&lt;ILedger&gt; → LedgerValidator ✓  (exact match, covariance not needed)
    /// // IValidator&lt;AddLedger&gt; → LedgerValidator ✓  (AddLedger : ILedger)
    /// // IValidator&lt;SubtractLedger&gt; → LedgerValidator ✓  (SubtractLedger : ILedger)
    /// // IValidator&lt;string&gt; → null  (string is not ILedger)
    /// </code>
    /// <para>
    /// <strong>Multiple type arguments — all positions are checked</strong>
    /// </para>
    /// <code>
    /// public interface IRepository&lt;TEntity, TKey&gt; { TEntity? Find(TKey id); }
    ///
    /// // One registration covers Animal/int and all subtypes of Animal with key int.
    /// [Singleton(typeof(IRepository&lt;Animal, int&gt;), CovariantResolution = CovariantResolution.Covariant)]
    /// public class AnimalRepository : IRepository&lt;Animal, int&gt; { }
    ///
    /// // IRepository&lt;Animal, int&gt; → AnimalRepository ✓  (exact)
    /// // IRepository&lt;Dog, int&gt; → AnimalRepository ✓  (Dog : Animal, int == int)
    /// // IRepository&lt;Dog, long&gt; → null  (int is NOT assignable from long)
    /// // IRepository&lt;string, int&gt; → null  (Animal is not assignable from string)
    /// </code>
    /// <para>
    /// <strong>Exact registration takes priority</strong><br/>
    /// If <c>IRepository&lt;Dog, int&gt;</c> is also registered directly, that exact
    /// registration wins and the covariant fallback is not used.
    /// </para>
    /// </remarks>
    Covariant,

    /// <summary>
    /// CLR-variance-aware resolution: MetalInjection honours the <c>out</c>/<c>in</c>/invariant
    /// annotation on each type parameter of the generic type definition and applies the
    /// corresponding matching rule per position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The three per-position rules are:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <term><c>out T</c> (CLR covariant)</term>
    ///     <description>
    ///       <c>TReg.IsAssignableFrom(TReq)</c> — the registered type is the base;
    ///       the requested type may be the same or more derived.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>in T</c> (CLR contravariant)</term>
    ///     <description>
    ///       <c>TReq.IsAssignableFrom(TReg)</c> — the registered type is more derived;
    ///       the requested type may be the same or wider (base).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>no annotation (invariant)</term>
    ///     <description>
    ///       Exact match only: <c>TReg == TReq</c>.
    ///       A fully-invariant interface with no <c>out</c>/<c>in</c> annotations on any
    ///       parameter will behave identically to <see cref="Disabled"/> under this mode.
    ///       Use <see cref="Covariant"/> instead for invariant interfaces (no <c>out</c>/<c>in</c>).
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Use this mode when the interface already carries correct CLR variance annotations
    /// and you want MetalInjection's resolution to honour them automatically.
    /// </para>
    /// <para>
    /// <strong>Covariant interface (<c>out T</c>)</strong>
    /// </para>
    /// <code>
    /// // 'out T' means IProducer&lt;Dog&gt; IS-A IProducer&lt;Animal&gt; at the CLR level.
    /// public interface IProducer&lt;out T&gt; { T Produce(); }
    ///
    /// [Singleton(typeof(IProducer&lt;Animal&gt;), CovariantResolution = Covariance.HonorInOut)]
    /// public class AnimalProducer : IProducer&lt;Animal&gt; { }
    ///
    /// // IProducer&lt;Animal&gt; → AnimalProducer ✓  (exact)
    /// // IProducer&lt;Dog&gt; → AnimalProducer ✓  (out T: Animal.IsAssignableFrom(Dog))
    /// // IProducer&lt;object&gt; → null  (Animal is not assignable from object)
    /// </code>
    /// <para>
    /// <strong>Contravariant interface (<c>in T</c>)</strong>
    /// </para>
    /// <code>
    /// // 'in T' means IConsumer&lt;Animal&gt; IS-A IConsumer&lt;Dog&gt; at the CLR level.
    /// public interface IConsumer&lt;in T&gt; { void Consume(T value); }
    ///
    /// [Singleton(typeof(IConsumer&lt;object&gt;), CovariantResolution = Covariance.HonorInOut)]
    /// public class ObjectConsumer : IConsumer&lt;object&gt; { }
    ///
    /// // IConsumer&lt;object&gt; → ObjectConsumer ✓  (exact)
    /// // IConsumer&lt;Animal&gt; → ObjectConsumer ✓  (in T: Animal.IsAssignableFrom(object))
    /// // IConsumer&lt;Dog&gt; → ObjectConsumer ✓  (in T: Dog.IsAssignableFrom(object))
    /// // IConsumer&lt;int&gt; → null  (int.IsAssignableFrom(object) is false — value types)
    /// </code>
    /// <para>
    /// <strong>Mixed variance interface</strong>
    /// </para>
    /// <code>
    /// public interface IConverter&lt;in TFrom, out TResult&gt;
    /// {
    ///     TResult Convert(TFrom input);
    /// }
    ///
    /// [Singleton(typeof(IConverter&lt;Dog, Animal&gt;), CovariantResolution = Covariance.HonorInOut)]
    /// public class DogToAnimalConverter : IConverter&lt;Dog, Animal&gt; { }
    ///
    /// // in TFrom  → TReq.IsAssignableFrom(TReg):  TReq(Animal).IsAssignableFrom(TReg(Dog))  ✓
    /// // out TResult → TReg.IsAssignableFrom(TReq): TReg(Animal).IsAssignableFrom(TReq(Cat)) ✓ (Cat : Animal)
    /// // IConverter&lt;Animal, Cat&gt; → DogToAnimalConverter ✓
    ///
    /// // in TFrom  → Animal.IsAssignableFrom(Dog) ✓
    /// // out TResult → Animal.IsAssignableFrom(Dog) ✓
    /// // IConverter&lt;Animal, Dog&gt; → DogToAnimalConverter ✓
    /// </code>
    /// <para>
    /// <strong>Fully invariant interface — use <see cref="Covariant"/> instead</strong>
    /// </para>
    /// <code>
    /// // No out/in: all positions are invariant → TypeParameterVariance requires exact match
    /// // on every position → behaves identically to Disabled.
    /// public interface IRepository&lt;TEntity, TKey&gt; { TEntity? Find(TKey id); }
    ///
    /// // Use Covariant, not TypeParameterVariance, for invariant interfaces.
    /// [Singleton(typeof(IRepository&lt;Animal, int&gt;), CovariantResolution = Covariance.Covariant)]
    /// public class AnimalRepository : IRepository&lt;Animal, int&gt; { }
    /// </code>
    /// </remarks>
    HonorInOut,
}
