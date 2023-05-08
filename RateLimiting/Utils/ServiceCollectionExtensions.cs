using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateLimiting.Filters;
using RateLimiting.Services.RateLimiting;

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
                .AddTransient<TokenBucketLimit>()
                .AddTransient<LeakyBucketLimit>();
        }
    }
}
