using Aspire.Hosting.Azure;
using Azure.Provisioning.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionsAspireApril.AppHost.Extensions
{
    public static class AzureStorageResourceExtensions
    {
        public static IResourceBuilder<AzureStorageResource> WithRequiredCustomizationsToSatisfyAzurePolicy(this IResourceBuilder<AzureStorageResource> storage)
        {
            return storage.ConfigureInfrastructure(infra =>
            {
                var sa = infra.GetProvisionableResources().OfType<StorageAccount>().FirstOrDefault()!;
                sa.AllowBlobPublicAccess = false;
            });
        }
    }
}
