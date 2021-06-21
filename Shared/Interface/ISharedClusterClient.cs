using System.Threading;
using System.Threading.Tasks;
using Proto.Cluster;

namespace DAM2.Core.Shared.Interface
{
    public interface ISharedClusterClient
    {
        Cluster Cluster { get; }
        Task<T> RequestAsync<T>(string actorPath, string clusterKind, object cmd, CancellationToken token = default);
        Task Startup();
        Task Shutdown();
    }
}
