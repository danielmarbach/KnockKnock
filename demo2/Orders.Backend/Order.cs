using System;

namespace Orders.Backend
{
    public class Order
    {
        public Guid CustomerId { get; set; }

        public decimal Total { get; set; }
    }
}