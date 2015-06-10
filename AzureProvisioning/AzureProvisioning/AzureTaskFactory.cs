using System;
using AzureProvisioning.AzureTasks;
using System.Collections.Generic;
using Microsoft.Azure;
using AzureProvisioning.DAG;
using AzureProvisioning.ResourceSettings;

namespace AzureProvisioning
{
    /// <summary>
    /// Use AzureTaskFactory to convert a DAG of Azure services into chained AzureTasks with dependencies.
    /// </summary>
    public sealed class AzureTaskFactory
    {
        private static readonly AzureTaskFactory instance = new AzureTaskFactory();

        public AzureTaskFactory() { }

        public static AzureTaskFactory Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Caution: When resolving tasks, this function *will* change input graph by altering ParentCount
        /// </summary>
        /// <param name="cred">Credential needed to create Azure services</param>
        /// <param name="topNodes">Nodes that have no dependencies in the DAG</param>
        /// <returns>AllTask which represents all the tasks in one awaitable AzureTask</returns>
        public AllTask ResolveTasks(TokenCloudCredentials cred, List<Vertex<ResourceSetting>> topNodes)
        {
            var ParentTasksMap = new Dictionary<Vertex<ResourceSetting>, List<AzureTask>>();
            var ReadyTasksQueue = new Queue<Vertex<ResourceSetting>>();
            var LeafTasks = new List<AzureTask>();

            foreach (var node in topNodes)
            {
                ParentTasksMap.Add(node, null);
                ReadyTasksQueue.Enqueue(node);
            }

            while (ReadyTasksQueue.Count != 0)
            {
                var node = ReadyTasksQueue.Dequeue();
                var task = createAzureTask(cred, node.Data, ParentTasksMap[node]);

                if (node.Childrens.Count == 0)
                {
                    LeafTasks.Add(task);
                    continue;
                }
                foreach (var child in node.Childrens)
                {
                    List<AzureTask> deps;
                    if (!ParentTasksMap.TryGetValue(child, out deps))
                    {
                        deps = new List<AzureTask>();
                        ParentTasksMap.Add(child, deps);
                    }
                    deps.Add(task);
                    child.ParentCount--;
                    if (child.ParentCount == 0)
                    {
                        ReadyTasksQueue.Enqueue(child);
                    }
                }
            }

            var all = new AllTask(cred, LeafTasks);
            return all;
        }

        /// <summary>
        /// Instantiate an AzureTask based on runtime Type using reflection
        /// </summary>
        /// <param name="cred">Credential needed by AzureTask</param>
        /// <param name="st">setting of this concrete AzureTask</param>
        /// <param name="deps">dependencies of this AzureTask</param>
        /// <returns>The instantiated AzureTask instance</returns>
        private AzureTask createAzureTask(TokenCloudCredentials cred, ResourceSetting st, List<AzureTask> deps)
        {
            var paramslist = new Object[]
             {
                 cred,
                 deps,
                 st
             };

            try
            {

                Type t = Type.GetType(st.Type);
                object result = Activator.CreateInstance(t, paramslist);
                return result as AzureTask;
            }
            catch (Exception e)
            {
                throw new AzureProvisioningException("Error when instantiating concrete AzureTask", e);
            }
        }
    }
}
