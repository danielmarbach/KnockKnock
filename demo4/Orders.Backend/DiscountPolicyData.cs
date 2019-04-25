using System;
using NServiceBus;

namespace Orders.Backend
{
    public class DiscountPolicyData : ContainSagaData
    {
        public Guid CustomerId { get; set; }
        public decimal RunningTotal { get; set; }
    }
}
