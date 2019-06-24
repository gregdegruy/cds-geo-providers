using Microsoft.Xrm.Sdk;

namespace Dynamics.FieldService.GeospatialPlugin.Providers
{
    interface IProvider
    {
        string ConstructUrl(ParameterCollection InputParameters, IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, ITracingService tracingService = null);
        void ExecuteGeocodeAddress(IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, ITracingService tracingService = null);
    }
}
