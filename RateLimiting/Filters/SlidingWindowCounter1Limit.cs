namespace RateLimiting.Filters
{
    public class SlidingWindowCounter1Limit : RateLimitFilter
    {
        public SlidingWindowCounter1Limit(string resource) : base(resource)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.SlidingWindowCounter1;
    }
}
