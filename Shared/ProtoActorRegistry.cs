using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAM2.Core.Shared;
using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;
using DAM2.Shared.Shared;
using DAM2.Shared.Shared.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ubiquitous.Metrics;

namespace DAM2.Shared
{
    public static class ProtoActorRegistry
    {
        public static ProtoActorClusterServices AddProtoCluster(this IServiceCollection services, IConfiguration configuration)
		{
			ConfigureClusterSettings(services, configuration);

			services.AddSingleton<ISharedClusterProviderFactory, SharedClusterProviderFactory>();
			services.AddTransient<ITokenFactory, TokenFactory>();
			services.AddSingleton<SharedClusterWorker>();
			services.AddHostedService<ProtoActorClusterHostedService>();
			
			return new ProtoActorClusterServices(services);
		}

        public static ProtoActorClientServices AddProtoClient(this IServiceCollection services, IConfiguration configuration)
        {
	        ConfigureClusterSettings(services, configuration);
	        services.AddSingleton<ISharedClusterProviderFactory, SharedClusterProviderFactory>();
	        services.AddTransient<ITokenFactory, TokenFactory>();
	        services.AddSingleton<ISharedClusterClient, SharedClusterClient>();
	        services.AddHostedService<ProtoActorClientHostedService>();

			return new ProtoActorClientServices(services);
        }

        private static void ConfigureClusterSettings(IServiceCollection services, IConfiguration configuration)
        {
	        services.Configure<ClusterSettings>(configuration.GetSection("ClusterSettings"));
	        services.AddSingleton<IClusterSettings>(sp =>
	        {
		        var firstSetting = sp.GetRequiredService<IOptions<ClusterSettings>>();
		        return firstSetting.Value;
	        });
        }
    }



    public class ProtoActorClusterServices
    {
	    private readonly IServiceCollection services;

	    public ProtoActorClusterServices(IServiceCollection services)
	    {
		    this.services = services;
	    }

	    public IServiceCollection Services => this.services;

		public ProtoActorClusterServices AddRootActors<TRootActors>() where TRootActors: class, ISharedSetupRootActors
		{
			services.AddTransient<ISharedSetupRootActors, TRootActors>();
			return this;
		}

		public ProtoActorClusterServices AddDescriptorProvider<TProvider>() where TProvider : class, IDescriptorProvider
		{
			services.AddTransient<IDescriptorProvider, TProvider>();
			return this;
		}

		public ProtoActorClusterServices AddMainWorker<TMainWorker>() where TMainWorker : class, IMainWorker
		{
			services.AddTransient<IMainWorker, TMainWorker>();
			return this;
		}

		public ProtoActorClusterServices AddProtoMetrics(params IMetricsProvider[] metricsProviders)
		{
			if (metricsProviders == null)
			{
				return this;
			}
			foreach (IMetricsProvider metricsProvider in metricsProviders)
			{
				services.AddSingleton<IMetricsProvider>(metricsProvider);
			}
			
			return this;
		}
	}

    public class ProtoActorClientServices
    {
	    private readonly IServiceCollection services;

	    public ProtoActorClientServices(IServiceCollection services)
	    {
		    this.services = services;
	    }

	    public IServiceCollection Services => this.services;

	    public ProtoActorClientServices AddDescriptorProvider<TProvider>() where TProvider : class, IDescriptorProvider
	    {
		    services.AddTransient<IDescriptorProvider, TProvider>();
		    return this;
	    }
    }
}
