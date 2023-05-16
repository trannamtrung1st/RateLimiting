namespace RateLimiting
{
    public static class Constants
    {
        public static class CustomHeaders
        {
            public const string ApiKey = nameof(ApiKey);
        }

        public static class HttpItemsKeys
        {
            public const string RateLimitingKey = nameof(RateLimitingKey);
            public const string RequestId = nameof(RequestId);
        }

        public static class RateLimitingAlgorithms
        {
            public const string TokenBucket = nameof(TokenBucket);
            public const string LeakyBucket = nameof(LeakyBucket);
            public const string FixedWindowCounter = nameof(FixedWindowCounter);
            public const string SlidingWindowLogs = nameof(SlidingWindowLogs);
        }
    }
}
