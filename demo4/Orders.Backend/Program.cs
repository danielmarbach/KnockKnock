using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

namespace Orders.Backend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += CancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;

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
                    await Task.Delay(2000, tokenSource.Token);
                }
            } while (!success);

            var endpointConfiguration = new EndpointConfiguration("Orders.Backend");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.UsePersistence<LearningPersistence>();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq.nsb;username=rabbitmq.nsb;password=rabbitmq.nsb");
            transport.UseConventionalRoutingTopology();

            var endpoint = await Endpoint.Start(endpointConfiguration);

            await semaphore.WaitAsync();

            await endpoint.Stop();
        }

        static SemaphoreSlim semaphore = new SemaphoreSlim(0);
        static CancellationTokenSource tokenSource = new CancellationTokenSource();

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            semaphore.Release();
            tokenSource.Cancel();
        }

        static void ProcessExit(object sender, EventArgs e)
        {
            semaphore.Release();
            tokenSource.Cancel();
        }
    }
}
