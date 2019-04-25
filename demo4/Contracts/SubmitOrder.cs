using NServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public class SubmitOrder : ICommand
    {
        public int OrderNumber { get; set; }
        public Guid CustomerId { get; set; }

        public decimal Total { get; set; }
    }
}
