using Microsoft.Crm.Sdk.Samples;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Runtime.Serialization;

namespace Dynamics.FieldService.GeospatialPlugin.Providers
{
    public sealed class GoogleMaps : Provider
    {
        private static System.Lazy<GoogleMaps> instance = null;
        private static readonly object padlock = new object();

        private GoogleMaps() 
                : base(ConfigurationManager.AppSettings["googlekey"], 
                      "maps.googleapis.com", 
                      "/maps/api/geocode", 
                      "") { }        

        public static GoogleMaps Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new System.Lazy<GoogleMaps>(() => new GoogleMaps());
                    }
                    return instance.Value;
                }
            }
        }

        public override string ConstructUrl(Microsoft.Xrm.Sdk.ParameterCollection InputParameters, IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, ITracingService tracingService = null)
        {
            int Lcid = (int)InputParameters[LcidKey];
            string _address = string.Empty;
            if (Lcid == 0)
            {
                var userSettingsQuery = new QueryExpression("usersettings");
                userSettingsQuery.ColumnSet.AddColumns("uilanguageid", "systemuserid");
                userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, pluginExecutionContext.InitiatingUserId);
                var userSettings = organizationService.RetrieveMultiple(userSettingsQuery);
                if (userSettings.Entities.Count > 0)
                    Lcid = (int)userSettings.Entities[0]["uilanguageid"];
            }

            // Arrange the address components in a single comma-separated string, according to LCID
            _address = GisUtility.FormatInternationalAddress(Lcid,
                (string)InputParameters[Address1Key],
                (string)InputParameters[PostalCodeKey],
                (string)InputParameters[CityKey],
                (string)InputParameters[StateKey],
                (string)InputParameters[CountryKey]);

            WebClient client = new WebClient();
            var url = $"https://{ApiServer}{GeocodePath}/json?address={_address}&key={ApiKey}";
            tracingService.Trace($"Calling {url}\n");

            return url;
        }
    }    

    [DataContract(Namespace = "")]
    public class GoogleMapsGeocodeResponse
    {
        [DataMember(Name = "status", Order = 1)]
        public string Status { get; set; }

        [DataMember(Name = "error_message")]
        public string ErrorMessage { get; set; }

        [DataMember(Name = "results", Order = 2)]
        public IList<CResult> Results { get; set; }

        [DataContract]
        public class CResult
        {
            [DataMember(Name = "geometry")]
            public CGeometry Geometry { get; set; }

            [DataContract]
            public class CGeometry
            {
                [DataMember(Name = "location")]
                public CLocation Location { get; set; }

                [DataContract]
                public class CLocation
                {
                    [DataMember(Name = "lat", Order = 1)]
                    public double Lat { get; set; }
                    [DataMember(Name = "lng", Order = 2)]
                    public double Lng { get; set; }
                }

                [DataMember(Name = "location_type")]
                public string LocationType { get; set; }

            }
        }
    }

    [DataContract(Namespace = "")]
    public class GoogleMapsDistanceMatrixResponse
    {
        [DataMember(Name = "destination_addresses")]
        public IList<string> DestinationAddresses { get; set; }

        [DataMember(Name = "error_message")]
        public string ErrorMessage { get; set; }

        [DataMember(Name = "origin_addresses")]
        public IList<string> OriginAddresses { get; set; }

        [DataMember(Name = "rows")]
        public IList<CResult> Rows { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataContract]
        public class CResult
        {
            [DataMember(Name = "elements")]
            public IList<CElement> Columns { get; set; }

            [DataContract]
            public class CElement
            {
                [DataMember(Name = "status")]
                public string Status { get; set; }

                [DataMember(Name = "duration")]
                public CProperty Duration { get; set; }

                [DataMember(Name = "distance")]
                public CProperty Distance { get; set; }

                [DataMember(Name = "duration_in_traffic")]
                public CProperty DurationInTraffic { get; set; }

                [DataContract]
                public class CProperty
                {
                    [DataMember(Name = "text", Order = 1)]
                    public string Text { get; set; }
                    [DataMember(Name = "value", Order = 2)]
                    public double Value { get; set; }
                }
            }
        }
    }

}
