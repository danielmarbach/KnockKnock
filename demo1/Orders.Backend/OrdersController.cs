using System.Linq;
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

        [HttpPost]
        public IActionResult Post([FromBody] Order order)
        {
            var totalOfAllOrdersOfLastWeek = Database.LastWeekOrdersFor(order.CustomerId).Sum(o => o.Total);

            var discount = 0m;
            if (totalOfAllOrdersOfLastWeek >= 500)
            {
                discount = 0.1m;
            }

            order.Total -= order.Total * discount;

            Database.Save(order);

            return Ok(order);
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            applicationLifetime.StopApplication();
            return Ok();
        }
    }
}