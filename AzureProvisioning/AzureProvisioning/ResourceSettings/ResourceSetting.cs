using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.ResourceSettings
{
    public abstract class ResourceSetting
    {
        /// <summary>
        /// Name of this Azure Resource
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Type of this Azure Resource, used to instantiate this Task
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Layer this Resource belongs to
        /// </summary>
        public LayerType Layer { get; private set; }

        /// <summary>
        /// Data center location of this Azure service
        /// </summary>
        public string Location { get; private set; }

        public ResourceSetting(string name, string type, LayerType layer, string location)
        {
            this.Name = name;
            this.Type = "AzureProvisioning.AzureTasks." + type;
            this.Layer = layer;
            this.Location = location;
        }

    }
}
