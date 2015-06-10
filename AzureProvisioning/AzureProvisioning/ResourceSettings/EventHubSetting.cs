using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.ResourceSettings
{
    /// <summary>
    /// parameters/settings for EventHub goes here
    /// </summary>
    public class EventHubSetting : ResourceSetting
    {
        public string ServiceBusName { get; set; }
        public string SendKeyName { get; set; }
        public string SendKeyValue { get; set; }
        public string ManageKeyName { get; set; }
        public string ManageKeyValue { get; set; }

        /// <summary>
        /// Optional settings can be added like this
        /// </summary>
        public int Partitions { get; set; }

        private const string type = "EventHubTask";

        public EventHubSetting(string name, LayerType layer, string location, int partitions)
            : base(name, type, layer, location) 
        {
            this.Partitions = partitions;
        }
    }
}
