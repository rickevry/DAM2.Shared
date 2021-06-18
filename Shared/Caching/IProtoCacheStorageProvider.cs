using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.Caching
{
    public interface IProtoCacheStorageProvider
    {
	    Task<CacheEntry> Get(string key);

	    Task Set(CacheEntry cacheEntry);
    }
}
