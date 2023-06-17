using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting.SlidingWindowLogs
{
    public class SlidingWindowLogsRateLimiter : RateLimiter
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public SlidingWindowLogsRateLimiter(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public override string Name => Constants.RateLimitingAlgorithms.SlidingWindowLogs;

        public override Task<bool> RequestAccess(string resource, string key, string requestId)
        {
            int maxAmount = _configuration.GetValue<int>("RateLimitingSettings:SlidingWindowLogs:MaxAmount");
            long windowInterval = _configuration.GetValue<long>("RateLimitingSettings:SlidingWindowLogs:WindowInterval");
            long currentWindowTime = (long)TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds;
            long lastWindowTime = currentWindowTime - windowInterval;

            string storedKey = GetStoredKey(resource, key);

            IDatabase db = _connectionMultiplexer.GetDatabase();

            ITransaction trans = db.CreateTransaction();

            trans.AddCondition(Condition.SortedSetLengthLessThan(storedKey, maxAmount, min: lastWindowTime));

            var _ = trans.SortedSetAddAsync(storedKey, requestId, currentWindowTime);

            bool executed = trans.Execute();

            string message = $"request to key {key}, window: {lastWindowTime} - {currentWindowTime}";

            if (executed)
            {
                ConsoleHelper.WriteLineDefault($"Accept {message}");
            }
            else
            {
                ConsoleHelper.WriteLineDefault($"Reject {message}");
            }

            return Task.FromResult(executed);
        }
    }
}
