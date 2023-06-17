namespace RateLimiting.Filters
{
    public class TokenBucketLimit : RateLimitFilter
    {
        public TokenBucketLimit(string resource) : base(resource)
        {
        }

        public override string Algorithm => Constants.RateLimitingAlgorithms.TokenBucket;
    }
}
