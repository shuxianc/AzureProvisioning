using Microsoft.Azure;
using AzureProvisioning.ResourceSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.AzureTasks
{
    /// <summary>
    /// AllTask is used to converge all tasks in DAG into one single awaitable AzureTask
    /// </summary>
    public class AllTask : AzureTask
    {
        public AllTask(TokenCloudCredentials cred, List<AzureTask> deps, ResourceSetting st = null)
            : base(cred, deps, st) 
        {
            dependenciesCNT = deps.Count;
        }

#pragma warning disable

        /// <summary>
        /// For AllTask, do nothing but wait for all dependencies to finish
        /// </summary>
        /// <returns>if all dependencies finished succeesfully</returns>
        protected override async Task<bool> setupService()
        {
            return true;
        }

#pragma warning restore

    }
}
