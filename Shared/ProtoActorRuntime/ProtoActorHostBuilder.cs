using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.ProtoActorRuntime
{
    public interface IProtoActorHostBuilder
    {
        IDictionary<object, object> Properties { get; }

        IProtoActorHost Build();
        IProtoActorHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate);
        IProtoActorHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate);
        IProtoActorHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);
        IProtoActorHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory);
        IProtoActorHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate);
    }
    public class ProtoActorHostBuilder : IProtoActorHostBuilder
    {
        private bool built;
        private readonly ServiceProviderBuilder serviceProviderBuilder = new();
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        private readonly List<Action<IConfigurationBuilder>> configureHostConfigActions = new List<Action<IConfigurationBuilder>>();
        private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> configureAppConfigActions = new List<Action<HostBuilderContext, IConfigurationBuilder>>();
        private HostBuilderContext hostBuilderContext;
        private IConfiguration hostConfiguration;
        private IConfiguration appConfiguration;
        private IHostingEnvironment hostingEnvironment;

        public IProtoActorHost Build()
        {
            if (this.built)
                throw new InvalidOperationException($"{nameof(this.Build)} can only be called once per {nameof(ProtoActorHostBuilder)} instance.");
            this.built = true;

            this.ConfigureDefaults();
            BuildHostConfiguration();
            CreateHostingEnvironment();
            CreateHostBuilderContext();
            BuildAppConfiguration();

            var serviceProvider = CreateServiceProvider();

            return serviceProvider.GetRequiredService<IProtoActorHost>();
        }

        private IServiceProvider CreateServiceProvider()
        {
            this.ConfigureServices(
                services =>
                {
                    services.AddSingleton(this.hostingEnvironment);
                    services.AddSingleton(this.hostBuilderContext);
                    services.AddSingleton(this.appConfiguration);
                    services.AddSingleton<IHostApplicationLifetime, ProtoClusterApplicationLifetime>();
                    services.AddOptions();
                    services.AddLogging();
                });
            var serviceProvider = this.serviceProviderBuilder.BuildServiceProvider(this.hostBuilderContext);
            return serviceProvider;
        }

        private void BuildAppConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();

            // replace with: configBuilder.AddConfiguration(this.hostConfiguration);
            // This method was added post v2.0.0 of Microsoft.Extensions.Configuration
            foreach (var buildAction in this.configureHostConfigActions)
            {
                buildAction(configBuilder);
            }
            // end replace

            foreach (var buildAction in this.configureAppConfigActions)
            {
                buildAction(this.hostBuilderContext, configBuilder);
            }
            this.appConfiguration = configBuilder.Build();
            this.hostBuilderContext.Configuration = this.appConfiguration;
        }

        private void CreateHostBuilderContext()
        {
            this.hostBuilderContext = new HostBuilderContext(this.Properties)
            {
                HostingEnvironment = this.hostingEnvironment,
                Configuration = this.hostConfiguration
            };
        }

        private void CreateHostingEnvironment()
        {
            this.hostingEnvironment = new HostingEnvironment()
            {
                ApplicationName = this.hostConfiguration[HostDefaults.ApplicationKey],
                EnvironmentName = this.hostConfiguration[HostDefaults.EnvironmentKey] ?? EnvironmentName.Production,
            };
        }

        public IProtoActorHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) => throw new NotImplementedException();

        public IProtoActorHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) => throw new NotImplementedException();

        public IProtoActorHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) => throw new NotImplementedException();

        public IProtoActorHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) => throw new NotImplementedException();

        public IProtoActorHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) => throw new NotImplementedException();

        private void BuildHostConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            foreach (var buildAction in this.configureHostConfigActions)
            {
                buildAction(configBuilder);
            }
            this.hostConfiguration = configBuilder.Build();
        }
    }
}
