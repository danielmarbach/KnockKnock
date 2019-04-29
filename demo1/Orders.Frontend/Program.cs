using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Orders.Frontend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var httpClient = new HttpClient {BaseAddress = new Uri("http://orders.backend:8080/") };

            await WaitUntilBackendReady(httpClient);

            var customerId = Guid.NewGuid();

            await Task.WhenAll(
                NewOrder(customerId, httpClient), 
                NewOrder(customerId, httpClient), 
                NewOrder(customerId, httpClient));

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

            var id = order.CustomerId.ToString();
            Console.WriteLine($"Order #{currentOrderNumber}: Value {order.Total} for customer {id.Substring(id.Length -7, 7)}");
            var orderResponse = await httpClient.PostAsync("api/orders",
                new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json"));
            orderResponse.EnsureSuccessStatusCode();
            var returnedOrder = JsonConvert.DeserializeObject<Order>(await orderResponse.Content.ReadAsStringAsync());
            Console.WriteLine(returnedOrder.Total == order.Total ? $"Order #{currentOrderNumber}: No discount" : $"Order #{currentOrderNumber}: Got a discount of {order.Total - returnedOrder.Total}");
        }

        private static async Task Shutdown(HttpClient httpClient)
        {
            await httpClient.DeleteAsync("api/orders");
        }

        private static async Task WaitUntilBackendReady(HttpClient httpClient)
        {
            var success = false;
            do
            {
                try
                {
                    var response = await httpClient.GetAsync("api/orders");
                    success = response.IsSuccessStatusCode;
                }
                catch (HttpRequestException)
                {
                }
            } while (!success);

            Console.WriteLine("Ready");
            Console.WriteLine();
        }

        private static int orderNumber;
    }
}
