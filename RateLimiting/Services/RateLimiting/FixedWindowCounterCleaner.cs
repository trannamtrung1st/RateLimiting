using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System.Linq;

namespace RateLimiting.Services.RateLimiting
{
    public interface IFixedWindowCounterCleaner
    {
        void Clean();
    }

    public class FixedWindowCounterCleaner : IFixedWindowCounterCleaner
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        private long _lastWindow;

        public FixedWindowCounterCleaner(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
            _lastWindow = 0;
        }

        public void Clean()
        {
            IDatabase db = _connectionMultiplexer.GetDatabase();

            string endpoint = RedisHelper.GetDefaultEndpoint(_configuration);

            IServer server = _connectionMultiplexer.GetServer(endpoint);

            long currentWindow = FixedWindowCounterRateLimiter.GetCurrentWindow(_configuration);
            long windowCounter = currentWindow;

            bool windowExists = true;

            ConsoleHelper.WriteLineDefault($"Cleaning windows older than {currentWindow}");

            while (_lastWindow != windowCounter && windowExists)
            {
                windowCounter -= 1;

                string keysPattern = RateLimiter.GetStoredKeyDefault(
                    Constants.RateLimitingAlgorithms.FixedWindowCounter,
                    $"*:{windowCounter}");

                RedisKey[] allExpiredKeys = server
                    .Keys(pattern: keysPattern, pageSize: int.MaxValue)
                    .ToArray();

                windowExists = allExpiredKeys.Length > 0;

                if (windowExists)
                {
                    ConsoleHelper.WriteLineDefault($"Cleaning FixedWindowCounter keys:\n{string.Join("\n", allExpiredKeys)}");

                    db.KeyDelete(allExpiredKeys);
                }
            }

            _lastWindow = currentWindow;
        }
    }
}
