using System;
using System.Data;
using Hangfire;

namespace Orders.Backend
{
    public static class Schedule
    {
        #region
        public static TimeSpan InAWeek = TimeSpan.FromSeconds(10);
        #endregion

        [AutomaticRetry(Attempts = 10, DelaysInSeconds = new int[] { 0 })]
        public static void DecreaseRunningTotal(Guid orderCustomerId, decimal amountToDecrease)
        {
            try
            {
                var currentRunningTotal = Database.GetRunningTotal(orderCustomerId);
                currentRunningTotal.Total -= amountToDecrease;
                Database.Save(currentRunningTotal);
                #region
                Console.WriteLine($"Decreased running total of {orderCustomerId.Short()} by {amountToDecrease}");
                #endregion
            }
            catch (DBConcurrencyException)
            {
                #region 
                Console.WriteLine($"--> Failed to decrease running total of {orderCustomerId.Short()} by {amountToDecrease}");
                #endregion
                throw;
            }
        }
    }
}