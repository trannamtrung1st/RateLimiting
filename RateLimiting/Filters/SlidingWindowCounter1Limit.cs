using RateLimiting.Services.RateLimiting;

namespace RateLimiting.Filters
{
    public class SlidingWindowCounter1Limit : RateLimitFilter
    {
        public SlidingWindowCounter1Limit(IRateLimiterProvider rateLimiterProvider) : base(rateLimiterProvider)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.SlidingWindowCounter1;
    }
}
