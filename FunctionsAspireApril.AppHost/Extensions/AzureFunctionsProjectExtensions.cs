using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Azure.AppContainers;
using Aspire.Hosting.Lifecycle;
using Azure.Core;
using Azure.Provisioning;
using Azure.Provisioning.AppContainers;
using Azure.Provisioning.Expressions;
using Azure.Provisioning.Primitives;
using Azure.Provisioning.Resources;
using Grpc.Core;

namespace FunctionsAspireApril.AppHost.Extensions
{
    public static class AzureFunctionsProjectExtensions
    {
        public static IResourceBuilder<T> PublishAsACANativeFunctions<T>(this IResourceBuilder<T> builder, IResourceBuilder<AzureContainerAppEnvironmentResource> containerAppEnvironment)
            where T : AzureFunctionsProjectResource
        {
            builder
            .PublishAsAzureContainerApp((infra, app) =>
            {
                var paramRegistryEndpoint = new BicepOutputReference("AZURE_CONTAINER_REGISTRY_ENDPOINT", containerAppEnvironment.Resource).ValueExpression
                        .Replace('.', '_').Replace('-', '_').Replace("{", "").Replace("}", "").ToLower()!;
                var paramRegistryIdentity = new BicepOutputReference("AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID", containerAppEnvironment.Resource).ValueExpression
                        .Replace('.', '_').Replace('-', '_').Replace("{", "").Replace("}", "").ToLower()!;

                var appWithKind = new ContainerAppWithKind(app.BicepIdentifier);
                appWithKind.Kind = "functionapp";
                appWithKind.ResourceVersion = "2024-10-02-preview";
                appWithKind.Name = builder.Resource.Name;
                appWithKind.Identity = new ManagedServiceIdentity() { ManagedServiceIdentityType = ManagedServiceIdentityType.UserAssigned };
                appWithKind.Identity.UserAssignedIdentities.Add(new InterpolatedStringExpression([new IdentifierExpression(builder.Resource.Name + "_identity_outputs_id")]).ToString(), new UserAssignedIdentityDetails());
                appWithKind.Identity.UserAssignedIdentities.Add(new InterpolatedStringExpression([new IdentifierExpression(paramRegistryIdentity)] ).ToString(), new UserAssignedIdentityDetails());

                appWithKind.EnvironmentId = app.EnvironmentId;
                appWithKind.Configuration = new ContainerAppConfiguration()
                {
                    ActiveRevisionsMode = ContainerAppActiveRevisionsMode.Single,
                    Ingress = new ContainerAppIngressConfiguration()
                    {
                        External = false,
                        TargetPort = 8080,
                        Transport = ContainerAppIngressTransportMethod.Http,
                    }
                };
                appWithKind.Configuration.Registries.Add(new ContainerAppRegistryCredentials()
                {
                    Server = new IdentifierExpression(paramRegistryEndpoint),
                    Identity = new IdentifierExpression(paramRegistryIdentity)
                });
                appWithKind.Template = new ContainerAppTemplate()
                {
                    Scale = new ContainerAppScale() { MinReplicas = 1}
                };

                var container = new ContainerAppContainer()
                {
                    Image = new IdentifierExpression(builder.Resource.Name + "_containerimage"),
                    Name = builder.Resource.Name
                };

                appWithKind.Template.Containers.Add(container);

                infra.Remove(app);
                infra.Add(appWithKind);
            });

            return builder;
        }
    }
    internal class ContainerAppWithKind : ContainerApp
    {
        /// <summary>
        /// The kind of the container app.
        /// </summary>
        public BicepValue<string> Kind
        {
            get { Initialize(); return _kind!; }
            set { Initialize(); _kind!.Assign(value); }
        }
        private BicepValue<string>? _kind;

        /// <summary>
        /// Creates a new instance of <see cref="ContainerAppWithKind"/>.
        /// </summary>
        /// <param name="bicepIdentifier"></param>
        /// <param name="resourceVersion"></param>
        public ContainerAppWithKind(string bicepIdentifier, string? resourceVersion = null) : base(bicepIdentifier, resourceVersion) { }

        /// <summary>
        /// Overrides provisionable properties.
        /// </summary>
        protected override void DefineProvisionableProperties()
        {
            base.DefineProvisionableProperties();

            _kind = DefineProperty<string>(nameof(Kind), ["kind"]);
        }
    }
}
