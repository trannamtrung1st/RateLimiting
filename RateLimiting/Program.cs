using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RateLimiting.Services.RateLimiting.LeakyBucket;
using RateLimiting.Services.RateLimiting.SlidingWindowLogs;
using System;
using System.Threading;

namespace RateLimiting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            StartLeakyBucketProcessor(host);

            StartSlidingWindowLogsCleaner(host);

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
                try
                {
                    using IServiceScope scope = host.Services.CreateScope();

                    ILeakyBucketProcessor processor = scope.ServiceProvider.GetRequiredService<ILeakyBucketProcessor>();

                    processor.ProcessRequests();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }, state: null, 0, processRateMs);
        }

        public static void StartSlidingWindowLogsCleaner(IHost host)
        {
            IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();

            int cleaningInterval = configuration.GetValue<int>("RateLimitingSettings:SlidingWindowLogs:CleaningInterval");

            Timer timer = new Timer((state) =>
            {
                try
                {
                    using IServiceScope scope = host.Services.CreateScope();

                    ISlidingWindowLogsCleaner cleaner = scope.ServiceProvider.GetRequiredService<ISlidingWindowLogsCleaner>();

                    cleaner.Clean();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }, state: null, 0, cleaningInterval);
        }
    }
}
