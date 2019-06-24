using System.Collections.Generic;
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
