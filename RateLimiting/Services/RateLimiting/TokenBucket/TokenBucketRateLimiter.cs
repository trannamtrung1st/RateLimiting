using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting.TokenBucket
{
    // [IMPORTANT] must use distributed lock or Lua script to make it consistent in distributed environments
    public class TokenBucketRateLimiter : RateLimiter
    {
        const string Key_LastUpdate = "LastUpdate";
        const string Key_Tokens = "Tokens";

        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public TokenBucketRateLimiter(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public override string Name => Constants.RateLimitingAlgorithms.TokenBucket;

        public override Task<bool> RequestAccess(string key, string requestId)
        {
            int maxBucketAmount = _configuration.GetValue<int>("RateLimitingSettings:TokenBucket:MaxBucketAmount");
            int refillAmount = _configuration.GetValue<int>("RateLimitingSettings:TokenBucket:RefillAmount");
            double refillIntervalMs = _configuration.GetValue<double>("RateLimitingSettings:TokenBucket:RefillIntervalMs");

            string lastUpdateKey = GetStoredKey(Key_LastUpdate);
            string tokenKey = GetStoredKey(Key_Tokens);

            IDatabase db = _connectionMultiplexer.GetDatabase();

            string lastUpdateVal = db.HashGet(lastUpdateKey, key);
            string remainingTokensVal = db.HashGet(tokenKey, key);
            long remainingTokens = long.TryParse(remainingTokensVal, out var tokens) ? tokens : 0;
            double lastUpdateMs = double.TryParse(lastUpdateVal, out var time) ? time : 0;
            double now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds;

            if (now - lastUpdateMs >= refillIntervalMs)
            {
                long resetAmount;

                if (lastUpdateMs == 0)
                {
                    resetAmount = maxBucketAmount;
                    lastUpdateMs = now;
                }
                else
                {
                    int refillCount = (int)Math.Floor((now - lastUpdateMs) / refillIntervalMs);
                    resetAmount = Math.Min(
                        maxBucketAmount,
                        remainingTokens + refillCount * refillAmount);
                    lastUpdateMs = Math.Min(now, lastUpdateMs + refillCount * refillIntervalMs);
                }

                remainingTokens = resetAmount;
                db.HashSet(tokenKey, key, resetAmount);
                db.HashSet(lastUpdateKey, key, lastUpdateMs);

                ConsoleHelper.WriteLineDefault($"Refill bucket: {resetAmount} token(s)");
            }

            ConsoleHelper.WriteLineDefault($"Remaining tokens (before): {remainingTokens}");

            bool accepted = remainingTokens > 0;

            if (accepted)
            {
                ConsoleHelper.WriteLineDefault($"Remaining tokens (after): {remainingTokens - 1}");

                db.HashDecrement(tokenKey, key);
            }
            else
            {
                ConsoleHelper.WriteLineDefault($"Failed to request, no token left");
            }

            return Task.FromResult(accepted);
        }
    }
}
