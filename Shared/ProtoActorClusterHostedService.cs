using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAM2.Core.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DAM2.Shared
{
    public class ProtoActorClusterHostedService : IHostedService
    {
	    private readonly ILogger<ProtoActorClusterHostedService> logger;
	    private readonly SharedClusterWorker sharedClusterWorker;
	    private readonly IConfiguration configuration;
	    private readonly IHostApplicationLifetime applicationLifetime;

	    public ProtoActorClusterHostedService(ILogger<ProtoActorClusterHostedService> logger, 
		    SharedClusterWorker sharedClusterWorker, 
			IConfiguration configuration,
		    IHostApplicationLifetime applicationLifetime)
	    {
		    this.logger = logger;
		    this.sharedClusterWorker = sharedClusterWorker;
		    this.configuration = configuration;
		    this.applicationLifetime = applicationLifetime;
	    }
	    public async Task StartAsync(CancellationToken cancellationToken)
	    {
		    applicationLifetime.ApplicationStarted.Register(OnStarted);
		    applicationLifetime.ApplicationStopping.Register(OnStopping);
		    applicationLifetime.ApplicationStopped.Register(OnStopped);

		    await this.sharedClusterWorker.Run();
	    }

	    public async Task StopAsync(CancellationToken cancellationToken)
	    {
			await this.sharedClusterWorker.Shutdown();
		}

	    private void OnStopped()
	    {
		    logger.LogInformation("OnStopped has been called.");
	    }

	    private void OnStopping()
	    {
		    logger.LogInformation("SIGTERM received, waiting for 30 seconds");
			
			if(this.configuration.GetChildren().Any(c => c.Key.StartsWith("Kubernetes", StringComparison.OrdinalIgnoreCase)))
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
