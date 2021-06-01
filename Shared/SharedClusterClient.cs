using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Remote.GrpcCore;
using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DAM2.Core.Shared
{
	public class SharedClusterClient : ISharedClusterClient
	{
		private readonly ILogger<SharedClusterClient> logger;
		private readonly IDescriptorProvider descriptorProvider;
		private readonly IClusterSettings clusterSettings;
		private Cluster cluster;
		private bool clusterReady = false;
		private readonly ISharedClusterProviderFactory clusterProviderFactory;

		public SharedClusterClient(ILogger<SharedClusterClient> logger, IDescriptorProvider descriptorProvider, IClusterSettings clusterSettings, ISharedClusterProviderFactory clusterProviderFactory)
		{
			this.logger = logger;
			this.descriptorProvider = descriptorProvider;
			this.clusterSettings = clusterSettings;
			this.clusterProviderFactory = clusterProviderFactory;
		}


		public async Task Startup()
		{
			await CreateCluster();
		}

		public async Task Shutdown()
		{
			if (this.cluster != null)
			{
				await this.cluster.ShutdownAsync(true).ConfigureAwait(false);
				await Task.Delay(1000);
			}
		}

		public async Task<Cluster> CreateCluster()
		{
			try
			{
				logger.LogInformation("Setting up Cluster without actors");
				logger.LogInformation("ClusterName: " + clusterSettings.ClusterName);
				logger.LogInformation("PIDDatabaseName: " + clusterSettings.PIDDatabaseName);
				logger.LogInformation("PIDCollectionName: " + clusterSettings.PIDCollectionName);

				var system = new ActorSystem();
				var clusterProvider = this.clusterProviderFactory.CreateClusterProvider(logger);

				var identity = MongoIdentityLookup.GetIdentityLookup(clusterSettings.ClusterName, clusterSettings.PIDConnectionString, clusterSettings.PIDCollectionName, clusterSettings.PIDDatabaseName);

				var (clusterConfig, remoteConfig) = GenericClusterConfig.CreateClusterConfig(clusterSettings, clusterProvider, identity, descriptorProvider, logger);

				_ = new GrpcCoreRemote(system, remoteConfig);
				this.cluster = new Cluster(system, clusterConfig);

				await cluster.StartClientAsync().ConfigureAwait(false);

				clusterReady = true;

				logger.LogInformation("Cluster Client ready");

				return cluster;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "SharedClusterClient failed");
				return null;
			}
		}

		public Cluster Cluster => cluster;

		public async Task<T> RequestAsync<T>(string actorPath, string clusterKind, object cmd)
		{
			int counter = 0;
			while (!clusterReady && counter < 40)
			{
				await Task.Delay(250).ConfigureAwait(false);
				counter++;
			}

			try
			{
				var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
				var res = await cluster.RequestAsync<T>(actorPath, clusterKind, cmd, tokenSource.Token).ConfigureAwait(false);

				return res;
			}
			catch (Exception x)
			{
				logger.LogError(x, "Failed Request {Id}", actorPath);
				return default(T);
			}
		}
	}
}
