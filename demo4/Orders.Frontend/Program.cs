using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using NServiceBus;

namespace Orders.Frontend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool success = false;
            do
            {
                try
                {
                    using (var tcpClientB = new TcpClient())
                    {
                        await tcpClientB.ConnectAsync("rabbitmq.nsb", 5672);
                        success = true;
                    }
                }
                catch (Exception)
                {
                    await Task.Delay(2000);
                }
            } while (!success);

            var endpointConfiguration = new EndpointConfiguration("Orders.Frontend");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq.nsb;username=rabbitmq.nsb;password=rabbitmq.nsb");
            transport.UseConventionalRoutingTopology();
            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(SubmitOrder), "Orders.Backend");

            var endpoint = await Endpoint.Start(endpointConfiguration);

            Console.WriteLine("Ready");
            Console.WriteLine();

            var customerId = Guid.NewGuid();

            for (var i = 0; i < 5; i++)
            {
                await NewOrder(customerId, endpoint);
            }

            await Task.Delay(15000);

            await NewOrder(customerId, endpoint);

            Console.WriteLine("Concurrency");
            Console.WriteLine();

            await Task.WhenAll(
                NewOrder(customerId, endpoint),
                NewOrder(customerId, endpoint),
                NewOrder(customerId, endpoint),
                NewOrder(customerId, endpoint),
                NewOrder(customerId, endpoint),
                NewOrder(customerId, endpoint),
                NewOrder(customerId, endpoint),
                NewOrder(customerId, endpoint));

            Console.WriteLine();

            await endpoint.Stop();
        }

        private static int orderNumber;

        private static async Task NewOrder(Guid customerId, IMessageSession messageSession)
        {
            var currentOrderNumber = Interlocked.Increment(ref orderNumber);

            var order = new SubmitOrder
            {
                OrderNumber = currentOrderNumber,
                CustomerId = customerId,
                Total = 300,
            };

            var id = order.CustomerId.ToString();
            Console.WriteLine($"Order #{currentOrderNumber}: Value {order.Total} for customer {id.Substring(id.Length -7, 7)}");

            await messageSession.Send(order);
        }
    }
}
