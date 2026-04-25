namespace RossWright.Data.Tests;

public class LatLongTests
{
    [Fact]
    public void DistanceTo_IdenticalCoordinates_ReturnsZero()
    {
        var point = new LatLong { Lat = 41.8781, Lng = -87.6298 };

        var result = point.DistanceTo(41.8781, -87.6298);

        result.ShouldBe(0.0);
    }

    [Fact]
    public void DistanceTo_KnownCoordinates_ReturnsApproximateDistance()
    {
        // Chicago to New York — approximately 790 miles (straight-line great-circle distance)
        var chicago = new LatLong { Lat = 41.8781, Lng = -87.6298 };

        var result = chicago.DistanceTo(40.7128, -74.0060);

        result.ShouldBeInRange(780.0, 840.0);
    }

    [Fact]
    public void DistanceTo_ReturnsValueRoundedToTwoDecimalPlaces()
    {
        var point = new LatLong { Lat = 34.0522, Lng = -118.2437 };

        var result = point.DistanceTo(37.7749, -122.4194);

        Math.Round(result, 2).ShouldBe(result);
    }

    [Fact]
    public void CalcBound_ZeroRadius_ReturnsTupleWithMinMaxAtSamePoint()
    {
        var point = new LatLong { Lat = 41.8781, Lng = -87.6298 };

        var (min, max) = point.CalcBound(0);

        min.Lat.ShouldBe(point.Lat, 0.0001);
        max.Lat.ShouldBe(point.Lat, 0.0001);
        min.Lng.ShouldBe(point.Lng, 0.0001);
        max.Lng.ShouldBe(point.Lng, 0.0001);
    }

    [Fact]
    public void CalcBound_PositiveRadius_ReturnsMinLessThanMax()
    {
        var point = new LatLong { Lat = 41.8781, Lng = -87.6298 };

        var (min, max) = point.CalcBound(50);

        (min.Lat < max.Lat).ShouldBeTrue();
        (min.Lng < max.Lng).ShouldBeTrue();
    }

    [Fact]
    public void CalcBound_ReturnsTupleWithLatLongMinMaxStructure()
    {
        var point = new LatLong { Lat = 41.8781, Lng = -87.6298 };

        var result = point.CalcBound(25);

        result.ShouldBeOfType<(LatLong min, LatLong max)>();
    }
}
