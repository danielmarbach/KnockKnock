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
            var provider = await CreateProviderAndWaitUntilBackendReady();

            var httpClient = provider.GetService<IHttpClientFactory>().CreateClient("backend");

            var customerId = Guid.NewGuid();

            for (var i = 0; i < 5; i++)
            {
                await NewOrder(customerId, httpClient);
            }

            await Task.Delay(15000);

            await NewOrder(customerId, httpClient);

            await EnableConcurrencyMode(httpClient);

            await Task.WhenAll(
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient),
                NewOrder(customerId, httpClient));

            await DisableConcurrencyMode(httpClient);

            await Shutdown(httpClient);
        }

        private static async Task NewOrder(Guid customerId, HttpClient httpClient)
        {
            var currentOrderNumber = Interlocked.Increment(ref orderNumber);

            var order = new Order
            {
                CustomerId = customerId,
                Total = 300,
            };

            Console.WriteLine($"Order #{currentOrderNumber}: Value {order.Total} for customer {order.CustomerId.Short()}");
            var orderResponse = await httpClient.PostAsync("api/orders",
                new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json"));
            orderResponse.EnsureSuccessStatusCode();
            var returnedOrder = JsonConvert.DeserializeObject<Order>(await orderResponse.Content.ReadAsStringAsync());
            Console.WriteLine(returnedOrder.Total == order.Total ? $"Order #{currentOrderNumber}: No discount" : $"Order #{currentOrderNumber}: Got a discount of {order.Total - returnedOrder.Total}");
        }

        private static async Task DisableConcurrencyMode(HttpClient httpClient)
        {
            await httpClient.PutAsync("/concurrency?enable=false", new StringContent(string.Empty));
        }

        private static async Task EnableConcurrencyMode(HttpClient httpClient)
        {
            Console.WriteLine("Concurrency");
            Console.WriteLine();

            await httpClient.PutAsync("/concurrency?enable=true", new StringContent(string.Empty));
        }

        private static async Task Shutdown(HttpClient httpClient)
        {
            await Task.Delay(15000);
            await httpClient.DeleteAsync("api/orders");
        }

        private static async Task<ServiceProvider> CreateProviderAndWaitUntilBackendReady()
        {
            var services = new ServiceCollection();
            services.AddHttpClient("backend",
                    client => { client.BaseAddress = new Uri("http://orders.backend.future:8080/"); })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(5, i =>
                {
                    Console.WriteLine($"--> Client side retry #{i}");
                    return TimeSpan.FromSeconds(1);
                }));

            services.AddHttpClient("backend-health",
                    client => { client.BaseAddress = new Uri("http://orders.backend.future:8080/"); })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(1)));

            var provider = services.BuildServiceProvider();

            var httpClient = provider.GetService<IHttpClientFactory>().CreateClient("backend-health");

            // health check
            await httpClient.GetAsync("api/orders");

            Console.WriteLine("Ready");
            Console.WriteLine();
            return provider;
        }

        private static int orderNumber;
    }
}
