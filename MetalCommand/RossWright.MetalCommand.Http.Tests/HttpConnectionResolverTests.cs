using RossWright.MetalCommand.Http.Internal;

namespace RossWright.MetalCommand.Http.Tests;

public class HttpConnectionResolverTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static HttpConnectionEntry Entry(
        string env, bool isDefault = false, bool isProtected = false) =>
        new()
        {
            Environment = env,
            IsDefault = isDefault,
            IsProtected = isProtected,
            BaseAddress = $"http://{env}.example.com"
        };

    private static HttpConnectionRegistry RegistryWith(
        string groupName, params HttpConnectionEntry[] entries)
    {
        var r = new HttpConnectionRegistry();
        r.Upsert(groupName, entries);
        return r;
    }

    private static HttpConnectionResolver Resolver(
        HttpConnectionRegistry registry,
        IEnumerable<IEnvironmentSource>? sources = null)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(ci => new HttpClient());
        return new HttpConnectionResolver(registry, factory, sources ?? []);
    }

    private static HttpConnectionResolver ResolverWithFactory(
        HttpConnectionRegistry registry,
        IHttpClientFactory factory,
        IEnumerable<IEnvironmentSource>? sources = null)
        => new(registry, factory, sources ?? []);

    // ── GetClientName — default group ──────────────────────────────────────

    [Fact]
    public void GetClientName_DefaultGroup_NullEnvironment_ReturnsDefaultEnvKey()
    {
        var registry = RegistryWith(string.Empty, Entry("dev", isDefault: true), Entry("prod"));
        var resolver = Resolver(registry);

        resolver.GetClientName().ShouldBe("MetalCommand:dev");
    }

    [Fact]
    public void GetClientName_DefaultGroup_ExplicitEnvironment_ReturnsQualifiedKey()
    {
        var registry = RegistryWith(string.Empty, Entry("dev"), Entry("prod"));
        var resolver = Resolver(registry);

        resolver.GetClientName("prod").ShouldBe("MetalCommand:prod");
    }

    // ── GetClientName — named group ────────────────────────────────────────

    [Fact]
    public void GetClientName_NamedGroup_ReturnsQualifiedKey()
    {
        var registry = RegistryWith("payments", Entry("dev"), Entry("prod", isDefault: true));
        var resolver = Resolver(registry);

        resolver.GetClientName(null, "payments").ShouldBe("MetalCommand:payments:prod");
    }

    [Fact]
    public void GetClientName_NamedGroup_ExplicitEnvironment_ReturnsQualifiedKey()
    {
        var registry = RegistryWith("payments", Entry("dev"), Entry("prod"));
        var resolver = Resolver(registry);

        resolver.GetClientName("dev", "payments").ShouldBe("MetalCommand:payments:dev");
    }

    // ── GetClientName — environment source fallback ────────────────────────

    [Fact]
    public void GetClientName_NullEnvironment_FallsBackToEnvironmentSource()
    {
        var registry = RegistryWith(string.Empty, Entry("dev"), Entry("staging"), Entry("prod"));
        var source = Substitute.For<IEnvironmentSource>();
        source.DefaultEnvironment.Returns("staging");

        var resolver = Resolver(registry, [source]);

        resolver.GetClientName().ShouldBe("MetalCommand:staging");
    }

    [Fact]
    public void GetClientName_NullEnvironment_SourceEnvNotRegistered_FallsBackToRegistryDefault()
    {
        var registry = RegistryWith(string.Empty, Entry("dev", isDefault: true), Entry("prod"));
        var source = Substitute.For<IEnvironmentSource>();
        source.DefaultEnvironment.Returns("unknown-env");

        var resolver = Resolver(registry, [source]);

        // "unknown-env" is not a registered entry, so fall back to registry default.
        resolver.GetClientName().ShouldBe("MetalCommand:dev");
    }

    [Fact]
    public void GetClientName_EnvironmentSource_SkipsSelf()
    {
        var registry = RegistryWith(string.Empty, Entry("dev", isDefault: true));
        var resolver = Resolver(registry);
        // Pass resolver itself as a source — should not cause infinite recursion.
        var resolverWithSelf = ResolverWithFactory(registry, Substitute.For<IHttpClientFactory>(), [resolver]);

        resolverWithSelf.GetClientName().ShouldBe("MetalCommand:dev");
    }

    // ── GetClientName — unknown environment throws ─────────────────────────

    [Fact]
    public void GetClientName_UnknownExplicitEnvironment_Throws()
    {
        var registry = RegistryWith(string.Empty, Entry("dev"), Entry("prod"));
        var resolver = Resolver(registry);

        Should.Throw<InvalidOperationException>(() => resolver.GetClientName("staging"));
    }

    [Fact]
    public void GetClientName_UnknownGroup_Throws()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev")]);
        var resolver = Resolver(registry);

        Should.Throw<InvalidOperationException>(() => resolver.GetClientName(null, "payments"));
    }

    // ── GetClient ──────────────────────────────────────────────────────────

    [Fact]
    public void GetClient_ReturnsClientFromFactory()
    {
        var registry = RegistryWith(string.Empty, Entry("dev", isDefault: true));
        var factory = Substitute.For<IHttpClientFactory>();
        var expectedClient = new HttpClient();
        factory.CreateClient("MetalCommand:dev").Returns(expectedClient);

        var resolver = ResolverWithFactory(registry, factory);

        resolver.GetClient().ShouldBeSameAs(expectedClient);
    }

    // ── IEnvironmentSource ─────────────────────────────────────────────────

    [Fact]
    public void DefaultEnvironment_ReturnsRegistryDefault()
    {
        var registry = RegistryWith(string.Empty, Entry("dev"), Entry("prod", isDefault: true));
        var resolver = Resolver(registry);

        ((IEnvironmentSource)resolver).DefaultEnvironment.ShouldBe("prod");
    }

    [Fact]
    public void Environments_MapsEntriesToEnvironmentEntries()
    {
        var registry = RegistryWith(string.Empty,
            Entry("dev"), Entry("prod", isProtected: true));
        var resolver = Resolver(registry);

        var envs = ((IEnvironmentSource)resolver).Environments;

        envs.Length.ShouldBe(2);
        envs[0].Name.ShouldBe("dev");
        envs[0].IsProtected.ShouldBeFalse();
        envs[1].Name.ShouldBe("prod");
        envs[1].IsProtected.ShouldBeTrue();
    }
}
