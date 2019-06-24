// ===================================================================== 
//  This file is part of the Microsoft Dynamics 365 Customer Engagement
//  SDK Code Samples. 
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved. 
// 
//  This source code is intended only as a supplement to Microsoft 
//  Development Tools and/or on-line documentation.  See these other 
//  materials for detailed information regarding Microsoft code samples. 
// 
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//  PARTICULAR PURPOSE. 
// =====================================================================

using Dynamics.FieldService.GeospatialPlugin.Providers;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Crm.Sdk.Samples
{
    public class msdyn_GeocodeAddress : IPlugin
    {
        GoogleMaps googleMaps = GoogleMaps.Instance;
        TrimbleMaps trimbleMaps = TrimbleMaps.Instance;

        const string PluginStatusCodeKey = "PluginStatus";
        const string Address1Key = "Line1";
        const string CityKey = "City";
        const string StateKey = "StateOrProvince";
        const string PostalCodeKey = "PostalCode";
        const string CountryKey = "Country";
        const string LatitudeKey = "Latitude";
        const string LongitudeKey = "Longitude";
        const string LcidKey = "Lcid";

        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException("serviceProvider");
            }

            ITracingService tracingService = tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("Executing msdyn_GeocodeAddress");

            IPluginExecutionContext PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService OrganizationService = factory.CreateOrganizationService(PluginExecutionContext.UserId);
            ITracingService TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // ExecuteGeocodeAddressGoogle(PluginExecutionContext, OrganizationService, TracingService);
            ExecuteGeocodeAddressTrimble(PluginExecutionContext, OrganizationService, TracingService);
        }


        /// <summary>
        /// Retrieve geocode address using Google Api
        /// </summary>
        public void ExecuteGeocodeAddressGoogle(IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService,  ITracingService tracingService = null)
        {            
            ParameterCollection InputParameters = pluginExecutionContext.InputParameters; // 5 fields (string) for individual parts of an address
            ParameterCollection OutputParameters = pluginExecutionContext.OutputParameters; // 2 fields (double) for resultant geolocation
            ParameterCollection SharedVariables = pluginExecutionContext.SharedVariables; // 1 field (int) for status of previous and this plugin

            tracingService.Trace("ExecuteGeocodeAddress started. InputParameters = {0}, OutputParameters = {1}", InputParameters.Count().ToString(), OutputParameters.Count().ToString());

            try
            {
                // If a plugin earlier in the pipeline has already geocoded successfully, quit
                if ((double)OutputParameters[LatitudeKey] != 0d || (double)OutputParameters[LongitudeKey] != 0d) return;

                // Get user Lcid if request did not include it
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
                var url = $"https://{googleMaps.ApiServer}{googleMaps.GeocodePath}/json?address={_address}&key={googleMaps.ApiKey}";
                tracingService.Trace($"Calling {url}\n");
                string response = client.DownloadString(url);

                tracingService.Trace("Parsing response ...\n");
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GoogleMapsGeocodeResponse));
                object objResponse = jsonSerializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(response)));
                GoogleMapsGeocodeResponse geocodeResponse = objResponse as GoogleMapsGeocodeResponse;

                tracingService.Trace("Response Status = " + geocodeResponse.Status + "\n");
                if (geocodeResponse.Status != "OK")
                    throw new ApplicationException($"Server {googleMaps.ApiServer} application error (Status {geocodeResponse.Status}).");

                tracingService.Trace("Checking geocodeResponse.Result...\n");
                if (geocodeResponse.Results != null)
                {
                    if (geocodeResponse.Results.Count() == 1)
                    {
                        tracingService.Trace("Checking geocodeResponse.Result.Geometry.Location...\n");
                        if (geocodeResponse.Results.First()?.Geometry?.Location != null)
                        {
                            tracingService.Trace("Setting Latitude, Longitude in OutputParameters...\n");

                            // update output parameters
                            OutputParameters[LatitudeKey] = geocodeResponse.Results.First().Geometry.Location.Lat;
                            OutputParameters[LongitudeKey] = geocodeResponse.Results.First().Geometry.Location.Lng;

                        }
                        else throw new ApplicationException($"Server {googleMaps.ApiServer} application error (missing Results[0].Geometry.Location)");
                    }
                    else throw new ApplicationException($"Server {googleMaps.ApiServer} application error (more than 1 result returned)");
                }
                else throw new ApplicationException($"Server {googleMaps.ApiServer} application error (missing Results)");
            }
            catch (Exception ex)
            {
                // Signal to subsequent plugins in this message pipeline that geocoding failed here.
                OutputParameters[LatitudeKey] = 0d;
                OutputParameters[LongitudeKey] = 0d;

                //TODO: You may need to decide which caught exceptions will rethrow and which ones will simply signal geocoding did not complete.
                throw new InvalidPluginExecutionException(string.Format("Geocoding failed at {0} with exception -- {1}: {2}"
                    , googleMaps.ApiServer, ex.GetType().ToString(), ex.Message), ex);
            }

        }

        public void ExecuteGeocodeAddressTrimble(IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, ITracingService tracingService = null)
        {
            // 5 fields (string) for individual parts of an address
            ParameterCollection InputParameters = pluginExecutionContext.InputParameters;
            // 2 fields (double) for resultant geolocation
            ParameterCollection OutputParameters = pluginExecutionContext.OutputParameters;

            tracingService.Trace("ExecuteGeocodeAddress Trimble started. InputParameters = {0}, OutputParameters = {1}", InputParameters.Count().ToString(), OutputParameters.Count().ToString());

            try
            {
                // If a plugin earlier in the pipeline has already geocoded successfully, quit
                if ((double)OutputParameters[LatitudeKey] != 0d || (double)OutputParameters[LongitudeKey] != 0d) return;

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

                _address = GisUtility.FormatInternationalAddress(Lcid,
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

                var url = $"https://{trimbleMaps.ApiServer}{trimbleMaps.GeocodePath}?street={street}&city={city}&state={state}&postcode={postcode}&region=NA&dataset=Current";
                tracingService.Trace($"Calling {url}\n");

                Uri requestUri = new Uri(url);
                HttpWebRequest req = WebRequest.Create(requestUri) as HttpWebRequest;
                req.Headers["Authorization"] = "03F68EA06887B2428771784EFEB79DDD";
                req.ContentType = "application/json";

                //WebClient client = new WebClient();
                //client.Headers["Authorization"] = trimbleMaps.ApiKey;
                //string response = client.DownloadString(url);

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                    {
                        //string txtResponse = "";
                        //using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        //{
                        //    string txtResponse.Text = sr.ReadToEnd();
                        //}
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
                        //if (geocodeResponse.First() != null)
                        //{
                        //if (geocodeResponse.Count() == 1)
                            //{
                                tracingService.Trace("Checking geocodeResponse.Coords...\n");
                                if (geocodeResponseNotList?.Coords != null)
                                {
                                    tracingService.Trace("Setting Latitude, Longitude in OutputParameters...\n");
                                    OutputParameters[LatitudeKey] = Convert.ToDouble(geocodeResponseNotList.Coords.Lat);
                                    OutputParameters[LongitudeKey] = Convert.ToDouble(geocodeResponseNotList.Coords.Lon);

                                }
                                else throw new ApplicationException($"Server {trimbleMaps.ApiServer} application error (missing Coords)");
                            //}
                          //  else throw new ApplicationException($"Server {trimbleMaps.ApiServer} application error (more than 1 result returned)");
                        //}
                        //else throw new ApplicationException($"Server {trimbleMaps.ApiServer} application error (missing response body)");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("Response parse failed at {0} with exception -- {1}: {2}"
                        , trimbleMaps.ApiServer, ex.GetType().ToString(), ex.Message), ex);
                }
            }
            catch (Exception ex)
            {
                // Signal to subsequent plugins in this message pipeline that geocoding failed here.
                OutputParameters[LatitudeKey] = 0d;
                OutputParameters[LongitudeKey] = 0d;

                //TODO: You may need to decide which caught exceptions will rethrow and which ones will simply signal geocoding did not complete.
                throw new InvalidPluginExecutionException(string.Format("Geocoding failed at {0} with exception -- {1}: {2}"
                    , trimbleMaps.ApiServer, ex.GetType().ToString(), ex.Message), ex);
            }

        }
    }
}
