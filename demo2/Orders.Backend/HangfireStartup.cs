using System;
using System.Linq;
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
            BackgroundJob.Schedule(() => ScheduledJob(), TimeSpan.FromSeconds(10));
            RecurringJob.AddOrUpdate(() => ScheduledJob(), Cron.Minutely);
            return Task.CompletedTask;
        }

        public static void ScheduledJob()
        {
            Console.WriteLine("Running nightly batch for all customers");
            foreach (var customer in Database.GetCustomers())
            {
                var totalOfAllOrdersOfLastWeek = Database.LastWeekOrdersFor(customer.CustomerId)
                    .Sum(o => o.Total);

                if (totalOfAllOrdersOfLastWeek >= 500)
                {
                    Console.WriteLine($"Customer {customer.ShortId} will get discount.");
                    customer.GiveDiscount = true;
                }
            }
            Console.WriteLine("Done with nightly batch for all customers");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}