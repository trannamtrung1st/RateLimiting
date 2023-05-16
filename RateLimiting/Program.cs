using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RateLimiting.Services.RateLimiting.FixedWindowCounter;
using RateLimiting.Services.RateLimiting.LeakyBucket;
using System.Threading;

namespace RateLimiting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            StartLeakyBucketProcessor(host);

            StartFixedWindowCounterCleaner(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static void StartLeakyBucketProcessor(IHost host)
        {
            IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();

            int processRateMs = configuration.GetValue<int>("RateLimitingSettings:LeakyBucket:ProcessRateMs");

            Timer timer = new Timer((state) =>
            {
                using IServiceScope scope = host.Services.CreateScope();

                ILeakyBucketProcessor processor = scope.ServiceProvider.GetRequiredService<ILeakyBucketProcessor>();

                processor.ProcessRequests();
            }, state: null, 0, processRateMs);
        }

        public static void StartFixedWindowCounterCleaner(IHost host)
        {
            IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();

            int cleaningInterval = configuration.GetValue<int>("RateLimitingSettings:FixedWindowCounter:CleaningInterval");

            Timer timer = new Timer((state) =>
            {
                using IServiceScope scope = host.Services.CreateScope();

                IFixedWindowCounterCleaner cleaner = scope.ServiceProvider.GetRequiredService<IFixedWindowCounterCleaner>();

                cleaner.Clean();
            }, state: null, 0, cleaningInterval);
        }
    }
}
