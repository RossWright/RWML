using Geolocation;

namespace RossWright;

/// <summary>
/// A geographic coordinate pair representing a latitude and longitude.
/// </summary>
public struct LatLong
{
    /// <summary>The latitude in decimal degrees.</summary>
    public double Lat;
    /// <summary>The longitude in decimal degrees.</summary>
    public double Lng;

    /// <summary>
    /// Calculates the straight-line distance in miles from this coordinate to another point.
    /// </summary>
    /// <param name="lat">The target latitude in decimal degrees.</param>
    /// <param name="lng">The target longitude in decimal degrees.</param>
    /// <returns>The distance in miles, rounded to 2 decimal places.</returns>
    public double DistanceTo(double lat, double lng)
    {
        var earthRadius = 6371;
        var deltaLat = (Lat - lat).FromDegreesToRadians();
        var deltaLng = (Lng - lng).FromDegreesToRadians();
        var a = Math.Pow(Math.Sin(deltaLat / 2), 2) + Math.Cos(lat.FromDegreesToRadians()) * Math.Pow(Math.Sin(deltaLng / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (double)Math.Round((c * earthRadius * 0.621371), 2);            
    }
    
    /// <summary>
    /// Calculates the bounding box (min/max coordinates) for a circle of a given radius around this point.
    /// </summary>
    /// <param name="distance">The radius in miles.</param>
    /// <returns>A tuple of the minimum and maximum <see cref="LatLong"/> corners of the bounding box.</returns>
    public (LatLong min, LatLong max) CalcBound(double distance)
    {
        CoordinateBoundaries boundaries = new CoordinateBoundaries(Lat, Lng, distance);
        return (
            new LatLong { Lat = boundaries.MinLatitude, Lng = boundaries.MinLongitude},
            new LatLong { Lat = boundaries.MaxLatitude, Lng = boundaries.MaxLongitude});
    }
}
