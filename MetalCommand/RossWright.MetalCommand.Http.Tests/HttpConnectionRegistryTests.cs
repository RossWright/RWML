using RossWright.MetalCommand.Http.Internal;

namespace RossWright.MetalCommand.Http.Tests;

public class HttpConnectionRegistryTests
{
    private static HttpConnectionEntry Entry(string env, bool isDefault = false, bool isProtected = false) =>
        new() { Environment = env, IsDefault = isDefault, IsProtected = isProtected, BaseAddress = $"http://{env}.example.com" };

    [Fact]
    public void Upsert_SingleGroup_StoresEntries()
    {
        var registry = new HttpConnectionRegistry();
        var entries = new[] { Entry("dev"), Entry("prod") };

        registry.Upsert(string.Empty, entries);

        registry.GetEntries(string.Empty).ShouldBe(entries);
    }

    [Fact]
    public void Upsert_MultipleGroups_BothAccessible()
    {
        var registry = new HttpConnectionRegistry();
        var defaultEntries = new[] { Entry("dev") };
        var paymentsEntries = new[] { Entry("prod") };

        registry.Upsert(string.Empty, defaultEntries);
        registry.Upsert("payments", paymentsEntries);

        registry.GetEntries(string.Empty).ShouldBe(defaultEntries);
        registry.GetEntries("payments").ShouldBe(paymentsEntries);
    }

    [Fact]
    public void Upsert_EmptyEntries_Throws()
    {
        var registry = new HttpConnectionRegistry();

        Should.Throw<ArgumentException>(() => registry.Upsert(string.Empty, []));
    }

    [Fact]
    public void DefaultEnvironment_ReturnsMarkedDefault()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev"), Entry("prod", isDefault: true)]);

        registry.DefaultEnvironment(string.Empty).ShouldBe("prod");
    }

    [Fact]
    public void DefaultEnvironment_NoExplicitDefault_ReturnsFirstEntry()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("local"), Entry("prod")]);

        registry.DefaultEnvironment(string.Empty).ShouldBe("local");
    }

    [Fact]
    public void DefaultEnvironment_NullGroupName_TreatedAsDefaultGroup()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev", isDefault: true)]);

        registry.DefaultEnvironment(null).ShouldBe("dev");
    }

    [Fact]
    public void GetEntries_UnknownGroup_Throws()
    {
        var registry = new HttpConnectionRegistry();

        Should.Throw<InvalidOperationException>(() => registry.GetEntries("unknown"));
    }

    [Fact]
    public void GetEntries_NullGroupName_ReturnsDefaultGroup()
    {
        var registry = new HttpConnectionRegistry();
        var entries = new[] { Entry("dev") };
        registry.Upsert(string.Empty, entries);

        registry.GetEntries(null).ShouldBe(entries);
    }

    [Fact]
    public void RegisteredBaseNames_ContainsEmptyStringForDefaultGroup()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev")]);

        registry.RegisteredBaseNames.ShouldContain(string.Empty);
    }

    [Fact]
    public void RegisteredBaseNames_ContainsNamedGroup()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert("payments", [Entry("prod")]);

        registry.RegisteredBaseNames.ShouldContain("payments");
    }

    [Fact]
    public void RegisteredBaseNames_DoesNotContainUnregisteredGroup()
    {
        var registry = new HttpConnectionRegistry();
        registry.Upsert(string.Empty, [Entry("dev")]);

        registry.RegisteredBaseNames.ShouldNotContain("payments");
    }
}
