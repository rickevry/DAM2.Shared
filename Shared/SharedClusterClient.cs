﻿using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proto;
using Proto.Cluster;
using Proto.Remote.GrpcCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DAM2.Core.Shared
{
    public class SharedClusterClientOptions
    {
        public const string Key = "ProtoClusterClient";
        public bool RestartOnFail { get; set; }
    }

    public class SharedClusterClient : ISharedClusterClient
    {
        private readonly ILogger<SharedClusterClient> logger;
        private readonly IDescriptorProvider descriptorProvider;
        private readonly IClusterSettings clusterSettings;
        private Cluster cluster;
        private bool clusterReady;
        private readonly ISharedClusterProviderFactory clusterProviderFactory;
        private readonly Dictionary<string, int> retries = new();
        private readonly SharedClusterClientOptions clientOptions;

        public SharedClusterClient(ILogger<SharedClusterClient> logger,
            IDescriptorProvider descriptorProvider,
            IClusterSettings clusterSettings,
            ISharedClusterProviderFactory clusterProviderFactory,
            IOptions<SharedClusterClientOptions> clientOptionsAccessor)
        {
            this.logger = logger;
            this.descriptorProvider = descriptorProvider;
            this.clusterSettings = clusterSettings;
            this.clusterProviderFactory = clusterProviderFactory;
            this.clientOptions = clientOptionsAccessor.Value;
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
                await Task.Delay(3000);
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

                var identity = MongoIdentityLookup.GetIdentityLookup(clusterSettings.ClusterName,
                    clusterSettings.PIDConnectionString,
                    clusterSettings.PIDCollectionName,
                    clusterSettings.PIDDatabaseName);

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
                throw;
            }
        }

        public Cluster Cluster => cluster;

        public async Task<T> RequestAsync<T>(string actorPath, string clusterKind, object cmd, CancellationToken token = default)
        {
            string key = $"{actorPath}_{clusterKind}";
            int counter = 0;
            while (!clusterReady && counter < 40)
            {
                await Task.Delay(250).ConfigureAwait(false);
                counter++;
            }

            try
            {
                if (token == default)
                {
                    var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    token = tokenSource.Token;
                }

                var res = await cluster.RequestAsync<T>(actorPath, clusterKind, cmd, token).ConfigureAwait(false);

                if (token.IsCancellationRequested && clusterReady && this.clientOptions.RestartOnFail)
                {
                    clusterReady = false;
                    await RestartMe();
                    return await Retry<T>(actorPath, clusterKind, cmd, key);
                }

                if (!clusterReady && this.clientOptions.RestartOnFail)
                {
                    return await Retry<T>(actorPath, clusterKind, cmd, key);
                }

                retries.Remove(key);

                return res;
            }
            catch (Exception x)
            {
                logger.LogError(x, "Failed Request {Id}", actorPath);
                return default;
            }
        }

        private async Task<T> Retry<T>(string actorPath, string clusterKind, object cmd, string key)
        {
            if (retries.TryGetValue(key, out int value))
            {
                Interlocked.Increment(ref value);
            }
            else
            {
                retries.Add(key, 1);
            }

            if (value > 5)
            {
                this.logger.LogError("Request timeout for {Id}", actorPath);
                return default;
            }

            this.logger.LogInformation("[{Client}] Retry request...", nameof(SharedClusterClient));
            await Task.Delay(value * 200);
            return await RequestAsync<T>(actorPath, clusterKind, cmd);
        }

        private async Task RestartMe()
        {
            this.logger.LogWarning("[{Client}] Restarting", nameof(SharedClusterClient));
            try { await this.Shutdown(); }
            catch
            {
                // ignored
            }
            this.cluster = null;
            await this.CreateCluster();
            await Task.Delay(3000);
        }
    }
}
