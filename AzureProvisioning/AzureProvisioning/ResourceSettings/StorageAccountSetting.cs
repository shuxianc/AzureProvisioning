using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.ResourceSettings
{
    /// <summary>
    /// parameters/settings for Storage Acount goes here
    /// </summary>
    public class StorageAccountSetting: ResourceSetting
    {
        public string PrimaryKey { get; set; }
        private const string type = "StorageAccountTask";

        public StorageAccountSetting(string name, LayerType layer, string location)
            : base(name, type, layer, location) { }
    }
}
