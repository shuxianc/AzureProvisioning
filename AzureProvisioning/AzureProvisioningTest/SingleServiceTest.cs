using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AzureProvisioning;
using AzureProvisioning.ResourceSettings;
using AzureProvisioning.DAG;
using System.Collections.Generic;
using System.Threading;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using System.Net;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Management.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure.Management.StreamAnalytics;
using Microsoft.Azure.Management.StreamAnalytics.Models;

namespace AzureProvisioningTest
{
    [TestClass]
    public class SingleServiceTest
    {
        private static AzureTaskFactory factory = AzureTaskFactory.Instance;
        private static string testLocation = "Central US";
        private static TokenCloudCredentials cred = null;
        
        // Refer to the link to setup AAD for your subscription:
        // https://msdn.microsoft.com/en-us/library/azure/ee460782.aspx
        // Then replace below values with the information of your own subscription. 
        private const string ADTenantID = @"<your tenant ID>";
        private const string ADRedirectUri = @"<your redirect url>";
        private const string ADClientID = @"<your client id>";
        private const string SubscriptionID = @"<your subscription id>";

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            var ADToken = AcquireADToken();
            cred = new TokenCloudCredentials(SubscriptionID, ADToken);
        }

        [TestMethod]
        public void TestResourceGroupCreation()
        {
            // Generate a unique suffix for naming of Azure services
            Random rnd = new Random();
            int suffix = rnd.Next(1000, 10000);

            var rgst = new ResourceGroupSetting("rg" + suffix, LayerType.Default, testLocation);
            var noderg = new Vertex<ResourceSetting>(rgst);

            var allTask = factory.ResolveTasks(cred, new List<Vertex<ResourceSetting>>() { noderg });
            var r = allTask.ServiceCreated.GetAwaiter().GetResult().Result;
            Assert.IsTrue(r);

            using (var rmc = new ResourceManagementClient(cred))
            {
                var result = rmc.ResourceGroups.CheckExistence(rgst.Name);
                Assert.IsTrue(result.Exists == true);
            }

            r = CleanupHelper.CleanupResource(cred, rgst).Result;
            Assert.IsTrue(r);
        }

        [TestMethod]
        public void TestStorageCreation()
        {
            // Generate a unique suffix for naming of Azure services
            Random rnd = new Random();
            int suffix = rnd.Next(1000, 10000);

            var stost = new StorageAccountSetting("sto" + suffix, LayerType.RawArchive, testLocation);
            var bcst = new BlobContainerSetting("bc" + suffix, LayerType.RawArchive, testLocation);
            var nodesto = new Vertex<ResourceSetting>(stost);
            var nodebc = new Vertex<ResourceSetting>(bcst);
            nodesto.AddChild(nodebc);

            var allTask = factory.ResolveTasks(cred, new List<Vertex<ResourceSetting>>() { nodesto });
            var r = allTask.ServiceCreated.GetAwaiter().GetResult().Result;
            Assert.IsTrue(r);

            using (var smc = new StorageManagementClient(cred))
            {
                var result = smc.StorageAccounts.Get(stost.Name);
                Assert.IsTrue(result.StorageAccount.Properties.Status == StorageAccountStatus.Created); // Ensure storage account is created
            }
            var connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                        bcst.StorageAccountName, bcst.StorageAccountKey);
            var account = CloudStorageAccount.Parse(connectionString);
            var blobs = account.CreateCloudBlobClient();
            var container = blobs.GetContainerReference(bcst.Name);
            r = container.Exists();
            Assert.IsTrue(r); // Ensure blob container is created

            r = CleanupHelper.CleanupResource(cred, stost).Result;
            Assert.IsTrue(r);
        }

        [TestMethod]
        public void TestEventHubCreation()
        {
            // Generate a unique suffix for naming of Azure services
            Random rnd = new Random();
            int suffix = rnd.Next(1000, 10000);

            var sbst = new ServiceBusNamespaceSetting("sb" + suffix, LayerType.Ingestion, testLocation);
            var ehst = new EventHubSetting("eh" + suffix, LayerType.Ingestion, testLocation, 8);
            var nodesb = new Vertex<ResourceSetting>(sbst);
            var nodeeh = new Vertex<ResourceSetting>(ehst);
            nodesb.AddChild(nodeeh);

            var allTask = factory.ResolveTasks(cred, new List<Vertex<ResourceSetting>>() { nodesb });
            var r = allTask.ServiceCreated.GetAwaiter().GetResult().Result;
            Assert.IsTrue(r);

            using (var smc = new ServiceBusManagementClient(cred))
            {
                var result = smc.Namespaces.Get(sbst.Name).Namespace;
                Assert.IsTrue(result.Status == "Active"); // Ensure service bus namespace is active
            }
            var nm = NamespaceManager.CreateFromConnectionString(sbst.ConnectionString);
            var des = nm.GetEventHub(ehst.Name);
            Assert.IsTrue(des.Status == EntityStatus.Active); // Ensure event hub is active

            r = CleanupHelper.CleanupResource(cred, sbst).Result;
            Assert.IsTrue(r);
        }

        [TestMethod]
        public void TestStreamAnalyticsCreation()
        {
            // Generate a unique suffix for naming of Azure services
            Random rnd = new Random();
            int suffix = rnd.Next(1000, 10000);

            var sbst = new ServiceBusNamespaceSetting("sb" + suffix, LayerType.Default, testLocation);
            var ehst = new EventHubSetting("eh" + suffix, LayerType.Ingestion, testLocation, 8);
            var nodesb = new Vertex<ResourceSetting>(sbst);
            var nodeeh = new Vertex<ResourceSetting>(ehst);
            nodesb.AddChild(nodeeh);

            var stost = new StorageAccountSetting("sto" + suffix, LayerType.Default, testLocation);
            var bcst = new BlobContainerSetting("bc" + suffix, LayerType.RawArchive, testLocation);
            var nodesto = new Vertex<ResourceSetting>(stost);
            var nodebc = new Vertex<ResourceSetting>(bcst);
            nodesto.AddChild(nodebc);

            var rgst = new ResourceGroupSetting("rg" + suffix, LayerType.Default, testLocation);
            var sast = new StreamAnalyticsSetting("sa" + suffix, LayerType.Transformation, testLocation);
            var noderg = new Vertex<ResourceSetting>(rgst);
            var nodesa = new Vertex<ResourceSetting>(sast);
            noderg.AddChild(nodesa);

            nodeeh.AddChild(nodesa);
            nodebc.AddChild(nodesa);

            var allTask = factory.ResolveTasks(cred, new List<Vertex<ResourceSetting>>() { nodesb, nodesto, noderg });
            var r = allTask.ServiceCreated.GetAwaiter().GetResult().Result;
            Assert.IsTrue(r);

            using (var smc = new StreamAnalyticsManagementClient(cred))
            {
                var result = smc.StreamingJobs.Get(rgst.Name, sast.Name,
                    new JobGetParameters());
                Assert.IsTrue(result.Job.Properties.JobState == "Idle");
            }

            r = CleanupHelper.CleanupResource(cred, rgst).Result;
            Assert.IsTrue(r);

            r = CleanupHelper.CleanupResource(cred, stost).Result;
            Assert.IsTrue(r);

            r = CleanupHelper.CleanupResource(cred, sbst).Result;
            Assert.IsTrue(r);
        }


        /// <summary>
        /// helper function to get AD token
        /// </summary>
        /// <returns></returns>
        static string AcquireADToken()
        {   
            AuthenticationResult result = null;

            var context = new AuthenticationContext("https://login.windows.net/" + ADTenantID);

            var thread = new Thread(() =>
            {
                result = context.AcquireToken(
                  "https://management.core.windows.net/",
                  ADClientID,
                  new Uri(ADRedirectUri),
                  PromptBehavior.Auto);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AquireTokenThread";
            thread.Start();
            thread.Join();

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            string token = result.AccessToken;
            return token;
        }
    }
}
