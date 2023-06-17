namespace RateLimiting.Filters
{
    public class SlidingWindowLogsLimit : RateLimitFilter
    {
        public SlidingWindowLogsLimit(string resource) : base(resource)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.SlidingWindowLogs;
    }
}
