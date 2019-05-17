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

        public static IEnumerable<Order> OrdersOfLastWeekFor(Guid customerId)
        {
            return ordersPerCustomer.GetOrAdd(customerId, new ConcurrentBag<Order>());
        }

        public static void Save(Order order)
        {
            Thread.Sleep(random.Next(500, 1000));
            var orders = ordersPerCustomer.GetOrAdd(order.CustomerId, new ConcurrentBag<Order>());
            orders.Add(order);
        }
    }
}