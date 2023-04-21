namespace RateLimiting
{
    public static class Constants
    {
        public static class HttpItemsKeys
        {
            public const string RateLimitingKey = nameof(RateLimitingKey);
        }

        public static class RateLimitingAlgorithms
        {
            public const string TokenBucket = nameof(TokenBucket);
        }
    }
}
