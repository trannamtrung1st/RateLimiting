using RateLimiting.Services.RateLimiting;

namespace RateLimiting.Filters
{
    public class FixedWindowCounterLimit : RateLimitFilter
    {
        public FixedWindowCounterLimit(IRateLimiterProvider rateLimiterProvider) : base(rateLimiterProvider)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.FixedWindowCounter;
    }
}
