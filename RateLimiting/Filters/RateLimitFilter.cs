using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using RateLimiting.Services.RateLimiting;
using RateLimiting.Utils;
using System.Threading.Tasks;

namespace RateLimiting.Filters
{
    public abstract class RateLimitFilter : ActionFilterAttribute
    {
        public string Resource { get; set; }

        public abstract string Algorithm { get; }

        public RateLimitFilter(string resource)
        {
            Resource = resource;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            IRateLimiterProvider rateLimiterProvider = context.HttpContext.RequestServices.GetRequiredService<IRateLimiterProvider>();

            string rateLimitingKey = HttpHelper.GetRateLimitingKey(context.HttpContext);

            IRateLimiter rateLimiter = rateLimiterProvider.GetRateLimiter(Algorithm);

            string requestId = HttpHelper.GetHttpRequestId(context.HttpContext);

            if (!await rateLimiter.RequestAccess(Resource, rateLimitingKey, requestId))
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
