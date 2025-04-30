using Aspire.Hosting.Azure;
using Azure.Provisioning.Primitives;
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

        public static IResourceBuilder<AzureBicepResource> AddContainer(this IResourceBuilder<AzureBlobStorageResource> builder, string containerName)
        {
            string storageAccountParameterName = "storageName";
            return builder.ApplicationBuilder
                .AddBicepTemplateString(containerName, $@"
@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location
param storageName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' existing = {{
    name: storageName
}}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2024-01-01' existing = {{
    parent: storageAccount
    name: 'default'
}}

resource {containerName.Replace('.', '_').Replace('-', '_')} 'Microsoft.Storage/storageAccounts/blobServices/containers@2024-01-01' = {{ 
    parent: blobService
    name: '{containerName}'
}}")
              .WithParameter(storageAccountParameterName, new BicepOutputReference("name", builder.Resource.Parent));
        }


        public static IResourceBuilder<AzureBicepResource> AddQueue(this IResourceBuilder<AzureQueueStorageResource> builder, string containerName)
        {
            string storageAccountParameterName = "storageName";
            return builder.ApplicationBuilder
                .AddBicepTemplateString(containerName, $@"
@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location
param storageName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' existing = {{
    name: storageName
}}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2024-01-01' existing = {{
    parent: storageAccount
    name: 'default'
}}

resource {containerName.Replace('.', '_').Replace('-', '_')} 'Microsoft.Storage/storageAccounts/queueServices/queues@2024-01-01' = {{ 
    parent: queueService
    name: '{containerName}'
}}")
              .WithParameter(storageAccountParameterName, new BicepOutputReference("name", builder.Resource.Parent));
        }
    }
}
