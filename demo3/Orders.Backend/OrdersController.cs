using System;
using System.Data;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Orders.Backend
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            var currentRunningTotal = Database.GetRunningTotal(order.CustomerId);

            var currentOrderTotal = order.Total;

            currentRunningTotal.Total += currentOrderTotal;

            Database.Save(currentRunningTotal);

            var discount = 0m;
            if (currentRunningTotal.Total >= 500)
            {
                discount = 0.1m;
            }

            order.Total -= order.Total * discount;

            Database.Save(order);

            BackgroundJob.Schedule(() => DecreaseRunningTotal(order.CustomerId, currentOrderTotal), Schedule.InAWeek);

            return Ok(order);
        }

        [AutomaticRetry(Attempts = 10, DelaysInSeconds = new int[] { 0 })]
        public static void DecreaseRunningTotal(Guid orderCustomerId, decimal amountToDecrease)
        {
            try
            {
                var currentRunningTotal = Database.GetRunningTotal(orderCustomerId);

                currentRunningTotal.Total -= amountToDecrease;

                Database.Save(currentRunningTotal);

                Console.WriteLine($"Decreased running total of {orderCustomerId} by {amountToDecrease}");
            }
            catch (DBConcurrencyException)
            {
                Console.WriteLine($"Failed to decrease running total of {orderCustomerId} by {amountToDecrease}");
                throw;
            }
        }
    }
}