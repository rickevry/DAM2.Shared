using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.ProtoActorRuntime
{
    public interface IHostingEnvironment
    {
        string EnvironmentName { get; set; }

        
        string ApplicationName { get; set; }
    }

    internal class HostingEnvironment : IHostingEnvironment
    {
        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; }
    }

    public static class HostDefaults
    {
        /// <summary>
        /// The configuration key used to set <see cref="IHostingEnvironment.ApplicationName"/>.
        /// </summary>
        public static readonly string ApplicationKey = "applicationName";

        /// <summary>
        /// The configuration key used to set <see cref="IHostingEnvironment.EnvironmentName"/>.
        /// </summary>
        public static readonly string EnvironmentKey = "environment";
    }

    public static class EnvironmentName
    {
        public static readonly string Development = "Development";
        public static readonly string Staging = "Staging";
        public static readonly string Production = "Production";
    }

}
