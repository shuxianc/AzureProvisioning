using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureProvisioning.ResourceSettings
{
    public class StreamAnalyticsSetting : ResourceSetting
    {
        private const string type = "StreamAnalyticsTask";
        public StreamAnalyticsSetting(string name, LayerType layer, string location)
            : base(name, type, layer, location) { }
    }
}
