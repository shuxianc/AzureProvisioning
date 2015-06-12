using AzureProvisioning.ResourceSettings;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.WindowsAzure.Management.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning
{
    public class CleanupHelper
    {
        public static async Task<bool> CleanupResource(TokenCloudCredentials cred, ResourceSetting resource)
        {
            bool succeeded = false;

            if (resource is ResourceGroupSetting)
            {
                using (var rmc = new ResourceManagementClient(cred))
                {
                    var result = await rmc.ResourceGroups.DeleteAsync(resource.Name);
                    succeeded = result.StatusCode == HttpStatusCode.OK;
                }
            }
            else if (resource is StorageAccountSetting)
            {
                using (var smc = new StorageManagementClient(cred))
                {
                    var result = await smc.StorageAccounts.DeleteAsync(resource.Name);
                    succeeded = result.StatusCode == HttpStatusCode.OK;
                }
            }

            return succeeded;
        }
    }
}
