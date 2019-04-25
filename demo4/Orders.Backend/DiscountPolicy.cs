using System;
using System.Threading.Tasks;
using Contracts;
using NServiceBus;

namespace Orders.Backend
{
    public class DiscountPolicy : Saga<DiscountPolicyData>,
        IAmStartedByMessages<SubmitOrder>,
        IHandleTimeouts<SubmitOrder>
    {
        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            Data.CustomerId = message.CustomerId;
            Data.RunningTotal += message.Total;

            var discount = 0m;
            if (Data.RunningTotal >= 500)
            {
                discount = 0.1m;
            }

            await context.SendLocal(new ProcessOrder { DiscountedTotal = message.Total - (message.Total * discount),
                CustomerId = message.CustomerId, OrderNumber = message.OrderNumber, Total = message.Total, });

            await RequestTimeout(context, Schedule.InAWeek, message);
        }

        public Task Timeout(SubmitOrder state, IMessageHandlerContext context)
        {
            Data.RunningTotal -= state.Total;
            Console.WriteLine($"Decreased running total of {state.CustomerId.Short()} by {state.Total}");
            return Task.CompletedTask;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<DiscountPolicyData> mapper)
        {
            mapper.ConfigureMapping<SubmitOrder>(m => m.CustomerId).ToSaga(s => s.CustomerId);
        }
    }
}
