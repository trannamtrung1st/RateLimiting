using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting.FixedWindowCounter
{
    public class FixedWindowCounterRateLimiter : RateLimiter
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public FixedWindowCounterRateLimiter(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public override string Name => Constants.RateLimitingAlgorithms.FixedWindowCounter;

        public override Task<bool> RequestAccess(string key, string requestId)
        {
            int windowInterval = _configuration.GetValue<int>("RateLimitingSettings:FixedWindowCounter:WindowInterval");
            int maxAmount = _configuration.GetValue<int>("RateLimitingSettings:FixedWindowCounter:MaxAmount");
            long currentWindow = GetCurrentWindow(windowInterval);

            IDatabase db = _connectionMultiplexer.GetDatabase();

            string storedKey = GetStoredKey(currentWindow.ToString());

            long requestCount = db.HashIncrement(storedKey, key);

            bool accepted = requestCount <= maxAmount;

            if (requestCount == 1)
            {
                // [NOTE] make sure we keep the required bucket as long as necessary for calculation
                TimeSpan expiry = TimeSpan.FromMilliseconds(windowInterval * 2);
                db.KeyExpire(storedKey, expiry, when: ExpireWhen.HasNoExpiry);
            }

            string value = $"request to key {key}, window {currentWindow}, new request count {requestCount}";

            if (accepted)
            {
                ConsoleHelper.WriteLineDefault("Accept " + value);
            }
            else
            {
                ConsoleHelper.WriteLineDefault("Reject " + value);
            }

            return Task.FromResult(accepted);
        }

        public static long GetCurrentWindow(int windowInterval)
        {
            return (long)TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds / windowInterval;
        }
    }
}
