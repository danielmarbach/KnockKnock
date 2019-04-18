using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Orders.Backend
{
    public static class Database
    {
        static Random random = new Random();
        static readonly ConcurrentDictionary<Guid, ConcurrentBag<Order>> ordersPerCustomer = new ConcurrentDictionary<Guid, ConcurrentBag<Order>>();

        public static IEnumerable<Order> LastWeekOrdersFor(Guid customerId)
        {
            Thread.Sleep(random.Next(500, 1000));
            return ordersPerCustomer.GetOrAdd(customerId, new ConcurrentBag<Order>());
        }

        public static void Save(Order order)
        {
            var orders = ordersPerCustomer.GetOrAdd(order.CustomerId, new ConcurrentBag<Order>());
            orders.Add(order);
        }
    }
}