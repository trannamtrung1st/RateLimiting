namespace RateLimiting.Filters
{
    public class LeakyBucketLimit : RateLimitFilter
    {
        public LeakyBucketLimit(string resource) : base(resource)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.LeakyBucket;
    }
}
