using System;
using System.Data;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Orders.Backend
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IApplicationLifetime applicationLifetime;

        public OrdersController(IApplicationLifetime applicationLifetime)
        {
            this.applicationLifetime = applicationLifetime;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpPut("/concurrency")]
        public IActionResult Put([FromQuery] bool enable)
        {
            Database.IncreaseChanceForConcurrencyException(enable);
            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            applicationLifetime.StopApplication();
            return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            var currentRunningTotal = Database.GetRunningTotal(order.CustomerId);
            var totalOfAllOrdersOfLastWeek = currentRunningTotal.Total;
            var currentOrderTotal = order.Total;
            currentRunningTotal.Total += currentOrderTotal;
            Database.Save(currentRunningTotal);

            var discount = 0m;
            if (totalOfAllOrdersOfLastWeek >= 500)
            {
                discount = 0.1m;
            }

            order.Total -= order.Total * discount;
            Database.Save(order);

            BackgroundJob.Schedule(() => Schedule.DecreaseRunningTotal(order.CustomerId, currentOrderTotal), Schedule.InAWeek);

            return Ok(order);
        }
    }
}