using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DAM2.Core.Shared;
using DAM2.Core.Shared.Interface;
using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DAM2.Shared
{
    public class ProtoActorClientHostedService : IHostedService
    {
	    private readonly ILogger<ProtoActorClientHostedService> logger;
	    private readonly ISharedClusterClient sharedClusterClient;
	    private readonly IConfiguration configuration;
	    private readonly IHostApplicationLifetime applicationLifetime;

	    public ProtoActorClientHostedService(ILogger<ProtoActorClientHostedService> logger, 
		    ISharedClusterClient sharedClusterClient, 
			IConfiguration configuration,
		    IHostApplicationLifetime applicationLifetime)
	    {
		    this.logger = logger;
		    this.sharedClusterClient = sharedClusterClient;
		    this.configuration = configuration;
		    this.applicationLifetime = applicationLifetime;
	    }
	    public async Task StartAsync(CancellationToken cancellationToken)
	    {
		    applicationLifetime.ApplicationStarted.Register(OnStarted);
		    applicationLifetime.ApplicationStopping.Register(OnStopping);
		    applicationLifetime.ApplicationStopped.Register(OnStopped);

		    await this.sharedClusterClient.Startup();
	    }

	    public async Task StopAsync(CancellationToken cancellationToken)
	    {
		    await this.sharedClusterClient.Shutdown();
	    }

	    private void OnStopped()
	    {
		    logger.LogInformation("OnStopped has been called.");
	    }

	    private void OnStopping()
	    {
		    logger.LogInformation("SIGTERM received, waiting for 30 seconds");
			if (this.configuration.GetChildren().Any(c => c.Key.StartsWith("Kubernetes", StringComparison.OrdinalIgnoreCase)))
			{
				Thread.Sleep(30_000);
			}
			logger.LogInformation("Termination delay complete, continuing stopping process");
	    }

	    private void OnStarted()
	    {
		    logger.LogInformation("OnStarted has been called.");
	    }
	}
}
