using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateLimiting.Services.RateLimiting;
using RateLimiting.Services.RateLimiting.FixedWindowCounter;
using RateLimiting.Services.RateLimiting.LeakyBucket;
using RateLimiting.Services.RateLimiting.SlidingWindowCounter;
using RateLimiting.Services.RateLimiting.SlidingWindowLogs;
using RateLimiting.Services.RateLimiting.TokenBucket;

namespace RateLimiting.Utils
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisConnection(this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddSingleton(provider =>
            {
                return RedisHelper.GetConnectionMultiplexer(RedisHelper.GetDefaultEndpoint(configuration));
            });
        }

        public static IServiceCollection AddRateLimiters(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRateLimiterProvider, RateLimiterProvider>()
                .AddSingleton<IRateLimiter, TokenBucketRateLimiter>()
                .AddSingleton<IRateLimiter, LeakyBucketRateLimiter>()
                .AddSingleton<ILeakyBucketProcessor, LeakyBucketProcessor>()
                .AddSingleton<IRateLimiter, FixedWindowCounterRateLimiter>()
                .AddSingleton<IRateLimiter, SlidingWindowLogsRateLimiter>()
                .AddSingleton<ISlidingWindowLogsCleaner, SlidingWindowLogsCleaner>()
                .AddSingleton<IRateLimiter, SlidingWindowCounterRateLimiter1>()
                .AddSingleton<IRateLimiter, SlidingWindowCounterRateLimiter2>();
        }
    }
}
