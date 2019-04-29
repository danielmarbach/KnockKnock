using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(config => {
                    config.ClearProviders();
                })
                .UseStartup<Startup>();
    }
}
