using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting.SlidingWindowCounter
{
    public class SlidingWindowCounterRateLimiter2 : RateLimiter
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public SlidingWindowCounterRateLimiter2(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public override string Name => Constants.RateLimitingAlgorithms.SlidingWindowCounter2;

        public override Task<bool> RequestAccess(string key, string requestId)
        {
            int maxAmount = _configuration.GetValue<int>("RateLimitingSettings:SlidingWindowCounter2:MaxAmount");
            long windowInterval = _configuration.GetValue<long>("RateLimitingSettings:SlidingWindowCounter2:WindowInterval");
            long currentMs = (long)TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds;
            long currentWindow = currentMs / windowInterval;
            string currentStoredKey = GetStoredKey(currentWindow.ToString());
            RedisKey[] keys = new RedisKey[2];
            keys[0] = GetStoredKey((currentWindow - 1).ToString());
            keys[1] = currentStoredKey;

            IDatabase db = _connectionMultiplexer.GetDatabase();
            bool accepted = false;

            long[] requests = keys.Select(k =>
            {
                RedisValue count = db.HashGet(k, key);
                return !string.IsNullOrWhiteSpace(count) ? long.Parse(count) : 0;
            }).ToArray();
            long windowStartMs = currentWindow * windowInterval;
            long rollingWindowStartMs = currentMs - windowInterval;
            double lastWindowPercentage = Math.Abs(windowStartMs - rollingWindowStartMs) / (double)windowInterval;
            double approximatedCount = (requests[0] * lastWindowPercentage) + requests[1];

            ConsoleHelper.WriteLineDefault($"Approximated request count: {approximatedCount}");

            if (approximatedCount < maxAmount)
            {
                ConsoleHelper.WriteLineDefault($"Accepted request {requestId}");
                accepted = true;
                db.HashIncrement(currentStoredKey, key);

                if (requests[1] == 0)
                {
                    // [NOTE] make sure we keep the required bucket as long as necessary for calculation
                    TimeSpan expiry = TimeSpan.FromMilliseconds(windowInterval * 3);
                    db.KeyExpire(currentStoredKey, expiry, when: ExpireWhen.HasNoExpiry);
                }
            }
            else
            {
                ConsoleHelper.WriteLineDefault($"Rejected request {requestId}");
            }

            return Task.FromResult(accepted);
        }
    }
}
