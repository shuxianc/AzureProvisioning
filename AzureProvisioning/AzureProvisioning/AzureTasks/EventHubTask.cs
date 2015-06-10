using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Management.ServiceBus;
using AzureProvisioning.ResourceSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.AzureTasks
{
    public class EventHubTask : AzureTask
    {
        private ServiceBusNamespaceTask serviceBusNamespaceTask;

        /// <summary>
        /// Constructor for EventHub creation task
        /// </summary>
        /// <param name="cred">Credential used to create Azure services</param>
        /// <param name="deps">Tasks this task depends on</param>
        /// <param name="st">Settings for this Task</param>
        public EventHubTask(TokenCloudCredentials cred, List<AzureTask> deps, EventHubSetting st)
            : base(cred, deps, st)
        {
            dependenciesCNT = 1;

            int depsCnt = dependenciesCNT;
            foreach (AzureTask t in dependencies)
            {
                if (t is ServiceBusNamespaceTask)
                {
                    serviceBusNamespaceTask = t as ServiceBusNamespaceTask;
                    depsCnt--;
                }
            }

            if (depsCnt != 0)
            {
                throw new AzureProvisioningException("Failed to setup dependencies for EventHubTask");
            }
        }

        /// <summary>
        /// Create EventHub (all dependencies are supposed to be ready)
        /// </summary>
        /// <returns>Task for creation</returns>
        protected override async Task<bool> setupService()
        {
            bool succeeded = false;

            try
            {
                var sbsetting = serviceBusNamespaceTask.Setting as ServiceBusNamespaceSetting;
                var nm = NamespaceManager.CreateFromConnectionString(sbsetting.ConnectionString);
                var ed = await createEventHubAsync(Setting.Name, nm);

                if (ed.Status == EntityStatus.Active)
                {
                    succeeded = true;
                }
                return succeeded;
            }
            catch (Exception e)
            {
                throw new AzureProvisioningException("Exception when creating event hub.", e);
            }
        }

        private async Task<EventHubDescription> createEventHubAsync(string path, NamespaceManager nm)
        {
            var sendRule = new SharedAccessAuthorizationRule("send", SharedAccessAuthorizationRule.GenerateRandomKey(), new[] { AccessRights.Send });
            var listenRule = new SharedAccessAuthorizationRule("listen", SharedAccessAuthorizationRule.GenerateRandomKey(), new[] { AccessRights.Listen });
            var manageRule = new SharedAccessAuthorizationRule("manage", SharedAccessAuthorizationRule.GenerateRandomKey(), new[] { AccessRights.Manage, AccessRights.Send, AccessRights.Listen });
            var ed = new EventHubDescription(path);
            ed.Authorization.Add(sendRule);
            ed.Authorization.Add(listenRule);
            ed.Authorization.Add(manageRule);

            var ehsetting = Setting as EventHubSetting;
            var sbsetting = serviceBusNamespaceTask.Setting as ServiceBusNamespaceSetting;

            // Add neccessary properties for EventHub setting
            ehsetting.SendKeyName = sendRule.KeyName;
            ehsetting.SendKeyValue = String.Format("Endpoint={0};SharedAccessKeyName=send;SharedAccessKey={1}", sbsetting.Endpoint, sendRule.PrimaryKey);   // TODO: Unify format of sendkey and managekey
            ehsetting.ManageKeyName = manageRule.KeyName;
            ehsetting.ManageKeyValue = manageRule.PrimaryKey;
            ehsetting.ServiceBusName = sbsetting.Name;

            var des = await nm.CreateEventHubIfNotExistsAsync(ed);
            return des;
        }

    }
}
