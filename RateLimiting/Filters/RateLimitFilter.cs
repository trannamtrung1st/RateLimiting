using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RateLimiting.Services.RateLimiting;
using RateLimiting.Utils;
using System.Threading.Tasks;

namespace RateLimiting.Filters
{
    public abstract class RateLimitFilter : ActionFilterAttribute
    {
        public abstract string Algorithm { get; }

        private readonly IRateLimiterProvider _rateLimiterProvider;

        public RateLimitFilter(IRateLimiterProvider rateLimiterProvider)
        {
            _rateLimiterProvider = rateLimiterProvider;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string rateLimitingKey = HttpHelper.GetRateLimitingKey(context.HttpContext);

            IRateLimiter rateLimiter = _rateLimiterProvider.GetRateLimiter(Algorithm);

            string requestId = HttpHelper.GetHttpRequestId(context.HttpContext);

            if (!await rateLimiter.RequestAccess(rateLimitingKey, requestId))
            {
                context.Result = new StatusCodeResult(429);
            }
            else
            {
                await next();
            }
        }
    }
}
