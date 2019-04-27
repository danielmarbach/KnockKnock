using System;
using System.Threading.Tasks;
using Contracts;
using NServiceBus.Pipeline;

namespace Orders.Backend
{
    class CustomBehavior : Behavior<IIncomingLogicalMessageContext>
    {
        public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            try
            {
                await next().ConfigureAwait(false);
            }
            catch (Exception)
            {
                var submitOrder = (SubmitOrder)context.Message.Instance;
                Console.WriteLine($"--> Order #{submitOrder.OrderNumber}: Value {submitOrder.Total} for customer {submitOrder.CustomerId.Short()} will be retried due to concurrency conflict.");
                throw;
            }
        }
    }
}
