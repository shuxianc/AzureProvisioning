using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using AzureProvisioning.ResourceSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.AzureTasks
{
    public class BlobContainerTask : AzureTask
    {
        private StorageAccountTask storageAccountTask;

        public BlobContainerTask(TokenCloudCredentials cred, List<AzureTask> deps, BlobContainerSetting st)
            : base(cred, deps, st)
        {
            dependenciesCNT = 1;

            int depsCnt = dependenciesCNT;
            foreach (AzureTask t in dependencies)
            {
                if (t is StorageAccountTask)
                {
                    storageAccountTask = t as StorageAccountTask;
                    depsCnt--;
                }
            }

            if (depsCnt != 0)
            {
                throw new AzureProvisioningException("Failed to setup dependencies for BlobContainerTask");
            }
        }

        /// <summary>
        /// Create Blob Container (all dependencies are supposed to be ready)
        /// </summary>
        /// <returns>Task for creation</returns>
        protected override async Task<bool> setupService()
        {

            try
            {
                var bcsetting = Setting as BlobContainerSetting;
                var sasetting = storageAccountTask.Setting as StorageAccountSetting;
                bcsetting.StorageAccountName = storageAccountTask.Setting.Name;
                bcsetting.StorageAccountKey = sasetting.PrimaryKey;


                var connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                            bcsetting.StorageAccountName, bcsetting.StorageAccountKey);
                var account = CloudStorageAccount.Parse(connectionString);
                var blobs = account.CreateCloudBlobClient();
                var container = blobs.GetContainerReference(Setting.Name);
                await container.CreateAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new AzureProvisioningException("Exception when creating blob container.", e);
            }
        }
    }
}
