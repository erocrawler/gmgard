using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace GmGard.Services
{
    public class BackgroundJobService : BackgroundService
    {
        private ILogger _logger;
        private BackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundJobService(BackgroundTaskQueue taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
        {
            _logger = loggerFactory.CreateLogger<BackgroundJobService>();
            _queue = taskQueue;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting BackgroundJobService");
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem =
                    await _queue.DequeueAsync(stoppingToken);

                try
                {
                    await RunJobAsync(workItem);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
            _logger.LogInformation("Stopping BackgroundJobService");
        }

        private async Task RunJobAsync(Job job)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<JobTaskRunner>();
                await runner.RunJob(job);
            }
        }
    }
}
