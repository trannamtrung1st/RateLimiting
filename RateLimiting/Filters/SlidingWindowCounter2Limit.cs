namespace RateLimiting.Filters
{
    public class SlidingWindowCounter2Limit : RateLimitFilter
    {
        public SlidingWindowCounter2Limit(string resource) : base(resource)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.SlidingWindowCounter2;
    }
}
