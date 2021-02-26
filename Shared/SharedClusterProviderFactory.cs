﻿using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;
using k8s;
using Microsoft.Extensions.Logging;
using Proto.Cluster;
using Proto.Cluster.Consul;
using Proto.Cluster.Kubernetes;
using System;

namespace DAM2.Core.Shared
{
    public class SharedClusterProviderFactory : ISharedClusterProviderFactory
    {

        private readonly IClusterSettings _clusterSettings = null;

        public SharedClusterProviderFactory(IClusterSettings clusterSettings)
        {
            _clusterSettings = clusterSettings;
        }

        public IClusterProvider CreateClusterProvider(ILogger logger)
        {
            try
            {
                var kubernetes = new Kubernetes(KubernetesClientConfiguration.InClusterConfig());
                logger.LogDebug("Running with Kubernetes Provider", kubernetes.BaseUri);
                return new KubernetesProvider(kubernetes);
            }
            catch
            {
                logger.LogDebug("Running with Consul Provider");
                return new ConsulProvider(new ConsulProviderConfig(), c => {
                    c.Address = new Uri(_clusterSettings.ConsulUri);
                });
            }
        }
    }
}
