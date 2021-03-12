using System.Threading.Tasks;

namespace DAM2.Core.Shared.Interface
{
    public interface ISharedClusterClient
    {
        Task<T> RequestAsync<T>(string actorPath, string clusterKind, object cmd);

        Task Startup();
        Task Shutdown();
    }
}
