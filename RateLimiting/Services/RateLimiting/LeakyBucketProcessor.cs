using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiting.Services.RateLimiting
{
    public interface ILeakyBucketProcessor
    {
        IEnumerable<string> ProcessRequests(string key);
    }

    public class LeakyBucketProcessor : ILeakyBucketProcessor
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public LeakyBucketProcessor(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public IEnumerable<string> ProcessRequests(string storedKey)
        {
            int processAmount = _configuration.GetValue<int>("RateLimitingSettings:LeakyBucket:ProcessAmount");

            IDatabase db = _connectionMultiplexer.GetDatabase();

            RedisValue[] requests = db.ListLeftPop(storedKey, processAmount);

            return requests.Select(r => (string)r).ToArray();
        }
    }
}
