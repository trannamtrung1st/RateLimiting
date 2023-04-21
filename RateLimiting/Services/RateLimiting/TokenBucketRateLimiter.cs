using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;

namespace RateLimiting.Services.RateLimiting
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

        public override bool RequestAccess(string key)
        {
            int maxBucketAmount = _configuration.GetValue<int>("RateLimitingSettings:TokenBucket:MaxBucketAmount");
            int refillAmount = _configuration.GetValue<int>("RateLimitingSettings:TokenBucket:RefillAmount");
            double refillTimeInMs = _configuration.GetValue<double>("RateLimitingSettings:TokenBucket:RefillTimeInMs");

            string storedKey = GetStoredKey(key);

            IDatabase db = _connectionMultiplexer.GetDatabase();

            string lastUpdateVal = db.HashGet(storedKey, Key_LastUpdate);
            string remainingTokensVal = db.HashGet(storedKey, Key_Tokens);
            long remainingTokens = long.TryParse(remainingTokensVal, out var tokens) ? tokens : 0;
            double lastUpdateMs = double.TryParse(lastUpdateVal, out var time) ? time : 0;
            double now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds;

            if (now - lastUpdateMs >= refillTimeInMs)
            {
                long resetAmount;

                if (lastUpdateMs == 0)
                {
                    resetAmount = maxBucketAmount;
                    lastUpdateMs = now;
                }
                else
                {
                    int refillCount = (int)Math.Floor((now - lastUpdateMs) / refillTimeInMs);
                    resetAmount = Math.Min(
                        maxBucketAmount,
                        remainingTokens + refillCount * refillAmount);
                    lastUpdateMs = Math.Min(now, lastUpdateMs + refillCount * refillTimeInMs);
                }

                remainingTokens = resetAmount;
                db.HashSet(storedKey, Key_Tokens, resetAmount);
                db.HashSet(storedKey, Key_LastUpdate, lastUpdateMs);

                Console.WriteLine($"[{DateTime.Now}] Refill bucket: {resetAmount} token(s)");
            }

            Console.WriteLine($"[{DateTime.Now}] Remaining tokens (before): {remainingTokens}");

            if (remainingTokens > 0)
            {
                Console.WriteLine($"[{DateTime.Now}] Remaining tokens (after): {remainingTokens - 1}");

                db.HashDecrement(storedKey, Key_Tokens);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
