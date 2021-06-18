using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;

namespace DAM2.Shared.Caching
{
    public class CacheActor : IActor
    {
	    private readonly IProtoCacheStorageProvider storageProvider;
	    private string key;
	    private CacheEntry item;

	    public CacheActor(IProtoCacheStorageProvider storageProvider)
	    {
		    this.storageProvider = storageProvider;
	    }

	    public Task ReceiveAsync(IContext context)
	    {
		    Task task = context.Message switch
		    {
			    Started _ => Started(context)
		    };

		    return task;
	    }

	    private async Task Started(IContext context)
	    {
		    try
		    {
			    this.key = context.Self.Id;
			    this.item = await this.storageProvider.Get(key);
		    }
		    catch (Exception e)
		    {
			    
		    }
	    }
    }
}
