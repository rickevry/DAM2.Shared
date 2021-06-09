using Microsoft.Extensions.DependencyInjection.Extensions;
using Proto.Cluster;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.ProtoActorRuntime
{
    public static class CoreHostingExtensions
    {
        /// <summary>
        /// Configure the container to use Orleans.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <returns>The host builder.</returns>
        public static IProtoActorHostBuilder ConfigureDefaults(this IProtoActorHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) =>
            {
                if (!context.Properties.ContainsKey("ProtoActorServicesAdded"))
                {
                    services.TryAddSingleton<Cluster>();
                    DefaultClusterServices.AddDefaultServices(services);

                    context.Properties.Add("ProtoActorServicesAdded", true);
                }
            });
        }

        /// <summary>
        /// Configure the container to use Orleans.
        /// </summary>
        /// <param name="builder">The silo builder.</param>
        /// <returns>The silo builder.</returns>
        public static IProtoActorBuilder ConfigureDefaults(this IProtoActorBuilder builder)
        {
            return builder.ConfigureServices((context, services) =>
            {
                if (!context.Properties.ContainsKey("ProtoActorServicesAdded"))
                {
                    services.TryAddSingleton<Cluster>();
                    DefaultClusterServices.AddDefaultServices(services);

                    context.Properties.Add("ProtoActorServicesAdded", true);
                }
            });
        }

        /// <summary>
        /// Configures the silo to use development-only clustering.
        /// </summary>
        //public static ISiloBuilder UseDevelopmentClustering(
        //    this ISiloBuilder builder,
        //    Action<OptionsBuilder<DevelopmentClusterMembershipOptions>> configureOptions)
        //{
        //    return builder.ConfigureServices(
        //        services =>
        //        {
        //            configureOptions?.Invoke(services.AddOptions<DevelopmentClusterMembershipOptions>());
        //            services.ConfigureFormatter<DevelopmentClusterMembershipOptions>();
        //            services
        //                .AddSingleton<SystemTargetBasedMembershipTable>()
        //                .AddFromExisting<IMembershipTable, SystemTargetBasedMembershipTable>();
        //        });
        //}
    }
}
