using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System;
using System.Linq;

namespace RateLimiting.Services.RateLimiting.SlidingWindowLogs
{
    public interface ISlidingWindowLogsCleaner
    {
        void Clean();
    }

    public class SlidingWindowLogsCleaner : ISlidingWindowLogsCleaner
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public SlidingWindowLogsCleaner(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public void Clean()
        {
            IDatabase db = _connectionMultiplexer.GetDatabase();

            string endpoint = RedisHelper.GetDefaultEndpoint(_configuration);

            IServer server = _connectionMultiplexer.GetServer(endpoint);

            string keysPattern = RateLimiter.GetStoredKeyDefault("*", Constants.RateLimitingAlgorithms.SlidingWindowLogs, "*");

            RedisKey[] allSlidingWindowLogsKeys = server.Keys(pattern: keysPattern, pageSize: int.MaxValue).ToArray();

            long windowInterval = _configuration.GetValue<long>("RateLimitingSettings:SlidingWindowLogs:WindowInterval");
            long lastWindowTime = (long)TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds - windowInterval;

            ConsoleHelper.WriteLineDefault($"Cleaning sliding windows older than {lastWindowTime}");

            foreach (RedisKey key in allSlidingWindowLogsKeys)
            {
                long removed = db.SortedSetRemoveRangeByScore(key, 0, lastWindowTime);

                ConsoleHelper.WriteLineDefault($"Cleaned {removed} logs of key {key} from old sliding windows");
            }
        }
    }
}
