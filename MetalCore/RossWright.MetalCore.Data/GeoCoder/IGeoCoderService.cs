namespace RossWright;

/// <summary>
/// Abstracts geographic coordinate lookup. Returns latitude/longitude for a given address or place search string.
/// </summary>
public interface IGeoCoderService
{
    /// <summary>
    /// Returns the <see cref="LatLong"/> coordinates for a given address or place search string.
    /// </summary>
    /// <param name="search">The address or place name to geocode.</param>
    /// <returns>The <see cref="LatLong"/> representing the best match for <paramref name="search"/>.</returns>
    LatLong GetCoordinates(string search);
}
