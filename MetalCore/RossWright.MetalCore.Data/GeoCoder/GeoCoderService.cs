namespace RossWright;

internal class GeoCoderService : IGeoCoderService
{
    public LatLong GetCoordinates(string search)
    {
        if (_postalCodeCoordinates.TryGetValue(search, out var result)) return result;
        var cityState = search.Split(',');
        if (cityState.Length == 2 && _stateAndCityCoordinates.TryGetValue((cityState[1].Trim().ToUpper(), cityState[0].Trim().ToUpper()), out result)) return result;
        throw new MetalCoreException($"Unknown Location: {search}");
    }

    class ZipCodeRow
    {
        public ZipCodeRow(string[] fromZipFile)
        {
            PostalCode = fromZipFile[0];
            StateCity = (fromZipFile[2].ToUpper(), fromZipFile[1].ToUpper());
            LatLong = new LatLong
            {
                Lat = float.Parse(fromZipFile[3]),
                Lng = float.Parse(fromZipFile[4])
            };
        }
        public string PostalCode { get; private set; }
        public (string, string) StateCity { get; private set; }
        public LatLong LatLong { get; private set; }
    }

    public GeoCoderService()
    {
        string zipText;
        var assembly = this.GetType().Assembly;
        var resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith("zipcodelatlongs.txt", StringComparison.CurrentCultureIgnoreCase));
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new InvalidOperationException("Could not load zipcodelatlongs.");
            }
            using (var reader = new StreamReader(stream))
            {
                zipText = reader.ReadToEnd();
            }
        }

        var zipRows = zipText.Split('\n');
        var zipCodes = zipRows
            .Where(row => !string.IsNullOrWhiteSpace(row))
            .Select(row => new ZipCodeRow(row.Split('\t')))
            .ToList();
        _postalCodeCoordinates = zipCodes
            .DistinctBy(_ => _.PostalCode)
            .ToDictionary(_ => _.PostalCode, _ => _.LatLong);
        _stateAndCityCoordinates = zipCodes
            .DistinctBy(_ => _.StateCity)
            .ToDictionary(_ => _.StateCity, _ => _.LatLong);
    }
    private readonly Dictionary<string, LatLong> _postalCodeCoordinates;
    private readonly Dictionary<(string, string), LatLong> _stateAndCityCoordinates;
}
