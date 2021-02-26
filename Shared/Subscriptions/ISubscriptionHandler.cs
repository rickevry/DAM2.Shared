using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Core.Shared.Subscriptions
{
    public interface ISubscriptionHandler<in TCommand>
    {
	    Task Handle(TCommand updateEventCmd);
    }
}
