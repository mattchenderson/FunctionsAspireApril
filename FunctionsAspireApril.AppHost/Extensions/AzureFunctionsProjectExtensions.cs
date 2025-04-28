using Aspire.Hosting.Azure;
using Aspire.Hosting.Azure.AppContainers;
using Aspire.Hosting.Lifecycle;
using Azure.Core;
using Grpc.Core;

namespace FunctionsAspireApril.AppHost.Extensions
{
    public static class AzureFunctionsProjectExtensions
    {
        public static IResourceBuilder<T> PublishAsACANativeFunctions<T>(this IResourceBuilder<T> builder, IResourceBuilder<Aspire.Hosting.Azure.AppContainers.AzureContainerAppEnvironmentResource> containerAppEnvironment)
            where T : AzureFunctionsProjectResource
        {
            builder.ApplicationBuilder.AddAzureContainerAppsWithKindInfrastructure();
            builder.WithAnnotation(new AzureContainerAppWithKindCustomizationAnnotation((_, containerApp) =>
            {
                containerApp.Kind = "functionapp";
            }));

#pragma warning disable ASPIRECOMPUTE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            //builder.WithComputeEnvironment(containerAppEnvironment);
#pragma warning restore ASPIRECOMPUTE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            foreach (string outputName in new[] {
                "AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN",
                "AZURE_CONTAINER_APPS_ENVIRONMENT_ID",
                "AZURE_CONTAINER_APPS_ENVIRONMENT_NAME",
                "AZURE_CONTAINER_REGISTRY_ENDPOINT",
                "AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID",
                "AZURE_CONTAINER_REGISTRY_NAME"
            })
            {
                var outputReference = new BicepOutputReference(outputName, containerAppEnvironment.Resource);
                builder.WithEnvironment(outputName, outputReference);
            }

            return builder;
        }

        private static IDistributedApplicationBuilder AddAzureContainerAppsWithKindInfrastructure(this IDistributedApplicationBuilder builder)
        {
            builder.Services.TryAddLifecycleHook<AzureContainerAppsWithKindInfrastructure>();

            return builder;
        }
    }
}
