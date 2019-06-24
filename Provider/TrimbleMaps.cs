using Microsoft.Crm.Sdk.Samples;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Configuration;
using System.Runtime.Serialization;

namespace Dynamics.FieldService.GeospatialPlugin.Providers
{
    public sealed class TrimbleMaps : Provider
    {
        private static System.Lazy<TrimbleMaps> instance = null;
        private static readonly object padlock = new object();

        private TrimbleMaps()
                : base(ConfigurationManager.AppSettings["trimblekey"],
                      "pcmiler.alk.com",
                      "/apis/rest/v1.0/Service.svc/locations",
                      "")
        { }

        public static TrimbleMaps Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new System.Lazy<TrimbleMaps>(() => new TrimbleMaps());
                    }
                    return instance.Value;
                }
            }
        }

        public override string ConstructUrl(ParameterCollection InputParameters, IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, ITracingService tracingService = null)
        {
            int Lcid = (int)InputParameters[LcidKey];
            string address = string.Empty;
            if (Lcid == 0)
            {
                var userSettingsQuery = new QueryExpression("usersettings");
                userSettingsQuery.ColumnSet.AddColumns("uilanguageid", "systemuserid");
                userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, pluginExecutionContext.InitiatingUserId);
                var userSettings = organizationService.RetrieveMultiple(userSettingsQuery);
                if (userSettings.Entities.Count > 0)
                    Lcid = (int)userSettings.Entities[0]["uilanguageid"];
            }

            address = GisUtility.FormatInternationalAddress(Lcid,
                (string)InputParameters[Address1Key],
                (string)InputParameters[PostalCodeKey],
                (string)InputParameters[CityKey],
                (string)InputParameters[StateKey],
                (string)InputParameters[CountryKey]);

            var street = ((string)InputParameters[Address1Key]).Replace(" ", "+");
            var city = ((string)InputParameters[CityKey]).Replace(" ", "+");
            var state = (string)InputParameters[StateKey];
            var postcode = (string)InputParameters[PostalCodeKey];
            tracingService.Trace("street " + street);
            tracingService.Trace("postcode " + postcode);
            tracingService.Trace("city " + city);
            tracingService.Trace("state " + state);

            var url = $"https://{ApiServer}{GeocodePath}?street={street}&city={city}&state={state}&postcode={postcode}&region=NA&dataset=Current";
            tracingService.Trace($"Calling {url}\n");

            return url;
        }

        public override void ExecuteGeocodeAddress(IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, string uri = null, ITracingService tracingService = null)
        {
            base.ExecuteGeocodeAddress(pluginExecutionContext, organizationService, ConstructUrl(), tracingService);
        }
    }

    [DataContract(Namespace = "")]
    public class TrimbleMapsGeocodeResponse
    {
        [DataMember(Name = "Address")]
        public CAddress Address { get; set; }
        [DataContract]
        public class CAddress
        {
            [DataMember(Name = "StreetAddress")]
            public string StreetAddress { get; set; }

            [DataMember(Name = "City")]
            public string City { get; set; }

            [DataMember(Name = "State")]
            public string State { get; set; }

            [DataMember(Name = "Zip")]
            public string Zip { get; set; }

            [DataMember(Name = "Country")]
            public string Country { get; set; }

            [DataMember(Name = "CountryAbbreviation")]
            public string CountryAbbreviation { get; set; }
        }
        [DataMember(Name = "Coords")]
        public CCoords Coords { get; set; }
        [DataContract]
        public class CCoords
        {
            [DataMember(Name = "Lat")]
            public string Lat { get; set; }
            [DataMember(Name = "Lon")]
            public string Lon { get; set; }
        }
        [DataMember(Name = "Label")]
        public string Label { get; set; }
        [DataMember(Name = "PlaceName")]
        public string PlaceName { get; set; }
        [DataMember(Name = "TimeZone")]
        public string TimeZone { get; set; }

    }
}
