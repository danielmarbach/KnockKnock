using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using NServiceBus;
using NServiceBus.Logging;

namespace Orders.Frontend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await WaitUntilRabbitMQReady();
            var endpoint = await CreateEndpointInstance();

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

            await endpoint.Stop();
        }

        private static async Task NewOrder(Guid customerId, IMessageSession messageSession)
        {
            var currentOrderNumber = Interlocked.Increment(ref orderNumber);

            var order = new SubmitOrder
            {
                OrderNumber = currentOrderNumber,
                CustomerId = customerId,
                Total = 300,
            };

            Console.WriteLine($"Order #{currentOrderNumber}: Value {order.Total} for customer {order.CustomerId.Short()}");

            await messageSession.Send(order);
        }

        private static async Task<IEndpointInstance> CreateEndpointInstance()
        {
            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Fatal);

            var endpointConfiguration = new EndpointConfiguration("Orders.Frontend");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.SendOnly();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=orders.rabbitmq.nsb;username=rabbitmq.nsb;password=rabbitmq.nsb");
            transport.UseConventionalRoutingTopology();
            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(SubmitOrder), "Orders.Backend");

            var endpoint = await Endpoint.Start(endpointConfiguration);

            Console.WriteLine("Ready");
            Console.WriteLine();
            return endpoint;
        }

        private static async Task WaitUntilRabbitMQReady()
        {
            var success = false;
            do
            {
                try
                {
                    using (var tcpClientB = new TcpClient())
                    {
                        await tcpClientB.ConnectAsync("orders.rabbitmq.nsb", 5672);
                        success = true;
                    }
                }
                catch (Exception)
                {
                    await Task.Delay(2000);
                }
            } while (!success);
        }

        private static int orderNumber;
    }
}
