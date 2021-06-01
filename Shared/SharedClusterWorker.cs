using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Remote.GrpcCore;
using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;
using DAM2.Core.Shared.Subscriptions;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Ubiquitous.Metrics;

namespace DAM2.Core.Shared
{
    public class SharedClusterWorker : ISharedClusterWorker
    {

        private readonly ILogger<SharedClusterWorker> logger;
        private readonly ISharedSetupRootActors setupRootActors;
        private readonly IClusterSettings clusterSettings;
        private readonly IMainWorker mainWorker;
        private readonly IDescriptorProvider descriptorProvider;
        private readonly ISharedClusterProviderFactory clusterProviderFactory;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ISubscriptionFactory subscriptionFactory;
        private readonly IMetricsProvider metricsProvider;
        private Cluster cluster;
        
        public SharedClusterWorker(
            ILogger<SharedClusterWorker> logger,
            IClusterSettings clusterSettings,
            IDescriptorProvider descriptorProvider,
            ISharedClusterProviderFactory clusterProviderFactory,
            IHostApplicationLifetime applicationLifetime,
            ISharedSetupRootActors setupRootActors = default,
            ISubscriptionFactory subscriptionFactory = default,
            IMainWorker mainWorker = default,
            IMetricsProvider metricsProvider = default
        )
        {
            this.logger = logger;
            this.setupRootActors = setupRootActors;
            this.clusterSettings = clusterSettings;
            this.mainWorker = mainWorker;
            this.descriptorProvider = descriptorProvider;
            this.clusterProviderFactory = clusterProviderFactory;
            this.applicationLifetime = applicationLifetime;
            this.subscriptionFactory = subscriptionFactory;
            this.metricsProvider = metricsProvider;
        }
        public async Task<bool> Run()
        {
            cluster = await CreateCluster().ConfigureAwait(false);

            if (mainWorker != null)
            {
                _ = SafeTask.Run(() => mainWorker.Run(cluster), this.applicationLifetime.ApplicationStopping);
            }

            return true;
        }

        public async Task<Cluster> CreateCluster()
        {
            try
            {
                var actorSystemConfig = ActorSystemConfig.Setup();

                if (metricsProvider != null)
                {
                    actorSystemConfig = actorSystemConfig
                        .WithMetricsProviders(metricsProvider);
                }

                var system = new ActorSystem(actorSystemConfig);
                logger.LogInformation("Setting up Cluster");
                logger.LogInformation("ClusterName: " + clusterSettings.ClusterName);
                logger.LogInformation("PIDDatabaseName: " + clusterSettings.PIDDatabaseName);
                logger.LogInformation("PIDCollectionName: " + clusterSettings.PIDCollectionName);

                var clusterProvider = this.clusterProviderFactory.CreateClusterProvider(logger);

                var identity = MongoIdentityLookup.GetIdentityLookup(clusterSettings.ClusterName, clusterSettings.PIDConnectionString, clusterSettings.PIDCollectionName, clusterSettings.PIDDatabaseName);

                var (clusterConfig, remoteConfig) = GenericClusterConfig.CreateClusterConfig(clusterSettings, clusterProvider, identity, descriptorProvider, logger);

                if (setupRootActors != null)
                {
                    clusterConfig = setupRootActors.AddRootActors(clusterConfig);
                }

                _ = new GrpcCoreRemote(system, remoteConfig);
                
                this.cluster = new Cluster(system, clusterConfig);

                await cluster.StartMemberAsync().ConfigureAwait(false);

                if (this.subscriptionFactory != null)
                {
                    logger.LogInformation("Fire up subscriptions for system {id} {address}", system.Id, system.Address);
                    await this.subscriptionFactory.FireUp(system).ConfigureAwait(false);
                }

                return cluster;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SharedClusterWork failed");
                throw;
            }
        }

        public Lazy<Cluster> Cluster => new Lazy<Cluster>(() => this.cluster ?? this.CreateCluster().ConfigureAwait(false).GetAwaiter().GetResult());
        public async Task Shutdown()
        {
            if (this.cluster != null)
            {
                await this.cluster.ShutdownAsync(true).ConfigureAwait(false);
                await Task.Delay(3000);
            }
        }
    }
}
