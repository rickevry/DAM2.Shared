using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.ProtoActorRuntime
{
    internal class DelegateServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private readonly Func<IServiceCollection, IServiceProvider> containerBuilder;

        public DelegateServiceProviderFactory(Func<IServiceCollection, IServiceProvider> containerBuilder)
        {
            this.containerBuilder = containerBuilder;
        }

        /// <inheritdoc />
        public IServiceCollection CreateBuilder(IServiceCollection services) => services;

        /// <inheritdoc />
        public IServiceProvider CreateServiceProvider(IServiceCollection services) => this.containerBuilder(services);
    }
}
