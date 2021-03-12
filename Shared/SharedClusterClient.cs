using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Remote.GrpcCore;
using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;

namespace DAM2.Core.Shared
{
    public class SharedClusterClient : ISharedClusterClient
    {
        private readonly ILogger<SharedClusterClient> _logger;
        private readonly IDescriptorProvider _descriptorProvider;
        private readonly IClusterSettings _clusterSettings;
        private Cluster _cluster;
        private bool _cluster_ready = false;
        private readonly ISharedClusterProviderFactory _clusterProvider;

        public SharedClusterClient(ILogger<SharedClusterClient> logger, IDescriptorProvider descriptorProvider, IClusterSettings clusterSettings, ISharedClusterProviderFactory clusterProvider)
        {
            _logger = logger;
            _descriptorProvider = descriptorProvider;
            _clusterSettings = clusterSettings;
            _clusterProvider = clusterProvider;
            
            var l = LoggerFactory.Create(l =>
                l.AddConsole().SetMinimumLevel(LogLevel.Information)
            );

            Proto.Log.SetLoggerFactory(l);
        }

        public async Task<T> RequestAsync<T>(string actorPath, string clusterKind,  object cmd)
        {
            int counter = 0;
            while (!_cluster_ready && counter < 40)
            {
                await Task.Delay(250).ConfigureAwait(false);
                counter++;
            }

            try
            {
                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var res = await _cluster.RequestAsync<T>(actorPath, clusterKind, cmd, tokenSource.Token).ConfigureAwait(false);
                return res;
            }
            catch (Exception x)
            {
                _logger.LogError(x, "Request timeout for {Id}", actorPath);
                return default(T);
            }
        }

        public async Task Startup()
        {
            await CreateCluster();
        }

        public async Task Shutdown()
        {
	        if (this._cluster != null)
	        {
		        await this._cluster.ShutdownAsync(true).ConfigureAwait(false);
            }
	        
        }
        public async Task<Cluster> CreateCluster()
        {
            try
            {
                _logger.LogInformation("Setting up Cluster without actors");
                _logger.LogInformation("ClusterName: " +  _clusterSettings.ClusterName);
                _logger.LogInformation("PIDDatabaseName: " + _clusterSettings.PIDDatabaseName);
                _logger.LogInformation("PIDCollectionName: " + _clusterSettings.PIDCollectionName);

                var system = new ActorSystem();
                var clusterProvider = _clusterProvider.CreateClusterProvider(_logger);
                
                var identity = MongoIdentityLookup.GetIdentityLookup(_clusterSettings.ClusterName, _clusterSettings.PIDConnectionString, _clusterSettings.PIDCollectionName, _clusterSettings.PIDDatabaseName);
                //var identity = RedisIdentityLookup.GetIdentityLookup(_clusterSettings.ClusterName, _clusterSettings.Host, _clusterSettings.RedisPort);

                var (clusterConfig, remoteConfig) = GenericClusterConfig.CreateClusterConfig(_clusterSettings, clusterProvider, identity, _descriptorProvider, _logger);

                //clusterConfig = _setupRootActors.AddRootActors(clusterConfig);

                var remote = new GrpcCoreRemote(system, remoteConfig);
                var cluster = new Cluster(system, clusterConfig);

                await cluster.StartMemberAsync().ConfigureAwait(false);

                _cluster = cluster;
                _cluster_ready = true;

                _logger.LogInformation("Cluster Client ready");

                return cluster;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SharedClusterClient failed");
                return null;
            }

        }
    }
}
