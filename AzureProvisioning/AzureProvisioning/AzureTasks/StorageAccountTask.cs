using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using AzureProvisioning.ResourceSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.AzureTasks
{
    public class StorageAccountTask : AzureTask
    {
        public StorageAccountTask(TokenCloudCredentials cred, List<AzureTask> deps, StorageAccountSetting st) : base(cred, deps, st)
        {
            dependenciesCNT = 0;
        }

        protected override async Task<bool> setupService()
        {
            try
            {
                using (var smc = new StorageManagementClient(tokenCred))
                {
                    var sasetting = Setting as StorageAccountSetting;
                    var result = await smc.StorageAccounts.CreateAsync(
                        new StorageAccountCreateParameters
                        {
                            Location = Setting.Location,
                            Name = Setting.Name,
                            Label = "StorageAccountTask",   // XXX: Label and AccountType are temporarily hard-coded here. Could be made as member of StorageAccountSetting class.
                            AccountType = "Standard_LRS"
                        });

                    if (result.StatusCode != HttpStatusCode.OK && result.StatusCode != HttpStatusCode.Created)
                    {
                        return false;
                    }

                    var keys = await smc.StorageAccounts.GetKeysAsync(Setting.Name);
                    sasetting.PrimaryKey = keys.PrimaryKey;
                }
                return true;
            }
            catch (Exception e)
            {
                throw new AzureProvisioningException("Exception when creating Azure storage account.", e);
            }
        }
    }
}
