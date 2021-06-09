using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Ubiquitous.Metrics;

namespace DAM2.Shared.ProtoActorRuntime
{
    public static class ProtoActorHostExtensions
    {
        public static IProtoActorBuilder UseClusterSettings(this IProtoActorBuilder builder, Action<ClusterSettings> configureOptions)
        {
            return builder.ConfigureServices((context, services) =>
            {
                if (configureOptions != null)
                {
                    services.Configure(configureOptions);
                }
            });
        }

        public static IProtoActorBuilder AddActors<TActors>(this IProtoActorBuilder builder) where TActors : class, ISharedSetupRootActors
        {
            return builder.ConfigureServices((context, services) =>
            {
                services.TryAddTransient<ISharedSetupRootActors, TActors>();
            });
        }

        public static IProtoActorBuilder AddDescriptionProvider<TDescriptor>(this IProtoActorBuilder builder) where TDescriptor : class, IDescriptorProvider
        {
            return builder.ConfigureServices((context, services) =>
            {
                services.TryAddTransient<IDescriptorProvider, TDescriptor>();
            });
        }

        public static IProtoActorBuilder AddMainWorker<TWorker>(this IProtoActorBuilder builder) where TWorker : class, IMainWorker
        {
            return builder.ConfigureServices((context, services) =>
            {
                services.TryAddSingleton<IMainWorker, TWorker>();
            });
        }

        public static IProtoActorBuilder AddMetrics(this IProtoActorBuilder builder, params IMetricsProvider[] metricsProviders) 
        {
            return builder.ConfigureServices((context, services) =>
            {
                if(metricsProviders == null)
                {
                    return;
                }
                foreach (IMetricsProvider metricsProvider in metricsProviders)
                {
                    services.TryAddSingleton<IMetricsProvider>(metricsProvider);
                }
            });
        }
    }
}
