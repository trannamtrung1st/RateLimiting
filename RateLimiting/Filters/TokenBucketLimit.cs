using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiting.Services.RateLimiting;
using RateLimiting.Utils;

namespace RateLimiting.Filters
{
    public class TokenBucketLimit : ActionFilterAttribute
    {
        private readonly IRateLimiterProvider _rateLimiterProvider;

        public TokenBucketLimit(IRateLimiterProvider rateLimiterProvider)
        {
            _rateLimiterProvider = rateLimiterProvider;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var rateLimitingKey = HttpHelper.GetRateLimitingKey(context.HttpContext);

            var rateLimiter = _rateLimiterProvider.GetRateLimiter(Constants.RateLimitingAlgorithms.TokenBucket);

            if (!rateLimiter.RequestAccess(rateLimitingKey))
            {
                context.Result = new StatusCodeResult(429);
            }
        }
    }
}
