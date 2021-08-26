using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace DAM2.Shared.ProtoActorRuntime
{
    public interface IProtoActorBuilder
    {
        IProtoActorBuilder ConfigureServices(Action<Microsoft.Extensions.Hosting.HostBuilderContext, IServiceCollection> configureDelegate);
        IDictionary<object, object> Properties { get; }
    }
    internal class ProtoActorBuilder : IProtoActorBuilder
    {
        private readonly IHostBuilder hostBuilder;
        private readonly List<Action<Microsoft.Extensions.Hosting.HostBuilderContext, IProtoActorBuilder>> configureClusterDelegates = new();
        private readonly List<Action<Microsoft.Extensions.Hosting.HostBuilderContext, IServiceCollection>> configureServicesDelegates = new();

        public IDictionary<object, object> Properties => this.hostBuilder.Properties;

        public ProtoActorBuilder(IHostBuilder hostBuilder)
        {
            this.hostBuilder = hostBuilder;
        }

        public void Build(Microsoft.Extensions.Hosting.HostBuilderContext context, IServiceCollection serviceCollection)
        {
            foreach (var configurationDelegate in this.configureClusterDelegates)
            {
                configurationDelegate(context, this);
            }

            serviceCollection.AddHostedService<ProtoActorHostedService>();
            this.ConfigureDefaults();

            foreach (var configurationDelegate in this.configureServicesDelegates)
            {
                configurationDelegate(context, serviceCollection);
            }
        }

        public IProtoActorBuilder ConfigureCluster(Action<Microsoft.Extensions.Hosting.HostBuilderContext, IProtoActorBuilder> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));
            this.configureClusterDelegates.Add(configureDelegate);
            return this;
        }

        public IProtoActorBuilder ConfigureServices(Action<Microsoft.Extensions.Hosting.HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));
            this.configureServicesDelegates.Add(configureDelegate);
            return this;
        }

        
    }

    
}
