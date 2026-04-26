using Microsoft.Extensions.Options;
using RossWright.MetalCommand.Http.Internal;

namespace RossWright.MetalCommand.Http.Tests;

public class EnvironmentAwareHttpClientFactoryTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static HttpConnectionEntry Entry(string env, bool isDefault = false) =>
        new()
        {
            Environment = env,
            IsDefault = isDefault,
            BaseAddress = $"http://{env}.example.com"
        };

    private static (EnvironmentAwareHttpClientFactory decorator, IHttpClientFactory real, HttpConnectionRegistry registry, IHttpConnectionResolver resolver)
        Build(HttpConnectionRegistry registry, string activeEnv)
    {
        var realFactory = Substitute.For<IHttpClientFactory>();
        realFactory.CreateClient(Arg.Any<string>()).Returns(ci => new HttpClient());

        var resolver = Substitute.For<IHttpConnectionResolver>();
        resolver.GetClientName(Arg.Any<string?>(), Arg.Any<string?>())
                .Returns(ci =>
                {
                    var baseConn = ci.ArgAt<string?>(1);
                    return string.IsNullOrEmpty(baseConn)
                        ? $"MetalCommand:{activeEnv}"
                        : $"MetalCommand:{baseConn}:{activeEnv}";
                });

        var decorator = new EnvironmentAwareHttpClientFactory(realFactory, registry, resolver);
        return (decorator, realFactory, registry, resolver);
    }

    // ── Null / empty name → default group ─────────────────────────────────

    [Fact]
    public void CreateClient_EmptyName_MapsToDefaultGroupEnvQualified()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev", isDefault: true)]);

        var (decorator, real, _, _) = Build(registry, "dev");

        decorator.CreateClient(string.Empty);

        real.Received(1).CreateClient("MetalCommand:dev");
    }

    [Fact]
    public void CreateClient_NullName_MapsToDefaultGroupEnvQualified()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev", isDefault: true)]);

        var (decorator, real, _, _) = Build(registry, "dev");

        decorator.CreateClient(null!);

        real.Received(1).CreateClient("MetalCommand:dev");
    }

    [Fact]
    public void CreateClient_OptionsDefaultName_MapsToDefaultGroup()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("prod", isDefault: true)]);

        var (decorator, real, _, _) = Build(registry, "prod");

        decorator.CreateClient(Options.DefaultName);

        real.Received(1).CreateClient("MetalCommand:prod");
    }

    // ── Registered base name → named group ────────────────────────────────

    [Fact]
    public void CreateClient_RegisteredBaseName_MapsToQualifiedKey()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert("payments", [Entry("staging", isDefault: true)]);

        var (decorator, real, _, _) = Build(registry, "staging");

        decorator.CreateClient("payments");

        real.Received(1).CreateClient("MetalCommand:payments:staging");
    }

    // ── Unrecognised / already-qualified name → passthrough ────────────────

    [Fact]
    public void CreateClient_UnrecognisedName_PassesThrough()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev")]);

        var (decorator, real, _, _) = Build(registry, "dev");

        decorator.CreateClient("SomeThirdPartyClient");

        real.Received(1).CreateClient("SomeThirdPartyClient");
    }

    [Fact]
    public void CreateClient_FullyQualifiedKey_PassesThrough()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("prod")]);

        var (decorator, real, _, _) = Build(registry, "prod");

        decorator.CreateClient("MetalCommand:prod");

        real.Received(1).CreateClient("MetalCommand:prod");
    }

    [Fact]
    public void CreateClient_UnrecognisedName_DoesNotInvokeResolver()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev")]);

        var (decorator, _, _, resolver) = Build(registry, "dev");

        decorator.CreateClient("SomeThirdPartyClient");

        resolver.DidNotReceive().GetClientName(Arg.Any<string?>(), Arg.Any<string?>());
    }
}
