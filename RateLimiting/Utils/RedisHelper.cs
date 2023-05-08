using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiting.Utils
{
    public static class RedisHelper
    {
        public static ConnectionMultiplexer GetConnectionMultiplexer(string endPoint)
        {
            var cfg = new ConfigurationOptions()
            {
                AllowAdmin = true,
            };

            cfg.EndPoints.Add(endPoint);

            return ConnectionMultiplexer.Connect(cfg);
        }

        public static string GetDefaultEndpoint(IConfiguration configuration)
        {
            var endpoints = configuration.GetSection("RedisEndpoints").Get<IEnumerable<string>>();

            return endpoints.First();
        }
    }
}
