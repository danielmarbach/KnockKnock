using System;
using NServiceBus;

namespace Orders.Backend
{
    public class ProcessOrder : ICommand
    {
        public int OrderNumber { get; set; }
        public Guid CustomerId { get; set; }

        public decimal Total { get; set; }
        public decimal DiscountedTotal { get; set; }
    }
}
