using RateLimiting.Services.RateLimiting;

namespace RateLimiting.Filters
{
    public class TokenBucketLimit : RateLimitFilter
    {
        public TokenBucketLimit(IRateLimiterProvider rateLimiterProvider) : base(rateLimiterProvider)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.TokenBucket;
    }
}
