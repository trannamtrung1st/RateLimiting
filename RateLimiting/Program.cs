using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RateLimiting.Services;
using RateLimiting.Services.RateLimiting;
using RateLimiting.Utils;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RateLimiting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            StartLeakyBucketProcessor(host);

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
                IRequestStore requestStore = scope.ServiceProvider.GetRequiredService<IRequestStore>();
                ConnectionMultiplexer connectionMultiplexer = scope.ServiceProvider.GetRequiredService<ConnectionMultiplexer>();

                // [NOTE] this one should be customized based on application business 
                string endpoint = RedisHelper.GetDefaultEndpoint(configuration);

                IServer server = connectionMultiplexer.GetServer(endpoint);

                string keysPattern = RateLimiter.GetStoredKeyDefault(Constants.RateLimitingAlgorithms.LeakyBucket, "*");

                IEnumerable<RedisKey> allLeakyBucketKeys = server.Keys(pattern: keysPattern, pageSize: int.MaxValue);

                foreach (string key in allLeakyBucketKeys)
                {
                    IEnumerable<string> requests = processor.ProcessRequests(key);

                    ConsoleHelper.WriteLineDefault($"Begin processing key {key}, {requests.Count()} request(s)");

                    foreach (string request in requests)
                    {
                        string data = requestStore.GetRequestData(request);

                        if (data != null)
                        {
                            ConsoleHelper.WriteLineDefault($"Processing request {request}, data: {data}");
                        }
                        else
                        {
                            ConsoleHelper.WriteLineDefault($"Processing request {request}, data expired or not found");
                        }
                    }
                }
            }, state: null, 0, processRateMs);
        }
    }
}
