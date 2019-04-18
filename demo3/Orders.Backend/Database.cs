using System;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;

namespace Orders.Backend
{
    public static class Database
    {
        static Random random = new Random();
        static readonly ConcurrentDictionary<Guid, ConcurrentBag<Order>> ordersPerCustomer = new ConcurrentDictionary<Guid, ConcurrentBag<Order>>();
        static readonly ConcurrentDictionary<Guid, RunningTotalPerCustomer> runningTotalPerCustomers = new ConcurrentDictionary<Guid, RunningTotalPerCustomer>();
        private static long runningTotalVersion;
        static Action delayAction = () => { };

        public static void IncreaseChanceForConcurrencyException(bool enable)
        {
            if (enable)
            {
                delayAction = () => Thread.Sleep(random.Next(0, 500));
            }
            else
            {
                delayAction = () => { };
            }
        }

        public static RunningTotalPerCustomer GetRunningTotal(Guid customerId)
        {
            var currentRunningTotal = runningTotalPerCustomers.GetOrAdd(customerId,
                new RunningTotalPerCustomer(Interlocked.Read(ref runningTotalVersion), customerId));

            return new RunningTotalPerCustomer(currentRunningTotal.Version, currentRunningTotal.CustomerId)
            {
                Total = currentRunningTotal.Total
            };
        }

        public static void Save(Order order)
        {
            var orders = ordersPerCustomer.GetOrAdd(order.CustomerId, new ConcurrentBag<Order>());
            orders.Add(order);
        }

        public static void Save(RunningTotalPerCustomer total)
        {
            delayAction();

            runningTotalPerCustomers.AddOrUpdate(total.CustomerId, total, (id, current) =>
            {
                if (current.Version != total.Version)
                {
                    throw new DBConcurrencyException();
                }

                return new RunningTotalPerCustomer(Interlocked.Increment(ref runningTotalVersion), total.CustomerId)
                {
                    Total = total.Total
                };
            });
        }
    }
}