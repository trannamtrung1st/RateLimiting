using Microsoft.AspNetCore.Http;

namespace RateLimiting.Utils
{
    public static class HttpHelper
    {
        public static string GetRateLimitingKey(HttpContext httpContext)
            => httpContext.Items[Constants.HttpItemsKeys.RateLimitingKey] as string
            ?? string.Empty;
    }
}
