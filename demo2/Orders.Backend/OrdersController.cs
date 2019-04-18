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
            var customer = Database.GetCustomerBy(order.CustomerId);

            var discount = 0m;
            if (customer.GiveDiscount)
            {
                discount = 0.1m;
            }

            order.Total -= order.Total * discount;

            Database.Save(order);

            return Ok(order);
        }
    }
}