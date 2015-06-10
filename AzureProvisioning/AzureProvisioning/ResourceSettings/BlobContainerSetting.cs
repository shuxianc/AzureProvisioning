using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.ResourceSettings
{
    /// <summary>
    /// parameters/settings for blob container
    /// </summary>
    public class BlobContainerSetting : ResourceSetting
    {
        public string StorageAccountName { get; set; }

        public string StorageAccountKey { get; set; }

        private const string type = "BlobContainerTask";

        public BlobContainerSetting(string name, LayerType layer, string location)
            : base(name, type, layer, location) { }
    }
}
