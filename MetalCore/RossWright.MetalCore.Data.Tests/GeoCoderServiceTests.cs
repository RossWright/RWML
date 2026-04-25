using Microsoft.Extensions.DependencyInjection;

namespace RossWright.Data.Tests;

public class GeoCoderServiceTests
{
    private static IGeoCoderService CreateService()
    {
        var services = new ServiceCollection();
        services.AddGeoCoderService();
        return services.BuildServiceProvider().GetRequiredService<IGeoCoderService>();
    }

    [Fact]
    public void GetCoordinates_ValidUsZipCode_ReturnsCoordinates()
    {
        var service = CreateService();

        var result = service.GetCoordinates("60601");

        result.Lat.ShouldNotBe(0.0);
        result.Lng.ShouldNotBe(0.0);
    }

    [Fact]
    public void GetCoordinates_ValidCityState_ReturnsCoordinates()
    {
        var service = CreateService();

        var result = service.GetCoordinates("Chicago, IL");

        result.Lat.ShouldNotBe(0.0);
        result.Lng.ShouldNotBe(0.0);
    }

    [Fact]
    public void GetCoordinates_UnrecognizedInput_ThrowsMetalCoreException()
    {
        var service = CreateService();

        Should.Throw<MetalCoreException>(() => service.GetCoordinates("NotARealPlace_XYZ_12345"));
    }

    [Fact]
    public void GetCoordinates_EmbeddedResourceLoadsWithoutError()
    {
        // Verifies the embedded resource is accessible on first construction
        Should.NotThrow(CreateService);
    }

    [Fact]
    public void GetCoordinates_CityStateCaseInsensitive_ReturnsCoordinates()
    {
        var service = CreateService();

        var result = service.GetCoordinates("chicago, il");

        result.Lat.ShouldNotBe(0.0);
        result.Lng.ShouldNotBe(0.0);
    }
}
