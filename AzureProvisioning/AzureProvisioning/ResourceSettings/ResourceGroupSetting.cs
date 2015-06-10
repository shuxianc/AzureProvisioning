using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.ResourceSettings
{
    /// <summary>
    /// Specific parameters/settings for Resource Group goes here
    /// </summary>
    public class ResourceGroupSetting : ResourceSetting
    {
        private const string type = "ResourceGroupTask";
        public ResourceGroupSetting(string name, LayerType layer, string location)
            : base(name, type, layer, location) { }
    }
}
