using System;
using System.Linq;

namespace Orders.Backend
{
    public static class ScheduledJob 
    {
        public static void Run() 
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

                    Database.Save(customer);
                }
            }
            Console.WriteLine("Done with nightly batch for all customers");
        }
    }
}