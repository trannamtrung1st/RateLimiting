using RateLimiting.Services.RateLimiting;

namespace RateLimiting.Filters
{
    public class SlidingWindowCounter2Limit : RateLimitFilter
    {
        public SlidingWindowCounter2Limit(IRateLimiterProvider rateLimiterProvider) : base(rateLimiterProvider)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.SlidingWindowCounter2;
    }
}
