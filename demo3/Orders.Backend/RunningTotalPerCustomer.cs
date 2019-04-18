using System;

namespace Orders.Backend
{
    public class RunningTotalPerCustomer
    {
        public RunningTotalPerCustomer(long version, Guid customerId)
        {
            Version = version;
            CustomerId = customerId;
        }
        public long Version { get; }
        public Guid CustomerId { get; }

        public decimal Total { get; set; }
    }
}