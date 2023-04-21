using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateLimiting.Filters;
using RateLimiting.Services.RateLimiting;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiting.Utils
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisConnection(this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddSingleton(provider =>
            {
                var endpoints = configuration.GetSection("RedisEndpoints").Get<IEnumerable<string>>();
                return RedisHelper.GetConnectionMultiplexer(endpoints.First());
            });
        }

        public static IServiceCollection AddRateLimiters(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRateLimiterProvider, RateLimiterProvider>()
                .AddSingleton<IRateLimiter, TokenBucketRateLimiter>()
                .AddTransient<TokenBucketLimit>();
        }
    }
}
