using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AzureProvisioning.ResourceSettings;

namespace AzureProvisioning.AzureTasks
{
    public class ResourceGroupTask : AzureTask
    {
        /// <summary>
        /// Constructor for ResourceGroup creation task
        /// </summary>
        /// <param name="cred">Credential used to create Azure services</param>
        /// <param name="deps">Tasks this task depends on</param>
        /// <param name="st">Settings for this Task</param>
        public ResourceGroupTask(TokenCloudCredentials cred, List<AzureTask> deps, ResourceGroupSetting st)
            : base(cred, deps, st)
        {
            dependenciesCNT = 0;
        }
        /// <summary>
        /// Create Resource Group
        /// </summary>
        /// <returns>Task for creation</returns>
        protected override async Task<bool> setupService()
        {
            bool succeeded = false;
            try
            {
                using (var resourceManagementClient = new ResourceManagementClient(tokenCred))
                {
                    var groupResult = await resourceManagementClient.ResourceGroups.CreateOrUpdateAsync(Setting.Name, new ResourceGroup { Location = Setting.Location });
                    succeeded = groupResult.StatusCode == HttpStatusCode.OK || groupResult.StatusCode == HttpStatusCode.Created;
                }

                return succeeded;
            }
            catch (Exception e)
            {
                throw new AzureProvisioningException("Exception when creating resource group.", e);
            }
        }
    }
}
