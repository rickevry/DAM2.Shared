using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Remote.GrpcCore;
using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;
using DAM2.Core.Shared.Subscriptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ubiquitous.Metrics;

namespace DAM2.Core.Shared
{
    public class SharedClusterWorkerOptions
    {
        public const string Key = "ProtoCluster";
        public bool RestartOnFail { get; set; }
    }
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
        private readonly SharedClusterWorkerOptions clusterOptions;


        public SharedClusterWorker(
            ILogger<SharedClusterWorker> logger,
            IClusterSettings clusterSettings,
            IDescriptorProvider descriptorProvider,
            ISharedClusterProviderFactory clusterProviderFactory,
            IHostApplicationLifetime applicationLifetime,
            IOptions<SharedClusterWorkerOptions> clusterOptionsAccessor,
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
            this.clusterOptions = clusterOptionsAccessor.Value;
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

                _ = SafeTask.Run(ConnectedLoop, this.applicationLifetime.ApplicationStopping);

                return cluster;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SharedClusterWork failed");
                throw;
            }
        }

        public Lazy<Cluster> Cluster => new(() => this.cluster ?? this.CreateCluster().ConfigureAwait(false).GetAwaiter().GetResult());
        public async Task Shutdown()
        {
            if (this.cluster != null)
            {
                await this.cluster.ShutdownAsync(true).ConfigureAwait(false);
                await Task.Delay(3000);
            }
        }

        private async Task ConnectedLoop()
        {
            await Task.Yield();

            try
            {
                int counter = 0;
                while (!this.applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    Member[] members = cluster.MemberList.GetAllMembers();
                    string[] clusterKinds = cluster.GetClusterKinds();

                    if (clusterKinds.Length == 0)
                    {
                        this.logger.LogWarning("[SharedClusterWorker] clusterKinds {clusterKinds}", clusterKinds.Length);
                        this.logger.LogWarning("[SharedClusterWorker] Restarting");
                        if (this.clusterOptions.RestartOnFail)
                        {
                            _ = this.RestartMe();
                            break;
                        }

                    }

                    this.Connected = members.Length > 0;
                    if (!this.Connected)
                    {
                        counter = 0;
                        logger.LogInformation("[SharedClusterWorker] Connected {Connected}", this.Connected);
                    }

                    if (this.Connected)
                    {
                        if (counter % 20 == 0)
                        {
                            logger.LogDebug("[SharedClusterWorker] Members {@Members}", members.Select(m => m.ToLogString()));
                            counter = 0;
                        }
                        counter++;
                    }

                    await Task.Delay(500);
                }
            }
            catch
            {
                // ignored
            }
        }

        public bool Connected { get; set; }

        private Task RestartMe()
        {
            _ = SafeTask.Run(async () =>
            {
                await Task.Delay(2000);
                try { await this.Shutdown(); }
                catch
                {
                    // ignored
                }

                cluster = null;
                await Task.Delay(5000);
                await this.Run();
            });

            return Task.CompletedTask;
        }
    }
}
