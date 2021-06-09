using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.ProtoActorRuntime
{
    public static class ProtoActorHostBuilderExtensions
    {
        public static IProtoActorHostBuilder ConfigureServices(this IProtoActorHostBuilder hostBuilder, Action<IServiceCollection> configureDelegate)
        {
            return hostBuilder.ConfigureServices((context, collection) => configureDelegate(collection));
        }

        public static IProtoActorHostBuilder ConfigureAppConfiguration(this IProtoActorHostBuilder hostBuilder, Action<IConfigurationBuilder> configureDelegate)
        {
            return hostBuilder.ConfigureAppConfiguration((context, builder) => configureDelegate(builder));
        }

        public static IProtoActorHostBuilder Configure<TOptions>(this IProtoActorHostBuilder builder, Action<TOptions> configureOptions) where TOptions : class
        {
            return builder.ConfigureServices(services => services.Configure(configureOptions));
        }

        public static IProtoActorHostBuilder Configure<TOptions>(this IProtoActorHostBuilder builder, IConfiguration configuration) where TOptions : class
        {
            return builder.ConfigureServices(services => services.AddOptions<TOptions>().Bind(configuration));
        }

        public static IProtoActorHostBuilder UseServiceProviderFactory<TContainerBuilder>(IProtoActorHostBuilder builder, IServiceProviderFactory<TContainerBuilder> factory)
        {
            return builder.UseServiceProviderFactory(services => factory.CreateServiceProvider(factory.CreateBuilder(services)));
        }

        public static IProtoActorHostBuilder UseServiceProviderFactory(this IProtoActorHostBuilder builder, Func<IServiceCollection, IServiceProvider> configureServiceProvider)
        {
            if (configureServiceProvider == null) throw new ArgumentNullException(nameof(configureServiceProvider));
            return builder.UseServiceProviderFactory(new DelegateServiceProviderFactory(configureServiceProvider));
        }

        public static IProtoActorHostBuilder ConfigureLogging(this IProtoActorHostBuilder builder, Action<HostBuilderContext, ILoggingBuilder> configureLogging)
        {
            return builder.ConfigureServices((context, collection) => collection.AddLogging(loggingBuilder => configureLogging(context, loggingBuilder)));
        }

        public static IProtoActorHostBuilder ConfigureLogging(this IProtoActorHostBuilder builder, Action<ILoggingBuilder> configureLogging)
        {
            return builder.ConfigureServices(collection => collection.AddLogging(configureLogging));
        }
    }
}
