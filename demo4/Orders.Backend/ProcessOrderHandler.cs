using System;
using System.Threading.Tasks;
using NServiceBus;

namespace Orders.Backend
{
    public class ProcessOrderHandler : IHandleMessages<ProcessOrder>
    {
        public Task Handle(ProcessOrder message, IMessageHandlerContext context)
        {
            Console.WriteLine(message.Total == message.DiscountedTotal ? $"Order #{message.OrderNumber}: No discount" : $"Order #{message.OrderNumber}: Got a discount of {message.Total - message.DiscountedTotal}");
            return Task.CompletedTask;
        }
    }
}
