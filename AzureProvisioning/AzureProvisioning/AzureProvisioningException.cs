using System;

namespace AzureProvisioning
{
    public class AzureProvisioningException : Exception
    {
        public AzureProvisioningException()
        {
        }

        public AzureProvisioningException(string message)
            : base(message)
        {
        }

        public AzureProvisioningException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
