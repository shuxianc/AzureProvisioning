using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.ServiceBus;
using AzureProvisioning.ResourceSettings;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.ServiceBus;

namespace AzureProvisioning.AzureTasks
{
    public class ServiceBusNamespaceTask : AzureTask
    {
        /// <summary>
        /// Constructor for service bus namespace creation task
        /// </summary>
        /// <param name="cred">Credential used to create Azure services</param>
        /// <param name="deps">Tasks this task depends on</param>
        /// <param name="st">Settings for this Task</param>
        public ServiceBusNamespaceTask(TokenCloudCredentials cred, List<AzureTask> deps, ServiceBusNamespaceSetting st) : base(cred, deps, st)
        {
            dependenciesCNT = 0;
        }

        /// <summary>
        /// Create service bus namespace
        /// </summary>
        /// <returns>Task for creation</returns>
        protected override async Task<bool> setupService()
        {
            bool succeeded = false;
            try
            {
                using (var sbmClient = new ServiceBusManagementClient(tokenCred))
                {
                    var response = await sbmClient.Namespaces.CreateAsync(Setting.Name, Setting.Location);
                    succeeded = response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;

                    if (succeeded)
                    {
                        var sbsetting = Setting as ServiceBusNamespaceSetting;
                        sbsetting.Endpoint = response.Namespace.ServiceBusEndpoint.ToString().Replace("https", "sb");
                        var result = (await sbmClient.Namespaces.GetAsync(Setting.Name)).Namespace;
                        while (result.Status != "Active")
                        {
                            await Task.Delay(10000);
                            result = (await sbmClient.Namespaces.GetAsync(Setting.Name)).Namespace;
                        }


                        var ns = (await sbmClient.Namespaces.GetAsync(Setting.Name)).Namespace;
                        if (ns != null)
                        {
                            var ndl = await sbmClient.Namespaces.GetNamespaceDescriptionAsync(Setting.Name);
                            var nd = ndl.First();
                            if (nd != null)
                            {
                                sbsetting.ConnectionString = nd.ConnectionString;
                            }
                        }
                    }
                }

                return succeeded;
            }
            catch (Exception e)
            {
                throw new AzureProvisioningException("Exception when creating service bus namespace.", e);
            }
        }
    }
}
