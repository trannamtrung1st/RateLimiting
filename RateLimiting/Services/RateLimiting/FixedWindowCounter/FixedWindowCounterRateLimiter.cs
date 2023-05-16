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
            int maxAmount = _configuration.GetValue<int>("RateLimitingSettings:FixedWindowCounter:MaxAmount");
            long currentWindow = GetCurrentWindow(_configuration);

            IDatabase db = _connectionMultiplexer.GetDatabase();

            string storedKey = $"{GetStoredKey(key)}:{currentWindow}";

            long requestCount = db.StringIncrement(storedKey);

            bool accepted = requestCount <= maxAmount;

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

        public static long GetCurrentWindow(IConfiguration configuration)
        {
            int windowInterval = configuration.GetValue<int>("RateLimitingSettings:FixedWindowCounter:WindowInterval");
            return (long)TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds / windowInterval;
        }
    }
}
