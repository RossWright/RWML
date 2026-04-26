using Microsoft.Extensions.Configuration;

namespace RossWright.MetalCommand.Data.Tests;

public class SqlServerExtensionsTests
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
    public void AddSqlServer_RegistersEnvironment_WithCorrectName()
    {
        var builder = new SpyBuilder();

        builder.AddSqlServer("dev", "Server=localhost;Database=test;Trusted_Connection=True");

        builder.CapturedEnvironment.ShouldBe("dev");
    }

    [Fact]
    public void AddSqlServer_SetOptions_DelegateIsNonNull()
    {
        var builder = new SpyBuilder();

        builder.AddSqlServer("dev", "Server=localhost;Database=test;Trusted_Connection=True");

        builder.CapturedSetOptions.ShouldNotBeNull();
    }

    [Fact]
    public void AddSqlServer_DefaultFlags_AreFalse()
    {
        var builder = new SpyBuilder();

        builder.AddSqlServer("dev", "Server=localhost;Database=test;Trusted_Connection=True");

        builder.CapturedIsDefault.ShouldBeFalse();
        builder.CapturedIsProtected.ShouldBeFalse();
    }

    [Fact]
    public void AddSqlServer_IsDefault_True_PropagatesFlag()
    {
        var builder = new SpyBuilder();

        builder.AddSqlServer("dev", "Server=localhost;Database=test;Trusted_Connection=True", isDefault: true);

        builder.CapturedIsDefault.ShouldBeTrue();
    }

    [Fact]
    public void AddSqlServer_IsProtected_True_PropagatesFlag()
    {
        var builder = new SpyBuilder();

        builder.AddSqlServer("prod", "Server=localhost;Database=test;Trusted_Connection=True", isProtected: true);

        builder.CapturedIsProtected.ShouldBeTrue();
    }
}
