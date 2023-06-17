namespace RateLimiting.Filters
{
    public class FixedWindowCounterLimit : RateLimitFilter
    {
        public FixedWindowCounterLimit(string resource) : base(resource)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.FixedWindowCounter;
    }
}
