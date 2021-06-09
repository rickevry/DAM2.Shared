using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAM2.Shared.ProtoActorRuntime
{
    public interface IProtoActorHost : IDisposable, IAsyncDisposable
    {
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));

        IServiceProvider Services { get; }

        
    }
    public class ProtoActorHost : IProtoActorHost
    {
        private readonly Cluster cluster;
        private ProtoClusterApplicationLifetime applicationLifetime;
        private bool isDisposing;

        public ProtoActorHost(Cluster cluster, IServiceProvider services)
        {
            this.cluster = cluster;
            Services = services;
            

            this.applicationLifetime = services.GetService<IHostApplicationLifetime>() as ProtoClusterApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.cluster.StartMemberAsync().ConfigureAwait(false);
            this.applicationLifetime?.NotifyStarted();
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                this.applicationLifetime?.NotifyStopped();
                await this.cluster.ShutdownAsync().ConfigureAwait(false);
            }
            finally
            {
                this.applicationLifetime?.NotifyStopped();
            }
        }

        public IServiceProvider Services { get; }

        public void Dispose() => this.DisposeAsync().AsTask().GetAwaiter().GetResult();

        public async ValueTask DisposeAsync()
        {
            if (!isDisposing)
            {
                this.isDisposing = true;
                if (this.Services is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (this.Services is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
