using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureProvisioning.ResourceSettings
{
    /// <summary>
    /// Data will always flow from small layer number to big layer number
    /// </summary>
    public enum LayerType
    {
        Default = 0,        // Azure services like ServiceBusNamespace or ResourceGoup goes here
        Source = 1,
        Collection = 2,
        Ingestion = 3,
        Transformation = 4,
        RawArchive = 5,
        ETL = 6,
        CleanArchive = 7
    }
}
