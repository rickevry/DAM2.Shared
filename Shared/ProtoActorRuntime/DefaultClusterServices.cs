using DAM2.Core.Shared;
using DAM2.Core.Shared.Interface;
using DAM2.Core.Shared.Settings;
using DAM2.Shared.Shared;
using DAM2.Shared.Shared.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DAM2.Shared.ProtoActorRuntime
{
    internal static class DefaultClusterServices
    {
        internal static void AddDefaultServices(IServiceCollection services)
        {
            services.AddOptions();

            services.TryAddSingleton<IProtoActorHost, ProtoActorHost>();

            services.AddSingleton<ISharedClusterProviderFactory, SharedClusterProviderFactory>();
            services.AddTransient<ITokenFactory, TokenFactory>();

            services.AddSingleton<IClusterSettings>(_ => _.GetRequiredService<IOptions<ClusterSettings>>().Value);

            services.AddSingleton<SharedClusterWorker>();
        }
    }
}
