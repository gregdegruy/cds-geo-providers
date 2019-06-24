using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace Dynamics.FieldService.GeospatialPlugin.Providers
{
    public class Provider : IProvider
    {
        public string ApiKey { get; }
        public string ApiServer { get; }
        public string GeocodePath { get; }
        public string DistanceMatrixPath { get; }
        protected const string PluginStatusCodeKey = "PluginStatus";
        protected const string Address1Key = "Line1";
        protected const string CityKey = "City";
        protected const string StateKey = "StateOrProvince";
        protected const string PostalCodeKey = "PostalCode";
        protected const string CountryKey = "Country";
        protected const string LatitudeKey = "Latitude";
        protected const string LongitudeKey = "Longitude";
        protected const string LcidKey = "Lcid";

        public Provider(string apiKey, string apiServer, string geocodePath, string distanceMatrixPath) {
            ApiKey = apiKey;
            ApiServer = apiServer;
            GeocodePath = geocodePath;
            DistanceMatrixPath = distanceMatrixPath;
        }
        
        public virtual string ConstructUrl(ParameterCollection InputParameters, IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, ITracingService tracingService = null)
        {
            return "";
        }

        public virtual void ExecuteGeocodeAddress(IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, ITracingService tracingService = null)
        {
            ParameterCollection InputParameters = pluginExecutionContext.InputParameters; // 5 fields (string) for individual parts of an address
            ParameterCollection OutputParameters = pluginExecutionContext.OutputParameters; // 2 fields (double) for resultant geolocation

            tracingService.Trace("ExecuteGeocodeAddress started. InputParameters = {0}, OutputParameters = {1}", InputParameters.Count().ToString(), OutputParameters.Count().ToString());

            try
            {
                // If a plugin earlier in the pipeline has already geocoded successfully, quit
                if ((double)OutputParameters[LatitudeKey] != 0d || (double)OutputParameters[LongitudeKey] != 0d) return;

                Uri requestUri = new Uri(uri);
                HttpWebRequest req = WebRequest.Create(requestUri) as HttpWebRequest;
                req.Headers["Authorization"] = "03F68EA06887B2428771784EFEB79DDD";
                req.ContentType = "application/json";

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                    {
                        tracingService.Trace("Parsing response ...\n");
                        tracingService.Trace(response.ToString() + '\n');
                        string txtResponse = "";
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            txtResponse = sr.ReadToEnd();
                        }
                        txtResponse = txtResponse.Remove(0, 1);
                        txtResponse = txtResponse.Remove(txtResponse.Length - 1, 1);
                        tracingService.Trace(txtResponse + '\n');
                        tracingService.Trace("about to serial\n");
                        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(TrimbleMapsGeocodeResponse));
                        tracingService.Trace("about to read object\n");
                        object objResponse = jsonSerializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(txtResponse)));
                        tracingService.Trace("about to ilist\n");
                        var geocodeResponseNotList = objResponse as TrimbleMapsGeocodeResponse;

                        tracingService.Trace("Checking geocodeResponse.Result...\n");

                        tracingService.Trace(geocodeResponseNotList.ToString() + "not list\n");
                        tracingService.Trace("hope i got it all geez\n");
                        tracingService.Trace("Checking geocodeResponse.Coords...\n");
                        if (geocodeResponseNotList?.Coords != null)
                        {
                            tracingService.Trace("Setting Latitude, Longitude in OutputParameters...\n");
                            OutputParameters[LatitudeKey] = Convert.ToDouble(geocodeResponseNotList.Coords.Lat);
                            OutputParameters[LongitudeKey] = Convert.ToDouble(geocodeResponseNotList.Coords.Lon);

                        }
                        else throw new ApplicationException($"Server {ApiServer} application error (missing Coords)");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("Response parse failed at {0} with exception -- {1}: {2}"
                        , ApiServer, ex.GetType().ToString(), ex.Message), ex);
                }
            }
            catch (Exception ex)
            {
                // Signal to subsequent plugins in this message pipeline that geocoding failed here.
                OutputParameters[LatitudeKey] = 0d;
                OutputParameters[LongitudeKey] = 0d;

                //TODO: You may need to decide which caught exceptions will rethrow and which ones will simply signal geocoding did not complete.
                throw new InvalidPluginExecutionException(string.Format("Geocoding failed at {0} with exception -- {1}: {2}"
                    , ApiServer, ex.GetType().ToString(), ex.Message), ex);
            }
        }
    }
}
