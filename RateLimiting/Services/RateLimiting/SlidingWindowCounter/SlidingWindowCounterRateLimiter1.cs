using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting.SlidingWindowCounter
{
    public class SlidingWindowCounterRateLimiter1 : RateLimiter
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public SlidingWindowCounterRateLimiter1(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public override string Name => Constants.RateLimitingAlgorithms.SlidingWindowCounter1;

        public override async Task<bool> RequestAccess(string resource, string key, string requestId)
        {
            int maxAmount = _configuration.GetValue<int>("RateLimitingSettings:SlidingWindowCounter1:MaxAmount");
            long windowInterval = _configuration.GetValue<long>("RateLimitingSettings:SlidingWindowCounter1:WindowInterval");
            long bucketSize = _configuration.GetValue<long>("RateLimitingSettings:SlidingWindowCounter1:BucketSize");
            long windowBuckets = windowInterval / bucketSize;
            long currentMs = (long)TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds;
            long currentBucket = currentMs / bucketSize;
            long fromBucket = currentBucket - windowBuckets + 1;
            string currentStoredKey = GetStoredKey(resource, currentBucket.ToString());
            bool accepted = false;

            RedisKey[] keys = new RedisKey[windowBuckets];
            keys[windowBuckets - 1] = currentStoredKey;

            for (long bucket = fromBucket, idx = 0; bucket < currentBucket; bucket++, idx++)
            {
                string bucketStoredKey = GetStoredKey(resource, bucket.ToString());
                keys[idx] = bucketStoredKey;
            }

            IDatabase db = _connectionMultiplexer.GetDatabase();
            string lockKey = GetLockKey(resource, key);
            string lockValue = GetRandomLockValue();
            int lockTry = 0;
            const int MaxLockTryCount = DefaultMaxLockTryCount;

            while (lockTry++ < MaxLockTryCount)
            {
                if (await db.LockTakeAsync(lockKey, lockValue, TimeSpan.FromSeconds(DefaultLockTimeOutMs)))
                {
                    try
                    {
                        lockTry = MaxLockTryCount;
                        long[] requests = keys.Select(k =>
                        {
                            RedisValue count = db.HashGet(k, key);
                            return !string.IsNullOrWhiteSpace(count) ? long.Parse(count) : 0;
                        }).ToArray();
                        long requestCount = requests.Sum();

                        ConsoleHelper.WriteLineDefault($"Request count in window ({fromBucket} - {currentBucket}): {requestCount}");

                        if (requestCount < maxAmount)
                        {
                            ConsoleHelper.WriteLineDefault($"Accepted request {requestId} in window ({fromBucket} - {currentBucket})");
                            accepted = true;
                            db.HashIncrement(currentStoredKey, key);

                            if (requests[windowBuckets - 1] == 0)
                            {
                                // [NOTE] make sure we keep the required bucket as long as necessary for calculation
                                TimeSpan expiry = TimeSpan.FromMilliseconds(windowInterval * (windowBuckets + 1));
                                db.KeyExpire(currentStoredKey, expiry, when: ExpireWhen.HasNoExpiry);
                            }
                        }
                        else
                        {
                            ConsoleHelper.WriteLineDefault($"Rejected request {requestId} in window ({fromBucket} - {currentBucket})");
                        }
                    }
                    finally
                    {
                        await db.LockReleaseAsync(lockKey, lockValue);
                    }
                }
                else
                {
                    await Task.Delay(DefaultLockRetryDelay);
                }
            }

            return accepted;
        }
    }
}
