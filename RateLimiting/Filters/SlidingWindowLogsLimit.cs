using RateLimiting.Services.RateLimiting;

namespace RateLimiting.Filters
{
    public class SlidingWindowLogsLimit : RateLimitFilter
    {
        public SlidingWindowLogsLimit(IRateLimiterProvider rateLimiterProvider) : base(rateLimiterProvider)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.SlidingWindowLogs;
    }
}
