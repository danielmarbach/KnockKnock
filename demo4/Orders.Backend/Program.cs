using System;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Persistence.Sql;

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
                        await tcpClientB.ConnectAsync("orders.rabbitmq.nsb", 5672);
                        success = true;
                    }
                }
                catch (Exception)
                {
                    await Task.Delay(2000, tokenSource.Token);
                }
            } while (!success);

            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Info);

            var endpointConfiguration = new EndpointConfiguration("Orders.Backend");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(
                connectionBuilder: () =>
                {
                    return new SqlConnection(@"Server=orders.backend.db.nsb;Initial Catalog=master;User Id=sa;Password=Your_password123;");
                });

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=orders.rabbitmq.nsb;username=rabbitmq.nsb;password=rabbitmq.nsb");
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
