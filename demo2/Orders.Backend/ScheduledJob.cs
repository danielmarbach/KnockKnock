using System;
using System.Linq;

namespace Orders.Backend
{
    public static class ScheduledJob 
    {
        public static void Run() 
        {
            #region
            Console.WriteLine("Running nightly batch for all customers");
            #endregion
            foreach (var customer in Database.GetCustomers())
            {
                var totalOfAllOrdersOfLastWeek = Database
                    .OrdersOfLastWeekFor(customer.CustomerId)
                    .Sum(o => o.Total);

                if (totalOfAllOrdersOfLastWeek >= 500)
                {
                    #region
                    Console.WriteLine($"Customer {customer.ShortId} will get discount.");
                    #endregion
                    customer.GiveDiscount = true;

                    Database.Save(customer);
                }
            }
            #region
            Console.WriteLine("Done with nightly batch for all customers");
            #endregion
        }
    }
}