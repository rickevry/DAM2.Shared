﻿using System;
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
using Ubiquitous.Metrics;

namespace DAM2.Core.Shared
{
    public class SharedClusterWorker : ISharedClusterWorker
    {

        private readonly ILogger<SharedClusterWorker> _logger;
        private readonly ISharedSetupRootActors _setupRootActors;
        private readonly IClusterSettings _clusterSettings;
        private readonly IMainWorker _mainWorker;
        private readonly IDescriptorProvider _descriptorProvider;
        private readonly ISharedClusterProviderFactory _clusterProvider;
        private readonly ISubscriptionFactory _subscriptionFactory;
        private readonly IMetricsProvider _metricsProvider;
        private Cluster _cluster;
        private Task _mainWorkerTask;
        private CancellationTokenSource _cancellationTokenSource;
        private bool Connected;

        public SharedClusterWorker(
	        ILogger<SharedClusterWorker> logger,
	        IClusterSettings clusterSettings,
	        IDescriptorProvider descriptorProvider,
	        ISharedClusterProviderFactory clusterProvider,
	        ISharedSetupRootActors setupRootActors = default,
	        ISubscriptionFactory subscriptionFactory = default,
	        IMainWorker mainWorker = default,
	        IMetricsProvider metricsProvider = default
        )
        {
	        _logger = logger;
	        _setupRootActors = setupRootActors;
	        _clusterSettings = clusterSettings;
	        _mainWorker = mainWorker;
	        _descriptorProvider = descriptorProvider;
	        _clusterProvider = clusterProvider;
	        _subscriptionFactory = subscriptionFactory;
	        _metricsProvider = metricsProvider;
	        _cancellationTokenSource = new CancellationTokenSource();
        }
        public async Task<bool> Run()
        {
	        _cluster = await CreateCluster().ConfigureAwait(false);

            if (_mainWorker != null)
            {
	            this._mainWorkerTask = SafeTask.Run(() => _mainWorker.Run(_cluster), _cancellationTokenSource.Token);
            }

            return true;
        }

        public async Task<Cluster> CreateCluster()
        {
            try
            {
	            var actorSystemConfig = ActorSystemConfig.Setup();

	            if (_metricsProvider != null)
                {
                    actorSystemConfig = actorSystemConfig
	                    .WithMetricsProviders(_metricsProvider);
                }

                var system = new ActorSystem(actorSystemConfig);
                _logger.LogInformation("Setting up Cluster");
                _logger.LogInformation("ClusterName: " + _clusterSettings.ClusterName);
                _logger.LogInformation("PIDDatabaseName: " + _clusterSettings.PIDDatabaseName);
                _logger.LogInformation("PIDCollectionName: " + _clusterSettings.PIDCollectionName);

                var clusterProvider = _clusterProvider.CreateClusterProvider(_logger);

                //var identity = RedisIdentityLookup.GetIdentityLookup(_clusterSettings.ClusterName, _clusterSettings.Host, _clusterSettings.RedisPort);
                var identity = MongoIdentityLookup.GetIdentityLookup(_clusterSettings.ClusterName, _clusterSettings.PIDConnectionString, _clusterSettings.PIDCollectionName, _clusterSettings.PIDDatabaseName);

                var (clusterConfig, remoteConfig) = GenericClusterConfig.CreateClusterConfig(_clusterSettings, clusterProvider, identity, _descriptorProvider, _logger);

                if (_setupRootActors != null)
                {
	                clusterConfig = _setupRootActors.AddRootActors(clusterConfig);
                }

                var remote = new GrpcCoreRemote(system, remoteConfig);
                var cluster = new Cluster(system, clusterConfig);

                await cluster.StartMemberAsync().ConfigureAwait(false);

                if(this._subscriptionFactory != null)
                {
                    _logger.LogInformation("Fire up subscriptions for system {id} {address}", system.Id, system.Address);
	                await this._subscriptionFactory.FireUp(system).ConfigureAwait(false);
                }

                _ = SafeTask.Run(async () =>
                {
	                while (true)
	                {
		                Member[] members = cluster.MemberList.GetAllMembers();

		                this.Connected = members.Length > 0;
		                _logger.LogInformation("[SharedClusterWorker] Connected {Connected}");
		                if (this.Connected)
		                {
			                _logger.LogInformation("[SharedClusterWorker] Members {@Members}",
				                members.Select(m => m.ToLogString()));
		                }

		                await Task.Delay(500);
	                }
                });

                return cluster;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SharedClusterWork failed");
                throw;
            }
        }

        public Lazy<Cluster> Cluster => new Lazy<Cluster>(() => this._cluster ?? this.CreateCluster().ConfigureAwait(false).GetAwaiter().GetResult());
        public async Task Shutdown()
        {
            _cancellationTokenSource.Cancel(false);
	        if (this._cluster != null)
	        {
		        await this._cluster.ShutdownAsync(true).ConfigureAwait(false);
		        await Task.Delay(1000);
            }
        }
    }
}
