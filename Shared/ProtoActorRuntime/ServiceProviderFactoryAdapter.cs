using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAM2.Shared.ProtoActorRuntime
{
    internal interface IServiceProviderFactoryAdapter
    {
        /// <summary>
        /// Creates a <see cref="IServiceProvider"/> from an <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="context">The host builder context.</param>
        /// <param name="services">The collection of services.</param>
        /// <returns>A <see cref="IServiceProvider" />.</returns>
        IServiceProvider BuildServiceProvider(HostBuilderContext context, IServiceCollection services);

        /// <summary>
        /// Adds a container configuration delegate.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The container builder type.</typeparam>
        /// <param name="configureContainer">The container builder configuration delegate.</param>
        void ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureContainer);
    }

    internal class ServiceProviderFactoryAdapter<TContainerBuilder> : IServiceProviderFactoryAdapter
    {
        private readonly IServiceProviderFactory<TContainerBuilder> serviceProviderFactory;
        private readonly List<Action<HostBuilderContext, TContainerBuilder>> configureContainerDelegates = new List<Action<HostBuilderContext, TContainerBuilder>>();

        public ServiceProviderFactoryAdapter(IServiceProviderFactory<TContainerBuilder> serviceProviderFactory)
        {
            this.serviceProviderFactory = serviceProviderFactory;
        }

        /// <inheritdoc />
        public IServiceProvider BuildServiceProvider(HostBuilderContext context, IServiceCollection services)
        {
            var builder = this.serviceProviderFactory.CreateBuilder(services);

            foreach (var configureContainer in this.configureContainerDelegates)
            {
                configureContainer(context, builder);
            }

            return this.serviceProviderFactory.CreateServiceProvider(builder);
        }

        /// <inheritdoc />
        public void ConfigureContainer<TBuilder>(Action<HostBuilderContext, TBuilder> configureContainer)
        {
            if (configureContainer == null) throw new ArgumentNullException(nameof(configureContainer));
            var typedDelegate = configureContainer as Action<HostBuilderContext, TContainerBuilder>;
            if (typedDelegate == null)
            {
                var msg = $"Type of configuration delegate requires builder of type {typeof(TBuilder)} which does not match previously configured container builder type {typeof(TContainerBuilder)}.";
                throw new InvalidCastException(msg);
            }

            this.configureContainerDelegates.Add(typedDelegate);
        }
    }
}
