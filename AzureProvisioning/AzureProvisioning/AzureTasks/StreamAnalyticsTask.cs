using Microsoft.Azure;
using Microsoft.Azure.Management.StreamAnalytics;
using Microsoft.Azure.Management.StreamAnalytics.Models;
using AzureProvisioning.ResourceSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.AzureTasks
{
    public class StreamAnalyticsTask : AzureTask
    {
        private AzureTask inputTask;
        private AzureTask outputTask;
        private ResourceGroupTask resourceGroupTask;
        private const string baseUri = @"https://management.azure.com/";

        public StreamAnalyticsTask(TokenCloudCredentials cred, List<AzureTask> deps, StreamAnalyticsSetting st)
            : base(cred, deps, st)
        {
            dependenciesCNT = 3;
            var sasetting = Setting as StreamAnalyticsSetting;

            if (deps.Count() != dependenciesCNT)
            {
                throw new AzureProvisioningException("Failed to setup dependencies for StreamAnalyticsTask");
            }

            // Sort dependencies by LayerNumber
            deps.Sort((t1, t2) => t1.Setting.Layer.CompareTo(t2.Setting.Layer));
            resourceGroupTask = deps[0] as ResourceGroupTask;
            inputTask = deps[1];
            outputTask = deps[2];
        }

        /// <summary>
        /// Create StreamAnalytics (all dependencies are supposed to be ready)
        /// </summary>
        /// <returns>Task for creation</returns>
        protected override async Task<bool> setupService()
        {
            try
            {
                var rmUri = new Uri(baseUri);
                using (var smc = new StreamAnalyticsManagementClient(tokenCred, rmUri))
                {
                    bool succeeded = false;

                    var response = await smc.StreamingJobs.CreateOrUpdateAsync(
                        resourceGroupTask.Setting.Name,
                        new JobCreateOrUpdateParameters(
                            new Job
                            {
                                Name = Setting.Name,
                                Location = Setting.Location,
                                Properties = new JobProperties
                                {
                                    Sku = new Sku { Name = "standard" },
                                    EventsOutOfOrderPolicy = "drop",
                                    OutputStartMode = "JobStartTime"
                                }
                            }
                            ));
                    succeeded = (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created);
                    if (!succeeded)
                    {
                        return false;
                    }

                    // Create SA input
                    string inputName = Setting.Name + "input";
                    if (inputTask is EventHubTask)
                    {
                        var ehsetting = inputTask.Setting as EventHubSetting;
                        var responseInput = await smc.Inputs.CreateOrUpdateAsync(
                            resourceGroupTask.Setting.Name,
                            Setting.Name,
                            new InputCreateOrUpdateParameters
                            {
                                Input = new Input(inputName)
                                {
                                    Properties = new StreamInputProperties
                                    {
                                        Type = "stream",
                                        Serialization = new JsonSerialization
                                        {
                                            Type = "Json",
                                            Properties = new JsonSerializationProperties
                                            {
                                                Encoding = "UTF8"
                                            }
                                        },
                                        DataSource = new EventHubStreamInputDataSource
                                        {
                                            Properties = new EventHubStreamInputDataSourceProperties
                                            {
                                                EventHubName = ehsetting.Name,
                                                ServiceBusNamespace = ehsetting.ServiceBusName,
                                                SharedAccessPolicyName = ehsetting.ManageKeyName,
                                                SharedAccessPolicyKey = ehsetting.ManageKeyValue
                                            }
                                        }
                                    }
                                }
                            });

                        succeeded = responseInput.StatusCode == HttpStatusCode.OK || responseInput.StatusCode == HttpStatusCode.Created;

                        if (!succeeded)
                        {
                            return false;
                        }
                    }
                    else if (inputTask is BlobContainerTask)
                    {
                        // TODO
                    }

                    // Create SA output
                    string outputName = Setting.Name + "output";
                    if (outputTask is BlobContainerTask)
                    {
                        var bcsetting = outputTask.Setting as BlobContainerSetting;
                        var responseOutput = await smc.Outputs.CreateOrUpdateAsync(resourceGroupTask.Setting.Name, Setting.Name,
                            new OutputCreateOrUpdateParameters
                            {
                                Output = new Output(outputName)
                                {
                                    Properties = new OutputProperties
                                    {
                                        Serialization = new CsvSerialization
                                        {
                                            Type = "CSV",
                                            Properties = new CsvSerializationProperties
                                            {
                                                FieldDelimiter = ",",
                                                Encoding = "UTF8"
                                            }
                                        },
                                        DataSource = new BlobOutputDataSource
                                        {
                                            Properties = new BlobOutputDataSourceProperties
                                            {
                                                BlobPathPrefix = "sa/",
                                                Container = bcsetting.Name,
                                                StorageAccounts = new List<StorageAccount>
                                                {
                                                    new StorageAccount
                                                    {
                                                        AccountName = bcsetting.StorageAccountName,
                                                        AccountKey = bcsetting.StorageAccountKey
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                            });
                        succeeded = responseOutput.StatusCode == HttpStatusCode.OK || responseOutput.StatusCode == HttpStatusCode.Created;
                        if (!succeeded)
                        {
                            return false;
                        }
                    }
                    else if (outputTask is EventHubTask)
                    {
                        // TODO
                    }

                    // Create SA Transformation
                    string query = "select * from " + inputName;    // TODO: this could be a member of StreamAnalyticsSetting
                    string queryName = Setting.Name + "transformation";

                    var responseTrans = await smc.Transformations.CreateOrUpdateAsync(resourceGroupTask.Setting.Name,
                        Setting.Name,
                        new TransformationCreateOrUpdateParameters
                        {
                            Transformation = new Transformation(queryName)
                            {
                                Properties = new TransformationProperties
                                {
                                    StreamingUnits = 1,
                                    Query = query
                                }
                            }

                        }
                        );
                    succeeded = responseTrans.StatusCode == HttpStatusCode.OK || responseTrans.StatusCode == HttpStatusCode.Created;
                    if (!succeeded)
                    {
                        return false;
                    }

                    // Start SA job
                    var r = await smc.StreamingJobs.StartAsync(resourceGroupTask.Setting.Name,
                        Setting.Name,
                        new JobStartParameters());
                    succeeded = r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.Created;

                    return succeeded;
                }
            }
            catch (Exception e)
            {
                throw new AzureProvisioningException("Exception when creating stream analytics.", e);
            }

        }
    }
}
