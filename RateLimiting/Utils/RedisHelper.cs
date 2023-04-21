using StackExchange.Redis;

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
    }
}
