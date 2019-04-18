using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;

namespace Orders.Frontend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddHttpClient("backend",
                client => { client.BaseAddress = new Uri("http://orders.backend.future:8080/"); })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(5, i =>
                {
                    Console.WriteLine($"Client side retry #{i}");
                    return TimeSpan.FromSeconds(1);
                }));

            services.AddHttpClient("backend-health",
                    client => { client.BaseAddress = new Uri("http://orders.backend.future:8080/"); })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(1)));

            var provider = services.BuildServiceProvider();

            var httpClient = provider.GetService<IHttpClientFactory>().CreateClient("backend-health");

            // health check
            await httpClient.GetAsync("api/orders");

            httpClient = provider.GetService<IHttpClientFactory>().CreateClient("backend");

            Console.WriteLine("Ready");
            Console.WriteLine();

            var customerId = Guid.NewGuid();

            for (var i = 0; i < 5; i++)
            {
                await NewOrder(customerId, httpClient);
            }

            await Task.Delay(15000);

            await NewOrder(customerId, httpClient);

            Console.WriteLine("Concurrency");
            Console.WriteLine();

            await httpClient.PutAsync("/concurrency?enable=true", new StringContent(string.Empty));

            await Task.WhenAll(
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient));

            await httpClient.PutAsync("/concurrency?enable=false", new StringContent(string.Empty));

            Console.WriteLine();
        }

        private static int orderNumber;

        private static async Task NewOrder(Guid customerId, HttpClient httpClient)
        {
            var currentOrderNumber = Interlocked.Increment(ref orderNumber);

            var order = new Order
            {
                CustomerId = customerId,
                Total = 300,
            };

            var id = order.CustomerId.ToString();
            Console.WriteLine($"Order #{currentOrderNumber}: Value {order.Total} for customer {id.Substring(id.Length -7, 7)}");
            var orderResponse = await httpClient.PostAsync("api/orders",
                new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json"));
            orderResponse.EnsureSuccessStatusCode();
            var returnedOrder = JsonConvert.DeserializeObject<Order>(await orderResponse.Content.ReadAsStringAsync());
            Console.WriteLine(returnedOrder.Total == order.Total ? $"Order #{currentOrderNumber}: No discount" : $"Order #{currentOrderNumber}: Got a discount of {order.Total - returnedOrder.Total}");
        }
    }
}
