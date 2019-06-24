using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomPlugin_FS_Geospatial
{
    class StepRegistration
    {
        enum CrmPluginStepDeployment
        {
           ServerOnly = 0,
           OfflineOnly = 1,
           Both = 2
        }

        enum CrmPluginStepMode
        {
            Asynchronous = 1,
            Synchronous = 0
        }

        enum CrmPluginStepStage
        {
            PreValidation = 10,
            PreOperation = 20,
            PostOperation = 40,
        }

        enum SdkMessageName
        {
            Create,
            Update,
            Delete,
            Retrieve,
            Assign,
            GrantAccess,
            ModifyAccess,
            RetrieveMultiple,
            RetrievePrincipalAccess,
            RetrieveSharedPrincipalsAndAccess,
            RevokeAccess,
            SetState,
            SetStateDynamicEntity,
        }
    }
}
