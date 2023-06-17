using Microsoft.Extensions.Configuration;
using RateLimiting.Utils;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RateLimiting.Services.RateLimiting.LeakyBucket
{
    public class LeakyBucketRateLimiter : RateLimiter
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IConfiguration _configuration;

        public LeakyBucketRateLimiter(
            ConnectionMultiplexer connectionMultiplexer,
            IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _configuration = configuration;
        }

        public override string Name => Constants.RateLimitingAlgorithms.LeakyBucket;

        public override Task<bool> RequestAccess(string resource, string key, string requestId)
        {
            if (requestId == null) throw new ArgumentNullException(nameof(requestId));

            int queueAmount = _configuration.GetValue<int>("RateLimitingSettings:LeakyBucket:QueueAmount");

            string storedKey = GetStoredKey(resource, key);

            IDatabase db = _connectionMultiplexer.GetDatabase();

            ITransaction trans = db.CreateTransaction();

            trans.AddCondition(Condition.ListLengthLessThan(storedKey, queueAmount));

            var _ = trans.ListRightPushAsync(storedKey, requestId);

            bool executed = trans.Execute();

            if (executed)
            {
                ConsoleHelper.WriteLineDefault($"Succesfully added to queue key {key}, request {requestId}");
            }
            else
            {
                ConsoleHelper.WriteLineDefault($"Failed to request, queue is full");
            }

            return Task.FromResult(executed);
        }
    }
}
