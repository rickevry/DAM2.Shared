using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.ProtoActorRuntime
{
    public class HostBuilderContext
    {
        public HostBuilderContext(IDictionary<object, object> properties)
        {
            Properties = properties ?? throw new System.ArgumentNullException(nameof(properties));
        }

        /// <summary>
        /// The <see cref="IHostingEnvironment" /> initialized by the host.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; set; }

        /// <summary>
        /// The <see cref="IConfiguration" /> containing the merged configuration of the application and the host.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// A central location for sharing state between components during the host building process.
        /// </summary>
        public IDictionary<object, object> Properties { get; }
    }
}
