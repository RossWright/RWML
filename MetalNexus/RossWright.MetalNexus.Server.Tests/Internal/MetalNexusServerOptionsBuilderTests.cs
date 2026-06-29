using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Tests;

public class MetalNexusServerOptionsBuilderTests
{
    [Fact]
    public void SetMultipartBodyLengthLimit_WithValue_SetsLimit()
    {
        // Arrange
        var builder = new MetalNexusServerOptionsBuilder();
        const long expectedLimit = 1024 * 1024 * 10; // 10 MB

        // Act
        builder.SetMultipartBodyLengthLimit(expectedLimit);

        // Assert
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        builder.InitializeServer(services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        var formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;
        formOptions.MultipartBodyLengthLimit.ShouldBe(expectedLimit);
    }

    [Fact]
    public void SetMultipartBodyLengthLimit_WithNull_SetsMaxValue()
    {
        // Arrange
        var builder = new MetalNexusServerOptionsBuilder();

        // Act
        builder.SetMultipartBodyLengthLimit(null);

        // Assert
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        builder.InitializeServer(services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        var formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;
        formOptions.MultipartBodyLengthLimit.ShouldBe(long.MaxValue);
    }

    [Fact]
    public void SetMultipartBodyLengthLimit_WithZero_SetsZero()
    {
        // Arrange
        var builder = new MetalNexusServerOptionsBuilder();

        // Act
        builder.SetMultipartBodyLengthLimit(0);

        // Assert
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        builder.InitializeServer(services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        var formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;
        formOptions.MultipartBodyLengthLimit.ShouldBe(0);
    }

    [Fact]
    public void InitializeServer_ConfiguresFormOptions()
    {
        // Arrange
        var builder = new MetalNexusServerOptionsBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        const long expectedLimit = 5000000;
        builder.SetMultipartBodyLengthLimit(expectedLimit);

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;
        formOptions.MultipartBodyLengthLimit.ShouldBe(expectedLimit);
    }

    [Fact]
    public void InitializeServer_WithDefaultLimit_UsesMaxValue()
    {
        // Arrange
        var builder = new MetalNexusServerOptionsBuilder();
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        // Act
        builder.InitializeServer(services, configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var formOptions = serviceProvider.GetRequiredService<IOptions<FormOptions>>().Value;
        formOptions.MultipartBodyLengthLimit.ShouldBe(long.MaxValue);
    }
}
