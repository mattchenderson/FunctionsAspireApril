using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Azure.Provisioning.Storage;
using FunctionsAspireApril.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var containerAppEnvironment = builder
    .AddAzureContainerAppEnvironment("my-env")
    .WithAzdResourceNaming();

var hostStorage = builder.AddAzureStorage("hoststorage")
    .RunAsEmulator()
    .WithRequiredCustomizationsToSatisfyAzurePolicy();

hostStorage.AddBlobs("blobs");

var appStorage = builder.AddAzureStorage("appstorage")
    .RunAsEmulator()
    .WithRequiredCustomizationsToSatisfyAzurePolicy();

var blobs = appStorage.AddBlobs("appBlobs");
var queues = appStorage.AddQueues("appQueues");

builder.AddAzureFunctionsProject<Projects.FunctionsAspireApril>("functionsaspireapril")
    .WithHostStorage(hostStorage)
    .WithRoleAssignments(hostStorage, 
        StorageBuiltInRole.StorageBlobDataOwner,
        StorageBuiltInRole.StorageQueueDataContributor,
        StorageBuiltInRole.StorageAccountContributor)
    .WithReference(blobs, "MyBlobConnection")
    .WithReference(queues, "MyBlobConnection")
    .WithReference(queues, "MyQueueConnection")
    .WithRoleAssignments(appStorage, 
        StorageBuiltInRole.StorageBlobDataOwner,
        StorageBuiltInRole.StorageQueueDataContributor)
    .PublishAsACANativeFunctions(containerAppEnvironment);

builder.Build().Run();