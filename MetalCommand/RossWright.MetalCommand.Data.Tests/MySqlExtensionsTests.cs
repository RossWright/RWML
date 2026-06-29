using Microsoft.Extensions.Configuration;

namespace RossWright.MetalCommand.Data.Tests;

#if !NET10_0
public class MySqlExtensionsTests
{
    private sealed class SpyBuilder : IDatabaseContextFactoryBuilder
    {
        public IConfiguration Configuration { get; } = new ConfigurationBuilder().Build();

        public string? CapturedEnvironment { get; private set; }
        public Action<DbContextOptionsBuilder>? CapturedSetOptions { get; private set; }
        public bool CapturedIsDefault { get; private set; }
        public bool CapturedIsProtected { get; private set; }

        public void Add(string environment, Action<DbContextOptionsBuilder> opts,
            bool isDefault = false, bool isProtected = false)
        {
            CapturedEnvironment = environment;
            CapturedSetOptions = opts;
            CapturedIsDefault = isDefault;
            CapturedIsProtected = isProtected;
        }
    }

    [Fact]
    public void AddMySql_RegistersEnvironment_WithCorrectName()
    {
        var builder = new SpyBuilder();

        builder.AddMySql("dev", "server=localhost;database=test;user=root;password=pw");

        builder.CapturedEnvironment.ShouldBe("dev");
    }

    [Fact]
    public void AddMySql_SetOptions_DelegateIsNonNull()
    {
        var builder = new SpyBuilder();

        builder.AddMySql("dev", "server=localhost;database=test;user=root;password=pw");

        builder.CapturedSetOptions.ShouldNotBeNull();
    }

    [Fact]
    public void AddMySql_DefaultFlags_AreFalse()
    {
        var builder = new SpyBuilder();

        builder.AddMySql("dev", "server=localhost;database=test;user=root;password=pw");

        builder.CapturedIsDefault.ShouldBeFalse();
        builder.CapturedIsProtected.ShouldBeFalse();
    }

    [Fact]
    public void AddMySql_IsDefault_True_PropagatesFlag()
    {
        var builder = new SpyBuilder();

        builder.AddMySql("dev", "server=localhost;database=test;user=root;password=pw", isDefault: true);

        builder.CapturedIsDefault.ShouldBeTrue();
    }

    [Fact]
    public void AddMySql_IsProtected_True_PropagatesFlag()
    {
        var builder = new SpyBuilder();

        builder.AddMySql("prod", "server=localhost;database=test;user=root;password=pw", isProtected: true);

        builder.CapturedIsProtected.ShouldBeTrue();
    }
}
#endif
