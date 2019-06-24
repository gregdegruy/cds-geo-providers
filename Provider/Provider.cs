namespace Dynamics.FieldService.GeospatialPlugin.Providers
{
    public class Provider
    {
        public string ApiKey { get; }
        public string ApiServer { get; }
        public string GeocodePath { get; }
        public string DistanceMatrixPath { get; }

        public Provider(string apiKey, string apiServer, string geocodePath, string distanceMatrixPath) {
            ApiKey = apiKey;
            ApiServer = apiServer;
            GeocodePath = geocodePath;
            DistanceMatrixPath = distanceMatrixPath;
        }
    }
}
