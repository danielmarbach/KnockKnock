using System.Linq;
using System.Threading.Tasks;
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
    }
}