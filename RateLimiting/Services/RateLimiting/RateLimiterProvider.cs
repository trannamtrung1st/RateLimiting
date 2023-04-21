
using System.Collections.Generic;
using System.Linq;

namespace RateLimiting.Services.RateLimiting
{
    public interface IRateLimiterProvider
    {
        IRateLimiter GetRateLimiter(string algorithm);
    }

    public class RateLimiterProvider : IRateLimiterProvider
    {
        private readonly IEnumerable<IRateLimiter> _rateLimiters;

        public RateLimiterProvider(IEnumerable<IRateLimiter> rateLimiters)
        {
            _rateLimiters = rateLimiters;
        }

        public IRateLimiter GetRateLimiter(string algorithm)
        {
            var rateLimiter = _rateLimiters.FirstOrDefault(r => r.IsAppliedFor(algorithm));

            return rateLimiter;
        }
    }
}
