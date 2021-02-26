using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Core.Actors.Shared.Generic
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActorAttribute : Attribute
    {
		public ActorAttribute(string kind)
	    {
		    this.Kind = kind;
	    }
	    public string Kind { get; set; }
    }
}
