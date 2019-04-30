using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace Orders.Backend
{
    class HangfireStartup : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            BackgroundJob.Schedule(() => ScheduledJob.Run(), TimeSpan.FromSeconds(10));
            RecurringJob.AddOrUpdate(() => ScheduledJob.Run(), Cron.Minutely);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}