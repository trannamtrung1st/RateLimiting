using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiting.Services.RateLimiting.LeakyBucket
{
    public interface ILeakyBucketProcessor
    {
        void ProcessRequests();
    }

    public class LeakyBucketProcessor : ILeakyBucketProcessor
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;
        private readonly IRequestStore _requestStore;

        public LeakyBucketProcessor(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration,
            IRequestStore requestStore)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
            _requestStore = requestStore;
        }

        public void ProcessRequests()
        {
            // [NOTE] this one should be customized based on application business 
            string endpoint = RedisHelper.GetDefaultEndpoint(_configuration);

            IServer server = _connectionMultiplexer.GetServer(endpoint);

            string keysPattern = RateLimiter.GetStoredKeyDefault(Constants.RateLimitingAlgorithms.LeakyBucket, "*");

            IEnumerable<RedisKey> allLeakyBucketKeys = server.Keys(pattern: keysPattern, pageSize: int.MaxValue);

            foreach (string key in allLeakyBucketKeys)
            {
                IEnumerable<string> requests = ProcessRequests(key);

                ConsoleHelper.WriteLineDefault($"Begin processing key {key}, {requests.Count()} request(s)");

                foreach (string request in requests)
                {
                    string data = _requestStore.GetRequestData(request);

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
