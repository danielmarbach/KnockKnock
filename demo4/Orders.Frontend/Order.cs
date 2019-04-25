using System;

namespace Orders.Frontend
{
    class Order
    {
        public Guid CustomerId { get; set; }

        public decimal Total { get; set; }
    }
}