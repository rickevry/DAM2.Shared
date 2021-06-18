using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace DAM2.Shared.Caching
{
    public class ProtoActorDistributedCache : IDistributedCache
    {
	    public byte[] Get(string key)
	    {
		    throw new NotImplementedException();
	    }

	    public Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
	    {
		    throw new NotImplementedException();
	    }

	    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
	    {
		    throw new NotImplementedException();
	    }

	    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
		    CancellationToken token = new CancellationToken())
	    {
		    throw new NotImplementedException();
	    }

	    public void Refresh(string key)
	    {
		    throw new NotImplementedException();
	    }

	    public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
	    {
		    throw new NotImplementedException();
	    }

	    public void Remove(string key)
	    {
		    throw new NotImplementedException();
	    }

	    public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
	    {
		    throw new NotImplementedException();
	    }
    }
}
