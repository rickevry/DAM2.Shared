using DAM2.Shared.ProtoActorRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting
{
    public static class HostExtensions
    {
        public static IHostBuilder UseProtoActorCluster(this IHostBuilder hostBuilder, Action<HostBuilderContext, IProtoActorBuilder> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));
            const string protoActorBuilderKey = "ProtoActorBuilder";
            ProtoActorBuilder protoActorBuilder;
            if (!hostBuilder.Properties.ContainsKey(protoActorBuilderKey))
            {
                protoActorBuilder = new ProtoActorBuilder(hostBuilder);
                hostBuilder.Properties.Add(protoActorBuilderKey, protoActorBuilder);
                hostBuilder.ConfigureServices((context, services) =>
                {
                    protoActorBuilder.Build(context, services);
                });
            }
            else
            {
                protoActorBuilder = (ProtoActorBuilder)hostBuilder.Properties[protoActorBuilderKey];
            }

            protoActorBuilder.ConfigureCluster(configureDelegate);
            return hostBuilder;
        }

        public static IHostBuilder UseProtoActorCluster(this IHostBuilder hostBuilder, Action<IProtoActorBuilder> configureDelegate)
        {
            if (configureDelegate == null) throw new ArgumentNullException(nameof(configureDelegate));
            return hostBuilder.UseProtoActorCluster((ctx, siloBuilder) => configureDelegate(siloBuilder));
        }
    }
}
