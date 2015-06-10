using Microsoft.Azure;
using AzureProvisioning.ResourceSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureProvisioning.AzureTasks
{
    /// <summary>
    /// Base class of all concreate Azure Service Creation Tasks
    /// </summary>
    abstract public class AzureTask
    {
        /// <summary>
        /// An AsyncLazy Task member represent the action of creating this Azure service
        /// </summary>
        public AsyncLazy<Task<bool>> ServiceCreated;

        /// <summary>
        /// List of Dependencies for current AzureTask
        /// </summary>
        protected List<AzureTask> dependencies;

        /// <summary>
        /// Setting for current AzureTask
        /// </summary>
        public ResourceSetting Setting { get; private set; }

        /// <summary>
        /// Number of dependencies
        /// </summary>
        protected int dependenciesCNT;

        /// <summary>
        /// Credential used to create Azure services
        /// </summary>
        protected TokenCloudCredentials tokenCred;

        /// <summary>
        /// For every AzureTask, createService() will deal with dependencies first,
        /// then create service of this AzureTask
        /// </summary>
        /// <returns>Task for creation</returns>
        protected async Task<bool> createService()
        {
            if (dependenciesCNT != 0)
            {
                List<Task<bool>> preWorks = new List<Task<bool>>();

                foreach (var t in dependencies)
                {
                    preWorks.Add(await t.ServiceCreated);
                }

                bool preResult = await WhenAllOrAnyFault(preWorks);

                if (!preResult)
                {
                    return false;
                }
            }

            return await setupService();
        }

        protected async Task<bool> WhenAllOrAnyFault(List<Task<bool>> works)
        {
            while (works.Count > 0)
            {
                // Will quit the whole setup program if any of the preWorks failed
                Task<bool> completedWork = await Task.WhenAny(works);
                works.Remove(completedWork);

                if (!completedWork.Result)
                {
                    // Logging failed work here
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Setup logics for a specific Azure service. Need to be implemented in sub-class.
        /// </summary>
        /// <returns></returns>
        abstract protected Task<bool> setupService();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cred">Credential used to create Azure services</param>
        /// <param name="deps">Tasks this task depends on</param>
        public AzureTask(TokenCloudCredentials cred, List<AzureTask> deps, ResourceSetting st)
        {
            tokenCred = cred;
            dependencies = deps;
            Setting = st;
            ServiceCreated = new AsyncLazy<Task<bool>>(
                () => createService()
            );
        }
    }
}
