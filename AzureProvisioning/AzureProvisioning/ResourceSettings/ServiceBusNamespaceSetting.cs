using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.ResourceSettings
{
    /// <summary>
    /// Specific parameters/settings for Service Bus goes here
    /// </summary>
    public class ServiceBusNamespaceSetting: ResourceSetting
    {
        public string Endpoint { get; set; }
        public string ConnectionString { get; set; }

        /// <summary>
        /// Optional settings can be added like this
        /// </summary>
        public int MessagingTier { get; set; }

        private const string type = "ServiceBusNamespaceTask";

        public ServiceBusNamespaceSetting(string name, LayerType layer, string location)
            : base(name, type, layer, location) { }

    }
}
