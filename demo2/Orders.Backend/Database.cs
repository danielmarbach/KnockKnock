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
        static readonly ConcurrentDictionary<Guid, Customer> customers = new ConcurrentDictionary<Guid, Customer>();

        public static Customer GetCustomerBy(Guid customerId)
        {
            Thread.Sleep(random.Next(500, 1000));
            return customers.GetOrAdd(customerId, new Customer { CustomerId = customerId });
        }

        public static IEnumerable<Customer> GetCustomers()
        {
            return customers.Values;
        }

        public static IEnumerable<Order> LastWeekOrdersFor(Guid customerId)
        {
            customers.TryAdd(customerId, new Customer());
            return ordersPerCustomer.GetOrAdd(customerId, new ConcurrentBag<Order>());
        }

        public static void Save(Order order)
        {
            var orders = ordersPerCustomer.GetOrAdd(order.CustomerId, new ConcurrentBag<Order>());
            orders.Add(order);
        }
    }
}