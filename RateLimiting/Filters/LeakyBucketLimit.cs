using RateLimiting.Services.RateLimiting;

namespace RateLimiting.Filters
{
    public class LeakyBucketLimit : RateLimitFilter
    {
        public LeakyBucketLimit(IRateLimiterProvider rateLimiterProvider) : base(rateLimiterProvider)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.LeakyBucket;
    }
}
